using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic;
using System.Collections.Generic;
using Moq;
using ObligatorioDA2.BusinessLogic.Exceptions;
using Match = ObligatorioDA2.BusinessLogic.Match;
using System.Linq;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EncounterTest
    {
        private Team teamA;
        private Team teamB;
        private Team teamC;
        private Sport sport;
        private DateTime date;
        private Encounter match;
        private Mock<Commentary> commentary1;
        private Mock<Commentary> commentary2;
        private Mock<Commentary> commentary3;
        private EncounterFactory factory;

        [TestInitialize]
        public void TestInitialize()
        {
            factory = new EncounterFactory();
            //Create mocks.
            sport = new Sport("Soccer",true);
            teamA = new Team(1, "TeamA", "Photo/A", sport);
            teamB = new Team(2, "TeamB", "Photo/B", sport);
            teamC = new Team(2, "TeamC", "Photo/C", sport);
            date = new DateTime(2019, 1, 25, 13, 30, 0);
            Mock<User> commentarist = CreateUser();
            commentary1 = new Mock<Commentary>(1, "Commentary 1", commentarist.Object);
            commentary2 = new Mock<Commentary>(2, "Commentary 2", commentarist.Object);
            commentary3 = new Mock<Commentary>(3, "Commentary 3", commentarist.Object);

            //Configure mocks.
            commentary1.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Commentary)?.Id == 1);
            commentary2.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Commentary)?.Id == 2);
            commentary3.Setup(t => t.Equals(It.IsAny<object>())).Returns<object>(t => (t as Commentary)?.Id == 3);

            match = new Match(3,new List<Team>() { teamA, teamB }, date, sport);
        }

        private Mock<User> CreateUser()
        {
            UserId identity = new UserId()
            {
                Name = "aName",
                Surname = "aSurname",
                UserName = "aUsername",
                Password = "aPassword",
                Email = "anEmail@aDomain.com"
            };
            Mock<User> toReturn = new Mock<User>(identity, false);
            return toReturn;
        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(match);
        }

        [TestMethod]
        public void GetIdTest()
        {
            Assert.AreEqual(match.Id, 3);
        }

        [TestMethod]
        public void GetTeamTest()
        {
            Assert.IsTrue(match.GetParticipants().Any(t => t.Id == teamA.Id));
        }

        [TestMethod]
        public void GetDateTimeTest()
        {
            Assert.AreEqual(date, match.Date);
        }

        [TestMethod]
        public void GetSportTest()
        {
            Sport played = match.Sport;
            Assert.AreEqual("Soccer", played.Name);
        }

        [TestMethod]
        public void SetDateTimeTest()
        {
            DateTime newDate = new DateTime(2019, 2, 2, 12, 0, 0);
            match.Date = newDate;
            Assert.AreEqual(newDate, match.Date);
        }

        [TestMethod]
        public void MatchHasNotCommentaryTest()
        {
            Assert.IsFalse(match.HasCommentary(commentary1.Object));
        }

        [TestMethod]
        public void AddCommentaryTest()
        {
            match.AddCommentary(commentary1.Object);
            Assert.IsTrue(match.HasCommentary(commentary1.Object));
        }

        [TestMethod]
        public void RemoveCommentaryTest()
        {
            match.AddCommentary(commentary1.Object);
            match.RemoveCommentary(commentary1.Object);
            Assert.IsFalse(match.HasCommentary(commentary1.Object));
        }

        [TestMethod]
        public void GetAllCommentsTest()
        {
            match.AddCommentary(commentary1.Object);
            match.AddCommentary(commentary2.Object);
            match.AddCommentary(commentary3.Object);

            ICollection<Commentary> commentaries = match.GetAllCommentaries();

            Assert.AreEqual(3, commentaries.Count);
        }

        [TestMethod]
        public void RemoveAllCommentsTest()
        {
            match.AddCommentary(commentary1.Object);
            match.AddCommentary(commentary2.Object);
            match.AddCommentary(commentary3.Object);

            match.RemoveAllComments();

            Assert.IsFalse(match.HasCommentary(commentary1.Object));
            Assert.IsFalse(match.HasCommentary(commentary2.Object));
            Assert.IsFalse(match.HasCommentary(commentary3.Object));
        }

        [TestMethod]
        public void GetCommentaryTest()
        {
            match.AddCommentary(commentary1.Object);
            Commentary commentary = match.GetCommentary(commentary1.Object.Id);
            Assert.AreEqual(commentary.Text, commentary1.Object.Text);
        }

        //Exceptions.

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void SetNullTeamsTest()
        {
            match = new Match(3, null, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void RepeatedTeamsTest()
        {
            match = new Match(3, new List<Team>() { teamA, teamA }, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void DifferentSportTeamsTest()
        {
            sport = new Sport("Basketball", true);
            teamB = new Team(2, "TeamB", "Photo/B", sport);
            match = new Match(3, new List<Team>() { teamA, teamB }, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void LessThanTwoTeamsTest() {
            match = new Match(3, new List<Team>() {}, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void TooManyTeamsAllowedBySport() {
            sport = new Sport("Basketball", true);
            match = new Match(3, new List<Team>() { teamA, teamB,  teamC}, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void CompetitionWithTwoTeamSportTest() {
            sport = new Sport("Basketball", true);
            match = new Competition(3, new List<Team>() { teamA, teamB, teamC }, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void MatchWithMoreThanTwoTeamSportTest()
        {
            sport = new Sport("Golf", false);
            match = new Match(3, new List<Team>() { teamA, teamB, teamC }, date, sport);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void SetNullSportTest()
        {
            match = new Match(3,new List<Team>() { teamA, teamB }, date, null);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void AddAlreadyExistentCommentaryTest()
        {
            match.AddCommentary(commentary1.Object);
            match.AddCommentary(commentary1.Object);
        }

        [TestMethod]
        public void SetMatchResultTest() {
            Result fakeResult = GetFakeResult();
            match.Result=fakeResult;
            Assert.IsTrue(match.HasResult());
            Assert.IsNotNull(match.Result);
        }

        [TestMethod]
        public void SetResultSortedPositionsTest() {
            sport = new Sport("Golf", false);
            teamA = new Team(1, "TeamA", "Photo/A", sport);
            teamB = new Team(2, "TeamB", "Photo/B", sport);
            teamC = new Team(3, "TeamC", "Photo/C", sport);
            Competition competition = new Competition(3, new List<Team>() { teamA, teamB, teamC }, date, sport);
            Result assorted = new Result();
            assorted.Add(teamA, 2);
            assorted.Add(teamB, 3);
            assorted.Add(teamC, 1);
            competition.Result = assorted;
            List<Tuple<Team, int>> positions = competition.Result.GetPositions().ToList();
            Assert.AreEqual(positions[0].Item2, 1);
            Assert.AreEqual(positions[1].Item2, 2);
            Assert.AreEqual(positions[2].Item2, 3);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void SetNullResultTest() {
            match.Result=null;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void TeamInResultNotInMatchTest() {
            Result fakeResult = GetFakeResult();
            match = new Match(3, new List<Team>() { teamA, teamC }, date, sport);
            match.Result=fakeResult;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void PositionsTeamsCountMismatchTest() {
            Result fakeResult = GetFakeResult();
            fakeResult.Add(teamC, 5);
            match.Result=fakeResult;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void NonConsecutivePositionsTest()
        {
            Result draw = new Result();
            draw.Add(teamA, 1);
            draw.Add(teamB, 4);
            match.Result=draw;
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidEncounterDataException))]
        public void SharedPositionsInCompetitionTest()
        {
            sport = new Sport("Archery", false);
            teamA = new Team(1, "TeamA", "Photo/A", sport);
            teamB = new Team(2, "TeamB", "Photo/B", sport);
            teamC = new Team(2, "TeamC", "Photo/C", sport);
            match = new Competition(3, new List<Team>() { teamA, teamB, teamC }, date, sport);
            Result fakeResult = GetFakeResult();
            fakeResult.Add(teamC, 1);
            match.Result=fakeResult;
        }

        [TestMethod]
        public void TiedMatchTest()
        {
            Result draw = new Result();
            draw.Add(teamA, 1);
            draw.Add(teamB, 1);
            match.Result=draw;
            Assert.AreEqual(match.Result.GetPositions().Count, 2);
        }

        private Result GetFakeResult()
        {
            Result toGenerate = new Result();
            toGenerate.Add(teamA, 1);
            toGenerate.Add(teamB, 2);
            return toGenerate;
        }
    }
}