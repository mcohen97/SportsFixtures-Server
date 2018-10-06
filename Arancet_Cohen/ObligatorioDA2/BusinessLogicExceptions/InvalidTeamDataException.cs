namespace BusinessLogic.Exceptions
{
    [System.Serializable]
    public class InvalidTeamDataException : System.Exception
    {
        public InvalidTeamDataException(string message) : base(message) { }
    }
}