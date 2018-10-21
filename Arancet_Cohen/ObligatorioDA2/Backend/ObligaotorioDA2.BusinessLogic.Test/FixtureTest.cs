using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic;
using System.Collections.Generic;
using Moq;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Linq;
using Match = ObligatorioDA2.BusinessLogic.Match;

namespace BusinessLogicTest
{
    [TestClass]
    public class FixtureTest
    {

        private ICollection<Team> teams;
        private FixtureGenerator oneMatchFixture;
        private FixtureGenerator homeAwayFixture;
        private DateTime initialDate;
        private DateTime finalDate;
        private int roundLength;
        private int daysBetweenRounds;
        private Mock<Sport> played;

        [TestInitialize]
        public void TestInitialize()
        {
            teams = new List<Team>();
            for (int i = 1; i <= 6; i++)
            {
                Team newTeam = new Team(i, "Team " + i, "Photo/" + i, new Sport("aSport"));
                teams.Add(newTeam);
            }
            initialDate = new DateTime(2019, 1, 1);
            finalDate = new DateTime(2019, 4, 4);
            roundLength = 2;
            daysBetweenRounds = 5;
            played = new Mock<Sport>("Soccer");
            oneMatchFixture = new OneMatchFixture(initialDate, roundLength, daysBetweenRounds);
            homeAwayFixture = new HomeAwayFixture(initialDate, roundLength, daysBetweenRounds);

        }

        [TestMethod]
        public void ConstructorTest()
        {
            Assert.IsNotNull(oneMatchFixture);
        }

        [TestMethod]
        public void SetInitialDateTest()
        {
            DateTime newInitialDate = new DateTime(2019, 3, 3);
            oneMatchFixture.InitialDate = newInitialDate;
            Assert.AreEqual(newInitialDate, oneMatchFixture.InitialDate);
        }

        [TestMethod]
        public void SetRoundLengthTest()
        {
            int newMax = 3;
            oneMatchFixture.RoundLength = newMax;
            Assert.AreEqual(newMax, oneMatchFixture.RoundLength);
        }

        [TestMethod]
        public void SetDaysBetweenRoundsTest()
        {
            int newDays = 6;
            oneMatchFixture.DaysBetweenRounds = newDays;
            Assert.AreEqual(newDays, oneMatchFixture.DaysBetweenRounds);
        }

        [TestMethod]
        public void ConstructorHomeAwayTest()
        {
            Assert.IsNotNull(homeAwayFixture);
        }

        [TestMethod]
        public void SetInitialDateHomeAwayTest()
        {
            DateTime newInitialDate = new DateTime(2019, 3, 3);
            homeAwayFixture.InitialDate = newInitialDate;
            Assert.AreEqual(newInitialDate, homeAwayFixture.InitialDate);
        }

        [TestMethod]
        public void SetRoundLengthHomeAwayTest()
        {
            int newMax = 3;
            homeAwayFixture.RoundLength = newMax;
            Assert.AreEqual(newMax, homeAwayFixture.RoundLength);
        }

        [TestMethod]
        public void SetDaysBetweenRoundsHomeAwayTest()
        {
            int newDays = 6;
            homeAwayFixture.DaysBetweenRounds = newDays;
            Assert.AreEqual(newDays, homeAwayFixture.DaysBetweenRounds);
        }

        [TestMethod]
        public void GenerateOneMatchFixture6TeamsTest()
        {

            ICollection<Match> fixtureResult = oneMatchFixture.GenerateFixture(teams);
            ICollection<Team> copy = new List<Team>(teams);
            bool everyMatch = true;
            foreach (Match actualMatch in GenereteMatches(copy))
            {
                everyMatch = everyMatch && CheckMatchInFixture(fixtureResult, actualMatch);
            }
            Assert.AreEqual(15, fixtureResult.Count);
            Assert.IsTrue(everyMatch);
        }

        [TestMethod]
        public void GenerateOneMatchFixture5TeamsTest()
        {
            teams.Remove(teams.Last());
            ICollection<Team> copy = new List<Team>(teams);
            ICollection<Match> fixtureResult = oneMatchFixture.GenerateFixture(teams);
            bool everyMatch = true;
            foreach (Match actualMatch in GenereteMatches(copy))
            {
                everyMatch = everyMatch && CheckMatchInFixture(fixtureResult, actualMatch);
            }
            Assert.AreEqual(10, fixtureResult.Count);
            Assert.IsTrue(everyMatch);
        }

