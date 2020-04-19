namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    [System.Serializable]
    public class TeamNotFoundException : EntityNotFoundException
    {
        public TeamNotFoundException() : base("Team not found") { }
    }
}