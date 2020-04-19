using System.ComponentModel.DataAnnotations;


namespace ObligatorioDA2.WebAPI.Models
{
    public class LoginModelIn
    {
        [Required(AllowEmptyStrings = false)]
        public string Username { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
