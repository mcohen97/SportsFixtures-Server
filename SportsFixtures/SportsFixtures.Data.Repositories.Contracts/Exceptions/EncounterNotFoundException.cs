using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class EncounterNotFoundException: EntityNotFoundException
    {
        public EncounterNotFoundException():base("Match not found")
        {

        }
    }
}
