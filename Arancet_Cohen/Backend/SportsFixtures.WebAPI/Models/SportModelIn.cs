using System.ComponentModel.DataAnnotations;

namespace ObligatorioDA2.WebAPI.Models
{
    public class SportModelIn
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
        [Required]
        public bool IsTwoTeams { get; set; }
    }
}
