using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class EntityAlreadyExistsException:DataAccessException
    {
        public EntityAlreadyExistsException(string message) : base(message) {
        }
    }
}
