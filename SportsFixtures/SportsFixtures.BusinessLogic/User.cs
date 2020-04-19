using System;
using ObligatorioDA2.BusinessLogic.Exceptions;
using System.Net.Mail;
using System.Collections.Generic;

namespace ObligatorioDA2.BusinessLogic
{
    public class User
    {
        private string name;

        public string Name { get { return name; } private set { SetName(value); } }

        private string surname;

        public string Surname { get { return surname; } private set { SetSurname(value); } }

        private string userName;

        public string UserName { get { return userName; } private set { SetUserName(value); } }

        private string password;
        public string Password { get { return password; } private set { SetPassword(value); } }

        private string email;

        public string Email { get { return email; } private set { SetEmail(value); } }

        public bool IsAdmin { get; private set; }

        public ICollection<Team> favourites { get; private set; }

        public User(UserId indentification, bool isAdmin)
        {
            Name = indentification.Name;
            Surname = indentification.Surname;
            UserName = indentification.UserName;
            Password = indentification.Password;
            Email = indentification.Email;
            IsAdmin = isAdmin;
            favourites = new List<Team>();
        }

        public User(UserId identification, bool isAdmin, ICollection<Team> someTeams)
            : this(identification, isAdmin)
        {
            favourites = someTeams;
        }

        private void SetName(string aName)
        {
            if (string.IsNullOrWhiteSpace(aName))
            {
                throw new InvalidUserDataException("Name can't be empty");
            }
            name = aName;
        }

        private void SetSurname(string aSurname)
        {
            if (string.IsNullOrWhiteSpace(aSurname))
            {
                throw new InvalidUserDataException("Surname can't be empty");
            }
            surname = aSurname;
        }

        private void SetUserName(string aUserName)
        {
            if (string.IsNullOrWhiteSpace(aUserName))
            {
                throw new InvalidUserDataException("User name can't be empty");
            }
            userName = aUserName;
        }


        private void SetPassword(string aPassword)
        {
            if (string.IsNullOrWhiteSpace(aPassword))
            {
                throw new InvalidUserDataException("Password can't be empty");
            }
            password = aPassword;
        }

        private void SetEmail(string anEmail)
        {
            if (string.IsNullOrWhiteSpace(anEmail))
            {
                throw new InvalidUserDataException("Email can't be empty");
            }
            if (!IsValidEmail(anEmail))
            {
                throw new InvalidUserDataException("Invalid email format");
            }
            email = anEmail;
        }

        private bool IsValidEmail(string anEmail)
        {
            bool valid;
            try
            {
                MailAddress m = new MailAddress(anEmail);
                valid = true;
            }
            catch (FormatException)
            {
                valid = false;
            }
            return valid;
        }


        public ICollection<Team> GetFavouriteTeams()
        {
            return favourites;
        }

        public void AddFavourite(Team team)
        {
            if (!favourites.Contains(team))
            {
                favourites.Add(team);
            }
            else
            {
                throw new InvalidUserDataException("User already follows team");
            }
        }

        public bool HasFavouriteTeam(Team aTeam)
        {
            return favourites.Contains(aTeam);
        }

        public void RemoveFavouriteTeam(Team aTeam)
        {
            if (favourites.Contains(aTeam))
            {
                favourites.Remove(aTeam);
            }
            else
            {
                throw new InvalidUserDataException("User does not follow the team");
            }
        }

        public override bool Equals(object obj)
        {
            bool areEqual;
            if (obj == null || obj.GetType() != GetType())
            {
                areEqual = false;
            }
            else
            {
                User knownCast = (User)obj;
                areEqual = UserName.Equals(knownCast.UserName);
            }
            return areEqual;
        }


    }
}
