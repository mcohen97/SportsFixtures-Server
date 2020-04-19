using System;
using System.Collections.Generic;

namespace ObligatorioDA2.Services.Contracts.Dtos
{
    public class EncounterDto
    {
   
        public int id;
        public string sportName;
        public bool isSportTwoTeams;
        public DateTime date;
        public ICollection<int> teamsIds;
        public ICollection<int> commentsIds;
        public bool hasResult;
        public ResultDto result;

        public EncounterDto()
        {
            teamsIds = new List<int>();
            commentsIds = new List<int>();
        }
    }
}
