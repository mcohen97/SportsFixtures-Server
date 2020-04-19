using System;
using System.Collections.Generic;

namespace ObligatorioDA2.WebAPI.Models
{
    public class EncountersGroupedByDate
    {
        public  DateTime Date {get;set; }
        public ICollection<EncounterModelOut> Encounters { get; set; }
    }
}