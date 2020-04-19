namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    [System.Serializable]
    public class TeamAlreadyExistsException : EntityAlreadyExistsException
    {
        public TeamAlreadyExistsException():base("Team already exists") { }
    }
}