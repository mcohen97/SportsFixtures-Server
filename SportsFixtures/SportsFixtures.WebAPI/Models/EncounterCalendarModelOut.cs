using System.Collections.Generic;

namespace ObligatorioDA2.WebAPI.Models
{
    public class EncounterCalendarModelOut
    {
        public string SportName { get; set; }
        public ICollection<EncountersGroupedByDate> EncountersByDate { get; set; }
    }
}
