
namespace ObligatorioDA2.Data.Entities
{
    public class UserTeam
    {
        public UserEntity Follower { get; set; }
        public string UserEntityUserName { get; set; }


        public TeamEntity Team { get; set; }
        public string TeamEntityName { get; set; }
        public string TeamEntitySportEntityName { get; set; }

    }
}
