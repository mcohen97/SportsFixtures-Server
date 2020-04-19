

namespace ObligatorioDA2.Services.Exceptions
{
    public class NotAuthenticatedException:ServiceException
    {
        public NotAuthenticatedException() : base("User is not authenticated", ErrorType.NOT_AUTHENTICATED) { }

        
    }
}
