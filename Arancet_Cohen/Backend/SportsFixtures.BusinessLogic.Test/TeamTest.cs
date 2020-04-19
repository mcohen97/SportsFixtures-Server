using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic.Exceptions;
using Moq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TeamTest
    {
        private Team testTeam;
        private Sport aSport;

        [TestInitialize]
        public void TestInitialize()
        {
            int id = 1;
            string name = "TheTeam";
            string photo = "myresource/theteam.png";
            aSport = new Mock<Sport>("TheSport",false).Object;
            testTeam = new Team(id, name, photo, aSport);
        }

        [TestMethod]
        public void ConstructorTeamTest()
        {
            Assert.IsNotNull(testTeam);
        }

        [TestMethod]
        public void GetIdTest()
        {
            int id = 1;
            Assert.AreEqual(id, testTeam.Id);
        }

        [TestMethod]
        public void SetIdTest()
        {
            int newId = 2;
            testTeam.Id = newId;
            Assert.AreEqual(newId, testTeam.Id);
        }

        [TestMethod]
        public void GetNameTest()
        {
            string name = "TheTeam";
            Assert.AreEqual(name, testTeam.Name);
        }

        [TestMethod]
        public void SetNameTest()
        {
            string newName = "TheNewTeam";
            testTeam.Name = newName;
            Assert.AreEqual(newName, testTeam.Name);
        }

        [TestMethod]
        public void GetSportTest()
        {
            Assert.AreEqual(testTeam.Sport, aSport);
        }

        [TestMethod]
        public void SetSportTest()
        {
            Sport sport = new Mock<Sport>("TheNewSport",false).Object;
            testTeam.Sport = sport;
            Assert.AreEqual(sport, testTeam.Sport);
        }

        [TestMethod]
        public void GetPhotoTest()
        {
            string photo = "myresource/theteam.png";
            Assert.AreEqual(photo, testTeam.PhotoPath);
        }

        [TestMethod]
        public void SetPhotoTest()
        {
            string newPhoto = "myresource/thenewteam.png";
            testTeam.PhotoPath = newPhoto;
            Assert.AreEqual(newPhoto, testTeam.PhotoPath);
        }

        [TestMethod]
        public void EqualsTest()
        {
            Team aTeam = new Team(1, "TeamA", "somePath", new Sport("aSport",true));
            Team sameTeam = new Team(1, "TeamA", "", new Sport("aSport",true));
            Assert.AreEqual(aTeam, sameTeam);
        }

        [TestMethod]
        public void NotEqualsTest()
        {
            Team aTeam = new Team(1, "TeamA", "somePath", new Sport("aSport",true));
            Team differentTeam = new Team(2, "TeamB", "", new Sport("aSport",true));
            Assert.AreNotEqual(aTeam, differentTeam);
        }

        [TestMethod]
        public void NotEqualsDifferentSport()
        {
            Team aTeam = new Team(1, "TeamA", "somePath", new Sport("aSport",true));
            Team differentTeam = new Team(2, "TeamA", "", new Sport("anotherSport",true));
            Assert.AreNotEqual(aTeam, differentTeam);
        }

        [TestMethod]
        public void NotEqualsDifferentTypeTest()
        {
            Team aTeam = new Team(1, "TeamA", "somePath", new Sport("aSport",true));
            Assert.IsFalse(aTeam.Equals("this should be false"));
        }

        [TestMethod]
        public void NotEqualsNullTest() {
            Team aTeam = new Team(1, "TeamA", "somePath", new Sport("aSport", true));
            Assert.IsFalse(aTeam.Equals(null));
        }

        [TestMethod]
        public void GetHashCodeTest()
        {
            Team aTeam = new Team(1, "TeamA", "somePath", new Sport("aSport",true));
            int hashCode = 539060726 + EqualityComparer<string>.Default.GetHashCode("TeamA");
            Assert.AreEqual(hashCode, aTeam.GetHashCode());
        }

        //Exceptions

        [TestMethod]
        [ExpectedException(typeof(InvalidTeamDataException))]
        public void EmptyNameConstructorTest()
        {
            Team team = new Team(1, "", "photo", new Sport("aSport",true));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTeamDataException))]
        public void SetEmptyNameTest()
        {
            Team team = new Team(1, "name", "photo", new Sport("aSport",true));
            team.Name = "";
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTeamDataException))]
        public void SetNullNameTest()
        {
            Team team = new Team(1, "name", "photo", new Sport("aSport",true));
            team.Name = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTeamDataException))]
        public void SetNullPhotoTest()
        {
            Team team = new Team(1, "name", "photo", new Sport("aSport",true));
            team.PhotoPath = null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidSportDataException))]
        public void SetNullSportTest()
        {
            Team team = new Team(1, "name", "photo", null);
        }
    }
}
