﻿using System;
using System.Runtime.Serialization;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    [Serializable]
    public class SportAlreadyExistsException : Exception
    {
        public SportAlreadyExistsException()
        {
        }

    }
}