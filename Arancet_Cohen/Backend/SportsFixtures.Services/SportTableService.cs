using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObligatorioDA2.Services
{
    public class SportTableService : ISportTableService
    {
        private ISportRepository sportsStorage;
        private ITeamRepository teamsStorage;
        private IInnerEncounterService matchesService;
        private TeamDtoMapper mapper;

        public SportTableService(ISportRepository sportsRepo, ITeamRepository teamsRepo, IInnerEncounterService service)
        {
            sportsStorage = sportsRepo;
            teamsStorage = teamsRepo;
            matchesService = service;
            mapper = new TeamDtoMapper();
        }

        public ICollection<Tuple<TeamDto, int>> GetScoreTable(string sportName)
        {
            try
            {
                Sport asked = sportsStorage.Get(sportName);
                return CalculateExistingSportTable(asked);
            }
            catch (SportNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
        }

        private ICollection<Tuple<TeamDto, int>> CalculateExistingSportTable(Sport aSport)
        {
            ICollection<Team> sportTeams = teamsStorage.GetTeams(aSport.Name);
            ICollection<Encounter> teamsEncounters = matchesService.GetAllEncounters(aSport.Name);
            ICollection<Tuple<Team, int>> positions;
            if (aSport.IsTwoTeams)
            {
                positions = CalculateMatchesTable(sportTeams, teamsEncounters);
            }
            else
            {
                positions = CalculateCompetitionsTable(sportTeams, teamsEncounters);
            }
            return positions
                .Select(t => new Tuple<TeamDto, int>(mapper.ToDto(t.Item1), t.Item2))
                .ToList();
        }

        private ICollection<Tuple<Team, int>> CalculateCompetitionsTable(ICollection<Team> sportTeams, ICollection<Encounter> teamsEncounters)
        {
            Dictionary<Team, int> table = sportTeams.ToDictionary(item => item, item => 0);
            foreach (Encounter game in teamsEncounters){
                if (game.HasResult()) {
                    AddCompetitionPoints(ref table,game);
                }
            }
            List<Tuple<Team, int>> assorted = table.Keys.Select(t => new Tuple<Team, int>(t, table[t])).ToList();
            assorted.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            return assorted;
        }

        private void AddCompetitionPoints(ref Dictionary<Team, int> table,Encounter game)
        {
            List<Tuple<Team, int>> positions = game.Result.GetPositions().ToList();
            positions.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            table[positions[0].Item1] += 3;
            table[positions[1].Item1] += 2;
            if (positions.Count > 2)
            {
                table[positions[2].Item1] += 1;
            }
        }

        private ICollection<Tuple<Team, int>> CalculateMatchesTable(ICollection<Team> sportTeams, ICollection<Encounter> teamsEncounters)
        {
            Dictionary<Team, int> table = sportTeams.ToDictionary(item => item, item => 0);
            foreach (Encounter game in teamsEncounters)
            {
                if (game.HasResult()) {
                    AddMatchPoints(ref table,game);
                }
            }
            List<Tuple<Team, int>> assorted = table.Keys.Select(t => new Tuple<Team, int>(t, table[t])).ToList();
            assorted.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            return assorted;
        }

        private void AddMatchPoints(ref Dictionary<Team, int> table, Encounter game)
        {
            List<Tuple<Team, int>> positions = game.Result.GetPositions().ToList();
            positions.Sort((x, y) => x.Item2.CompareTo(y.Item2));

            if (positions[0].Item2 == positions[1].Item2)
            {
                table[positions[0].Item1] += 1;
                table[positions[1].Item1] += 1;
            }
            else
            {
                table[positions[0].Item1] += 3;
            }
        }
    }
}
