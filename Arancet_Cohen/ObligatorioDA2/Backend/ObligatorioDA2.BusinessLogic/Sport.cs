﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ObligatorioDA2.BusinessLogic.Exceptions;

[assembly: InternalsVisibleTo("BusinessLogicTest")]
[assembly: InternalsVisibleTo("DataRepositoriesTest")]

namespace ObligatorioDA2.BusinessLogic
{
    public class Sport
    {
        private string name;
      
        public string Name { get { return name; } set { SetName(value); } }

        public bool IsTwoTeams { get; private set; }
        private void SetName(string value)
        {
            if (value == null)
            {
                throw new InvalidSportDataException("Name can't be null");
            }
            if (value == "")
            {
                throw new InvalidSportDataException("Name can't be empty");
            }
            name = value;
        }

        public Sport(string aName, bool isTwoTeams)
        {
            Name = aName;
            IsTwoTeams = isTwoTeams;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Sport))
            {
                return false;
            }

            var sport = obj as Sport;
            return name == sport.name;
        }
    }
}