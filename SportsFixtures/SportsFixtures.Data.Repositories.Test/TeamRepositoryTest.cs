using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.Data.DataAccess;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using Match = ObligatorioDA2.BusinessLogic.Match;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DataRepositoriesTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TeamRepositoryTest
    {
        private ITeamRepository teamsStorage;
        private ISportRepository sportsStorage;
        private IUserRepository usersStorage;
        private IEncounterRepository matchesStorage;


        [TestInitialize]
        public void TestInitialize()
        {
            SetUpRepository();
            ClearDataBase();
        }
        private void SetUpRepository()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
               .UseInMemoryDatabase(databaseName: "TeamRepository")
               .Options;
            DatabaseConnection context = new DatabaseConnection(options);
            sportsStorage = new SportRepository(context);
            teamsStorage = new TeamRepository(context);
            usersStorage = new UserRepository(context);
            matchesStorage = new EncounterRepository(context);
        }

        private void CreateDisconnectedDatabase()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "TeamRepositoryTest")
                .Options;
            Mock<DatabaseConnection> contextMock = new Mock<DatabaseConnection>(options);
            Mock<DbException> toThrow = new Mock<DbException>();
            contextMock.Setup(c => c.Teams).Throws(toThrow.Object);
            contextMock.Setup(c => c.Sports).Throws(toThrow.Object);
            teamsStorage = new TeamRepository(contextMock.Object);
        }

        private void ClearDataBase()
        {
            usersStorage.Clear();
            matchesStorage.Clear();
            teamsStorage.Clear();
            sportsStorage.Clear();
        }

        [TestMethod]
        public void NoTeamsTest()
        {
            bool noTeams = teamsStorage.IsEmpty();
            Assert.IsTrue(noTeams);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void IsEmptyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.IsEmpty();
        }


        [TestMethod]
        public void AddTeamTest()
        {
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team.Object);
            Assert.AreEqual(1, teamsStorage.GetAll().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamAlreadyExistsException))]
        public void AddAlreadyExistentTeamTest()
        {
            Team team = new Team(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team);
            SetUpRepository();
            teamsStorage.Add(team);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void AddTeamNoAccessTest()
        {
            CreateDisconnectedDatabase();
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team.Object);
        }

        [TestMethod]
        public void GetTeamTest()
        {
            ITeamRepository specific = (ITeamRepository)teamsStorage;
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team.Object);
            Team teamInDb = specific.Get("Soccer", "DreamTeam");
            Assert.AreEqual("DreamTeam", teamInDb.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFoundException))]
        public void GetNotExistentTeamTest()
        {
            Team teamInDb = teamsStorage.Get("Soccer", "DreamTeam");
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Get("Soccer", "DreamTeam");
        }

        [TestMethod]
        public void ExistsTeamWithSportTest()
        {
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team.Object);
            bool result = teamsStorage.Exists("Soccer", team.Object.Name);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesNotExistTest()
        {
            Team team1 = new Team(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Team team2 = new Team(2, "DreamTeam2", "MyResources/DreamTeam2.png", new Sport("Soccer",true));
            teamsStorage.Add(team1);
            bool result = teamsStorage.Exists("Basketball", team1.Name);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ExistsNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Exists("Basketball", "name");
        }

        [TestMethod]
        public void DeleteTest()
        {
            Team team = new Team(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team);
            teamsStorage.Delete("Soccer", team.Name);
            Assert.IsTrue(teamsStorage.IsEmpty());
        }

        [TestMethod]
        public void DeleteWithMatchesTest()
        {
            Sport played = new Sport("Soccer",true);
            Team team1 = new Team(1, "DreamTeam", "MyResources/DreamTeam.png", played);
            Team team2 = new Team(2, "DreamTeam2", "MyResources/DreamTeam2.png", played);
            Match match = new Match(1, new List<Team>() { team1, team2 }, DateTime.Now, played);
            teamsStorage.Add(team1);
            teamsStorage.Add(team2);
            matchesStorage.Add(match);
            teamsStorage.Delete(1);
            Assert.IsTrue(matchesStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void DeleteNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Delete("Soccer", "name");
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFoundException))]
        public void DeleteByIdNotExistentTest()
        {
            teamsStorage.Delete(3);
        }


        [TestMethod]
        [ExpectedException(typeof(TeamNotFoundException))]
        public void DeleteNotExistentTest()
        {
            Team team1 = new Team(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Team team2 = new Team(2, "DreamTeam2", "MyResources/DreamTeam2.png", new Sport("Soccer",true));
            teamsStorage.Add(team1);
            teamsStorage.Delete(team2.Sport.Name, team2.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void DeleteByIdNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Delete(3);
        }

        [TestMethod]
        public void ModifyTeamTest()
        {
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team.Object);
            team.Object.PhotoPath = "NewDreamTeam.png";
            SetUpRepository();
            teamsStorage.Modify(team.Object);
            Team editedTeam = teamsStorage.Get("Soccer", team.Object.Name);
            Assert.AreEqual(team.Object.PhotoPath, editedTeam.PhotoPath);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFoundException))]
        public void ModifyNotExistentTest()
        {
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Modify(team.Object);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ModifyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Modify(team.Object);
        }

        [TestMethod]
        public void ClearTest()
        {
            Mock<Team> team1 = new Mock<Team>(1, "DreamTeam1", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Mock<Team> team2 = new Mock<Team>(2, "DreamTeam2", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Mock<Team> team3 = new Mock<Team>(3, "DreamTeam3", "MyResources/DreamTeam.png", new Sport("Soccer",true));

            teamsStorage.Add(team1.Object);
            teamsStorage.Add(team2.Object);
            teamsStorage.Add(team3.Object);

            teamsStorage.Clear();
            Assert.IsTrue(teamsStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ClearNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Clear();
        }

        [TestMethod]
        public void GetAllTest()
        {
            Mock<Team> team1 = new Mock<Team>(1, "DreamTeam1", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Mock<Team> team2 = new Mock<Team>(2, "DreamTeam2", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Mock<Team> team3 = new Mock<Team>(3, "DreamTeam3", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            team1.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Team)?.Name == "DreamTeam1");
            team2.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Team)?.Name == "DreamTeam2");
            team3.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Team)?.Name == "DreamTeam3");

            teamsStorage.Add(team1.Object);
            teamsStorage.Add(team2.Object);
            teamsStorage.Add(team3.Object);

            ICollection<Team> teams = teamsStorage.GetAll();

            Assert.AreEqual(3, teams.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetAllNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.GetAll();
        }

        [TestMethod]
        public void GetTeamsOfSport()
        {
            Mock<Team> team1 = new Mock<Team>(1, "DreamTeam1", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Mock<Team> team2 = new Mock<Team>(2, "DreamTeam2", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            Mock<Team> team3 = new Mock<Team>(3, "DreamTeam3", "MyResources/DreamTeam.png", new Sport("Basketball",true));
            team1.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Team)?.Name == "DreamTeam1");
            team2.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Team)?.Name == "DreamTeam2");
            team3.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Team)?.Name == "DreamTeam3");

            teamsStorage.Add(team1.Object);
            teamsStorage.Add(team2.Object);
            teamsStorage.Add(team3.Object);

            ICollection<Team> teams = teamsStorage.GetTeams("Soccer");

            Assert.AreEqual(2, teams.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(SportNotFoundException))]
        public void GetTeamsOfNoExistingSportTest()
        {
            teamsStorage.GetTeams("Soccer");
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetTeamsNoAccess()
        {
            CreateDisconnectedDatabase();
            teamsStorage.GetTeams("Soccer");
        }

        [TestMethod]
        public void GetByIdTest()
        {
            Mock<Team> team = new Mock<Team>(1, "DreamTeam", "MyResources/DreamTeam.png", new Sport("Soccer",true));
            teamsStorage.Add(team.Object);
            Team teamInDb = teamsStorage.Get("Soccer", team.Object.Name);
            Assert.AreEqual("DreamTeam", teamInDb.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamNotFoundException))]
        public void GetByIdNotExistentTeamTest()
        {
            Team teamInDb = teamsStorage.Get("Soccer", "Nacional");
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetByIdNoAccess()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Get("Soccer", "Nacional");
        }

        [TestMethod]
        public void GetUserTeamsTest()
        {
            User fake = GetFakeUser();

            ICollection<Team> teams = GetFakeTeams();
            foreach (Team created in teams)
            {
                teamsStorage.Add(created);
                fake.AddFavourite(created);
            }
            usersStorage.Add(fake);
            ICollection<Team> followedTeams = usersStorage.Get(fake.UserName).GetFavouriteTeams();
            Assert.AreEqual(followedTeams.Count, 3);
        }


        [TestMethod]
        public void ExistsTeamTest()
        {
            Sport sport = new Mock<Sport>("Soccer",true).Object;
            Team aTeam = new Mock<Team>(1, "DreamTeam", "photo", sport).Object;
            aTeam = teamsStorage.Add(aTeam);
            Assert.IsTrue(teamsStorage.Exists(aTeam.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ExistsIdNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Exists(3);
        }

        [TestMethod]
        public void DeleteTeamByIdTest()
        {
            Sport sport = new Mock<Sport>("Soccer",true).Object;
            Team aTeam = new Mock<Team>(1, "DreamTeam", "photo", sport).Object;
            aTeam = teamsStorage.Add(aTeam);
            teamsStorage.Delete(aTeam.Id);
            Assert.IsFalse(teamsStorage.Exists(aTeam.Id));
        }


        [TestMethod]
        public void GetTeamByIdTest()
        {
            Sport sport = new Mock<Sport>("Soccer",true).Object;
            Team aTeam = new Mock<Team>(1, "DreamTeam", "photo", sport).Object;
            aTeam = teamsStorage.Add(aTeam);
            Assert.AreEqual(teamsStorage.Get(aTeam.Id), aTeam);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetTeamByIdNoAccessTest()
        {
            CreateDisconnectedDatabase();
            teamsStorage.Get(3);
        }

        [TestMethod]
        public void GetTeamsFollowedTest()
        {
            User follower = GetFakeUser();
            usersStorage.Add(follower);
            foreach (Team dummy in GetFakeTeams())
            {
                teamsStorage.Add(dummy);
                follower.AddFavourite(dummy);
            }
            usersStorage.Modify(follower);

            ICollection<Team> retrieved = teamsStorage.GetFollowedTeams("JohnDoe");
            Assert.AreEqual(3, retrieved.Count);
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



        private ICollection<Team> GetFakeTeams()
        {
            Sport played = new Sport("Soccer",true);
            ICollection<Team> teams = new List<Team>() {
                new Team(1, "DreamTeam1", "MyResources/DreamTeam.png",played),
                new Team(2, "DreamTeam2", "MyResources/DreamTeam.png",played),
                new Team(3, "DreamTeam3", "MyResources/DreamTeam.png",played)
            };
            return teams;
        }
    }
}