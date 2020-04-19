using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.WebAPI.Models;
using ObligatorioDA2.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Contracts;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services;
using ObligatorioDA2.Services.Contracts.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserControllerTest
    {
        UsersController controller;
        Mock<IUserService> service;
        Mock<IAuthenticationService> auth;
        UserModelIn input;

        [TestInitialize]
        public void SetUp()
        {
            service = new Mock<IUserService>();
            auth = new Mock<IAuthenticationService>();
            controller = new UsersController(service.Object,auth.Object ,new ImageService("aPath"));
            input = new UserModelIn() { Name = "name", Surname = "surname", Username = "username", Password = "password", Email = "mail@gmail.com", IsAdmin=false };
            controller.ControllerContext = GetFakeControllerContext();
        }

        [TestMethod]
        public void GetTest()
        {
            //Arrange.
            UserDto fake = GetFakeUser();
            service.Setup(us => us.GetUser(fake.username)).Returns(fake);

            //Act.
            IActionResult result = controller.Get(fake.username);
            OkObjectResult okResult = result as OkObjectResult;
            UserModelOut modelOut = okResult.Value as UserModelOut;

            //Assert.
            service.Verify(us => us.GetUser(fake.username), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(modelOut);
            Assert.AreEqual(modelOut.Username, fake.username);
            Assert.AreEqual(modelOut.Email, fake.email);
            Assert.AreEqual(modelOut.Name, fake.name);
            Assert.AreEqual(modelOut.Surname, fake.surname);
            Assert.IsFalse(modelOut.IsAdmin);
        }

        [TestMethod]
        public void GetNotExistentTest()
        {
            //Arrange.
            Exception internalEx = new UserNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            service.Setup(us => us.GetUser(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Get("username");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.GetUser("username"), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void GetNoDataAccessTest() {
            
             //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.GetUser(It.IsAny<string>())).Throws(toThrow);


            //Act.
            IActionResult result = controller.Get("username");
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500,noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
        }

        [TestMethod]
        public void CreateValidUserTest()
        {
            //Arrange.
            service.Setup(us => us.AddUser(It.IsAny<UserDto>())).Returns(GetFakeUser());

            //Act.
            IActionResult result = controller.Post(input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            UserModelOut modelOut = createdResult.Value as UserModelOut;

            //Assert.
            service.Verify(us => us.AddUser(It.IsAny<UserDto>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual("GetUserById", createdResult.RouteName);
            Assert.AreEqual(modelOut.Username, input.Username);
        }

        [TestMethod]
        public void CreateFailedUserTest()
        {
            //Arrange.
            var modelIn = new UserModelIn()
            {
                Name = "name",
                Surname = "surname",
                Password = "password",
                IsAdmin = true
            };
            //We need to force the error in de ModelState.
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.Post(modelIn);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;

            //Assert.
            service.Verify(us => us.AddUser(It.IsAny<UserDto>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void CreateAlreadyExistentUserTest()
        {
            //Arrange.
            Exception internalEx = new UserAlreadyExistsException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_ALREADY_EXISTS);
            service.Setup(us => us.AddUser(It.IsAny<UserDto>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Post(input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.AddUser(It.IsAny<UserDto>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void CreateUserNoDataAccessTest()
        {
            
             //Arrange.
             Exception internalEx = new DataInaccessibleException();
             Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
             service.Setup(us => us.AddUser(It.IsAny<UserDto>())).Throws(toThrow);


            //Act.
            IActionResult result = controller.Post(input);
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500,noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
           
        }


        [TestMethod]
        public void PutModifyTest()
        {
            //Arrange.
            var modelIn = new UpdateUserModelIn()
            {
                Name = "name1",
                Surname = "surname1",
                Password = "password1",
                Email = "mail@domain.com",
                IsAdmin = true
            };
            service.Setup(us => us.ModifyUser(It.IsAny<UserDto>())).Returns(GetFakeUser());

            //Act.
            IActionResult result = controller.Put("username", modelIn);
            OkObjectResult okResult = result as OkObjectResult;
            UserModelOut modified = okResult.Value as UserModelOut;

            //Assert.
            service.Verify(us => us.ModifyUser(It.IsAny<UserDto>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(modified);
        }

        [TestMethod]
        public void PutCreateTest()
        {
            //Arrange.
            var modelIn = new UpdateUserModelIn()
            {
                Name = "name1",
                Surname = "surname1",
                Password = "password1",
                Email = "mail@domain.com"
            };
            service.Setup(us => us.ModifyUser(It.IsAny<UserDto>())).Throws(new ServiceException("",ErrorType.ENTITY_NOT_FOUND));
            service.Setup(us => us.AddUser(It.IsAny<UserDto>())).Returns(GetFakeUser());

            //Act.
            IActionResult result = controller.Put("username", modelIn);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            UserModelOut added = createdResult.Value as UserModelOut;

            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.IsNotNull(added);
            Assert.AreEqual("GetUserById", createdResult.RouteName);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual("username", added.Username);
        }

        [TestMethod]
        public void PutUserNoDataAccessTest()
        {

            //Arrange.
            UpdateUserModelIn modelIn = new UpdateUserModelIn()
            {
                Name = "name1",
                Surname = "surname1",
                Password = "password1",
                Email = "mail@domain.com"
            };
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.ModifyUser(It.IsAny<UserDto>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Put("username", modelIn);
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500,noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
           
        }

        [TestMethod]
        public void DeleteTest()
        {
            //Act.
            IActionResult result = controller.Delete("username");
            OkObjectResult deletedResult = result as OkObjectResult;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(deletedResult);
            Assert.AreEqual(200, deletedResult.StatusCode);
        }

        [TestMethod]
        public void DeleteNotExistentTest()
        {
            //Arrange.
            Exception internalEx = new UserNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            service.Setup(ms => ms.DeleteUser("notExistent")).Throws(toThrow);

            //Act.
            IActionResult result = controller.Delete("notExistent");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void DeleteUserNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.DeleteUser(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Delete("username");
            ObjectResult notData = result as ObjectResult;
            ErrorModelOut error = notData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notData);
            Assert.AreEqual(500,notData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
           
        }

        [TestMethod]
        public void GetAllTest()
        {
            //Arrange.
            ICollection<UserDto> fakeList = new List<UserDto>() { GetFakeUser(), GetFakeUser(), GetFakeUser() };
            service.Setup(us => us.GetAllUsers()).Returns(fakeList);

            //Act.
            IActionResult result = controller.Get();
            OkObjectResult listResult = result as OkObjectResult;
            ICollection<UserModelOut> list = listResult.Value as ICollection<UserModelOut>;

            //Assert.
            service.Verify(us => us.GetAllUsers(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(listResult);
            Assert.AreEqual(200, listResult.StatusCode);
            Assert.IsNotNull(list);
            Assert.AreEqual(fakeList.Count, list.Count);
        }

        [TestMethod]
        public void GetAllUsersNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.GetAllUsers()).Throws(toThrow);

            //Act.
            IActionResult result = controller.Get();
            ObjectResult notData = result as ObjectResult;
            ErrorModelOut error = notData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notData);
            Assert.AreEqual(500,notData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);

        }

        [TestMethod]
        public void FollowTeamTest()
        {
            //Act.
            IActionResult result = controller.FollowTeam(3);
            OkObjectResult okResult = result as OkObjectResult;
            OkModelOut okMessage = okResult.Value as OkModelOut;

            //Assert.
            service.Verify(us => us.FollowTeam("username", 3));
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200,okResult.StatusCode);
            Assert.IsNotNull(okMessage);
        }

        [TestMethod]
        public void FollowTeamAlreadyFollowing() {
            //Arrange.
            Exception toThrow = new TeamAlreadyFollowedException();
            service.Setup(us => us.FollowTeam(It.IsAny<string>(), It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.FollowTeam(3);
            BadRequestObjectResult notFound = result as BadRequestObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.FollowTeam(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(400, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void FollowTeamNotExistentTest() {

            //Arrange.
            Exception internalEx = new TeamNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message,ErrorType.ENTITY_NOT_FOUND);
            service.Setup(us => us.FollowTeam(It.IsAny<string>(), It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.FollowTeam(3);
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.FollowTeam(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void FollowTeamNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.FollowTeam(It.IsAny<string>(), It.IsAny<int>())).Throws(toThrow);


            //Act.
            IActionResult result = controller.FollowTeam(3);
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500,noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
           
        }

        [TestMethod]
        public void UnFollowTeamTest() {
            //Arrange.
            TeamModelIn input = GetTeamModelIn();

            //Act.
            IActionResult result = controller.UnFollowTeam(3);
            OkObjectResult okResult = result as OkObjectResult;
            OkModelOut okModel = okResult.Value as OkModelOut;

            //Assert.
            service.Verify(us => us.UnFollowTeam(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(okModel);
        }

        [TestMethod]
        public void UnfollowTeamNotFoundTest() {
            //Arrange.
            TeamModelIn input = GetTeamModelIn();
            Exception internalEx = new TeamNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            service.Setup(us => us.UnFollowTeam(It.IsAny<string>(), It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.UnFollowTeam(3);
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.UnFollowTeam(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void UnfollowNotFollowedTest() {
            //Arrange.
            TeamModelIn input = GetTeamModelIn();
            Exception toThrow = new TeamNotFollowedException();
            service.Setup(us => us.UnFollowTeam(It.IsAny<string>(), It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.UnFollowTeam(3);
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.UnFollowTeam(It.IsAny<string>(), It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void UnfollowTeamNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.UnFollowTeam(It.IsAny<string>(), It.IsAny<int>())).Throws(toThrow);
            TeamModelIn input = GetTeamModelIn();

            //Act.
            IActionResult result = controller.UnFollowTeam(3);
            ObjectResult notData = result as ObjectResult;
            ErrorModelOut error = notData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notData);
            Assert.AreEqual(500,notData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
        }

        [TestMethod]
        public void GetFollowedTeamsTest() {
            //Arrange.
            TeamDto aTeam = new TeamDto() { name = "aTeam", photo = "aPhoto", sportName = "Soccer" };
            ICollection<TeamDto> list2return = new List<TeamDto>() { aTeam, aTeam, aTeam };
            service.Setup(us => us.GetUserTeams(It.IsAny<string>())).Returns(list2return);

            //Act.
            IActionResult result = controller.GetFollowedTeams("username");
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<TeamModelOut> teamsFollowed = okResult.Value as ICollection<TeamModelOut>;

            //Assert.
            service.Verify(us => us.GetUserTeams("username"), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(teamsFollowed);
            Assert.AreEqual(list2return.Count,teamsFollowed.Count);
        }

        [TestMethod]
        public void GetFollowedTeamsNotExistingUserTest() {
            //Arrange.
            Exception internalEx = new UserNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            service.Setup(us => us.GetUserTeams(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.GetFollowedTeams("username");
            NotFoundObjectResult badRequest = result as NotFoundObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            service.Verify(us => us.GetUserTeams("username"), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(404, badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);

        }

        [TestMethod]
        public void GetFollowedTeamsNoDataAccessTest()
        {
             //Arrange.
             Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            service.Setup(us => us.GetUserTeams(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.GetFollowedTeams("username");
            ObjectResult notData = result as ObjectResult;
            ErrorModelOut error = notData.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notData);
            Assert.AreEqual(500,notData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
           
        }

        private TeamModelIn GetTeamModelIn()
        {
            TeamModelIn fake = new TeamModelIn() { Name = "Internazionale de Milano", SportName = "Soccer", Id = 3, Photo="" };
            return fake;
        }
        private ControllerContext GetFakeControllerContext() {
            ICollection<Claim> fakeClaims = new List<Claim>() { new Claim("Username", "username") };

            Mock<ClaimsPrincipal> cp = new Mock<ClaimsPrincipal>();
            cp.Setup(m => m.Claims).Returns(fakeClaims);
            Mock<HttpContext> contextMock = new Mock<HttpContext>();
            contextMock.Setup(ctx => ctx.User).Returns(cp.Object);

            Mock<ControllerContext> controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Object.HttpContext = contextMock.Object;
            return controllerContextMock.Object;
        }
        private UserDto GetFakeUser()
        {
            UserDto created = new UserDto()
            {
                name = "name",
                surname = "surname",
                username = "username",
                password = "password",
                email = "mail@mail.com",
                isAdmin = false
            };
            return created;
        }
    }
}