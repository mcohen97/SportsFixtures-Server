using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.Services.Contracts;
using Moq;
using ObligatorioDA2.WebAPI.Models;
using ObligatorioDA2.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Exceptions;
using System;
using ObligatorioDA2.Services.Contracts.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AuthControllerTest
    {
        private AuthenticationController controllerToTest;
        private Mock<ILogInService> loginService;
        private UserDto testUser;

        [TestInitialize]
        public void StartUp() {
            loginService = new Mock<ILogInService>();
            testUser = new UserDto() { name ="aName",surname ="aUsername",username = "aUsername",
                password = "aPassword",email= "anEmail@aDomain.com", isAdmin=true };
            controllerToTest = new AuthenticationController(loginService.Object);
        }

        [TestMethod]
        public void LoginAdminSuccesfullyTest() {
            //arrange
            loginService.Setup(l => l.Login("aUsername", "aPassword")).Returns(testUser);

            //act
            LoginModelIn credentials = new LoginModelIn() { Username = "aUsername", Password = "aPassword" };
            IActionResult result = controllerToTest.Authenticate(credentials);
            OkObjectResult okResult = result as OkObjectResult;

            //assert
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public void LoginFollowerSuccesfullyTest()
        {
            //arrange
            testUser.isAdmin = false;
            loginService.Setup(l => l.Login("aUsername", "aPassword")).Returns(testUser);

            //act
            LoginModelIn credentials = new LoginModelIn() { Username = "aUsername", Password = "aPassword" };
            IActionResult result = controllerToTest.Authenticate(credentials);
            OkObjectResult okResult = result as OkObjectResult;

            //assert
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public void LoginNotFoundTest() {
            //Arrange.
            Exception internalEx = new UserNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            loginService.Setup(l => l.Login("otherUsername", "aPassword")).Throws(toThrow);

            //Act.
            LoginModelIn credentials = new LoginModelIn() { Username = "otherUsername", Password = "aPassword" };
            IActionResult result = controllerToTest.Authenticate(credentials);
            BadRequestObjectResult badRequestResult = result as BadRequestObjectResult;
            ErrorModelOut error = badRequestResult.Value as ErrorModelOut;

            //Assert.
            loginService.VerifyAll();
            Assert.IsNotNull(badRequestResult);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void LoginNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            loginService.Setup(us => us.Login(It.IsAny<string>(),It.IsAny<string>())).Throws(toThrow);
            LoginModelIn credentials = new LoginModelIn() { Username = "otherUsername", Password = "aPassword" };

            //Act.
            IActionResult result = controllerToTest.Authenticate(credentials);
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
        public void LoginWrongPasswordTest() {
            //arrange
            Exception toThrow = new WrongPasswordException();
            loginService.Setup(l => l.Login("aUsername", "otherPassword")).Throws(toThrow);

            //act
            LoginModelIn credentials = new LoginModelIn() { Username = "aUsername", Password = "otherPassword" };
            IActionResult result = controllerToTest.Authenticate(credentials);
            BadRequestObjectResult badRequestResult = result as BadRequestObjectResult;
            ErrorModelOut error = badRequestResult.Value as ErrorModelOut;

            //assert
            loginService.VerifyAll();
            Assert.IsNotNull(badRequestResult);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message,error.ErrorMessage);
        }

        [TestMethod]
        public void LoginInvalidModelTest() {
            LoginModelIn credentials = new LoginModelIn();
            controllerToTest.ModelState.AddModelError("", "Error");
            IActionResult result = controllerToTest.Authenticate(credentials);
            BadRequestObjectResult badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
        }
    }
}
