using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.Services.Mappers
{
    public class TeamDtoMapper
    {
        public TeamDto ToDto(Team aTeam) {
            TeamDto data = new TeamDto()
            {
                id = aTeam.Id,
                name = aTeam.Name,
                photo = aTeam.PhotoPath,
                sportName = aTeam.Sport.Name
            };
            return data;
        }
    }
}
