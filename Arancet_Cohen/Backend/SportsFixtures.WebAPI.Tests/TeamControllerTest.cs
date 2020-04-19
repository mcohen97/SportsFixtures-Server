using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.WebAPI.Models;
using ObligatorioDA2.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using System;
using System.Collections.Generic;
using ObligatorioDA2.Services;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Exceptions;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TeamControllerTest
    {
        private TeamsController controller;
        private Mock<ITeamService> teamsService;
        private Mock<IAuthenticationService> auth;
        TeamDto team;

        [TestInitialize]
        public void SetUp() {
            Sport testSport = new Sport("Soccer", true);
            team = new TeamDto() { id = 2, name = "Nacional",photo= "foto", sportName =testSport.Name };
            teamsService = new Mock<ITeamService>();
            auth = new Mock<IAuthenticationService>();
            controller = new TeamsController(teamsService.Object, new ImageService("TestDirectory"),auth.Object);
            controller.ControllerContext = GetFakeControllerContext();
        }

        [TestMethod]
        public void GetAllTeamsTest() {
            //Arrange.
            ICollection<TeamDto> dummyTeams = new List<TeamDto>() { team, team, team };
            teamsService.Setup(r => r.GetAllTeams()).Returns(dummyTeams);

            //Act.
            IActionResult result = controller.Get();
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<TeamModelOut> allTeams = okResult.Value as ICollection<TeamModelOut>;

            //Assert.
            teamsService.Verify(r => r.GetAllTeams(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(allTeams);
            Assert.AreEqual(dummyTeams.Count,allTeams.Count);
        }

        [TestMethod]
        public void GetAllNoDataAccessTest()
        {

            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            teamsService.Setup(us => us.GetAllTeams()).Throws(toThrow);


            //Act.
            IActionResult result = controller.Get();
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
        public void GetTeamByIdTest() {
            //Arrange.
            teamsService.Setup(r => r.GetTeam(2)).Returns(team);

            //Act.
            IActionResult result = controller.Get(2);
            OkObjectResult okResult = result as OkObjectResult;
            TeamModelOut resultTeam = okResult.Value as TeamModelOut;

            //Assert.
            teamsService.Verify(r => r.GetTeam(2), Times.Once);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(resultTeam.Name, team.name);
            Assert.AreEqual(resultTeam.SportName, team.sportName);
            Assert.AreEqual(resultTeam.Photo, team.photo);
        }

        [TestMethod]
        public void GetTeamByIdNotFoundTest() {
            //Arrange.
            Exception internalEx = new TeamNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            teamsService.Setup(r => r.GetTeam(It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Get(2);
            NotFoundObjectResult notFoundResult = result as NotFoundObjectResult;
            ErrorModelOut error = notFoundResult.Value as ErrorModelOut;

            //Assert.
            teamsService.Verify(r => r.GetTeam(2), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void GetTeamByIdNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            teamsService.Setup(r => r.GetTeam(It.IsAny<int>())).Throws(toThrow);


            //Act.
            IActionResult result = controller.Get(2);
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
        public void CreateValidTeamTest()
        {
            //Arrange.
            TeamModelIn input = CreateTeamModelIn();
            teamsService.Setup(r => r.AddTeam(It.IsAny<TeamDto>())).Returns(team);

            //Act.
            IActionResult result = controller.Post(input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            TeamModelOut modelOut = createdResult.Value as TeamModelOut;

            //Assert
            teamsService.Verify(r => r.AddTeam(It.IsAny<TeamDto>()), Times.Once);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("GetTeamById", createdResult.RouteName);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(input.Name, modelOut.Name);
        }

        private TeamModelIn CreateTeamModelIn()
        {
            return new TeamModelIn()
            {
                Name = team.name,
                Photo = team.photo,
                SportName = team.sportName
            };
        }

        [TestMethod]
        public void CreateFailedTeamRequiredFieldsTest()
        {
            //Arrange.
           TeamModelIn modelIn = new TeamModelIn() {
                 Photo = "/MyResource/DreamTeam.png"
            };
            //We need to force the error in de ModelState.
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.Post(modelIn);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            
            //Assert.
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void CreateAlreadyExistingTeam() {
            //Arrange.
            Exception toThrow = new TeamAlreadyExistsException();
            teamsService.Setup(tr => tr.AddTeam(It.IsAny<TeamDto>())).Throws(new ServiceException(toThrow.Message,ErrorType.ENTITY_ALREADY_EXISTS));
            TeamModelIn input = CreateTeamModelIn();

            //Act.
            IActionResult result = controller.Post(input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void CreateNoDataAccessTest()
        {
            //Arrange.
            Exception toThrow = new DataInaccessibleException();
            teamsService.Setup(tr => tr.AddTeam(It.IsAny<TeamDto>())).Throws(new ServiceException(toThrow.Message, ErrorType.DATA_INACCESSIBLE));
            TeamModelIn input = CreateTeamModelIn();

            //Act.
            IActionResult result = controller.Post(input);
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
        public void PutTest() {
            //Arrange.
            TeamModelIn input = CreateTeamModelIn();
            teamsService.Setup(r => r.Modify(It.IsAny<TeamDto>())).Returns(team);


            //Act.
            IActionResult result = controller.Put(2,input);
            OkObjectResult okResult = result as OkObjectResult;
            TeamModelOut modified = okResult.Value as TeamModelOut;

            //Assert.
            teamsService.Verify(r => r.Modify(It.IsAny<TeamDto>()), Times.Once);
            teamsService.Verify(r => r.AddTeam(It.IsAny<TeamDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200,okResult.StatusCode);
        }

        [TestMethod]
        public void PutAddTest() {

            //Arrange.
            Exception internalEx = new TeamNotFoundException();
            teamsService.Setup(r => r.Modify(It.IsAny<TeamDto>())).Throws(new ServiceException(internalEx.Message,ErrorType.ENTITY_NOT_FOUND));
            teamsService.Setup(r => r.AddTeam(It.IsAny<TeamDto>())).Returns(team);
            TeamModelIn input = CreateTeamModelIn();

            //Act.
            IActionResult result =controller.Put(2,input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            TeamModelOut modelOut = createdResult.Value as TeamModelOut;

            //Assert.
            teamsService.Verify(r => r.Modify(It.IsAny<TeamDto>()), Times.Once);
            teamsService.Verify(r => r.AddTeam(It.IsAny<TeamDto>()), Times.Once);
            Assert.AreEqual("GetTeamById", createdResult.RouteName);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual(2, modelOut.Id);
        }

        [TestMethod]
        public void PutWrongFormatTest() {
            //Arrange.
            TeamModelIn modelIn = new TeamModelIn(){Photo = "/MyResource/DreamTeam.png"};
            //We need to force the error in de ModelState.
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.Put(2,modelIn);
            BadRequestObjectResult createdResult = result as BadRequestObjectResult;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(400, createdResult.StatusCode);
        }

        [TestMethod]
        public void PutNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            teamsService.Setup(r => r.Modify(It.IsAny<TeamDto>())).Throws(toThrow);
            TeamModelIn input = CreateTeamModelIn();

            //Act.
            IActionResult result = controller.Put(2,input);
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
        public void DeleteByIdTest()
        {

            //Act.
            IActionResult result = controller.Delete(2);
            OkObjectResult okResult = result as OkObjectResult;
            OkModelOut okMessage = okResult.Value as OkModelOut;

            //Assert.
            teamsService.Verify(r => r.DeleteTeam(2), Times.Once);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(okResult.StatusCode, 200);
            Assert.IsNotNull(okMessage);
            Assert.IsNotNull("The Team was deleted successfully",okMessage.OkMessage);
        }

        [TestMethod]
        public void DeleteByIdNotExistentTest()
        {
            //Arrange.
            Exception internalEx = new TeamNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            teamsService.Setup(r => r.DeleteTeam(It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Delete(2);
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;
            
            //Assert.
            teamsService.Verify(r => r.DeleteTeam(2), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void DeleteByIdNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            teamsService.Setup(r => r.DeleteTeam(It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Delete(2);
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        private ControllerContext GetFakeControllerContext()
        {
            ICollection<Claim> fakeClaims = new List<Claim>() { new Claim("Username", "username") };

            Mock<ClaimsPrincipal> cp = new Mock<ClaimsPrincipal>();
            cp.Setup(m => m.Claims).Returns(fakeClaims);
            Mock<HttpContext> contextMock = new Mock<HttpContext>();
            contextMock.Setup(ctx => ctx.User).Returns(cp.Object);

            Mock<ControllerContext> controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Object.HttpContext = contextMock.Object;
            return controllerContextMock.Object;
        }
    }
}