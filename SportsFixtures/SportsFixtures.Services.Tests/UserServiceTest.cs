using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Exceptions;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.Services.Contracts.Dtos;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserServiceTest
    {
        private Mock<IUserRepository> users;
        private Mock<ITeamRepository> teams;
        private Mock<IAuthenticationService> authentication;
        private IUserService service;
        private User testUser;
        private UserDto dto;
        private Team toFollow;

        [TestInitialize]
        public void SetUp()
        {
            users = new Mock<IUserRepository>();
            teams = new Mock<ITeamRepository>();
            authentication = new Mock<IAuthenticationService>();
            service = new UserService(users.Object, teams.Object, authentication.Object);
            testUser = GetFakeUser();
            users.Setup(r => r.Get("JohnDoe")).Returns(testUser);
            users.Setup(r => r.Get(It.Is<string>(s => !s.Equals("JohnDoe")))).Throws(new UserNotFoundException());
            dto = new UserDto()
            {
                name = testUser.Name,
                surname = testUser.Surname,
                username = testUser.UserName,
                password = testUser.Password,
                email = testUser.Email,
                isAdmin = testUser.IsAdmin
            };
            toFollow = GetFakeTeam();
        }

        private User GetFakeUser()
        {
            UserId identity = new UserId
            {
                Name = "John",
                Surname = "Doe",
                UserName = "JohnDoe",
                Password = "Password",
                Email = "John@Doe.com"
            };
            return new User(identity, true);
        }

        private Team GetFakeTeam()
        {
            Sport played = new Sport("Basketball",true);
            Team fake = new Team(1, "Lakers", "aPhoto", played);
            return fake;
        }

        [TestMethod]
        public void GetUserTest()
        {
            GrantAdminPermissions();
            UserDto retrieved = service.GetUser("JohnDoe");

            users.Verify(r => r.Get("JohnDoe"), Times.Once);
            Assert.AreEqual(retrieved.username, testUser.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetNotExistsTest()
        {
            GrantAdminPermissions();
            UserDto retrieved = service.GetUser("JohnLennon");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetNoDataAccessTest() {
            GrantAdminPermissions();
            users.Setup(r => r.Get(It.IsAny<string>())).Throws(new DataInaccessibleException());
            UserDto retrieved = service.GetUser("username");
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void GetNotAuthenticatedTest() {
            LogOut();
            service.GetUser("username");
        }


        [TestMethod]
        public void AddAdminTest()
        {
            GrantAdminPermissions();
            service.AddUser(dto);
            users.Verify(r => r.Add(testUser), Times.Once);
        }

        [TestMethod]
        public void AddFollowerTest()
        {
            GrantAdminPermissions();
            dto.isAdmin = false;
            service.AddUser(dto);
            users.Verify(r => r.Add(testUser), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddAlreadyExistentUserTest()
        {
            GrantAdminPermissions();
            users.Setup(r => r.Add(testUser)).Throws(new UserAlreadyExistsException());
            service.AddUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddUserNoDataAccessTest() {
            GrantAdminPermissions();
            users.Setup(r => r.Add(It.IsAny<User>())).Throws(new DataInaccessibleException());
            service.AddUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddInvalidUserTest() {
            GrantAdminPermissions();
            dto.email = "asdads";
            service.AddUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void AddUserNoAuthenticationTest() {
            LogOut();
            service.AddUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(NoPermissionsException))]
        public void AddUserNoPermissionsTest() {
            GrantFollowerPermissions();
            service.AddUser(dto);
        }

        [TestMethod]
        public void ModifyUserTest()
        {
            GrantAdminPermissions();
            service.ModifyUser(dto);
            users.Verify(r => r.Get(testUser.UserName), Times.Once);
            users.Verify(r => r.Modify(testUser), Times.Once);
        }

        [TestMethod]
        public void ModifyNullFieldsTest()
        {
            GrantAdminPermissions();
            UserDto changeName = new UserDto() {username= testUser.UserName ,name = "a new name" };
            UserDto modified = service.ModifyUser(changeName);
            Assert.AreEqual(changeName.name,modified.name);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyInvalidUserTest()
        {
            GrantAdminPermissions();
            UserDto changeName = new UserDto() { username = testUser.UserName, name = "a new name", email = "asdads" };
            service.ModifyUser(changeName);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyNoDataAccessTest() {
            GrantAdminPermissions();
            users.Setup(r => r.Modify(It.IsAny<User>())).Throws(new DataInaccessibleException());
            service.ModifyUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyNotExistentTest()
        {
            GrantAdminPermissions();
            users.Setup(r => r.Modify(testUser)).Throws(new UserNotFoundException());
            service.ModifyUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void ModifyNoAuthenticationTest()
        {
            LogOut();
            service.ModifyUser(dto);
        }

        [TestMethod]
        [ExpectedException(typeof(NoPermissionsException))]
        public void ModifyNoPermissionsTest()
        {
            GrantFollowerPermissions();
            service.ModifyUser(dto);
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            GrantAdminPermissions();
            service.DeleteUser(testUser.UserName);
            users.Verify(r => r.Delete(testUser.UserName));
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void DeleteNotExistentUserTest()
        {
            GrantAdminPermissions();
            users.Setup(r => r.Delete(testUser.UserName)).Throws(new UserNotFoundException());
            service.DeleteUser(testUser.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void DeleteNoDataAcessTest() {
            GrantAdminPermissions();
            users.Setup(r => r.Delete(It.IsAny<string>())).Throws(new DataInaccessibleException());
            service.DeleteUser(testUser.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(NotAuthenticatedException))]
        public void DeleteNoAuthenticationTest()
        {
            LogOut();
            service.DeleteUser("username");
        }

        [TestMethod]
        [ExpectedException(typeof(NoPermissionsException))]
        public void DeleteNoPermissionsTest()
        {
            GrantFollowerPermissions();
            service.DeleteUser("username");
        }

        [TestMethod]
        public void FollowTeamtest()
        {
            GrantFollowerPermissions();
            service.FollowTeam(testUser.UserName, toFollow.Id);
            users.Verify(r => r.Get(testUser.UserName), Times.Once);
            users.Verify(r => r.Modify(testUser), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamAlreadyFollowedException))]
        public void FollowAlreadyFollowingTeam()
        {
            GrantFollowerPermissions();
            service.FollowTeam(testUser.UserName, toFollow.Id);
            service.FollowTeam(testUser.UserName, toFollow.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void FollowTeamNotExistentUserTest()
        {
            GrantFollowerPermissions();
            users.Setup(r => r.Get(testUser.UserName)).Throws(new UserNotFoundException());
            service.FollowTeam(testUser.UserName, toFollow.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFoundException))]
        public void FollowNotExistentTeamTest()
        {
            GrantFollowerPermissions();
            users.Setup(r => r.Modify(testUser)).Throws(new TeamNotFoundException());
            service.FollowTeam(testUser.UserName, toFollow.Id);
        }

        [TestMethod]
        public void FollowByIdTest()
        {
            GrantFollowerPermissions();
            service.FollowTeam(testUser.UserName, toFollow.Id);

            users.Verify(r => r.Get(testUser.UserName), Times.Once);
            teams.Verify(r => r.Get(toFollow.Id), Times.Once);
            users.Verify(r => r.Modify(testUser), Times.Once);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void FollowTeamNotExistentUserIdTest()
        {
            GrantFollowerPermissions();
            users.Setup(r => r.Get(testUser.UserName)).Throws(new UserNotFoundException());
            service.FollowTeam(testUser.UserName, toFollow.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void FollowNotExistentTeamIdTest()
        {
            GrantFollowerPermissions();
            teams.Setup(r => r.Get(toFollow.Id)).Throws(new TeamNotFoundException());
            service.FollowTeam(testUser.UserName, toFollow.Id);
        }

        [TestMethod]
        public void GetUserTeamsTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            testUser.AddFavourite(fake);
            users.Setup(r => r.Get(testUser.UserName)).Returns(testUser);

            ICollection<TeamDto> userTeams = service.GetUserTeams(testUser.UserName);
            users.Verify(r => r.Get(testUser.UserName), Times.Once);
            Assert.AreEqual(userTeams.Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetNotExistentUserTeams()
        {
            GrantFollowerPermissions();
            users.Setup(r => r.Get(testUser.UserName)).Throws(new UserNotFoundException());
            ICollection<TeamDto> userTeams = service.GetUserTeams(testUser.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetUserTeamsNoDataAccessTest() {
            GrantFollowerPermissions();
            users.Setup(r => r.Get(It.IsAny<string>())).Throws(new DataInaccessibleException());
            service.GetUserTeams(testUser.UserName);
        }


        [TestMethod]
        public void UnfollowTeamTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            testUser.AddFavourite(fake);
            users.Setup(r => r.Get(testUser.UserName)).Returns(testUser);
            teams.Setup(r => r.Get(fake.Id)).Returns(fake);

            service.UnFollowTeam(testUser.UserName, fake.Id);

            users.Verify(r => r.Get(testUser.UserName), Times.Once);
            users.Verify(r => r.Modify(testUser), Times.Once);
            Assert.IsFalse(testUser.GetFavouriteTeams().Any());
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFollowedException))]
        public void UnfollowNotFollowedTeamTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            users.Setup(r => r.Get(testUser.UserName)).Returns(testUser);

            service.UnFollowTeam(testUser.UserName, fake.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void UnfollowNotFoundUserTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            users.Setup(r => r.Get(testUser.UserName)).Throws(new UserNotFoundException());
            testUser.AddFavourite(fake);

            service.UnFollowTeam(testUser.UserName, fake.Id);


        }

        [TestMethod]
        public void UnfollowTeamIdTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            testUser.AddFavourite(fake);
            users.Setup(r => r.Get(testUser.UserName)).Returns(testUser);
            teams.Setup(r => r.Get(fake.Id)).Returns(fake);

            service.UnFollowTeam(testUser.UserName, fake.Id);

            users.Verify(r => r.Get(testUser.UserName), Times.Once);
            teams.Verify(r => r.Get(fake.Id), Times.Once);
            users.Verify(r => r.Modify(testUser), Times.Once);
            Assert.IsFalse(testUser.GetFavouriteTeams().Any());
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFollowedException))]
        public void UnfollowNotFollowedTeamIdTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            users.Setup(r => r.Get(testUser.UserName)).Returns(testUser);

            service.UnFollowTeam(testUser.UserName, fake.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void UnfollowNotFoundUserIdTest()
        {
            GrantFollowerPermissions();
            Team fake = GetFakeTeam();
            users.Setup(r => r.Get(testUser.UserName)).Throws(new UserNotFoundException());
            testUser.AddFavourite(fake);

            service.UnFollowTeam(testUser.UserName, fake.Id);
        }

        [TestMethod]
        public void GetAllUsersTest()
        {
            GrantAdminPermissions();
            ICollection<User> fakeUsers = new List<User>() { testUser, testUser, testUser };
            users.Setup(r => r.GetAll()).Returns(fakeUsers);

            ICollection<UserDto> stored = service.GetAllUsers();

            Assert.AreEqual(stored.Count, fakeUsers.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetAllUsersNoDataAccessTest() {
            GrantAdminPermissions();
            users.Setup(r => r.GetAll()).Throws(new DataInaccessibleException());
            service.GetAllUsers();
        }

        private void GrantAdminPermissions(){}

        private void GrantFollowerPermissions()
        {
            authentication.Setup(r => r.AuthenticateAdmin()).Throws(new NoPermissionsException());
        }

        private void LogOut()
        {
            authentication.Setup(r => r.Authenticate()).Throws(new NotAuthenticatedException());
            authentication.Setup(r => r.AuthenticateAdmin()).Throws(new NotAuthenticatedException());
        }
    }
}
