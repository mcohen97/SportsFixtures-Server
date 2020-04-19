using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Match = ObligatorioDA2.BusinessLogic.Match;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Entities;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System;

namespace DataRepositoriesTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SportRepositoryTest
    {
        private ISportRepository sportStorage;
        private Sport sportA;
        private Sport sportB;
        DatabaseConnection context;


        [TestInitialize]
        public void TestInitialize()
        {
            context = CreateContext();
            CreateSports();
            ClearDataBase(context);
        }

        private void CreateSports()
        {
            sportA = new Sport("SportA",true);
            sportB = new Sport("SportB",false);
        }

        private DatabaseConnection CreateContext()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                            .UseInMemoryDatabase(databaseName: "SportRepository")
                            .Options;
            DatabaseConnection context = new DatabaseConnection(options);
            sportStorage = new SportRepository(context);
            return context;
        }

        private void CreateDisconnectedDatabase()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "UserRepositoryTest")
                .Options;
            Mock<DatabaseConnection> contextMock = new Mock<DatabaseConnection>(options);
            Mock<DbException> toThrow = new Mock<DbException>();
            contextMock.Setup(c => c.Sports).Throws(toThrow.Object);
            sportStorage = new SportRepository(contextMock.Object);
        }

        private void ClearDataBase(DatabaseConnection context)
        {
            foreach (SportEntity sport in context.Sports)
            {
                context.Sports.Remove(sport);
            }
            context.SaveChanges();
        }

        [TestMethod]
        public void NoSportsTest()
        {
            bool noSports = sportStorage.IsEmpty();
            Assert.IsTrue(noSports);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void IsEmptyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.IsEmpty();
        }

        [TestMethod]
        public void AddSportTest()
        {
            sportStorage.Add(sportA);
            Assert.AreEqual(1, sportStorage.GetAll().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(SportAlreadyExistsException))]
        public void AddAlreadyExistentTeamTest()
        {
            sportStorage.Add(sportA);
            sportStorage.Add(sportA);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void AddSportNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.Add(sportA);
        }

        [TestMethod]
        public void GetSportTest()
        {
            ISportRepository specific = (ISportRepository)sportStorage;
            sportStorage.Add(sportA);
            Sport sportInDb = specific.Get("SportA");
            Assert.AreEqual(sportA.Name, sportInDb.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(SportNotFoundException))]
        public void GetNotExistentTeamTest()
        {
            Sport teamInDb = sportStorage.Get("DreamSport");
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetSportNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.Get("DreamSport");
        }

        [TestMethod]
        public void ExistsTeamTest()
        {
            sportStorage.Add(sportA);
            bool result = sportStorage.Exists(sportA.Name);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesNotExistTest()
        {

            sportStorage.Add(sportA);
            bool result = sportStorage.Exists(sportB.Name);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ExistsNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.Exists("DreamSport");
        }

        [TestMethod]
        public void DeleteTest()
        {
            sportStorage.Add(sportA);
            sportStorage.Delete(sportA.Name);
            Assert.IsTrue(sportStorage.IsEmpty());
        }

        [TestMethod]
        public void DeleteSportCascadeTest() {
            sportStorage.Add(sportA);
            ITeamRepository teams = new TeamRepository(context);
            Team teamA = new Team("TeamA", "photoA", sportA);
            Team teamB = new Team("TeamB", "photoB", sportA);
            teams.Add(teamA);
            teams.Add(teamB);
            IEncounterRepository encounters = new EncounterRepository(context);
            Encounter aVsB = new Match(new List<Team>() { teamA, teamB }, DateTime.Today, sportA);
            encounters.Add(aVsB);
            sportStorage.Delete(sportA.Name);
            Assert.IsTrue(teams.IsEmpty());
        }


        [TestMethod]
        [ExpectedException(typeof(SportNotFoundException))]
        public void DeleteNotExistentTest()
        {
            sportStorage.Add(sportA);
            sportStorage.Delete(sportB.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void DeleteNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.Delete("DreamSport");
        }

        [TestMethod]
        public void ModifySportTest()
        {
            sportStorage.Add(sportA);
            sportA = new Sport(sportA.Name, false);
            CreateContext();
            sportStorage.Modify(sportA);
            Sport editedSport = sportStorage.Get(sportA.Name);
            Assert.AreEqual(sportA.Name, editedSport.Name);
            Assert.IsFalse(sportA.IsTwoTeams);
        }

        [TestMethod]
        [ExpectedException(typeof(SportNotFoundException))]
        public void ModifyNotExistentTest()
        {
            sportStorage.Modify(sportA);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ModifyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.Modify(sportA);
        }

        [TestMethod]
        public void ClearTest()
        {
            sportStorage.Add(sportA);
            sportStorage.Add(sportB);
            sportStorage.Clear();
            Assert.IsTrue(sportStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ClearNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.Clear();
        }

        [TestMethod]
        public void GetAllTest()
        {
            sportStorage.Add(sportA);
            sportStorage.Add(sportB);
            ICollection<Sport> sports = sportStorage.GetAll();
            Assert.AreEqual(2, sports.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetAllNoAccessTest()
        {
            CreateDisconnectedDatabase();
            sportStorage.GetAll();
        }
    }
}
