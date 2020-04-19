using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using System;
using System.Linq;

namespace ObligatorioDA2.Services.Mappers
{
    public class ResultDtoMapper
    {
        private ITeamRepository teamsStorage;
        public ResultDtoMapper(ITeamRepository teamsRepo) {
            teamsStorage = teamsRepo;
        }

        public ResultDtoMapper(){}

        public ResultDto ToDto(Result aResult) {
            ResultDto dto = new ResultDto()
            {
                teams_positions = aResult.GetPositions()
                .Select(p => new Tuple<int, int>(p.Item1.Id, p.Item2)).ToList()
            };
            return dto;
        }

      
    }
}
