﻿using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Interfaces;
using ObligatorioDA2.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.WebAPI.Controllers;
using ObligatorioDA2.WebAPI.Models;
using Microsoft.Extensions.Options;
using System.Linq;
using ObligatorioDA2.Services.Interfaces.Dtos;
using ObligatorioDA2.Services.Exceptions;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    public class SportsControllerTest
    {
        private Mock<ISportService> sportsService;
        private Mock<ITeamService> teamsRepo;
        private Mock<ISportTableService> tableGenerator;
        private SportsController controllerToTest;

        [TestInitialize]
        public void SetUp() {
            SetUpRepository();
        }

        private void SetUpRepository()
        {
            sportsService = new Mock<ISportService>();
            teamsRepo = new Mock<ITeamService>();
            Mock<IMatchRepository> matchesRepo = new Mock<IMatchRepository>();
            Mock<IAuthenticationService> mockService = new Mock<IAuthenticationService>();
            Mock<IImageService> mockImgService = new Mock<IImageService>();

            Sport testSport1 = new Sport("Tennis",true);
            Sport testSport2 = new Sport("Basketball",true);

            sportsService.Setup(s => s.AddSport(It.IsAny<SportDto>())).Returns(testSport1);
            sportsService.Setup(r => r.GetSport("Tennis")).Returns(testSport1);
            sportsService.Setup(r => r.GetSport(It.Is<String>(x => (x != "Tennis") && (x !="Basketball")))).Throws(new SportNotFoundException());
            sportsService.Setup(r => r.GetAllSports()).Returns(new List<Sport>() {new Sport("Basketball",true), new Sport("Tennis",true) });

            IFixtureService dummyService = new Mock<IFixtureService>().Object;
            tableGenerator = new Mock<ISportTableService>();

            controllerToTest = new SportsController(sportsService.Object, teamsRepo.Object, dummyService, 
                tableGenerator.Object, mockService.Object, mockImgService.Object);
        }

        [TestMethod]
        public void CreateSportTest() {
            //Arrange.
            SportModelIn input= new SportModelIn()
            {
                Name = "Tennis"
            };

            //Act.
            IActionResult result = controllerToTest.Post(input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            SportModelOut output = createdResult.Value as SportModelOut;
            //Assert.
            sportsService.Verify(r => r.AddSport(It.IsAny<SportDto>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(createdResult.StatusCode, 201);
            Assert.AreEqual(createdResult.RouteName, "GetSportById");
            Assert.IsNotNull(output);
            Assert.AreEqual(output.Name, input.Name);
        }

        [TestMethod]
        public void CreateSportAlreadyExistingTest() {
            //Arrange.
            Exception internalEx = new SportAlreadyExistsException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_ALREADY_EXISTS);
            sportsService.Setup(r => r.AddSport(It.IsAny<SportDto>())).Throws(toThrow);
            SportModelIn input = new SportModelIn()
            {
                Name = "Soccer"
            };

            //Act.
            IActionResult result = controllerToTest.Post(input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            sportsService.Verify(r => r.AddSport(It.IsAny<SportDto>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400,badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void CreateInvalidSportTest() {
            SportModelIn input = new SportModelIn();

            controllerToTest.ModelState.AddModelError("", "Error");
            IActionResult result = controllerToTest.Post(input);

            BadRequestObjectResult createdResult = result as BadRequestObjectResult;

            sportsService.Verify(r => r.AddSport(It.IsAny<SportDto>()), Times.Never);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(400, createdResult.StatusCode);
        }

       [TestMethod]
        public void CreateSportNoDataAccessTest()
        {

            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            sportsService.Setup(r => r.AddSport(It.IsAny<SportDto>())).Throws(toThrow);
            SportModelIn input = new SportModelIn()
            {
                Name = "Soccer"
            };

            //Act.
            IActionResult result = controllerToTest.Post(input);
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void GetSportTest() {

            IActionResult result = controllerToTest.Get("Tennis");
            OkObjectResult okResult = result as OkObjectResult;
            SportModelOut modelOut = okResult.Value as SportModelOut;

            sportsService.Verify(r => r.GetSport("Tennis"), Times.Once);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(modelOut.Name, "Tennis");
            Assert.AreEqual(okResult.StatusCode, 200);
        }

        [TestMethod]
        public void GetNotExistentTest() {
            //Arrange.
            Exception internalEx = new SportNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            sportsService.Setup(s => s.GetSport(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.Get("Golf");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            sportsService.Verify(r => r.GetSport("Golf"), Times.Once);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(notFound.StatusCode, 404);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void GetSportNoDataAccessTest()
        {

            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            sportsService.Setup(r => r.GetSport(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.Get("soccer");
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }


        [TestMethod]
        public void GetAlltest() {
            IActionResult result = controllerToTest.Get();

            OkObjectResult okResult = result as OkObjectResult;
            IEnumerable<SportModelOut> resultObject = okResult.Value as IEnumerable<SportModelOut>;

            Assert.IsNotNull(okResult);
            Assert.IsNotNull(resultObject);
        }

        [TestMethod]
        public void GetAllNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message,ErrorType.DATA_INACCESSIBLE);
            sportsService.Setup(r => r.GetAllSports()).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.Get();
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void DeleteTest() {
            IActionResult result = controllerToTest.Delete("Tennis");

            OkObjectResult okResult = result as OkObjectResult;

            sportsService.Verify(r => r.DeleteSport("Tennis"), Times.Once);
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public void DeleteNotFoundTest() {

            Exception internalEx = new SportNotFoundException();
            sportsService.Setup(r => r.DeleteSport("Golf")).Throws(new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND));

            IActionResult result = controllerToTest.Delete("Golf");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;

            sportsService.Verify(r => r.DeleteSport("Golf"), Times.Once);
            Assert.IsNotNull(notFound);
            Assert.IsNotNull(notFound.Value);
        }

        [TestMethod]
        public void DeleteNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx= new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            sportsService.Setup(r => r.DeleteSport("Golf")).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.Delete("Golf");
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void GetSportTeamsTest() {
            //Arrange.
            Team dummyTeam = new Team("Dummy", "Dummy", new Sport("Dummy",true));
            ICollection<Team> cannedResponse = new List<Team>() { dummyTeam, dummyTeam, dummyTeam };
            sportsService.Setup(r => r.GetSport(It.IsAny<string>())).Returns(new Sport("Dummy",true));
            teamsRepo.Setup(r => r.GetSportTeams(It.IsAny<string>())).Returns(cannedResponse);

            //Act.
            IActionResult result = controllerToTest.GetTeams("Dummy");
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<TeamModelOut> teams = okResult.Value as ICollection<TeamModelOut>;

            //Assert.
            teamsRepo.Verify(r => r.GetSportTeams(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(teams);
            Assert.AreEqual(cannedResponse.Count, teams.Count);
        }

        [TestMethod]
        public void GetSportTeamsNotFoundTest() {
            //Arrange.
            Exception internalEx = new SportNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            teamsRepo.Setup(r => r.GetSportTeams(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.GetTeams("Dummy");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            teamsRepo.Verify(r => r.GetSportTeams(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void GetSportTeamsNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            teamsRepo.Setup(r => r.GetSportTeams(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.GetTeams("Basketball");
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void CalculateMatchTableResult() {
            //Arrange.
            tableGenerator.Setup(tg => tg.GetScoreTable("Golf")).Returns(GetFakeTable());

            //Act.
            IActionResult result = controllerToTest.CalculateSportTable("Golf");
            OkObjectResult okResult = result as OkObjectResult;
            IEnumerable<StandingModelOut> positions = okResult.Value as IEnumerable<StandingModelOut>;
            List<StandingModelOut> conversion = positions.ToList();

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(positions);
            Assert.AreEqual(15, conversion[0].Points);
            Assert.AreEqual(1, conversion[0].TeamId);
        }

        [TestMethod]
        public void CalculateTableNotExistingSportTest() {
            //Arrange.
            Exception internalEx = new SportNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            tableGenerator.Setup(tg => tg.GetScoreTable(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controllerToTest.CalculateSportTable("Golf");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            tableGenerator.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        private ICollection<Tuple<Team, int>> GetFakeTable() {
            Sport sport = new Sport("Golf", false);
            Team teamA = new Team(1, "TeamA", "Photo/A", sport);
            Team teamB = new Team(2, "TeamB", "Photo/B", sport);
            Team teamC = new Team(3, "TeamC", "Photo/C", sport);
            ICollection<Tuple<Team, int>> tuples = new List<Tuple<Team, int>>() {
                new Tuple<Team, int>(teamA,15),
                new Tuple<Team, int>(teamB,12),
                new Tuple<Team, int>(teamC,3)
            };
            return tuples;
        }
    }
}
