using ObligatorioDA2.BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DomainMappers.Mappers.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SportMapperTest
    {
        private SportMapper testMapper;
        private Mock<Sport> sport;
        private SportEntity entity;
        [TestInitialize]
        public void SetUp()
        {
            testMapper = new SportMapper();
            sport = new Mock<Sport>("Soccer",true);
            entity = new SportEntity()
            {
                Name = "Soccer",
            };
        }


        [TestMethod]
        public void SportToEntityNameTest()
        {
            SportEntity converted = testMapper.ToEntity(sport.Object);
            Assert.AreEqual(converted.Name, entity.Name);
        }


        [TestMethod]
        public void EntityToSportNameTest()
        {
            Sport converted = testMapper.ToSport(entity);
            Assert.AreEqual(converted.Name, sport.Object.Name);
        }

    }
}
