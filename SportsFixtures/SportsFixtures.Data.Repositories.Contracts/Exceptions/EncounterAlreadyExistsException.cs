using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class EncounterAlreadyExistsException : EntityAlreadyExistsException
    {
        public EncounterAlreadyExistsException():base("This match already exists")
        {
        }
    }
}
