using System;
using System.Collections.Generic;

namespace ObligatorioDA2.BusinessLogic
{
    public abstract class FixtureGenerator
    {
        public DateTime InitialDate { get; set; }
        public int RoundLength { get; set; }
        public int DaysBetweenRounds { get; set; }

        public FixtureGenerator(DateTime initialDate, int roundLength, int daysBetweenRounds) {
            InitialDate = initialDate;
            RoundLength = roundLength;
            DaysBetweenRounds = daysBetweenRounds;
        }
        public abstract ICollection<Encounter> GenerateFixture(ICollection<Team> teams);
    }
}