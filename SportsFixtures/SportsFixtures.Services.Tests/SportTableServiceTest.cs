using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using Match = ObligatorioDA2.BusinessLogic.Match;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SportTableServiceTest
    {
        private ISportTableService serviceToTest;
        private Mock<ISportRepository> sportsStorage;
        private Mock<ITeamRepository> teamsStorage;
        private Mock<IInnerEncounterService> matchesService;

        [TestInitialize]
        public void SetUp() {
            sportsStorage = new Mock<ISportRepository>();
            teamsStorage = new Mock<ITeamRepository>();
            matchesService = new Mock<IInnerEncounterService>();
            serviceToTest = new SportTableService(sportsStorage.Object, teamsStorage.Object, matchesService.Object);
            SetUpSports();
            SetUpTeams();
        }

        private void SetUpSports()
        {
            Sport twoTeamSport = new Sport("Soccer", true);
            Sport multipleTeamSport = new Sport("Archery", false);
            sportsStorage.Setup(r => r.Get("Soccer")).Returns(twoTeamSport);
            sportsStorage.Setup(r => r.Get("Archery")).Returns(multipleTeamSport);
            sportsStorage.Setup(r => r.Get(It.Is<string>(s => !s.Equals("Soccer") && !s.Equals("Archery"))))
                .Throws(new SportNotFoundException());
        }

        private void SetUpTeams()
        {
            Sport twoTeamSport = new Sport("Soccer", true);
            Team teamA = new Team(1,"teamA", "photoA", twoTeamSport);
            Team teamB = new Team(2,"teamB", "photoB", twoTeamSport);
            Team teamC = new Team(3,"teamC", "photoC", twoTeamSport);
            ICollection<Encounter> matches = CreateMatches(teamA, teamB, teamC,twoTeamSport);
            matchesService.Setup(r => r.GetAllEncounters("Soccer")).Returns(matches);

            Sport multipleTeamSport = new Sport("Archery", false);
            Team athleteD = new Team(4,"athleteD", "photoD", multipleTeamSport);
            Team athleteE = new Team(5,"athleteE", "photoE", multipleTeamSport);
            Team athleteF = new Team(6,"athleteF", "photoF", multipleTeamSport);
            ICollection<Encounter> competitions = CreateCompetitions(athleteD, athleteE,athleteF,multipleTeamSport);
            matchesService.Setup(r => r.GetAllEncounters("Archery")).Returns(competitions);

            teamsStorage.Setup(r => r.GetTeams("Soccer")).Returns(new List<Team>() { teamA, teamB, teamC });
            teamsStorage.Setup(r => r.GetTeams("Archery")).Returns(new List<Team>() { athleteD, athleteE, athleteF });
        }

        private ICollection<Encounter> CreateMatches(Team teamA, Team teamB, Team teamC,Sport played)
        {
            //Create matches.
            Match matchAvsB = new Match(new List<Team>() {teamA,teamB },DateTime.Now,played);
            Match matchAvsC = new Match(new List<Team>() { teamA, teamC }, DateTime.Now, played);
            Match matchBvsC = new Match(new List<Team>() { teamB, teamC }, DateTime.Now, played);
            //Create results.
            Result resultAvsB = new Result();
            resultAvsB.Add(teamA, 1);
            resultAvsB.Add(teamB, 2);
            Result resultAvsC = new Result();
            resultAvsC.Add(teamA, 2);
            resultAvsC.Add(teamC, 1);
            Result resultBvsC = new Result();
            resultBvsC.Add(teamB, 1);
            resultBvsC.Add(teamC, 1);
            //Assign results to matches.
            matchAvsB.Result = resultAvsB;
            matchAvsC.Result = resultAvsC;
            matchBvsC.Result = resultBvsC;

            return new List<Encounter>() { matchAvsB, matchAvsC, matchBvsC };
        }
        private ICollection<Encounter> CreateCompetitions(Team teamD, Team teamE, Team teamF, Sport played)
        {
            //Create competition.
            Encounter competitiomDEF = new Competition(new List<Team>() { teamD, teamE, teamF }, DateTime.Now, played);
            Encounter competitionED = new Competition(new List<Team>() { teamE, teamD }, DateTime.Now, played);
            Encounter competitionEDF = new Competition(new List<Team>() { teamE, teamD,teamF }, DateTime.Now, played);
            Encounter noResultCompetition = new Competition(new List<Team>() { teamD, teamE, teamF }, DateTime.Now, played);
            //Create results.
            Result resultDEF = new Result();
            resultDEF.Add(teamD, 2);
            resultDEF.Add(teamE, 3);
            resultDEF.Add(teamF, 1);
            Result resultED = new Result();
            resultED.Add(teamE, 2);
            resultED.Add(teamD, 1);
            Result resultEDF = new Result();
            resultEDF.Add(teamE, 1);
            resultEDF.Add(teamD, 2);
            resultEDF.Add(teamF, 3);
            //Assign results to competitions.
            competitiomDEF.Result = resultDEF;
            competitionED.Result = resultED;
            competitionEDF.Result = resultEDF;
            return new List<Encounter>() { competitiomDEF, competitionED, competitionEDF, noResultCompetition };
        }

        [TestMethod]
        public void ArcheryTableTest() {
            List<Tuple<TeamDto, int>> positions = serviceToTest.GetScoreTable("Archery").ToList();
            //ids.
            Assert.AreEqual(positions[0].Item1.id, 4);
            Assert.AreEqual(positions[1].Item1.id, 5);
            Assert.AreEqual(positions[2].Item1.id, 6);
            //names.
            Assert.AreEqual(positions[0].Item1.name, "athleteD");
            Assert.AreEqual(positions[1].Item1.name, "athleteE");
            Assert.AreEqual(positions[2].Item1.name, "athleteF");
            //points.
            Assert.AreEqual(positions[0].Item2, 7);
            Assert.AreEqual(positions[1].Item2, 6);
            Assert.AreEqual(positions[2].Item2, 4);
        }

        [TestMethod]
        public void SoccerTableTest() {
            List<Tuple<TeamDto, int>> positions = serviceToTest.GetScoreTable("Soccer").ToList();
            //ids.
            Assert.AreEqual(3,positions[0].Item1.id);
            Assert.AreEqual(1,positions[1].Item1.id);
            Assert.AreEqual(2,positions[2].Item1.id);
            //names.
            Assert.AreEqual("teamC",positions[0].Item1.name);
            Assert.AreEqual("teamA",positions[1].Item1.name);
            Assert.AreEqual("teamB",positions[2].Item1.name);
            //points.
            Assert.AreEqual(4,positions[0].Item2);
            Assert.AreEqual(3,positions[1].Item2);
            Assert.AreEqual(1,positions[2].Item2);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void CalculateTableOfNoExistingSportTest() {
            sportsStorage.Setup(r => r.Get(It.IsAny<string>())).Throws(new SportNotFoundException());
            serviceToTest.GetScoreTable("Soccer");
        }
    }
}
