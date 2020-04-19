using System;
using System.Runtime.Serialization;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class UserNotFoundException : EntityNotFoundException
    {
        public UserNotFoundException() : base("User not found") { }
    }
}