using ObligatorioDA2.BusinessLogic.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObligatorioDA2.BusinessLogic
{
    public abstract class Encounter
    {
        public int Id { get; private set; }
        private Sport played;
        private DateTime date;
        private ICollection<Team> participants;
        private ICollection<Commentary> commentaries;
        private Result result;
        public Result Result { get { return result; } set { SetResult(value); } }

        public Encounter(ICollection<Team> teams, DateTime date, Sport aSport)
        {
            Sport = aSport;
            SetTeams(teams);
            Date = date;
            commentaries = new List<Commentary>();
        }
        public Encounter(int anId, ICollection<Team> teams, DateTime date, Sport sport) : this(teams, date, sport)
        {
            Id = anId;
        }

        public Encounter(int anId, ICollection<Team> teams, DateTime date, Sport sport,
         ICollection<Commentary> comments) : this(anId, teams, date, sport)
        {
            commentaries = comments;
        }

        public Sport Sport { get { return played; } set { SetSport(value); } }

        private void SetSport(Sport value)
        {
            if (value == null)
            {
                throw new InvalidEncounterDataException();
            }
            played = value;
        }
        public DateTime Date { get { return date; } set { SetDate(value); } }

        private void SetDate(DateTime value)
        {
            date = value;
        }

        public void SetTeams(ICollection<Team> teams)
        {
            if (teams == null)
            {
                throw new InvalidEncounterDataException("A match can't be null");
            }
            if (teams.Count < 2)
            {
                throw new InvalidEncounterDataException("A match can't have less than 2 teams");
            }
            if (RepeatedMatches(teams))
            {
                throw new InvalidEncounterDataException("A match can't have repeated teams");
            }
            if (!ValidTeamsForSport(teams))
            {
                throw new InvalidEncounterDataException("The sport does not allow more than two teams");
            }
            if (!TeamsPlaySport(teams, Sport))
            {
                throw new InvalidEncounterDataException("The teams must play the specified sport");
            }
            participants = teams;
        }

        private bool TeamsPlaySport(ICollection<Team> teams, Sport sport)
        {
            return !teams.Any(t => !t.Sport.Equals(sport));
        }

        private bool RepeatedMatches(ICollection<Team> teams)
        {
            return teams.Count != teams.Distinct().Count();
        }

        protected abstract bool ValidTeamsForSport(ICollection<Team> teams);

        public ICollection<Team> GetParticipants()
        {
            return participants;
        }

        public bool HasCommentary(Commentary commentary)
        {
            return commentaries.Contains(commentary);
        }

        public void AddCommentary(Commentary commentary)
        {
            if (HasCommentary(commentary))
                throw new InvalidEncounterDataException("Commentary already exists in this match");
            commentaries.Add(commentary);
        }

        public void RemoveCommentary(Commentary commentary)
        {
            commentaries.Remove(commentary);
        }

        public ICollection<Commentary> GetAllCommentaries()
        {
            return new List<Commentary>(commentaries);
        }

        public void RemoveAllComments()
        {
            commentaries.Clear();
        }

        public Commentary GetCommentary(int id)
        {
            return commentaries.First(c => c.Id == id);
        }

        public bool HasResult()
        {
            return Result != null;
        }
        private void SetResult(Result aResult)
        {
            if (aResult == null) {
                throw new InvalidEncounterDataException("Result can't be null");
            }
            if (!ResultContainsTheTeams(aResult)) {
                throw new InvalidEncounterDataException("The result must contain all the encounter," +
                    " teams and only them");
            }
            if (!ConsecutivePositions(aResult)) {
                throw new InvalidEncounterDataException("The result can't have gap positions");
            }
            SpecificResultValidation(aResult);

            //result = aResult;
            result =SortResult(aResult);
        }

        private bool ResultContainsTheTeams(Result aResult)
        {
            ICollection<Team> teamsInPositions = aResult.GetPositions()
                .Select(p => p.Item1).ToList();
            bool sameTeams = teamsInPositions.Count== participants.Count 
                && !teamsInPositions.Except(participants).Any();
            return sameTeams;
        }

        private bool ConsecutivePositions(Result aResult)
        {
            int lowestPosition = aResult.GetPositions()
                .Select(p => p.Item2).Max();
            bool[] positions = new bool[lowestPosition];
            foreach (Tuple<Team,int> standing in aResult.GetPositions()) {
                positions[standing.Item2-1] = true;
            }
            return !positions.Any(p => !p);
        }
        private Result SortResult(Result aResult)
        {
            List<Tuple<Team,int>>positions =aResult.GetPositions().ToList();
            positions.Sort((x, y) => x.Item2.CompareTo(y.Item2));
            Result sorted = new Result();
            foreach (Tuple<Team, int> t in positions) {
                sorted.Add(t.Item1, t.Item2);
            }
            return sorted;
        }
        protected virtual void SpecificResultValidation(Result aResult) { }
    }
}
