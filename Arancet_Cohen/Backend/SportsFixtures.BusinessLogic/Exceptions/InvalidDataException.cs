using System;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    public abstract class InvalidDataException:Exception
    {
        public InvalidDataException() : base() {
        }
        public InvalidDataException(string message) : base(message)
        {
        }
    }
}
