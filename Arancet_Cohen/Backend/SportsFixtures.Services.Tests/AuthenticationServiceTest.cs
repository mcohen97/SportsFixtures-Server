using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class AuthenticationServiceTest
    {

        private Mock<IUserRepository> repo;
        private Mock<ILogInfoRepository> logger;
        private AuthenticationService authService;
        private User admin;
        private User follower;

        [TestInitialize]
        public void SetUp()
        {
            repo = new Mock<IUserRepository>();
            logger = new Mock<ILogInfoRepository>();
            authService = new AuthenticationService(repo.Object, logger.Object);
            UserId id = new UserId
            {
                Name = "aName",
                Surname = "aSurname",
                Password = "aPassword",
                UserName = "aUsername",
                Email = "anEmail@aDomain.com"
            };
            follower = new User(id, false);
            admin = new User(id, true);
        }

        [TestMethod]
        public void LoginSuccesfullyTest()
        {
            //arrange
            repo.Setup(r => r.Get("aUsername")).Returns(admin);

            //act
            UserDto logged = authService.Login("aUsername", "aPassword");

            repo.VerifyAll();
            logger.Verify(r => r.Add(It.IsAny<LogInfo>()));
            Assert.AreEqual(logged.name, "aName");
            Assert.AreEqual(logged.surname, "aSurname");
            Assert.AreEqual(logged.username, "aUsername");
            Assert.AreEqual(logged.password, "aPassword");
            Assert.AreEqual(logged.email, "anEmail@aDomain.com");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void UserNotFoundTest()
        {
            //arrange.
            repo.Setup(r => r.Get("otherUsername")).Throws(new UserNotFoundException());
            //act.
            authService.Login("otherUsername", "password");
        }

        [TestMethod]
        [ExpectedException(typeof(WrongPasswordException))]
        public void WrongPasswordTest()
        {
            //arrange
            repo.Setup(r => r.Get("aUsername")).Returns(admin);

            //act
            UserDto logged = authService.Login("aUsername", "otherPassword");
        }

        [TestMethod]
        public void SetConnectedUserTest() {
            //Arrange.
            repo.Setup(r => r.Get(admin.UserName)).Returns(admin);
            //Act.
            authService.SetSession(admin.UserName);
            //Assert.
            repo.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void SetUnexistentUserTest() {
            //Arrange.
            repo.Setup(r => r.Get(It.IsAny<string>())).Throws(new UserNotFoundException());
            //Act.
            authService.SetSession(admin.UserName);
        }

        [TestMethod]
        public void SetSessionTest() {
            repo.Setup(r => r.Get("aUsername")).Returns(admin);
            authService.SetSession("aUsername");
            repo.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void SetSessionNoDataAccessTest() {
            repo.Setup(r => r.Get(It.IsAny<string>())).Throws(new DataInaccessibleException());
            authService.SetSession("aUsername");
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void IsNotLoggedInTest() {
            repo.Setup(r => r.Get("aUsername")).Throws(new UserNotFoundException());
            authService.Authenticate();
        } 

        [TestMethod]
        public void HasAdminPermissionTest() {
            //Arrange.
            repo.Setup(r => r.Get(admin.UserName)).Returns(admin);
            //Act.
            authService.SetSession(admin.UserName);
            //Assert.
            authService.AuthenticateAdmin();
            repo.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(NoPermissionsException))]
        public void HasNoAdminPermissionsTest()
        {
            //Arrange.
            repo.Setup(r => r.Get(admin.UserName)).Returns(follower);
            //Act.
            authService.SetSession(follower.UserName);
            //Assert.
            authService.AuthenticateAdmin();
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void AuthenticateAdminNotLoggedTest() {
            authService.AuthenticateAdmin();
        }

        [TestMethod]
        public void GetCurrentUserTest() {
            //Arrange.
            repo.Setup(r => r.Get("aUsername")).Returns(admin);
            authService.SetSession("aUsername");
            //Act.
            UserDto current = authService.GetConnectedUser();
            //Assert.
            Assert.AreEqual("aUsername",current.username);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void GetCurrentUserWithNoSessionTest()
        {
            //Act.
            UserDto current = authService.GetConnectedUser();
        }
    }
}
