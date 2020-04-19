using System;
using System.Runtime.Serialization;

namespace ObligatorioDA2.BusinessLogic.Data.Exceptions
{
    [Serializable]
    public class SportNotFoundException : EntityNotFoundException
    {
        public SportNotFoundException():base("Sport not found")
        {
        }

    }
}