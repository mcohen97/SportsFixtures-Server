using System;
using System.Collections.Generic;

namespace BusinessLogic
{
    public class HomeAwayFixture : FixtureGenerator
    {
        private DateTime initialDate;
        private int roundLength;
        private int daysBetweenRounds;

        public HomeAwayFixture(DateTime initialDate, int roundLength, int daysBetweenRounds)
        {
            this.initialDate = initialDate;
            this.roundLength = roundLength;
            this.daysBetweenRounds = daysBetweenRounds;
        }
        public override DateTime InitialDate { get => initialDate; set => SetInitialDate(value); }
        public override int RoundLength { get => roundLength; set => SetRoundLength(value); }
        public override int DaysBetweenRounds { get => daysBetweenRounds; set => SetDaysBetweenRounds(value); }

        public override ICollection<Match> GenerateFixture(ICollection<Team> teams)
        {
            ICollection<Match> generatedFixture = new List<Match>();

            if(teams.Count % 2 != 0)
                teams.Add(new Team(-1,"Free Match", "Photos/freeMatch.png"));

            int teamsCount = teams.Count;
            int matchesCount = teamsCount * (teamsCount -1) / 2; //Combinations(teams, 2);
            int matchesPerRound =  teamsCount / 2;

            Team[,] actualRound = InitializeRound(teams);
            int matchesAdded = 0;
            int actualRoundLength = 0;
            DateTime roundDate = initialDate;
            DateTime secondDate;

            while(matchesAdded < matchesCount){
                secondDate = NextDate(roundDate, actualRoundLength);
                AddMatches(generatedFixture, actualRound, roundDate, secondDate);
                roundDate = NextDate(roundDate, actualRoundLength);
                actualRound = RotateTeams(actualRound);
                
                matchesAdded += matchesPerRound;
            
                if(actualRoundLength == roundLength)
                    actualRoundLength = 0;
                else
                    actualRoundLength++;
            }

            RemoveFreeMatches(generatedFixture);
            DuplicateMatches(generatedFixture);

            return generatedFixture;
        }

        private void DuplicateMatches(ICollection<Match> generatedFixture)
        {
            int matchesCount = generatedFixture.Count;
            IEnumerator<Match> matches = generatedFixture.GetEnumerator();
            int actualRoundLength = 0;
            for (int i = 0; i < matchesCount; i++)
            {
                matches.MoveNext();
                Match current = matches.Current;
                DateTime nextDate = NextDate(current.Date, actualRoundLength);
                Match newMatch = new Match(current.AwayTeam, current.HomeTeam, nextDate, current.Sport);
                
                if(actualRoundLength == roundLength)
                    actualRoundLength = 0;
                else
                    actualRoundLength++;
            }
        }

        private void RemoveFreeMatches(ICollection<Match> generatedFixture)
        {
            IEnumerator<Match> matches = generatedFixture.GetEnumerator();
            ICollection<Match> matchesToRemove = new List<Match>();
            while(matches.MoveNext()){
                Match actual = matches.Current;
                if(actual.HomeTeam.Id == -1 || actual.AwayTeam.Id == -1)
                    matchesToRemove.Add(actual);
            }

            foreach (Match actual in matchesToRemove)
            {
                generatedFixture.Remove(actual);
            }
        }

        private DateTime NextDate(DateTime roundDate, int actualRoundLength)
        {
            int change = 1;
            if(actualRoundLength == roundLength)
                change = daysBetweenRounds;
            DateTime nextDate = roundDate.AddDays(change);
            return nextDate;
        }

        private Team[,] RotateTeams(Team[,] actualRound)
        {
            Team[,] newRound = new Team[actualRound.GetLength(0), actualRound.GetLength(1)];
            
            //Fixed team
            newRound[0,0] = actualRound[0,0];
           
            //Teams changing rows
            Team goesDown = actualRound[0, actualRound.GetLength(1) - 1];
            Team goesUp = actualRound[1,0];

            //Move home teams
            for (int i = 1; i < newRound.GetLength(1) - 1; i++)
            {
                newRound[0,i+1] = actualRound[0,i];
            }

            //Move away teams
            for (int i = 1; i < newRound.GetLength(1); i++)
            {
                newRound[1, i-1] = actualRound[1, i];
            }

            newRound[0,1] = goesUp;
            newRound[1,newRound.GetLength(1)-1] = goesDown;

            return newRound;
        }

        private void AddMatches(ICollection<Match> fixture, Team[,] actualRound, DateTime firstDate, DateTime secondDate)
        {
            for(int i = 0; i < actualRound.GetLength(1); i++){
                Sport sport = actualRound[0, i].Sport;
                Match firstMatch = new Match(actualRound[0,i], actualRound[1,i], firstDate, sport);
                Match secondMatch = new Match(actualRound[1,i], actualRound[0,i], secondDate, sport);

                fixture.Add(firstMatch);
                fixture.Add(secondMatch);
            }
        }

        private Team[,] InitializeRound(ICollection<Team> teams){
            IEnumerator<Team> enumerator = teams.GetEnumerator();
            Team[,] actualRound = new Team[2, teams.Count/2];
            for(int i = 0; i < 2; i++){
                for(int j = 0; j < actualRound.GetLength(1); j++)
                    if(enumerator.MoveNext())
                        actualRound[i,j] = enumerator.Current;
            }
            return actualRound;
        }

         private void SetInitialDate(DateTime value)
        {
            initialDate = value;
        }

        private void SetRoundLength(int value)
        {
            roundLength = value;
        }

        private void SetDaysBetweenRounds(int value)
        {
            daysBetweenRounds = value;
        }
    }
}