using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public abstract class DataAccessException:Exception
    {
        public DataAccessException(string aMessage):base(aMessage) {}

    }
}
