using Mite.DAL.Core;
using Mite.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Mite.DAL.Infrastructure;

namespace Mite.DAL.Repositories
{
    public class SocialLinksRepository : Repository<SocialLinks>
    {
        public SocialLinksRepository(AppDbContext db) : base(db)
        {
            TableName = "SocialLinks";
        }
        public SocialLinks Get(string userId)
        {
            var query = "select * from dbo.SocialLinks where UserId=@userId";
            return Db.QueryFirstOrDefault<SocialLinks>(query, new { userId });
        }
        public Task<SocialLinks> GetAsync(string userId)
        {
            var query = "select * from dbo.SocialLinks where UserId=@userId";
            return Db.QueryFirstOrDefaultAsync<SocialLinks>(query, new { userId });
        }
        public override Task UpdateAsync(SocialLinks entity)
        {
            var query = $"update dbo.{TableName} set Vk=@Vk, Twitter=@Twitter, Facebook=@Facebook, Dribbble=@Dribbble, ArtStation=@ArtStation where Id=@Id";
            return Db.ExecuteAsync(query, entity);
        }
    }
}