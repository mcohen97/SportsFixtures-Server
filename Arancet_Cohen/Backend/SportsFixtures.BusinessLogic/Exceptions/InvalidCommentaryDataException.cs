using System;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    [System.Serializable]
    public class InvalidCommentaryDataException : InvalidDataException
    {
        public InvalidCommentaryDataException(string message) { }
    }
}