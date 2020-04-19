using System;

namespace ObligatorioDA2.Services.Exceptions
{
    public class ServiceException: Exception
    {
        public ErrorType Error { get; set; }
        public ServiceException(string message, ErrorType type) : base(message) {
            Error = type;
        }
    }
}
