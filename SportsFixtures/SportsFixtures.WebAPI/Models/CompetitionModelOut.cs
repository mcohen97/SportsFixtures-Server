using System.Collections.Generic;

namespace ObligatorioDA2.WebAPI.Models
{
    public class CompetitionModelOut:EncounterModelOut
    {
        public ICollection<StandingModelOut> Team_Position { get; set; }
    }
}
