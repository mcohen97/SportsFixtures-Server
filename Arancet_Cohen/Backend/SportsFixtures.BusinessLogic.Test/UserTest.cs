using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class UserTest
    {
        User toTest;
        UserId userId;

        [TestInitialize]
        public void SetUp()
        {
            userId = new UserId()
            {
                Name = "name",
                Surname = "surname",
                UserName = "username",
                Password = "password",
                Email = "mail@domain.com"
            };
            toTest = new User(userId, false);

        }
        [TestMethod]
        public void GetNameTest()
        {
            Assert.AreEqual("name", toTest.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetNameTest()
        {
            userId.Name = "";
            toTest = new User(userId, true);

        }

        [TestMethod]
        public void GetSurnameTest()
        {
            Assert.AreEqual("surname", toTest.Surname);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetInvalidSurnameTest()
        {
            userId.Surname = "";
            toTest = new User(userId, true);

        }

        [TestMethod]
        public void GetUserNameTest()
        {
            Assert.AreEqual("username", toTest.UserName);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetInvalidUserNameTest()
        {
            userId.UserName = "";
            toTest = new User(userId, true);

        }

        [TestMethod]
        public void GetPasswordTest()
        {
            Assert.AreEqual("password", toTest.Password);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetInvalidPasswordTest()
        {
            userId.Password = "";
            toTest = new User(userId, true);

        }

        [TestMethod]
        public void GetEmailTest()
        {
            Assert.AreEqual("mail@domain.com", toTest.Email);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetEmptyEmailTest()
        {
            userId.Email = "";
            toTest = new User(userId, true);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetEmailWithNoDomainTest()
        {
            userId.Email = "mail";
            toTest = new User(userId, true);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetEmailWithNoNameTest()
        {
            userId.Email = "@domain.com";
            toTest = new User(userId, true);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetEmailWithNoDomainTagTest()
        {
            userId.Email = "@domain";
            toTest = new User(userId, true);

        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void SetEmailWithNoAtTest()
        {
            userId.Email = "maildomain.com";
            toTest = new User(userId, true);

        }

        [TestMethod]
        public void EqualsTest()
        {
            toTest = new User(userId, true);
            UserId userId2 = new UserId()
            {
                Name = "name2",
                Surname = "surname2",
                UserName = "username",
                Password = "password2",
                Email = "mail@domain.com"
            };
            User another = new User(userId, true);
            Assert.IsTrue(toTest.Equals(another));
        }

        [TestMethod]
        public void NotEqualsTest()
        {
            UserId userId2 = new UserId()
            {
                Name = "name2",
                Surname = "surname2",
                UserName = "username2",
                Password = "password2",
                Email = "mail@domain.com"
            };
            User another = new User(userId2, true);
            Assert.IsFalse(toTest.Equals(another));
        }

        [TestMethod]
        public void NotEqualsOtherTypeTest()
        {
            object differentType = new object();
            Assert.IsFalse(toTest.Equals(differentType));
        }

        [TestMethod]
        public void NotEqualsNullTest()
        {
            Assert.IsFalse(toTest.Equals(null));
        }

        [TestMethod]
        public void IsAdminTest()
        {
            toTest = new User(userId, true);
            Assert.IsTrue(toTest.IsAdmin);
        }

        [TestMethod]
        public void IsNotAdminTest()
        {
            Assert.IsFalse(toTest.IsAdmin);
        }

        [TestMethod]
        public void GetTeamsTest()
        {
            Assert.AreEqual(toTest.GetFavouriteTeams().Count, 0);
        }

        [TestMethod]
        public void AddFavouriteTeamTest()
        {
            Team aTeam = new Team(1, "partisanos fc", "aPath", new Sport("aSport",true));
            toTest.AddFavourite(aTeam);
            Assert.AreEqual(toTest.GetFavouriteTeams().Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void AddAlreadyFollowingTeamTest()
        {
            Team aTeam = new Team(1, "partisanos fc", "aPath", new Sport("aSport",true));
            toTest.AddFavourite(aTeam);
            toTest.AddFavourite(aTeam);
        }

        [TestMethod]
        public void HasTeamTest()
        {
            Team aTeam = new Team(1, "partisanos fc", "aPath", new Sport("aSport",true));
            toTest.AddFavourite(aTeam);
            Assert.IsTrue(toTest.HasFavouriteTeam(aTeam));
        }

        [TestMethod]
        public void DoesNotHaveTeamTest()
        {
            Team aTeam = new Team(1, "partisanos fc", "aPath", new Sport("aSport",true));
            Assert.IsFalse(toTest.HasFavouriteTeam(aTeam));
        }

        [TestMethod]
        public void RemoveTeamTest()
        {
            Team aTeam = new Team(1, "partisanos fc", "aPath", new Sport("aSport",true));
            toTest.AddFavourite(aTeam);
            toTest.RemoveFavouriteTeam(aTeam);
            Assert.IsFalse(toTest.HasFavouriteTeam(aTeam));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidUserDataException))]
        public void RemoveNotExistentTest()
        {
            Team aTeam = new Team(1, "partisanos fc", "aPath", new Sport("aSport",true));
            toTest.RemoveFavouriteTeam(aTeam);
        }
    }
}
