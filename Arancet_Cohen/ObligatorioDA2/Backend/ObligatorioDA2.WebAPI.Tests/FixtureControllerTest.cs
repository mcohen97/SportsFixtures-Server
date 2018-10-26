﻿using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services;
using ObligatorioDA2.Services.Interfaces;
using ObligatorioDA2.WebAPI.Controllers;
using ObligatorioDA2.WebAPI.Models;
using System;
using System.Collections.Generic;
using Match = ObligatorioDA2.BusinessLogic.Match;
using Microsoft.Extensions.Options;
using System.IO;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    public class FixtureControllerTest
    {
        private FixturesController controller;
        private Sport testSport;
        private Mock<ISportRepository> sportsRepo;
        private Mock<IMatchRepository> matchesRepo;
        private Mock<ITeamRepository> teamsRepo;
        private IFixtureService fixture;
        private ICollection<Team> teamsCollection;
        private ICollection<Encounter> oneMatchCollection;
        private ICollection<Encounter> homeAwayMatchCollection;
        private Mock<IOptions<FixtureStrategies>> settings;


        [TestInitialize]
        public void Initialize()
        {
            SetUpData();
            SetUpController();
        }

        private void SetUpData()
        {
            testSport = new Sport("Football",true);

            Team teamA = new Team(1, "teamA", "photo", testSport);
            Team teamB = new Team(2, "teamB", "photo", testSport);
            Team teamC = new Team(3, "teamC", "photo", testSport);
            Team teamD = new Team(4, "teamD", "photo", testSport);
            teamsCollection = new List<Team>
            {
                teamA,
                teamB,
                teamC,
                teamD
            };
            oneMatchCollection = new List<Encounter>
            {
                new Match(1, List(teamA, teamB), DateTime.Now.AddDays(1), testSport),
                new Match(2, List(teamC, teamD), DateTime.Now.AddDays(1), testSport),
                new Match(3, List(teamA, teamC), DateTime.Now.AddDays(8), testSport),
                new Match(4, List(teamD, teamB), DateTime.Now.AddDays(8), testSport),
                new Match(5, List(teamA, teamD), DateTime.Now.AddDays(16), testSport),
                new Match(6, List(teamC, teamB), DateTime.Now.AddDays(16), testSport)
            };
            homeAwayMatchCollection = new List<Encounter>
            {
                new Match(1, List(teamA, teamB), DateTime.Now.AddDays(1), testSport),
                new Match(2, List(teamC, teamD), DateTime.Now.AddDays(1), testSport),
                new Match(3, List(teamA, teamC), DateTime.Now.AddDays(8), testSport),
                new Match(4, List(teamD, teamB), DateTime.Now.AddDays(8), testSport),
                new Match(5, List(teamA, teamD), DateTime.Now.AddDays(16), testSport),
                new Match(6, List(teamC, teamB), DateTime.Now.AddDays(16), testSport),
                new Match(7, List(teamB, teamA), DateTime.Now.AddDays(24), testSport),
                new Match(8, List(teamD, teamC), DateTime.Now.AddDays(24), testSport),
                new Match(9, List(teamC, teamA), DateTime.Now.AddDays(32), testSport),
                new Match(10, List(teamB, teamD), DateTime.Now.AddDays(32), testSport),
                new Match(11, List(teamD, teamA), DateTime.Now.AddDays(40), testSport),
                new Match(12, List(teamB, teamC), DateTime.Now.AddDays(40), testSport)
            };
        }

        private ICollection<Team> List(Team home, Team away) {
            return new List<Team>() { home, away };
        }
        private void SetUpController()
        {
            sportsRepo = new Mock<ISportRepository>();
            sportsRepo.Setup(r => r.Get((testSport.Name))).Returns(testSport);
            sportsRepo.Setup(r => r.Get(It.Is<String>(x => (x != testSport.Name)))).Throws(new SportNotFoundException());
            sportsRepo.Setup(r => r.GetAll()).Returns(new List<Sport>() { testSport });

            matchesRepo = new Mock<IMatchRepository>();
            matchesRepo.Setup(m => m.Add(It.IsAny<Match>())).Returns((Match mat) => { return mat; });
            matchesRepo.Setup(m => m.Exists(It.IsAny<int>())).Returns(false);
            matchesRepo.Setup(m => m.GetAll()).Returns(new List<Encounter>());

            teamsRepo = new Mock<ITeamRepository>();
            teamsRepo.Setup(t => t.GetTeams(It.IsAny<string>())).Returns(teamsCollection);
            teamsRepo.Setup(t => t.GetAll()).Returns(teamsCollection);

            fixture = new FixtureService(matchesRepo.Object, teamsRepo.Object, sportsRepo.Object);

            Mock<IOptions<FixtureStrategies>> mockSettings = new Mock<IOptions<FixtureStrategies>>();
            FileInfo dllFile = new FileInfo(@".\");
            mockSettings.Setup(m => m.Value).Returns(new FixtureStrategies() { DllPath = dllFile.FullName });
            controller = new FixturesController(fixture,mockSettings.Object,sportsRepo.Object);
        }

        [TestMethod]
        public void CreateFixtureTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
                Day = DateTime.Now.Day,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                FixtureName = "ObligatorioDA2.BusinessLogic.FixtureAlgorithms.OneMatchFixture"
            };

            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name,input);
            CreatedResult createdResult = result as CreatedResult;
            ICollection<MatchModelOut> modelOut = createdResult.Value as ICollection<MatchModelOut>;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(modelOut.Count, oneMatchCollection.Count);
        }

        [TestMethod]
        public void CreateFixtureWrongFixtureTest()
        {
            //Arrange
            FixtureModelIn input = new FixtureModelIn()
            {
                Day = DateTime.Now.Day,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                FixtureName = "ObligatorioDA2.BusinessLogic.FixtureAlgorithms.OneMatchFixture"
            };
            matchesRepo.Setup(m => m.GetAll()).Returns(oneMatchCollection);
            ICollection<string> errorMessagges = TeamAlreadyHasMatchErrorMessagges(oneMatchCollection);


            //Act
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.IsTrue(errorMessagges.Contains(error.ErrorMessage));
        }

        [TestMethod]
        public void CreateOneMatchFixtureWrongDateFixtureTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
                Day = 99,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                FixtureName = "OneMatchFixture"
            };
            matchesRepo.Setup(m => m.GetAll()).Returns(oneMatchCollection);
            string errorMessagge = "Invalid date format";

            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.AreEqual(error.ErrorMessage, errorMessagge);
        }

        private ICollection<string> TeamAlreadyHasMatchErrorMessagges(ICollection<Encounter> matches)
        {
            ICollection<string> errorMessagges = new List<string>();
            foreach (Match aMatch in matches)
            {
                foreach (Team team in aMatch.GetParticipants()) {
                    errorMessagges.Add(team.Name + " already has a match on date " + new DateTime(aMatch.Date.Year, aMatch.Date.Month, aMatch.Date.Day));
                }
            }
            return errorMessagges;
        }

        [TestMethod]
        public void CreateFixtureBadModelTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
            };
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void CreateHomeAwayFixtureTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
                Day = DateTime.Now.Day,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                FixtureName = "ObligatorioDA2.BusinessLogic.FixtureAlgorithms.HomeAwayFixture"
            };

            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            CreatedResult createdResult = result as CreatedResult;
            ICollection<MatchModelOut> modelOut = createdResult.Value as ICollection<MatchModelOut>;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(modelOut.Count, homeAwayMatchCollection.Count);
        }

        [TestMethod]
        public void CreateHomeAwayFixtureWrongFixtureTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
                Day = DateTime.Now.Day,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                FixtureName = "ObligatorioDA2.BusinessLogic.FixtureAlgorithms.HomeAwayFixture"
            };
            matchesRepo.Setup(m => m.GetAll()).Returns(homeAwayMatchCollection);
            ICollection<string> errorMessagges = TeamAlreadyHasMatchErrorMessagges(homeAwayMatchCollection);


            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.IsTrue(errorMessagges.Contains(error.ErrorMessage));
        }

        [TestMethod]
        public void CreateHomeAwayFixtureWrongDateFixtureTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
                Day = 99,
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                FixtureName = "HomeAwayFixture"
            };
            matchesRepo.Setup(m => m.GetAll()).Returns(homeAwayMatchCollection);


            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void CreateHomeAwayFixtureBadModelTest()
        {
            //Arrange.
            FixtureModelIn input = new FixtureModelIn()
            {
            };
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.CreateFixture(testSport.Name, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void GetAllFixtureAlgorithmsTest() {

            //Act.
            IActionResult result = controller.GetFixtureAlgorithms();
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<string> strategies = okResult.Value as ICollection<string>;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(strategies);
        }
    }
}
