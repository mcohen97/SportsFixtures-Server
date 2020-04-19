using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.BusinessLogic
{
    public class UserFactory
    {

        public User CreateAdmin(UserId identity)
        {
            return new User(identity, true);
        }

        public User CreateAdmin(UserId identity, ICollection<Team> following)
        {
            return new User(identity, true, following);
        }

        public User CreateFollower(UserId identity)
        {
            return new User(identity, false);
        }

        public User CreateFollower(UserId identity, ICollection<Team> following)
        {
            return new User(identity, false, following);
        }

    }
}
