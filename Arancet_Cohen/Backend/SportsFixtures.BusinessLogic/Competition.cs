using ObligatorioDA2.BusinessLogic.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObligatorioDA2.BusinessLogic
{
    public class Competition: Encounter
    {
        public Competition(ICollection<Team> teams, DateTime date, Sport aSport) : base(teams, date, aSport) { }

        public Competition(int anId, ICollection<Team> teams, DateTime date, Sport sport) : base(anId, teams, date, sport) { }

        public Competition(int anId, ICollection<Team> teams, DateTime date, Sport sport,
         ICollection<Commentary> comments) : base(anId, teams, date, sport, comments) {}

        protected override bool ValidTeamsForSport(ICollection<Team> teams)
        {
            return !Sport.IsTwoTeams;
        }

        protected override void SpecificResultValidation(Result aResult)
        {
            ICollection<int> positions = aResult.GetPositions()
                .Select(p => p.Item2).ToList();
            if (RepeatedPosition(positions)) {
                throw new InvalidEncounterDataException("A result can't have repeated positions");
            }
        }

        private bool RepeatedPosition(ICollection<int> positions)
        {
            bool repeated = false;
            bool[] map = new bool[positions.Count];
            foreach (int pos in positions) {
                repeated = map[pos-1];
                map[pos-1] = true;
            }
            return repeated;
        }
    }
}
