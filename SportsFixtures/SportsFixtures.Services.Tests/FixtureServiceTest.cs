using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.BusinessLogic.FixtureAlgorithms;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Mappers;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FixtureServiceTest
    {
        private EncounterDtoMapper mapper;
        private Sport sport;
        private Team teamA;
        private Team teamB;
        private Team teamC;
        private Team teamD;
        private ICollection<Team> teamsCollection;
        private ICollection<string> teamsNames;
        private IEncounterRepository matchStorage;
        private ITeamRepository teamStorage;
        private ISportRepository sportStorage;
        private FixtureService fixtureService;
        private FixtureDto testFixture;
        private DatabaseConnection context;
        private EncounterFactory factory;
        private string algorithmPaths;

        [TestInitialize]
        public void Initialize()
        {
            factory = new EncounterFactory();
            sport = new Sport("Soccer",true);
            teamA = new Mock<Team>(1, "teamA", "photo", sport).Object;
            teamB = new Mock<Team>(2, "teamB", "photo", sport).Object;
            teamC = new Mock<Team>(3, "teamC", "photo", sport).Object;
            teamD = new Mock<Team>(4, "teamD", "photo", sport).Object;
            testFixture = new FixtureDto() { fixtureName= "ObligatorioDA2.BusinessLogic.FixtureAlgorithms.OneMatchFixture"
                ,initialDate = DateTime.Today, daysBetweenRounds = 5, roundLength = 2 };
            teamsCollection = GetTeamsList();
            teamsNames = teamsCollection.Select(tn => tn.Name).ToList();
            SetUpRepository();
            mapper = new EncounterDtoMapper(teamStorage, matchStorage, sportStorage);
            context.Database.EnsureDeleted();
            Mock<IAuthenticationService> auth = new Mock<IAuthenticationService>();
            auth.Setup(a => a.GetConnectedUser()).Returns(GetFakeUser());
            EncounterService service = new EncounterService(matchStorage, teamStorage, sportStorage, auth.Object);
            algorithmPaths = @".\";
            Mock<ILoggerService> logService = new Mock<ILoggerService>();
            fixtureService = new FixtureService(teamStorage,service, auth.Object, logService.Object);
        }

        private void SetUpRepository()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "FixtureService")
                .Options;
            context = new DatabaseConnection(options);
            matchStorage = new EncounterRepository(context);
            teamStorage = new TeamRepository(context);
            sportStorage = new SportRepository(context);
        }

        private void AddTeamsToRepo()
        {
            foreach (Team team in teamsCollection)
            {
                teamStorage.Add(team);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void SetFixtureAlgorithmTest()
        {
            testFixture.fixtureName = "Unexistent";
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
        }

        [TestMethod]
        public void SetFixtureAlgorithmDefaultValues() {
            testFixture.daysBetweenRounds = 0;
            testFixture.roundLength = 0;
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            Assert.IsNotNull(fixtureService.FixtureAlgorithm);
        }

        [TestMethod]
        public void AddFixtureOneMatchTest()
        {
            AddSportAndTeams();
            testFixture.fixtureName = (typeof(OneMatchFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sport.Name);
            Assert.IsTrue(matchesAdded.All(m => matchStorage.Exists(m.id)));
        }

        [TestMethod]
        public void AddFixtureTwoMatchesTest()
        {
            AddSportAndTeams();
            testFixture.fixtureName = (typeof(HomeAwayFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sport.Name);
            Assert.IsTrue(matchesAdded.All(m => matchStorage.Exists(m.id)));
        }

        [TestMethod]
        public void AddFixtureOneMatchImpairTest()

        {
            AddSportAndTeams();
            teamsCollection.Remove(teamD);
            testFixture.fixtureName = (typeof(OneMatchFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sport.Name);
            Assert.IsTrue(matchesAdded.All(m => matchStorage.Exists(m.id)));
        }

        [TestMethod]
        public void AddFixtureTwoMatchesImpairTest()
        {
            AddSportAndTeams();
            teamsCollection.Remove(teamD);
            testFixture.fixtureName = (typeof(HomeAwayFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sport.Name);
            Assert.IsTrue(matchesAdded.All(m => matchStorage.Exists(m.id)));
        }

        [TestMethod]
        public void AddFixtureWithNamesTest()
        {
            AddTeamsToRepo();
            testFixture.fixtureName = (typeof(OneMatchFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            string sportName = sport.Name;
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sportName);
            Assert.IsTrue(matchesAdded.All(m => matchStorage.Exists(m.id)));
        }

        [TestMethod]
        public void AddFixtureOfSport()
        {
            AddTeamsToRepo();
            testFixture.fixtureName = (typeof(OneMatchFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(sport.Name);
            Assert.IsTrue(matchesAdded.All(m => matchStorage.Exists(m.id)));
        }

        //Exceptions

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddFixtureOfNoExistentSport()
        {
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture("Golf");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddFixtureInvalidDate()
        {
            testFixture.initialDate = new DateTime();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture("Golf");
        }

        [TestMethod]
        [ExpectedException(typeof(WrongFixtureException))]
        public void AddFixtureTeamAlreadyHasMatchTest()
        {
            AddSportAndTeams();
            testFixture.fixtureName = (typeof(OneMatchFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            Encounter aMatch = factory.CreateEncounter(new List<Team>() { teamA, teamB }, testFixture.initialDate, sport);
            matchStorage.Add(aMatch);
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sport.Name);
            Assert.IsTrue(matchStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddFixtureTeamNotFoundTest()
        {
            ICollection<string> teamsNames = teamsCollection.Select(t => t.Name).ToList();
            string sportName = sport.Name;
            ICollection<EncounterDto> matchesAdded = fixtureService.AddFixture(teamsNames, sportName);
        }

        [TestMethod]
        [ExpectedException(typeof(WrongFixtureException))]
        public void AddFixtureTooFewTeamsTest() {
            sportStorage.Add(sport);
            teamStorage.Add(teamA);
            testFixture.fixtureName = (typeof(OneMatchFixture)).ToString();
            fixtureService.SetFixtureAlgorithm(testFixture, algorithmPaths);
            fixtureService.AddFixture(sport.Name);
        }

        [TestMethod]
        public void GetAvailableStrategiesTest() {
            ICollection<Type> algorithms = fixtureService.GetAlgorithms(@"..\");
            Assert.AreEqual(0, algorithms.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetAvailableStrategiesInvalidPathTest()
        {
            ICollection<Type> algorithms = fixtureService.GetAlgorithms("wrong path");
            Assert.AreEqual(0, algorithms.Count);
        }

        private ICollection<Team> GetTeamsList()
        {
            return new List<Team> {teamA,
                teamB,
                teamC,
                teamD
            };
        }

        private void AddSportAndTeams() {
            sportStorage.Add(sport);
            teamStorage.Add(teamA);
            teamStorage.Add(teamB);
            teamStorage.Add(teamC);
            teamStorage.Add(teamD);
        }

        private UserDto GetFakeUser()
        {
            return new UserDto()
            {
                name = "name",
                surname = "surname",
                username = "username",
                password = "password",
                email = "mail@domain.com",
                isAdmin = true
            };
        }
    }
}
