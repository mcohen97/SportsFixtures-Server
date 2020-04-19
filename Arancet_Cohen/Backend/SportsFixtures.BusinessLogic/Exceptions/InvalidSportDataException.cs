using System;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    [Serializable]
    public class InvalidSportDataException : InvalidDataException
    {
        public InvalidSportDataException(string message) : base(message) { }
    }
    
}