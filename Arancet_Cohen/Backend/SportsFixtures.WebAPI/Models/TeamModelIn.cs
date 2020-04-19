using System.ComponentModel.DataAnnotations;

namespace ObligatorioDA2.WebAPI.Models
{
    public class TeamModelIn
    {
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string SportName { get; set; }

        public string Photo{ get; set; }
    }
}