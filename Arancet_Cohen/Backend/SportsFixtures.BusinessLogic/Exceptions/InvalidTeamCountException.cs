using System;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    public class InvalidTeamCountException : InvalidDataException
    {
        public InvalidTeamCountException(string message) : base(message) { }
    }

}