using ObligatorioDA2.BusinessLogic.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObligatorioDA2.BusinessLogic
{
    public class Result
    {
        private ICollection<Tuple<Team, int>> positions;

        public Result() {
            positions = new List<Tuple<Team, int>>();
        }

        public void Add(Team participant, int position)
        {
            if (participant == null) {
                throw new InvalidResultDataException("participant can't be null");
            }

            if (position <= 0)
            {
                throw new InvalidResultDataException("position cant be less than 1");
            }

            if (positions.Any(p => p.Item1.Equals(participant)))
            {
                throw new InvalidResultDataException("a same team can't have two positions");
            }
            positions.Add(new Tuple<Team, int>(participant, position));
        }

        public ICollection<Tuple<Team, int>> GetPositions() {
            return positions;
        }
    }
}
