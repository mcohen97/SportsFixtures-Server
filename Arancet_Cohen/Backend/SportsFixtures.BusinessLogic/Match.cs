using System;
using System.Collections.Generic;

namespace ObligatorioDA2.BusinessLogic
{
    public class Match:Encounter
    {
        public Match(ICollection<Team> teams, DateTime date, Sport aSport) : base(teams, date, aSport) { }

        public Match(int anId, ICollection<Team> teams, DateTime date, Sport sport) : base(anId, teams, date, sport) { }

        public Match(int anId, ICollection<Team> teams, DateTime date, Sport sport,
         ICollection<Commentary> comments) : base(anId, teams, date, sport,comments) { }

        protected override bool ValidTeamsForSport(ICollection<Team> teams)
        {
            return Sport.IsTwoTeams && teams.Count == 2;
        }
    }
}