using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Entities;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DomainMappers.Mappers.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TeamMapperTest
    {
        private Mock<Team> mockTeam;
        private TeamEntity mockTeamEntity;
        private TeamMapper teamMapper;

        [TestInitialize]
        public void TestInitialize()
        {
            mockTeam = new Mock<Team>(1, "DreamTeam", "/MyResource/DreamTeam.png");
            SportEntity sport = new SportEntity() { Name = "aSport", IsTwoTeams = false };
            mockTeamEntity = new TeamEntity(1, "DreamTeam", "/MyResource/DreamTeam.png","aSport",sport);
            teamMapper = new TeamMapper();
        }


        [TestMethod]
        public void EntityToTeamTest()
        {
            Team convertedTeam = teamMapper.ToTeam(mockTeamEntity);
            Assert.AreEqual(convertedTeam.Name, mockTeamEntity.Name);
            Assert.AreEqual(convertedTeam.PhotoPath, mockTeamEntity.Photo);
        }
    }
}