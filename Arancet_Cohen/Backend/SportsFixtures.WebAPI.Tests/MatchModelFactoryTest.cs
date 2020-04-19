using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.WebAPI.Controllers;
using ObligatorioDA2.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MatchModelFactoryTest
    {
        private EncounterDto testMatch;
        private EncounterDto testCompetition;
        private EncounterModelFactory factory;

        [TestInitialize]
        public void SetUp() {
            Sport sport = new Sport("Soccer", true);
            Team teamA = new Team(1, "teamA", "photo", sport);
            Team teamB = new Team(2, "teamB", "photo", sport);
            Team teamC = new Team(3, "teamC", "photo", sport);
            testMatch = new EncounterDto() { id = 1, teamsIds = new List<int>() { teamA.Id, teamB.Id },
                date = DateTime.Now.AddDays(1), sportName = sport.Name, isSportTwoTeams = sport.IsTwoTeams };
            ChangeSport(ref sport,ref teamA,ref teamB,ref teamC);
            testCompetition = new EncounterDto() { id = 2, teamsIds = new List<int>() { teamA.Id, teamB.Id, teamC.Id },
                date = DateTime.Now.AddDays(2), sportName = sport.Name,isSportTwoTeams = sport.IsTwoTeams };
            SetResult(testMatch);
            SetResult(testCompetition);
            factory = new EncounterModelFactory();
        }

        private void ChangeSport(ref Sport sport, ref Team teamA, ref Team teamB, ref Team teamC)
        {
            sport = new Sport("Golf", false);
            teamA = new Team(1, "teamA", "photo", sport);
            teamB = new Team(2, "teamB", "photo", sport);
            teamC = new Team(3, "teamC", "photo", sport);
        }

        private void SetResult(EncounterDto testMatch)
        {
            ICollection<Tuple<int,int>> res = new List<Tuple<int, int>>();
            int pos = 1;
            foreach (int t in testMatch.teamsIds) {
                res.Add(new Tuple<int,int>(t, pos));
                pos++;
            }
            testMatch.hasResult = true;
            testMatch.result = new ResultDto() { teams_positions = res };
        }

        [TestMethod]
        public void CompetitionModelOutTest()
        {
            EncounterModelOut modelOut = factory.CreateModelOut(testCompetition);
            CompetitionModelOut competition = modelOut as CompetitionModelOut;
            Assert.IsNotNull(competition);
            Assert.AreEqual(competition.Team_Position.Count, 3);
        }

        [TestMethod]
        public void MatchModelOutTest()
        {
            EncounterModelOut modelOut = factory.CreateModelOut(testMatch);
            MatchModelOut match = modelOut as MatchModelOut;
            Assert.IsNotNull(match);
            Assert.IsTrue(match.HasResult);
            Assert.IsTrue(match.HasWinner);
            Assert.AreEqual(match.WinnerId,match.TeamIds.ToList()[0]);
        }
    }
}
