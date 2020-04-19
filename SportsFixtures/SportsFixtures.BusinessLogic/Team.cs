using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DataRepositoriesTest")]

namespace ObligatorioDA2.BusinessLogic
{
    public class Team
    {
        private string name;
        private string photo;
        private int id;
        private Sport sport;

        public Team(string name, string photo, Sport aSport)
        {
            Name = name;
            PhotoPath = photo;
            Sport = aSport;
        }
        public Team(int id, string name, string photo, Sport aSport) : this(name, photo, aSport)
        {
            Id = id;
        }

        public string Name { get { return name; } set { SetName(value); } }

        public string PhotoPath { get { return photo; } set { SetPhoto(value); } }

        public int Id { get { return id; } set { SetId(value); } }

        public Sport Sport { get => sport; set => SetSport(value); }

        private void SetSport(Sport aSport)
        {
            sport = aSport ?? throw new InvalidSportDataException("Sport can't be null");
        }

        public void SetId(int value)
        {
            id = value;
        }

        private void SetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidTeamDataException("Name must be a non-empty character string");
            }
            name = value;
        }

        private void SetPhoto(string value)
        {
            if (value == null)
                throw new InvalidTeamDataException("Photo can't be null");

            photo = value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is Team))
                return false;

            var team = obj as Team;
            return Sport.Equals(team.Sport) && Name.Equals(team.Name);
        }

        public override int GetHashCode()
        {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }
    }
}