
namespace ObligatorioDA2.Services.Exceptions
{
    public class TeamAlreadyHasEncounterException : ServiceException
    {
        public TeamAlreadyHasEncounterException():base("One of these teams already plays in this date", ErrorType.ENTITY_ALREADY_EXISTS)
        {
        }

        public TeamAlreadyHasEncounterException(string message) : base(message, ErrorType.ENTITY_ALREADY_EXISTS)
        {
        }
    }
}