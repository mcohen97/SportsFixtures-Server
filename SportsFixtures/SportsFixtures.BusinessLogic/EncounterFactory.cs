using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic
{
    public class EncounterFactory
    {
        public Encounter CreateEncounter(ICollection<Team> list, DateTime date, Sport sport)
        {
            Encounter built;
            if (sport.IsTwoTeams)
            {
                built = new Match(list, date, sport);
            }
            else {
                built = new Competition(list, date, sport);
            }
            return built;
        }

        public Encounter CreateEncounter(int id,ICollection<Team> list, DateTime date, Sport sport)
        {
            Encounter built;
            if (sport.IsTwoTeams)
            {
                built = new Match(id,list, date, sport);
            }
            else
            {
                built = new Competition(id,list, date, sport);
            }
            return built;
        }

        public Encounter CreateEncounter(int id, ICollection<Team> list, DateTime date, Sport sport, ICollection<Commentary> comments)
        {
            Encounter built;
            if (sport.IsTwoTeams)
            {
                built = new Match(id, list, date, sport,comments);
            }
            else
            {
                built = new Competition(id, list, date, sport,comments);
            }
            return built;
        }
    }
}
