﻿using System;
using BusinessLogicExceptions;
using System.Net.Mail;

namespace BusinessLogic
{
    public abstract class User
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

        public User(string aName, string aSurname, string aUserName, string aPassword, string anEmail)
        {
            Name = aName;
            Surname = aSurname;
            UserName = aUserName;
            Password = aPassword;
            Email = anEmail;
        }

        private void SetName(string aName)
        {
            if (String.IsNullOrWhiteSpace(aName))
            {
                throw new InvalidUserDataException("Name can't be empty");
            }
            name = aName;
        }

        private void SetSurname(string aSurname)
        {
            if (String.IsNullOrWhiteSpace(aSurname))
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

        public abstract bool IsAdmin();

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
