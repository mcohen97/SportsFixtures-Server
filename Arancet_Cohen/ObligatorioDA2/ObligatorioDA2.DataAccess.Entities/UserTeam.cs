﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.DataAccess.Entities
{
    public class UserTeam
    {
        public UserEntity Follower { get; set; }
        public string UserEntityUserName { get; set; }


        public TeamEntity Team { get; set; }
        public string TeamEntityName { get; set; }
        public string TeamEntitySportEntityName { get; set; }

    }
}
