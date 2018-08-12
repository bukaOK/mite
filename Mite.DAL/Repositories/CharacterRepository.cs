using Dapper;
using Mite.CodeData.Enums;
using Mite.DAL.Core;
using Mite.DAL.Entities;
using Mite.DAL.Filters;
using Mite.DAL.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace Mite.DAL.Repositories
{
    public class CharacterRepository : Repository<Character>
    {
        public CharacterRepository(AppDbContext dbContext) : base(dbContext)
        {
        }
        public async Task<Character> GetAsync(Guid id)
        {
            return await DbContext.Characters.Include(x => x.Features).FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<IEnumerable<Character>> GetForPostAsync(string userId)
        {
            return await Table.AsNoTracking().Where(x => !x.Original || x.UserId == userId).ToListAsync();
        }
        public async Task<IEnumerable<Character>> GetByFilterAsync(CharacterTopFilter filter)
        {
            var query = "select * from dbo.\"Characters\" characters left outer join dbo.\"Users\" users " +
                "on characters.\"UserId\"=users.\"Id\" where ";
            switch (filter.OriginalType)
            {
                case CharacterOriginalType.Original:
                    query += "\"Original\" is true ";
                    break;
                case CharacterOriginalType.NonOriginal:
                    query += "\"Original\" is false ";
                    break;
                case CharacterOriginalType.All:
                default:
                    query += "\"Original\" is not null ";
                    break;  
            }
            if (!string.IsNullOrEmpty(filter.Input))
                query += "and to_tsvector('mite_ru', characters.\"Name\") @@ plainto_tsquery(@Input) ";

            if (filter.Filter == CharacterFilter.Own)
                query += "and characters.\"UserId\"=@UserId";
            query += ";";
            return await Db.QueryAsync<Character, User, Character>(query, (character, user) =>
            {
                character.User = user;
                return character;
            }, filter);
        }

        public async Task AddWithPostAsync(List<Guid> chars, Guid postId)
        {
            var existingChars = await DbContext.PostCharacters.Where(x => x.PostId == postId).ToListAsync();

            var charsToRemove = existingChars.Where(x => chars.All(y => y != x.CharacterId));
            var charsToAdd = chars.Where(newId => !existingChars.Any(exc => exc.CharacterId == newId))
                .Select(x => new PostCharacter
                {
                    CharacterId = x,
                    PostId = postId
                });

            DbContext.PostCharacters.AddRange(charsToAdd);
            DbContext.PostCharacters.RemoveRange(charsToRemove);
            await SaveAsync();
        }

        public override async Task UpdateAsync(Character entity)
        {
            if (entity.Features != null && entity.Features.Count > 0)
            {
                var newFeatures = entity.Features.Select(x =>
                {
                    x.CharacterId = entity.Id;
                    return x;
                }).ToList();

                entity.Features = null;
                var existingFeatures = await DbContext.CharacterFeatures.Where(x => x.CharacterId == entity.Id).ToListAsync();

                var featuresToUpdate = existingFeatures.Where(x => newFeatures.Any(y => x.Id == y.Id));
                var featuresToRemove = existingFeatures.Where(exf => newFeatures.All(newf => newf.Id != exf.Id));
                var featuresToAdd = newFeatures.Where(newf => !existingFeatures.Any(exf => exf.Id == newf.Id));

                DbContext.CharacterFeatures.AddRange(featuresToAdd);
                foreach(var feature in featuresToUpdate)
                {
                    var newf = newFeatures.First(x => x.Id == feature.Id);
                    feature.Name = newf.Name;
                    feature.Description = newf.Description;
                    DbContext.Entry(feature).State = EntityState.Modified;
                }
                DbContext.CharacterFeatures.RemoveRange(featuresToRemove);
                await SaveAsync();
            }
            else
            {
                var featuresToRemove = DbContext.CharacterFeatures.Where(x => x.CharacterId == entity.Id);
                DbContext.CharacterFeatures.RemoveRange(featuresToRemove);
            }
            var existingChar = await GetAsync(entity.Id);

            DbContext.Entry(existingChar).CurrentValues.SetValues(entity);
            await SaveAsync();
        }
    }
}
