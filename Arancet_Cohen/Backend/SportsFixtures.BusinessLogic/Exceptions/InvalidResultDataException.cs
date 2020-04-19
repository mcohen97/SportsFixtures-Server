using System;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    public class InvalidResultDataException : InvalidDataException
    {
        public InvalidResultDataException(string message) : base(message)
        {
        }
    }
}
