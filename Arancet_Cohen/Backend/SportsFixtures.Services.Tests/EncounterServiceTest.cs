using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Data.Repositories;
using ObligatorioDA2.Services.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Match = ObligatorioDA2.BusinessLogic.Match;
using ObligatorioDA2.Services.Exceptions;
using System.Linq;
using ObligatorioDA2.Services.Contracts.Dtos;
using System.Diagnostics.CodeAnalysis;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EncounterServiceTest
    {
        private IEncounterService serviceToTest;
        private IEncounterRepository matchesRepo;
        private ISportRepository sportsRepo;
        private ITeamRepository teamsRepo;
        private IUserRepository usersRepo;
        Mock<IAuthenticationService> auth;
        private Sport sport;
        private Team teamA;
        private Team teamB;
        private Team teamC;
        private Match matchAvsB;
        private Match matchAvsC;
        private Match matchBvsC;
        private EncounterDto matchAvsBDto;
        private EncounterDto matchAvsCDto;
        private EncounterDto matchBvsCDto;
        private DatabaseConnection context;

        [TestInitialize]
        public void SetUp()
        {
            sport = new Sport("Soccer",true);
            teamA = new Team(1, "teamA", "photo", sport);
            teamB = new Team(2, "teamB", "photo", sport);
            teamC = new Team(3, "teamC", "photo", sport);
            matchAvsB = new Match(1, new List<Team>() { teamA, teamB }, DateTime.Now.AddDays(1), sport);
            matchAvsC = new Match(2, new List<Team>() { teamA, teamC }, DateTime.Now.AddDays(2), sport);
            matchBvsC = new Match(3, new List<Team>() { teamB, teamC }, DateTime.Now.AddDays(3), sport);
            matchAvsBDto = new EncounterDto() { id = matchAvsB.Id, teamsIds = new List<int>() { 1, 2 }, sportName = sport.Name, date = matchAvsB.Date};
            matchAvsCDto = new EncounterDto() { id = matchAvsC.Id, teamsIds = new List<int>() { 1, 3 }, sportName = sport.Name, date = matchAvsC.Date };
            matchBvsCDto = new EncounterDto() { id = matchBvsC.Id, teamsIds = new List<int>() { 2, 3 }, sportName = sport.Name, date = matchBvsC.Date };
            SetUpRepository();
            context.Database.EnsureDeleted();
        }

        private void SetUpRepository()
        {

            DbContextOptions<DatabaseConnection> options = new DbContextOptionsBuilder<DatabaseConnection>()
               .UseInMemoryDatabase(databaseName: "MatchService")
               .Options;
            context = new DatabaseConnection(options);
            matchesRepo = new EncounterRepository(context);
            sportsRepo = new SportRepository(context);
            teamsRepo = new TeamRepository(context);
            usersRepo = new UserRepository(context);
            auth = new Mock<IAuthenticationService>();
            serviceToTest = new EncounterService(matchesRepo, teamsRepo, sportsRepo, usersRepo,auth.Object);
        }

        [TestMethod]
        public void AddMatchTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            serviceToTest.AddEncounter(matchAvsBDto);
            Assert.AreEqual(serviceToTest.GetAllEncounter().Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddInvalidFormatTest() {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            matchAvsBDto.teamsIds = new List<int>() { teamA.Id };
            serviceToTest.AddEncounter(matchAvsBDto);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddAlreadyExistentTest()
        {
            serviceToTest.AddEncounter(matchAvsBDto);
            EncounterDto sameMatch = new EncounterDto() { id = matchAvsBDto.id, teamsIds = matchAvsBDto.teamsIds, date = matchAvsBDto.date, sportName = matchAvsBDto.sportName };
            serviceToTest.AddEncounter(sameMatch);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddNoDataAccessTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.GetAll()).Throws(new DataInaccessibleException());
            fakeRepo.Setup(r => r.Add(It.IsAny<Encounter>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);

            serviceToTest.AddEncounter(matchAvsBDto);
        }

        [TestMethod]
        public void AddMatchGivingIdsTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            ICollection<int> teamsIds = matchAvsB.GetParticipants().Select(t => t.Id).ToList();
            serviceToTest.AddEncounter(teamsIds, matchAvsB.Sport.Name, matchAvsB.Date);
            Assert.AreEqual(serviceToTest.GetAllEncounter().Count, 1);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddGivingIdsAlreadyExistentTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            ICollection<int> teamsIds = matchAvsB.GetParticipants().Select(t => t.Id).ToList();
            EncounterDto added = serviceToTest.AddEncounter(teamsIds, matchAvsB.Sport.Name, matchAvsB.Date);
            EncounterDto sameMatch = new EncounterDto() { id = added.id, teamsIds = teamsIds, date = matchAvsB.Date.AddDays(1), sportName = sport.Name };
            serviceToTest.AddEncounter(sameMatch);
        }

        [TestMethod]
        public void AddAssignedGivingIdsTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            ICollection<int> teamsIds = matchAvsB.GetParticipants().Select(t => t.Id).ToList();
            EncounterDto stored = serviceToTest.AddEncounter(3, teamsIds, matchAvsB.Sport.Name, matchAvsB.Date);
            Assert.AreEqual(stored.date, matchAvsB.Date);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetUnexistentMatchTest()
        {
            serviceToTest.GetEncounter(9);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetEncounterNoDataAccessTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Get(It.IsAny<int>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            serviceToTest.GetEncounter(9);
        }

        [TestMethod]
        public void GetExistentMatchTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            serviceToTest.AddEncounter(matchAvsBDto);
            EncounterDto retrieved = serviceToTest.GetEncounter(matchAvsB.Id);
            Assert.AreEqual(retrieved.id, matchAvsB.Id);

        }

        [TestMethod]
        public void GetMatchesOfSportTest()
        {
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);

            serviceToTest.AddEncounter(matchAvsBDto);
            serviceToTest.AddEncounter(matchBvsCDto);
            ICollection<EncounterDto> matches = serviceToTest.GetAllEncounterDtos(sport.Name);
            Assert.AreEqual(matches.Count, 2);
        }


        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetMatchesOfNotExistingSportTest()
        {
            serviceToTest.GetAllEncounterDtos(sport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetMatchesOfSportNoDataAccessTest()
        {
            sportsRepo.Add(sport);

            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.GetAll()).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            serviceToTest.GetAllEncounterDtos(sport.Name);
        }

        [TestMethod]
        public void GetMatchesFromTeamTest()
        {
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);

            serviceToTest.AddEncounter(matchAvsBDto);
            serviceToTest.AddEncounter(matchAvsCDto);
            serviceToTest.AddEncounter(matchBvsCDto);
            ICollection<EncounterDto> matches = serviceToTest.GetAllEncounterDtos(teamA.Id);
            Assert.AreEqual(2, matches.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetMatchesOfNotExistingTeamTest()
        {
            serviceToTest.GetAllEncounterDtos(matchAvsB.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void DeleteUnexistentMatchTest()
        {
            serviceToTest.DeleteEncounter(3);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void DeleteMatchNoDataAccessTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Delete(It.IsAny<int>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            serviceToTest.DeleteEncounter(3);
        }

        [TestMethod]
        public void ModifyTest()
        {
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            serviceToTest.AddEncounter(matchAvsBDto);
            EncounterDto modifiedAvsB = new EncounterDto() { id = 1, teamsIds = new List<int>() { teamB.Id, teamA.Id }, date = matchAvsB.Date.AddDays(1), sportName = sport.Name };
            SetUpRepository();
            serviceToTest.ModifyEncounter(modifiedAvsB);
            EncounterDto modified = serviceToTest.GetEncounter(matchAvsB.Id);

            Assert.AreEqual(modifiedAvsB.teamsIds.Count, modified.teamsIds.Count);
            Assert.AreEqual(modifiedAvsB.date, modified.date);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyUnexistentTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamC);
            serviceToTest.ModifyEncounter(matchAvsCDto);
        }

        [TestMethod]
        public void ModifyGivingIdsTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            serviceToTest.AddEncounter(matchAvsBDto);
            SetUpRepository();
            Encounter modifiedAvsB = new Match(1, new List<Team>() { teamB, teamA }, matchAvsB.Date.AddDays(1), sport);
            serviceToTest.ModifyEncounter(modifiedAvsB.Id, new List<int>() { teamB.Id, teamA.Id }, modifiedAvsB.Date, sport.Name);
            EncounterDto stored = serviceToTest.GetEncounter(matchAvsB.Id);
            Assert.AreEqual(modifiedAvsB.Date, stored.date);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyNoMatchWithIdTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            serviceToTest.ModifyEncounter(5,new List<int>() { teamC.Id, teamA.Id }, matchAvsB.Date, sport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyNoTeamWithIdTest()
        {
            serviceToTest.AddEncounter(matchAvsBDto);
            serviceToTest.ModifyEncounter(matchAvsB.Id,new List<int>() { teamC.Id, teamA.Id }, matchAvsB.Date, sport.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ModifyNoDataAccessTest()
        {
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);

            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.GetAll()).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            EncounterDto modifiedAvsB = new EncounterDto() { id = 1, teamsIds = new List<int>() { teamB.Id, teamA.Id }, date = matchAvsB.Date.AddDays(1), sportName = sport.Name };
            serviceToTest.ModifyEncounter(modifiedAvsB);
        }

        [TestMethod]
        public void GetAllMatchesTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            serviceToTest.AddEncounter(matchAvsCDto);
            serviceToTest.AddEncounter(matchAvsBDto);
            Assert.AreEqual(serviceToTest.GetAllEncounter().Count, 2);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetAllEncountersNoDataAccessTest() {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.GetAll()).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            serviceToTest.GetAllEncounter();
        }

        [TestMethod]
        public void ExistMatchTrueTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            serviceToTest.AddEncounter(matchAvsBDto);
            Assert.IsTrue(serviceToTest.Exists(matchAvsB.Id));
        }

        [TestMethod]
        public void ExistMatchFalseTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            serviceToTest.AddEncounter(matchAvsBDto);
            Assert.IsFalse(serviceToTest.Exists(matchAvsC.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void ExistsNoDataAccessTest() {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Exists(It.IsAny<int>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            serviceToTest.Exists(3);
        }

        [TestMethod]
        [ExpectedException(typeof(TeamAlreadyHasEncounterException))]
        public void AwayTeamWithTwoMatchesSameDateTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            matchAvsBDto.date = matchBvsCDto.date;
            serviceToTest.AddEncounter(matchBvsCDto);
            serviceToTest.AddEncounter(matchAvsBDto);

        }

        [TestMethod]
        public void CommentOnMatchByIdsTest()
        {
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "mail@mail.com" };
            User commentarist = new User(identity, true);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            usersRepo.Add(commentarist);
            Encounter added = matchesRepo.Add(matchAvsB);
            SetUpRepository();
            serviceToTest.CommentOnEncounter(added.Id, commentarist.UserName, "a Comment");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void CommentNoMatchNoDataAccessTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.CommentOnEncounter(It.IsAny<int>(), It.IsAny<Commentary>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "mail@mail.com" };
            User commentarist = new User(identity, true);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            usersRepo.Add(commentarist);
            Encounter added = matchesRepo.Add(matchAvsB);
            serviceToTest.CommentOnEncounter(added.Id, commentarist.UserName, "a Comment");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void CommentNoMatchWithIdTest()
        {
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "mail@mail.com" };
            User commentarist = new User(identity, true);
            usersRepo.Add(commentarist);
            serviceToTest.CommentOnEncounter(3, commentarist.UserName, "a Comment");
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void CommentNoUserWithIdTest()
        {
            Encounter added = matchesRepo.Add(matchAvsB);
            serviceToTest.CommentOnEncounter(added.Id, "usernae", "a Comment");
        }

        [TestMethod]
        public void GetMatchCommentsTest()
        {
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "mail@mail.com" };
            User commentarist = new User(identity, true);
            usersRepo.Add(commentarist);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            Encounter added1 = matchesRepo.Add(matchAvsB);
            Encounter added2 = matchesRepo.Add(matchAvsC);
            SetUpRepository();
            serviceToTest.CommentOnEncounter(added1.Id, commentarist.UserName, "a Comment");
            serviceToTest.CommentOnEncounter(added1.Id, commentarist.UserName, "another Comment");
            serviceToTest.CommentOnEncounter(added2.Id, commentarist.UserName, "a Comment");
            ICollection<CommentaryDto> comments = serviceToTest.GetEncounterCommentaries(added1.Id);
            Assert.AreEqual(comments.Count, 2);
        }

        [TestMethod]
        public void GetCommentsTest()
        {
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "mail@mail.com" };
            User commentarist = new User(identity, true);
            usersRepo.Add(commentarist);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            Encounter added1 = matchesRepo.Add(matchAvsB);
            Encounter added2 = matchesRepo.Add(matchAvsC);
            SetUpRepository();
            serviceToTest.CommentOnEncounter(added1.Id, commentarist.UserName, "a Comment");
            serviceToTest.CommentOnEncounter(added1.Id, commentarist.UserName, "another Comment");
            serviceToTest.CommentOnEncounter(added2.Id, commentarist.UserName, "a Comment");
            ICollection<CommentaryDto> comments = serviceToTest.GetAllCommentaries();
            Assert.AreEqual(comments.Count, 3);

        }

        [TestMethod]
        public void GetCommentTest()
        {
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "mail@mail.com" };
            User commentarist = new User(identity, true);
            usersRepo.Add(commentarist);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            teamsRepo.Add(teamC);
            Encounter added1 = matchesRepo.Add(matchAvsB);
            Encounter added2 = matchesRepo.Add(matchAvsC);
            SetUpRepository();
            CommentaryDto comment = serviceToTest.CommentOnEncounter(added1.Id, commentarist.UserName, "a Comment");
            CommentaryDto retrieved = serviceToTest.GetComment(comment.commentId);
            Assert.AreEqual(comment.text, retrieved.text);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetCommentNoDataAccess() {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.GetComment(It.IsAny<int>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            serviceToTest.GetComment(3);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetAllCommentsNoDataAccessTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.GetComments()).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            serviceToTest.GetAllCommentaries();
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetNotExistingCommentTest()
        {
            serviceToTest.GetComment(3);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetCommentsOfEncounterNoDataAccessTest() {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Get(It.IsAny<int>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            serviceToTest.GetEncounterCommentaries(8);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void GetCommentsOfUnexistentEncounterTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Get(It.IsAny<int>())).Throws(new EncounterNotFoundException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);
            serviceToTest.GetEncounterCommentaries(8);
        }

        [TestMethod]
        public void SetResultTest() {
            ICollection<Tuple<int, int>> standings = GetFakeResult();
            ResultDto result = new ResultDto() { teams_positions = standings };
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            matchesRepo.Add(matchAvsB);
            serviceToTest.SetResult(matchAvsB.Id,result);
            EncounterDto retrieved = serviceToTest.GetEncounter(matchAvsB.Id);
            ResultDto retrievedResult = retrieved.result;
            Assert.AreEqual(result.teams_positions.Count, retrievedResult.teams_positions.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void SetResultUnexistentMatchTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Get(It.IsAny<int>())).Throws(new EncounterNotFoundException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            ICollection<Tuple<int, int>> standings = GetFakeResult();
            ResultDto result = new ResultDto() { teams_positions = standings };
            serviceToTest.SetResult(matchAvsB.Id, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void SetResultNoDataAccessTest()
        {
            Mock<IEncounterRepository> fakeRepo = new Mock<IEncounterRepository>();
            fakeRepo.Setup(r => r.Get(It.IsAny<int>())).Throws(new DataInaccessibleException());
            serviceToTest = new EncounterService(fakeRepo.Object, teamsRepo, sportsRepo, usersRepo, auth.Object);

            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);

            ICollection<Tuple<int, int>> standings = GetFakeResult();
            ResultDto result = new ResultDto() { teams_positions = standings };
            serviceToTest.SetResult(matchAvsB.Id, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void SetInvalidResultTest()
        {
            sportsRepo.Add(sport);
            teamsRepo.Add(teamA);
            teamsRepo.Add(teamB);
            matchesRepo.Add(matchAvsB);

            ICollection<Tuple<int, int>> standings = new List<Tuple<int, int>>() {new Tuple<int, int>(teamA.Id,1),
                                                                                new Tuple<int, int>(teamB.Id,5) };
            ResultDto result = new ResultDto() { teams_positions = standings };
            serviceToTest.SetResult(matchAvsB.Id, result);
        }


        private ICollection<Tuple<int,int>> GetFakeResult()
        {
            ICollection<Tuple<int, int>> standings = new List<Tuple<int, int>>() {new Tuple<int, int>(teamA.Id,1),
                                                                                new Tuple<int, int>(teamB.Id,2) };
            return standings;
        }
    }
}
