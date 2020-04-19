using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using ObligatorioDA2.BusinessLogic;
using Microsoft.EntityFrameworkCore;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DataRepositoriesTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserRepositoryTest
    {
        IUserRepository usersStorage;
        ITeamRepository teamsStorage;
        ISportRepository sportsStorage;
        UserId userId;
        UserFactory factory;
        User user;
        DatabaseConnection context;

        [TestInitialize]
        public void SetUp()
        {
            factory = new UserFactory();
            userId = new UserId()
            {
                Name = "name",
                Surname = "surname",
                UserName = "username",
                Password = "password",
                Email = "mail@domain.com"
            };
            user = factory.CreateAdmin(userId);
            CreateRepository();
            usersStorage.Clear();
            teamsStorage.Clear();
        }

        private void CreateRepository()
        {

            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "UserRepositoryTest")
                .Options;
            context = new DatabaseConnection(options);
            usersStorage = new UserRepository(context);
            teamsStorage = new TeamRepository(context);
            sportsStorage = new SportRepository(context);
            context.UserTeams.RemoveRange(context.UserTeams);
        }

        private void CreateDisconnectedDatabase()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "UserRepositoryTest")
                .Options;
            Mock<DatabaseConnection> contextMock = new Mock<DatabaseConnection>(options);
            Mock<DbException> toThrow = new Mock<DbException>();
            contextMock.Setup(c => c.Users).Throws(toThrow.Object);
            usersStorage = new UserRepository(contextMock.Object);
        }

        [TestMethod]
        public void NoUsersTest()
        {
            bool noUsers = usersStorage.IsEmpty();
            Assert.IsTrue(noUsers);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void IsEmptyNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            usersStorage.IsEmpty();
        }

        [TestMethod]
        public void AddUserTest()
        {
            usersStorage.Add(user);
            int expectedResult = 1;
            int actualResult = usersStorage.GetAll().Count;
            Assert.AreEqual(expectedResult, actualResult);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void AddUserNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            usersStorage.Add(user);
        }

        [TestMethod]
        [ExpectedException(typeof(UserAlreadyExistsException))]
        public void AddAlreadyExistentUserTest()
        {
            usersStorage.Add(user);
            usersStorage.Add(user);
        }

        [TestMethod]
        public void GetUserTest()
        {

            User user = factory.CreateAdmin(userId);
            usersStorage.Add(user);
            User fetched = usersStorage.Get("username");
            Assert.AreEqual("name", fetched.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void GetNotExistentUserTest()
        {
            User fetched = usersStorage.Get("username3");
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetUserNoConnectionException()
        {
            CreateDisconnectedDatabase();
            User fetched = usersStorage.Get("username3");
        }

        [TestMethod]
        public void ExistsUserTest()
        {
            usersStorage.Add(user);
            bool result = usersStorage.Exists(user.UserName);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesNotExistTest()
        {
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username1", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username2", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            bool result = usersStorage.Exists(user2.UserName);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ExistsNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            usersStorage.Exists("user");
        }

        [TestMethod]
        public void DeleteTest()
        {
            usersStorage.Add(user);
            usersStorage.Delete(user.UserName);
            Assert.IsTrue(usersStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void DeleteNotExistetTest()
        {
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username1", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username2", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            usersStorage.Delete(user2.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void DeleteNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            usersStorage.Delete(user.UserName);
        }

        [TestMethod]
        public void ModifyTest()
        {
            usersStorage.Clear();
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            CreateRepository();
            usersStorage.Modify(user2);
            User toVerify = usersStorage.Get(user2);
            Assert.AreEqual(toVerify.Name, user2.Name);
        }

        [TestMethod]
        public void ModifyAddedTeams()
        {
            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username", Password = "password1", Email = "mail1@domain.com" };
            User user = factory.CreateAdmin(userId1);
            usersStorage.Add(user);
            Sport toPlay = new Sport("Soccer", true);
            Team fakeTeam = GetFakeTeam(toPlay);
            teamsStorage.Add(fakeTeam);
            user.AddFavourite(fakeTeam);
            usersStorage.Modify(user);
            User stored = usersStorage.Get(user.UserName);
            Assert.AreEqual(1, stored.GetFavouriteTeams().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void ModifyUserNotExistsTest()
        {
            usersStorage.Modify(user);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ModifyNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            usersStorage.Modify(user);

        }

        [TestMethod]
        public void GetTest()
        {
            User user = factory.CreateAdmin(userId);
            usersStorage.Add(user);
            User fetched = usersStorage.Get(user);
            Assert.AreEqual(user.UserName, fetched.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(UserNotFoundException))]
        public void GetNotExistentTest()
        {

            UserId userId1 = new UserId { Name = "name1", Surname = "surname1", UserName = "username2", Password = "password1", Email = "mail1@domain.com" };
            UserId userId2 = new UserId { Name = "name2", Surname = "surname2", UserName = "username1", Password = "password2", Email = "mail2@domain.com" };
            User user1 = factory.CreateAdmin(userId1);
            User user2 = factory.CreateAdmin(userId2);
            usersStorage.Add(user1);
            usersStorage.Get(user2);
        }


        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            User fetched = usersStorage.Get(user);
        }

        [TestMethod]
        public void ClearUsersTest()
        {
            usersStorage.Add(user);
            usersStorage.Clear();
            Assert.IsTrue(usersStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ClearNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            usersStorage.Clear();
        }

        [TestMethod]
        public void DeleteByUsername()
        {
            usersStorage.Add(user);
            User fetched = usersStorage.Get(user);
            usersStorage.Delete(fetched.UserName);
            Assert.IsTrue(usersStorage.IsEmpty());
        }



        [TestMethod]
        public void GetUserFollowedTeamsTest()
        {
            Sport toPlay  = new Sport("Soccer", true);
            Team toFollow = GetFakeTeam(toPlay);
            teamsStorage.Add(toFollow);
            user.AddFavourite(toFollow);
            usersStorage.Add(user);
            CreateRepository();
            User fromDB = usersStorage.Get(user.UserName);
            Assert.AreEqual(fromDB.GetFavouriteTeams().Count, 1);
        }

        private Team GetFakeTeam(Sport played)
        {
            Team fake = new Team(1, "RealMadrid", "aPath", played);
            return fake;
        }
    }
}
