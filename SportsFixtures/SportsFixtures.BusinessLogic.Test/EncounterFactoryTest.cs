using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.BusinessLogic.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class EncounterFactoryTest
    {
        private EncounterFactory testFactory;

        private Team teamA;
        private Team teamB;
        private Team teamC;
        private Sport sport;
        private DateTime date;
        private Encounter match;
        private List<Commentary> comments; 

        [TestInitialize]
        public void SetUp() {
            testFactory = new EncounterFactory();

            sport = new Sport("Soccer", true);
            teamA = new Team(1, "TeamA", "Photo/A", sport);
            teamB = new Team(2, "TeamB", "Photo/B", sport);
            teamC = new Team(2, "TeamC", "Photo/C", sport);
            date = new DateTime(2019, 1, 25, 13, 30, 0);
            User commentarist = CreateUser();
            Commentary commentary1 = new Commentary(1, "Commentary 1", commentarist);
            Commentary commentary2 = new Commentary(2, "Commentary 2", commentarist);
            Commentary commentary3 = new Commentary(3, "Commentary 3", commentarist);
            comments = new List<Commentary>() { commentary1, commentary2, commentary3 };
        }

        private User CreateUser()
        {
            UserId identity = new UserId()
            {
                Name = "aName",
                Surname = "aSurname",
                UserName = "aUsername",
                Password = "aPassword",
                Email = "anEmail@aDomain.com"
            };
            User toReturn = new User(identity, false);
            return toReturn;
        }

        [TestMethod]
        public void BuildMatch3ArgumentsTest() {
            Encounter testMatch = testFactory.CreateEncounter(new List<Team>() { teamA, teamB }, date, sport);
            Match downcast = testMatch as Match;
            Assert.IsNotNull(downcast);
        }

        [TestMethod]
        public void BuildCompetition3ArgumentsTest() {
            ChangeSport();
            Encounter testMatch = testFactory.CreateEncounter(new List<Team>() { teamA, teamB }, date, sport);
            Competition downcast = testMatch as Competition;
            Assert.IsNotNull(downcast);
        }

        [TestMethod]
        public void BuildMatch4ArgumentsTest()
        {
            Encounter testMatch = testFactory.CreateEncounter(3,new List<Team>() { teamA, teamB }, date, sport);
            Match downcast = testMatch as Match;
            Assert.IsNotNull(downcast);
        }

        [TestMethod]
        public void BuildCompetition4ArgumentsTest()
        {
            ChangeSport();
            Encounter testMatch = testFactory.CreateEncounter(3,new List<Team>() { teamA, teamB }, date, sport);
            Competition downcast = testMatch as Competition;
            Assert.IsNotNull(downcast);
        }

        [TestMethod]
        public void BuildMatch5ArgumentsTest()
        {
            Encounter testMatch = testFactory.CreateEncounter(3, new List<Team>() { teamA, teamB }, date, sport, comments);
            Match downcast = testMatch as Match;
            Assert.IsNotNull(downcast);
        }

        [TestMethod]
        public void BuildCompetition5ArgumentsTest()
        {
            ChangeSport();
            Encounter testMatch = testFactory.CreateEncounter(3, new List<Team>() { teamA, teamB }, date, sport, comments);
            Competition downcast = testMatch as Competition;
            Assert.IsNotNull(downcast);
        }


        private void ChangeSport() {
            sport = new Sport("Archery", false);
            teamA = new Team(1, "TeamA", "Photo/A", sport);
            teamB = new Team(2, "TeamB", "Photo/B", sport);
            teamC = new Team(2, "TeamC", "Photo/C", sport);
        }

    }
}
