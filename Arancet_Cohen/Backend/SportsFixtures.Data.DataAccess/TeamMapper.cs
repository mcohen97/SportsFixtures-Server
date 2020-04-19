using System;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Entities;

namespace ObligatorioDA2.Data.DomainMappers
{
    public class TeamMapper
    {
        private SportMapper sportConverter;
        public TeamMapper()
        {
            sportConverter = new SportMapper();
        }
        public Team ToTeam(TeamEntity teamEntity)
        {
            Sport teamSport = sportConverter.ToSport(teamEntity.Sport);
            Team convertedTeam = new Team(teamEntity.TeamNumber, teamEntity.Name, teamEntity.Photo, teamSport);
            return convertedTeam;
        }

        public TeamEntity ToEntity(Team team)
        {
            SportEntity convertedSport = sportConverter.ToEntity(team.Sport);
            TeamEntity convertedTeam = new TeamEntity(team.Id, team.Name, team.PhotoPath, convertedSport.Name,convertedSport);
            convertedTeam.Sport = convertedSport;
            convertedTeam.SportEntityName = team.Sport.Name;
            return convertedTeam;
        }
    }
}