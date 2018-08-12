using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mite.DAL.Entities;
using Mite.DAL.Infrastructure;
using Mite.DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mite.Tests.DAL.Repositories
{
    [TestClass]
    public class CharacterTest
    {
        private CharacterRepository repository;
        private AppDbContext dbContext;

        private Guid CharacterId = Guid.Parse("{EA9E4206-EA56-470D-AA50-8E882FE723D1}");
        private Guid FeatureId1 = Guid.Parse("{B55892B9-80C0-419F-B016-3C60C34D4EA1}");
        private Guid FeatureId2 = Guid.Parse("{8F60D2FE-F9B1-4F85-BAAC-CCCCC56FA352}");
        private Guid FeatureId3 = Guid.Parse("{5D98A8B0-34AF-47A8-8DA5-6A4ECA45B9F7}");

        [TestInitialize]
        public void Init()
        {
            dbContext = new AppDbContext();
            repository = new CharacterRepository(dbContext);

        }
        [TestMethod]
        [Priority(1)]
        public async Task AddAsync_Character()
        {
            var character = new Character
            {
                Id = CharacterId,
                Name = "Fake name",
                DescriptionSrc = "fake descr",
                ImageSrc = "fake image src",
            };
            character.Features = new List<CharacterFeature>
            {
                new CharacterFeature {Id = FeatureId1, CharacterId = character.Id, Description = "fake1", Name = "fakename1" },
                new CharacterFeature {Id = FeatureId2, CharacterId = character.Id, Description = "fake2", Name = "fakename2" }
            };
            await repository.AddAsync(character);
            Assert.IsNotNull(await repository.GetAsync(CharacterId));
        }
        [TestMethod]
        [Priority(2)]
        public async Task UpdateAsync_Character()
        {
            var character = new Character
            {
                Id = CharacterId,
                Name = "new fake name",
                DescriptionSrc = "new fake descr",
                ImageSrc = "fake image src",
            };
            character.Features = new List<CharacterFeature>
            {
                new CharacterFeature {Id = FeatureId1, CharacterId = character.Id, Description = "fake1updated", Name = "fakename1" },
                new CharacterFeature {Id = FeatureId3, CharacterId = character.Id, Description = "fake3", Name = "fakename3" }
            };
            await repository.UpdateAsync(character);
            var newChar = await repository.GetAsync(CharacterId);

            Assert.AreEqual(newChar.DescriptionSrc, character.DescriptionSrc);
            Assert.IsNull(await dbContext.CharacterFeatures.FirstOrDefaultAsync(x => x.Id == FeatureId2));
            Assert.IsNotNull(await dbContext.CharacterFeatures.FirstOrDefaultAsync(x => x.Id == FeatureId1));
            Assert.IsNotNull(await dbContext.CharacterFeatures.FirstOrDefaultAsync(x => x.Id == FeatureId3));
        }
        [TestMethod]
        [Priority(3)]
        public async Task RemoveAsync_Character()
        {
            var character = await repository.GetAsync(CharacterId);
            await repository.RemoveAsync(character);
            var charCheck = await repository.GetAsync(CharacterId);
            Assert.IsNull(charCheck);
            var featureCheck = await dbContext.CharacterFeatures.FirstOrDefaultAsync(x => x.CharacterId == CharacterId);
            Assert.IsNull(featureCheck);
        }
    }
}