        [TestMethod]
        public void GenerateHomeAwayMatchFixture6TeamsTest()
        {
            ICollection<Match> fixtureResult = homeAwayFixture.GenerateFixture(teams);
            ICollection<Team> copy = new List<Team>(teams);
            bool everyMatch = true;
            foreach (Match actualMatch in GenereteMatchesHomeAway(copy))
            {
                everyMatch = everyMatch && CheckMatchInFixture(fixtureResult, actualMatch);
            }
            Assert.AreEqual(30, fixtureResult.Count);
            Assert.IsTrue(everyMatch);
        }

        [TestMethod]
        public void GenerateHomeAwayMatchFixture5TeamsTest()
        {
            teams.Remove(teams.Last());
            ICollection<Team> copy = new List<Team>(teams);
            ICollection<Match> fixtureResult = homeAwayFixture.GenerateFixture(teams);
            bool everyMatch = true;
            foreach (Match actualMatch in GenereteMatchesHomeAway(copy))
            {
                everyMatch = everyMatch && CheckMatchInFixture(fixtureResult, actualMatch);
            }
            Assert.AreEqual(20, fixtureResult.Count);
            Assert.IsTrue(everyMatch);
        }

        [TestMethod]
        public void GenerateOneMatchFixture2TeamsTest()
        {
            while (teams.Count > 2)
                teams.Remove(teams.Last());
            ICollection<Team> copy = new List<Team>(teams);
            ICollection<Match> fixtureResult = oneMatchFixture.GenerateFixture(teams);
            bool everyMatch = true;
            foreach (Match actualMatch in GenereteMatches(copy))
            {
                everyMatch = everyMatch && CheckMatchInFixture(fixtureResult, actualMatch);
            }
            Assert.AreEqual(1, fixtureResult.Count);
            Assert.IsTrue(everyMatch);
        }

        [TestMethod]
        public void GenerateHomeAwayMatchFixture2TeamsTest()
        {
            while (teams.Count > 2)
                teams.Remove(teams.Last());
            ICollection<Match> fixtureResult = homeAwayFixture.GenerateFixture(teams);
            ICollection<Team> copy = new List<Team>(teams);
            bool everyMatch = true;
            foreach (Match actualMatch in GenereteMatchesHomeAway(copy))
            {
                everyMatch = everyMatch && CheckMatchInFixture(fixtureResult, actualMatch);
            }
            Assert.AreEqual(2, fixtureResult.Count);
            Assert.IsTrue(everyMatch);
        }

        private ICollection<Match> GenereteMatches(ICollection<Team> teams)
        {
            ICollection<Match> matchesGenerated = new List<Match>();
            Team[] teamsArray = teams.ToArray();
            for (int i = 0; i < teamsArray.Length; i++)
            {
                for (int j = i; j < teamsArray.Length; j++)
                {
                    if (i != j)
                        matchesGenerated.Add(new Match(teamsArray[i], teamsArray[j], new DateTime(), played.Object));
                }
            }
            return matchesGenerated;
        }
        private ICollection<Match> GenereteMatchesHomeAway(ICollection<Team> teams)
        {
            ICollection<Match> matchesGenerated = new List<Match>();
            Team[] teamsArray = teams.ToArray();
            for (int i = 0; i < teamsArray.Length; i++)
            {
                for (int j = 0; j < teamsArray.Length; j++)
                {
                    if (i != j)
                        matchesGenerated.Add(new Match(teamsArray[i], teamsArray[j], new DateTime(), played.Object));
                }
            }
            return matchesGenerated;
        }
        private bool CheckMatchInFixture(ICollection<Match> fixture, Match match)
        {
            return fixture.Any(m =>
                (m.HomeTeam.Equals(match.HomeTeam) || m.HomeTeam.Equals(match.AwayTeam)) &&
                (m.AwayTeam.Equals(match.AwayTeam) || m.AwayTeam.Equals(match.HomeTeam))
            );
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTeamCountException))]
        public void LessThan2TeamsHomeAwayTest()
        {
            while (teams.Count > 1)
                teams.Remove(teams.Last());
            ICollection<Match> fixtureResult = homeAwayFixture.GenerateFixture(teams);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidTeamCountException))]
        public void LessThan2TeamsOneMatchTest()
        {
            while (teams.Count > 1)
                teams.Remove(teams.Last());
            ICollection<Match> fixtureResult = oneMatchFixture.GenerateFixture(teams);
        }


    }
}