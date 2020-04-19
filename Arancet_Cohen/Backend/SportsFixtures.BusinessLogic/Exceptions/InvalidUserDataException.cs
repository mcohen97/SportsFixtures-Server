using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObligatorioDA2.BusinessLogic.Exceptions
{
    public class InvalidUserDataException: InvalidDataException
    {
        public InvalidUserDataException(string message) : base(message) {

        }
    }
}
