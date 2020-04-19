using System.Collections.Generic;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services.Contracts
{
    public interface ITeamService
    {
        TeamDto GetTeam(int id);
        TeamDto AddTeam(TeamDto testDto);
        TeamDto Modify(TeamDto testDto);
        ICollection<TeamDto> GetAllTeams();
        void DeleteTeam(int id);
        ICollection<TeamDto> GetSportTeams(string name);
    }
}
