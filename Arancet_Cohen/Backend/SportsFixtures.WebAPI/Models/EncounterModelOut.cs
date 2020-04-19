using System;
using System.Collections.Generic;

namespace ObligatorioDA2.WebAPI.Models
{
    public abstract class EncounterModelOut
    {
        public bool HasResult { get; set; }
        public int Id { get; set; }
        public string SportName { get; set; }
        public ICollection<int> TeamIds { get; set; } 
        public DateTime Date { get; set; }
        public ICollection<int> CommentsIds { get; set; }
    }
}
