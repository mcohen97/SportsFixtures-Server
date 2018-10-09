using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Linq;

namespace ObligatorioDA2.BusinessLogic
{
    public class Match
    {
        private Team homeTeam;
        private Team awayTeam;
        private DateTime date;
        private Sport sport;
        private ICollection<Commentary> commentaries;
        public int Id { get; private set; }

        public Match(Team home, Team away, DateTime date,Sport aSport)
        {
            HomeTeam = home;
            AwayTeam = away;
            Date = date;
            Sport = aSport;
            commentaries = new List<Commentary>();
        }

        public Match(int anId, Team home, Team away, DateTime date,Sport sport):this(home,away,date,sport) {
            Id = anId;
        }

        public Match(int anId, Team home, Team away, DateTime date, Sport sport, ICollection<Commentary> comments) : this(anId,home, away, date,sport)
        {
            commentaries = comments;
        }

        public Team HomeTeam { get{return homeTeam;} set{SetHomeTeam(value);} }
        public Team AwayTeam { get{return awayTeam;} set{SetAwayTeam(value);} }
        public DateTime Date { get{return date;} set{SetDate(value);} }

        public Sport Sport { get { return sport; }set { SetSport(value); } }

        public bool HasCommentary(Commentary commentary)
        {
            return commentaries.Contains(commentary);
        }

        public void AddCommentary(Commentary commentary)
        {
            if(HasCommentary(commentary))
                throw new InvalidMatchDataException("Commentary already exists in this match");
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
        

        private void SetHomeTeam(Team value)
        {
            if(value == null)
                throw new InvalidMatchDataException("Home team can't be null");
            if(value.Equals(awayTeam))
                throw new InvalidMatchDataException("Home team can't be same as away team");

            homeTeam = value;
        }

        private void SetAwayTeam(Team value)
        {
            if(value == null)
                throw new InvalidMatchDataException("Away team can't be null");
            if(value.Equals(homeTeam))
                throw new InvalidMatchDataException("Away team can't be same as home team");
                
            awayTeam = value;
        }

        private void SetDate(DateTime value)
        {
            date = value;
        }


        private void SetSport(Sport value)
        {
            if (value == null) {
                throw new InvalidMatchDataException();
            }
            sport = value;
        }


        public Commentary GetCommentary(int id)
        {
            return commentaries.First(c => c.Id == id);
        }
    }
}