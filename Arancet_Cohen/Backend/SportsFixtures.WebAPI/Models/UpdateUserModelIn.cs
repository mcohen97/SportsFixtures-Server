
namespace ObligatorioDA2.WebAPI.Models
{
    public class UpdateUserModelIn
    {
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }
    }
}
