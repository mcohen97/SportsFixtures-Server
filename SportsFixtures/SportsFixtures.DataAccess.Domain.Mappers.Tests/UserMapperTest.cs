using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Entities;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.DomainMappers.Mappers.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserMapperTest
    {
        User toStore;
        UserEntity toGet;
        UserMapper toTest;
        UserFactory factory;

        [TestInitialize]
        public void StartUp()
        {
            factory = new UserFactory();
            UserId identity = new UserId
            {
                Name = "name",
                Surname = "surname",
                UserName = "username",
                Password = "password",
                Email = "email@domain.com"
            };
            toStore = factory.CreateAdmin(identity);
            toGet = new UserEntity()
            {
                Name = "name",
                Surname = "surname",
                UserName = "username",
                Password = "password",
                Email = "email@domain.com"
            };
            toTest = new UserMapper();
        }

        [TestMethod]
        public void UserToEntityNameTest()
        {
            UserEntity conversion = toTest.ToEntity(toStore);
            Assert.AreEqual(conversion.Name, toStore.Name);
        }

        [TestMethod]
        public void UserToEntitySurnameTest()
        {
            UserEntity conversion = toTest.ToEntity(toStore);
            Assert.AreEqual(conversion.Surname, toStore.Surname);
        }

        [TestMethod]
        public void UserToEntityUserNameTest()
        {
            UserEntity conversion = toTest.ToEntity(toStore);
            Assert.AreEqual(conversion.UserName, toStore.UserName);
        }

        [TestMethod]
        public void UserToEntityPasswordTest()
        {
            UserEntity conversion = toTest.ToEntity(toStore);
            Assert.AreEqual(conversion.Password, toStore.Password);
        }

        [TestMethod]
        public void UserToEntityEmailTest()
        {
            UserEntity conversion = toTest.ToEntity(toStore);
            Assert.AreEqual(conversion.Email, toStore.Email);
        }

        [TestMethod]
        public void EntityToUserNameTest()
        {
            User conversion = toTest.ToUser(toGet, new List<TeamEntity>());
            Assert.AreEqual(conversion.Name, toGet.Name);
        }

        [TestMethod]
        public void EntityToUserSurnameTest()
        {
            User conversion = toTest.ToUser(toGet, new List<TeamEntity>());
            Assert.AreEqual(conversion.Surname, toGet.Surname);
        }

        [TestMethod]
        public void EntityToUserUserNameTest()
        {
            User conversion = toTest.ToUser(toGet, new List<TeamEntity>());
            Assert.AreEqual(conversion.UserName, toGet.UserName);
        }

        [TestMethod]
        public void EntityToUserPasswordTest()
        {
            User conversion = toTest.ToUser(toGet, new List<TeamEntity>());
            Assert.AreEqual(conversion.Password, toGet.Password);
        }

        [TestMethod]
        public void EntityToUserEmailTest()
        {
            User conversion = toTest.ToUser(toGet, new List<TeamEntity>());
            Assert.AreEqual(conversion.Email, toGet.Email);
        }

        [TestMethod]
        public void EntityToUserFollowersTest()
        {
            TeamEntity entity = new TeamEntity()
            {
                Name = "Partisanos Fc",
                SportEntityName = "Soccer",
                Sport = new SportEntity() { Name = "Soccer" },
                TeamNumber = 1,
                Photo = "aPhoto"
            };
            User conversion = toTest.ToUser(toGet, new List<TeamEntity>() { entity });
            Assert.AreEqual(conversion.GetFavouriteTeams().Count, 1);
        }

        [TestMethod]
        public void GetUserTeamsTest()
        {
            toStore.AddFavourite(new Team(1, "Nacional", "aPath", new Sport("Soccer",true)));
            ICollection<UserTeam> relationships = toTest.GetUserTeams(toStore);
            Assert.AreEqual(relationships.Count, 1);
        }

    }
}
