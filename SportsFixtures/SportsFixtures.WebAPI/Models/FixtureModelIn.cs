using System;
using System.ComponentModel.DataAnnotations;


namespace ObligatorioDA2.WebAPI.Models
{
    public class FixtureModelIn
    {
        [Required]
        public DateTime InitialDate { get; set; }

        [Required]
        public string FixtureName { get; set; }

        public int RoundLength;

        public int DaysBetweenRounds;
    }
}
