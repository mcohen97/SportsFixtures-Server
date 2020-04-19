
namespace ObligatorioDA2.Services.Exceptions
{
    public class TeamAlreadyFollowedException:ServiceException
    {
        public TeamAlreadyFollowedException():base("User already follows team", ErrorType.ENTITY_ALREADY_EXISTS) {

        }
    }
}
