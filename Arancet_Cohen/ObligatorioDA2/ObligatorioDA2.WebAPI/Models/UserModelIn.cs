﻿namespace ObligatorioDA2.WebAPI.Models
{
    public class UserModelIn
    {
        public UserModelIn()
        {
        }

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}