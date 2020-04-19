using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using Match = ObligatorioDA2.BusinessLogic.Match;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace DataRepositoriesTest
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EncounterRepositoryTest
    {
        private IEncounterRepository matchesStorage;
        private ISportRepository sportsStorage;
        private ITeamRepository teamsStorage;
        private IUserRepository usersRepo;
        private Match match;
        private Sport sport;
        DatabaseConnection context;

        [TestInitialize]
        public void SetUp()
        {
            sport = new Sport("Soccer",true);
            SetUpRepository();
            match = BuildFakeMatch();
            context.Database.EnsureDeleted();
        }

        private void SetUpRepository()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "MatchRepositoryTest")
                .Options;
            context = new DatabaseConnection(options);
            matchesStorage = new EncounterRepository(context);
            sportsStorage = new SportRepository(context);
            teamsStorage = new TeamRepository(context);
            usersRepo = new UserRepository(context);

        }

        private void CreateDisconnectedDatabase()
        {
            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
                .UseInMemoryDatabase(databaseName: "MatchDisconnectedTest")
                .Options;
            Mock<DatabaseConnection> contextMock = new Mock<DatabaseConnection>(options);
            Mock<DbException> toThrow = new Mock<DbException>();
            contextMock.Setup(c => c.Encounters).Throws(toThrow.Object);
            contextMock.Setup(c => c.Comments).Throws(toThrow.Object);
            matchesStorage = new EncounterRepository(contextMock.Object);
        }

        private Match BuildFakeMatch()
        {
            ICollection<Team> fakeTeams = GetFakeTeams();
            Match match = new Match(3, fakeTeams, DateTime.Now, sport);
            return match;
        }

        [TestMethod]
        public void EmptyTest()
        {
            Assert.IsTrue(matchesStorage.IsEmpty());
        }

        [TestMethod]
        public void ClearTest() {
            matchesStorage.Add(match);
            matchesStorage.Clear();
            Assert.IsTrue(matchesStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ClearNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.Clear();
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void EmptyNoDataAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.IsEmpty();
        }

        [TestMethod]
        public void AddMatchNotemptyTest()
        {
            matchesStorage.Add(match);
            Assert.IsFalse(matchesStorage.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(EncounterAlreadyExistsException))]
        public void AddRepeatedMatchTest()
        {
            matchesStorage.Add(match);
            SetUpRepository();
            matchesStorage.Add(match);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void AddMatchNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.Add(match);
        }

        [TestMethod]
        public void GetMatchTeamsTest()
        {
            AddSportAndTeams();
            matchesStorage.Add(match);
            Encounter retrieved = matchesStorage.Get(match.Id);
            Assert.AreEqual(retrieved.GetParticipants().Count, match.GetParticipants().Count);
        }

        [TestMethod]
        public void GetMatchCommentsTest()
        {
            AddSportAndTeams();
            Commentary dummy = BuildFakeCommentary();
            usersRepo.Add(dummy.Maker);
            match.AddCommentary(dummy);
            Encounter added = matchesStorage.Add(match);
            Encounter retrieved = matchesStorage.Get(added.Id);
            Assert.AreEqual(retrieved.GetAllCommentaries().Count, 1);
        }

        private void AddSportAndTeams()
        {
            sportsStorage.Add(sport);
            foreach (Team t in GetFakeTeams())
            {
                teamsStorage.Add(t);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(EncounterNotFoundException))]
        public void GetMatchNotFoundTest()
        {
            matchesStorage.Add(match);
            Encounter retrieved = matchesStorage.Get(4);
        }


        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetMatchNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.Get(2);
        }

        [TestMethod]
        public void GetCommentsTest()
        {
            Commentary dummy = BuildFakeCommentary();
            Mock<Team> home = new Mock<Team>(3, "Manchester United", "aPath", sport);
            Mock<Team> away = new Mock<Team>(5, "Real Madrid", "aPath", sport);
            Match match = new Match(3, new List<Team>() { home.Object, away.Object }, DateTime.Now, sport);

            matchesStorage.Add(match);
            matchesStorage.CommentOnEncounter(3, dummy);

            ICollection<Commentary> allComments = matchesStorage.GetComments();
            Assert.AreEqual(1, allComments.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetCommentsNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.GetComments();
        }

        [TestMethod]
        public void GetCommentTest()
        {
            Commentary dummy = BuildFakeCommentary();
            Mock<Team> home = new Mock<Team>(3, "Manchester United", "aPath", sport);
            Mock<Team> away = new Mock<Team>(5, "Real Madrid", "aPath", sport);
            Match match = new Match(3, new List<Team>() { home.Object, away.Object }, DateTime.Now, sport);

            matchesStorage.Add(match);
            Commentary added = matchesStorage.CommentOnEncounter(3, dummy);
            Commentary retrieved = matchesStorage.GetComment(added.Id);
            Assert.AreEqual(dummy.Text, retrieved.Text);
        }

        [TestMethod]
        [ExpectedException(typeof(CommentNotFoundException))]
        public void GetNotExistingCommentTest()
        {
            matchesStorage.GetComment(3);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetCommentNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.GetComment(3);
        }

        private Commentary BuildFakeCommentary()
        {
            UserId identity = new UserId()
            {
                Name = "aName",
                Surname = "aSurname",
                UserName = "aUsername",
                Password = "aPassword",
                Email = "anEmail@aDomain.com"
            };
            User somebody = new User(identity, false);
            Commentary comment = new Commentary("Some comment", somebody);
            return comment;
        }

        [TestMethod]
        public void GetAllTest()
        {
            AddSportAndTeams();
            matchesStorage.Add(match);
            ICollection<Encounter> all = matchesStorage.GetAll();
            Assert.AreEqual(all.Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetAllNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.GetAll();
        }

        [TestMethod]
        public void ModifyTest()
        {
            AddSportAndTeams();
            matchesStorage.Add(match);
            Match modified = BuildModifiedFakeMatch();
            SetUpRepository();
            matchesStorage.Modify(modified);
            Encounter retrieved = matchesStorage.Get(3);
            Assert.AreEqual(retrieved.GetParticipants().Count, modified.GetParticipants().Count);
            Assert.AreEqual(retrieved.Date, modified.Date);
        }

        [TestMethod]
        public void ModifyAddTeamTest()
        {
            ICollection<Team> modifiedTeams = GetFakeTeams();
            foreach (Team t in modifiedTeams) {
                teamsStorage.Add(t);
            }
            Team thirdTeam = new Team(6, "Internazionale de Milano", "aPath", sport);
            teamsStorage.Add(thirdTeam);
            matchesStorage.Add(match);
            SetUpRepository();
            //to make things simpler, we pretend soccer can be played by more than two teams
            sport = new Sport("Soccer", false);
            sportsStorage.Modify(sport);
            modifiedTeams.Add(thirdTeam);
            Encounter modified = new Competition(3, modifiedTeams, DateTime.Now.AddYears(2), sport);
            matchesStorage.Modify(modified);
            Encounter retrieved = matchesStorage.Get(3);
            Assert.AreEqual(retrieved.GetParticipants().Count, modified.GetParticipants().Count);
            Assert.AreEqual(retrieved.Date, modified.Date);
        }

        [TestMethod]
        [ExpectedException(typeof(EncounterNotFoundException))]
        public void ModifyUnexistentItemTest()
        {
            Mock<Team> home = new Mock<Team>(3, "Manchester United", "aPath", sport);
            Mock<Team> away = new Mock<Team>(4, "Bayern Munich", "aPath", sport);
            Match match = new Match(7,new List<Team>() { home.Object, away.Object }, DateTime.Now.AddYears(2), sport);
            matchesStorage.Modify(match);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ModifyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            Mock<Team> home = new Mock<Team>(3, "Manchester United", "aPath", sport);
            Mock<Team> away = new Mock<Team>(4, "Bayern Munich", "aPath", sport);
            Match match = new Match(7, new List<Team>() { home.Object, away.Object }, DateTime.Now.AddYears(2), sport);
            matchesStorage.Modify(match);
        }

        private ICollection<Team> GetFakeTeams() {
            Mock<Team> home = new Mock<Team>(3, "Manchester United", "aPath", sport);
            Mock<Team> away = new Mock<Team>(4, "Bayern Munich", "aPath", sport);
            ICollection<Team> fakeTeams = new List<Team>() { home.Object, away.Object };
            return fakeTeams;
        }

        private Match BuildModifiedFakeMatch()
        {
            ICollection<Team> teams = GetFakeTeams();
            Match match = new Match(3,teams, DateTime.Now.AddYears(2), sport);
            return match;
        }



        [TestMethod]
        public void ExistsTest()
        {
            matchesStorage.Add(match);
            bool exists = matchesStorage.Exists(match.Id);
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void DoesNotExistTest()
        {
            bool exists = matchesStorage.Exists(5);
            Assert.IsFalse(exists);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ExistsNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.Exists(5);
        }

        [TestMethod]
        public void DeleteTest()
        {
            matchesStorage.Add(match);
            matchesStorage.Delete(match.Id);
            bool exists = matchesStorage.Exists(match.Id);
            Assert.IsFalse(exists);
        }

        [TestMethod]
        [ExpectedException(typeof(EncounterNotFoundException))]
        public void DeleteUnexistentTest()
        {
            matchesStorage.Delete(match.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void DeleteNoAccessTest()
        {
            CreateDisconnectedDatabase();
            matchesStorage.Delete(5);
        }

    }
}
