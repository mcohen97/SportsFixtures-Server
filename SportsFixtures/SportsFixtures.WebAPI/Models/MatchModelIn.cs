using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObligatorioDA2.WebAPI.Models
{
    public class MatchModelIn
    {
        [Required(AllowEmptyStrings = false)]
        public string SportName { get; set; }
        [Required(AllowEmptyStrings = false)]
        public ICollection<int> TeamIds { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
    }
}
