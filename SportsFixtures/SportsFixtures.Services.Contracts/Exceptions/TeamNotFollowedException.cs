namespace ObligatorioDA2.Services.Exceptions
{
    public class TeamNotFollowedException:ServiceException
    {
        public TeamNotFollowedException():base("You don't follow this team", ErrorType.ENTITY_NOT_FOUND) {}
    }
}
