using ObligatorioDA2.BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DomainMappers.Mappers.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EncounterMapperTest
    {
        private EncounterMapper testMapper;
        private EncounterEntity entity;
        private ICollection<EncounterTeam> playingTeams;
        private EncounterFactory factory;
        private Encounter match;
        Team homeMock;
        Team awayMock;

        [TestInitialize]
        public void SetUp()
        {
            factory = new EncounterFactory();
            testMapper = new EncounterMapper();
            SportEntity testSport = new SportEntity() { Name = "Soccer", IsTwoTeams = true };
            TeamEntity homeTest = new TeamEntity { TeamNumber = 3, SportEntityName = "Soccer", Sport =testSport,Name = "Nacional", Photo = "aPath" };
            TeamEntity awayTest = new TeamEntity { TeamNumber = 4, SportEntityName = "Soccer", Sport = testSport,Name = "Torque", Photo = "aPath" };
            entity = new EncounterEntity()
            {
                Id = 3,
                Date = DateTime.Now,
                SportEntity = testSport,
                HasResult = false,
                Commentaries = new List<CommentEntity>()
            };
            EncounterTeam homeRel = new EncounterTeam() { Team = homeTest, TeamNumber = 3, Encounter = entity, EncounterId = 3 };
            EncounterTeam awayRel = new EncounterTeam() { Team = awayTest, TeamNumber = 4, Encounter = entity, EncounterId = 3 };
            playingTeams = new List<EncounterTeam>() { homeRel, awayRel };
            homeMock = new Team(3, "Nacional", "aPath", new Sport("Soccer",true));
            awayMock = new Team(4, "Torque", "aPath", new Sport("Soccer",true));
            Sport sport = new Sport("Soccer",true);
            match = factory.CreateEncounter(new List<Team>() { homeMock, awayMock }, DateTime.Now, sport);
        }

        [TestMethod]
        public void GetMatchTeamsTest()
        {
            ICollection<EncounterTeam> teams = testMapper.ConvertParticipants(match);
            Assert.AreEqual(teams.Count, match.GetParticipants().Count);
        }

        [TestMethod]
        public void MatchToEntityDateTest()
        {
            EncounterEntity converted = testMapper.ToEntity(match);
            Assert.AreEqual(converted.Date.ToString(), entity.Date.ToString());
        }

        [TestMethod]
        public void MatchToEntityCommentsCountTest()
        {
            UserId identity = new UserId
            {
                Name = "aName",
                Surname = "aSurname",
                UserName = "aUsername",
                Password = "aPassword",
                Email = "anEmail@aDomain.com"
            };
            Mock<User> user = new Mock<User>(identity, false);
            match.AddCommentary(new Commentary("test comment", user.Object));
            EncounterEntity converted = testMapper.ToEntity(match);
            Assert.AreEqual(converted.Commentaries.Count, 1);
        }

        [TestMethod]
        public void MatchToEntityNoResultTest() {
            EncounterEntity converted = testMapper.ToEntity(match);
            Assert.IsFalse(converted.HasResult);
        }

        [TestMethod]
        public void MatchToEntityWithResultTest()
        {
            Result matchResult = GetFakeResult();
            match.Result=matchResult;
            EncounterEntity converted = testMapper.ToEntity(match);
            Assert.IsTrue(converted.HasResult);
        }

        [TestMethod]
        public void MatchToEntityResultsTest()
        {
            Result matchResult = GetFakeResult();
            match.Result=matchResult;
            ICollection<EncounterTeam> teams = testMapper.ConvertParticipants(match);
            Assert.IsTrue(teams.Any(t => t.Position == 1));
            Assert.IsTrue(teams.Any(t => t.Position == 2));
        }

        private Result GetFakeResult()
        {
            Result toGenerate = new Result();
            toGenerate.Add(homeMock, 1);
            toGenerate.Add(awayMock, 2);
            return toGenerate;
        }

        [TestMethod]
        public void EntityToMatchTeamsTest()
        {
            Encounter conversion = testMapper.ToEncounter(entity,playingTeams);
            Assert.AreEqual(match.GetParticipants().Count, playingTeams.Count);
        }

        [TestMethod]
        public void EntityToMatchDateTest()
        {
            Encounter conversion = testMapper.ToEncounter(entity,playingTeams);
            Assert.AreEqual(match.Date.ToString(), conversion.Date.ToString());
        }

        [TestMethod]
        public void EntityToMatchCommentsTest()
        {
            entity.Commentaries.Add(new CommentEntity()
            {
                Id = 3,
                Text = "text",
                Maker = new UserEntity
                {
                    Name = "aName",
                    Surname = "aSurname",
                    UserName = "aUsername",
                    Password = "aPassword",
                    Email = "aEmail@aDomain.com"
                }
            });
            Encounter conversion = testMapper.ToEncounter(entity,playingTeams);
            Assert.AreEqual(conversion.GetAllCommentaries().Count, 1);
        }

        [TestMethod]
        public void EntityToMatchNoResultTest() {
            Encounter conversion = testMapper.ToEncounter(entity, playingTeams);
            Assert.IsFalse(conversion.HasResult());
        }

        [TestMethod]
        public void EntityToMatchWithResultTest() {
            entity.HasResult = true;
            int position = 1;
            foreach (EncounterTeam team in playingTeams) {
                team.Position = position;
                position++;
            }
            Encounter conversion = testMapper.ToEncounter(entity, playingTeams);
            ICollection<Tuple<Team,int>> translated = conversion.Result.GetPositions();
            Assert.IsTrue(translated.Any(t=>t.Item2 == 1));
            Assert.IsTrue(translated.Any(t => t.Item2 == 2));
        }
    }
}
