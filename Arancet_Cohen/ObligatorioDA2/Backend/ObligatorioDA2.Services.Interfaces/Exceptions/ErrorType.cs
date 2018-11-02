﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.Services.Exceptions
{
    public enum ErrorType
    {
        DATA_INACCESSIBLE,
        ENTITY_ALREADY_EXISTS,
        ENTITY_NOT_FOUND,
        INVALID_DATA,
        NO_PERMISSION
    }
}
