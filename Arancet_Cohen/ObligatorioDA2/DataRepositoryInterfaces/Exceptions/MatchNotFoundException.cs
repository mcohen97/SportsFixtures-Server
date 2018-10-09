﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    public class MatchNotFoundException: EntityNotFoundException
    {
        public MatchNotFoundException():base("Match not found")
        {

        }
    }
}
