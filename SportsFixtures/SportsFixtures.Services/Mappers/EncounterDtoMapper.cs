using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObligatorioDA2.Services.Mappers
{
    public class EncounterDtoMapper
    {
        private ResultDtoMapper resultMapper;
        private ITeamRepository teamsStorage;
        private IEncounterRepository encountersStorage;
        private ISportRepository sportsStorage;
        private EncounterFactory factory;
        public EncounterDtoMapper(ITeamRepository teamsRepo, IEncounterRepository encountersRepo, ISportRepository sportsRepository) {
            teamsStorage = teamsRepo;
            encountersStorage = encountersRepo;
            sportsStorage = sportsRepository;
            resultMapper = new ResultDtoMapper(teamsRepo);
            factory = new EncounterFactory();
        }

        public EncounterDtoMapper() {
            factory = new EncounterFactory();
            resultMapper = new ResultDtoMapper();
        }
        public EncounterDto ToDto(Encounter encounterDto) {
            EncounterDto dto = new EncounterDto()
            {
                id = encounterDto.Id,
                sportName = encounterDto.Sport.Name,
                isSportTwoTeams = encounterDto.Sport.IsTwoTeams,
                date = encounterDto.Date,
                teamsIds = encounterDto.GetParticipants().Select(t => t.Id).ToList(),
                commentsIds = encounterDto.GetAllCommentaries().Select(c => c.Id).ToList(),
                hasResult = encounterDto.HasResult()
            };
            if (dto.hasResult) {
                dto.result = resultMapper.ToDto(encounterDto.Result);
            }
            return dto;
        }

        public Encounter ToEncounter(EncounterDto dto) {
            ICollection<Team> teams = TryGetTeams(dto.teamsIds);
            ICollection<Commentary> commentaries = TryGetCommentaries(dto.commentsIds);
            Sport played = TryGetSport(dto.sportName);
            return factory.CreateEncounter(dto.id,teams, dto.date, played, commentaries);
        }

        private ICollection<Commentary> TryGetCommentaries(ICollection<int> commentsIds)
        {
            return commentsIds.Select(id => TryGetCommentary(id)).ToList();
        }

        private Commentary TryGetCommentary(int id)
        {
            try
            {
                return encountersStorage.GetComment(id);
            }
            catch (CommentNotFoundException e)
            {
                throw new ServiceException("Commentary " + id + " not found", ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        private ICollection<Team> TryGetTeams(ICollection<int> teamsIds)
        {
            return teamsIds.Select(id => TryGetTeam(id)).ToList();
        }

        private Team TryGetTeam(int id)
        {
            try
            {
                return teamsStorage.Get(id);
            }
            catch (TeamNotFoundException e)
            {
                throw new ServiceException("Team " + id + " not found", ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }
        private Sport TryGetSport(string sportName)
        {
            try
            {
                return sportsStorage.Get(sportName);
            }
            catch (SportNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }
    }
}
