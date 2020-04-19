using ObligatorioDA2.BusinessLogic.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObligatorioDA2.BusinessLogic.FixtureAlgorithms
{
    public class OneMatchFixture : FixtureGenerator
    {
        private DateTime initialDate;
        private int roundLength;
        private int daysBetweenRounds;
        private EncounterFactory factory;

        public OneMatchFixture(DateTime initialDate, int roundLength, int daysBetweenRounds) : base(initialDate, roundLength, daysBetweenRounds)

        {
            this.initialDate = initialDate;
            this.roundLength = roundLength;
            this.daysBetweenRounds = daysBetweenRounds;
            this.factory = new EncounterFactory();
        }

        public override ICollection<Encounter> GenerateFixture(ICollection<Team> teams)
        {
            if (teams.Count < 2)
                throw new InvalidTeamCountException("Can not generate any fixture with less than 2 teams");

            ICollection<Encounter> generatedFixture = new List<Encounter>();

            if (teams.Count % 2 != 0)
                teams.Add(new Team(-1, "Free Match", "Photos/freeMatch.png", teams.First().Sport));

            int teamsCount = teams.Count;
            int matchesCount = teamsCount * (teamsCount - 1) / 2; //Combinations(teams, 2);
            int matchesPerRound = teamsCount / 2;


            Team[,] actualRound = InitializeRound(teams);
            int matchesAdded = 0;
            int actualRoundLength = 0;
            DateTime roundDate = initialDate;

            while (matchesAdded < matchesCount)
            {
                AddMatches(generatedFixture, actualRound, roundDate);
                roundDate = NextDate(roundDate, actualRoundLength);
                actualRound = RotateTeams(actualRound);

                matchesAdded += matchesPerRound;

                if (actualRoundLength == roundLength)
                    actualRoundLength = 0;
                else
                    actualRoundLength++;
            }

            RemoveFreeMatches(generatedFixture);

            return generatedFixture;
        }

        private void RemoveFreeMatches(ICollection<Encounter> generatedFixture)
        {
            IEnumerator<Encounter> matches = generatedFixture.GetEnumerator();
            ICollection<Encounter> matchesToRemove = new List<Encounter>();
            while (matches.MoveNext())
            {
                Encounter actual = matches.Current;
                if (actual.GetParticipants().Any(t => t.Id == -1))
                {
                    matchesToRemove.Add(actual);
                }
            }

            foreach (Match actual in matchesToRemove)
            {
                generatedFixture.Remove(actual);
            }
        }

        private DateTime NextDate(DateTime roundDate, int actualRoundLength)
        {
            int change = 1;
            if (actualRoundLength == roundLength)
                change = daysBetweenRounds;
            DateTime nextDate = roundDate.AddDays(change);
            return nextDate;
        }

        private Team[,] RotateTeams(Team[,] actualRound)
        {
            Team[,] newRound = new Team[actualRound.GetLength(0), actualRound.GetLength(1)];

            //Fixed team
            newRound[0, 0] = actualRound[0, 0];

            //Teams changing rows
            Team goesDown = actualRound[0, actualRound.GetLength(1) - 1];
            Team goesUp = actualRound[1, 0];

            //Move home teams
            for (int i = 1; i < newRound.GetLength(1) - 1; i++)
            {
                newRound[0, i + 1] = actualRound[0, i];
            }

            //Move away teams
            for (int i = 1; i < newRound.GetLength(1); i++)
            {
                newRound[1, i - 1] = actualRound[1, i];
            }

            if (actualRound.GetLength(1) == 1)
            {
                newRound[0, 0] = goesUp;
                newRound[1, 0] = goesDown;
            }
            else
            {
                newRound[0, 1] = goesUp;
                newRound[1, newRound.GetLength(1) - 1] = goesDown;
            }

            return newRound;
        }

        private void AddMatches(ICollection<Encounter> fixture, Team[,] actualRound, DateTime roundDate)
        {
            for (int i = 0; i < actualRound.GetLength(1); i++)
            {
                Sport sport = actualRound[0, i].Sport;
                Encounter newMatch = factory.CreateEncounter(new List<Team>() { actualRound[0, i], actualRound[1, i] }, roundDate, sport);
                fixture.Add(newMatch);
            }
        }

        private Team[,] InitializeRound(ICollection<Team> teams)
        {
            IEnumerator<Team> enumerator = teams.GetEnumerator();
            Team[,] actualRound = new Team[2, teams.Count / 2];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < actualRound.GetLength(1); j++)
                    if (enumerator.MoveNext())
                        actualRound[i, j] = enumerator.Current;
            }
            return actualRound;
        }

    }
}