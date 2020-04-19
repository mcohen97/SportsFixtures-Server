using System;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    [System.Serializable]
    public class InvalidEncounterDataException : InvalidDataException
    {
        public InvalidEncounterDataException() { }
        public InvalidEncounterDataException(string message) : base(message) { }

    }
}