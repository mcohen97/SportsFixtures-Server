namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    [System.Serializable]
    public class InvalidTeamDataException : InvalidDataException
    {
        public InvalidTeamDataException(string message) : base(message) { }
    }
}