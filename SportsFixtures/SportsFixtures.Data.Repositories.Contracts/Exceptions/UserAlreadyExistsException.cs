using System;
using System.Runtime.Serialization;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class UserAlreadyExistsException : EntityAlreadyExistsException
    {
        public UserAlreadyExistsException() : base("User already exists") { }
    }
}