using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class LogNotFoundException : EntityNotFoundException
    {
        public LogNotFoundException() : base("Log not found")
        {
        }
    }
}
