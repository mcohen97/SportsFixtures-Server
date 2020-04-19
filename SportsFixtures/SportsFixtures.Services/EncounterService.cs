using System;
using System.Collections.Generic;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Contracts;
using System.Linq;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Mappers;
using ObligatorioDA2.BusinessLogic.Exceptions;

namespace ObligatorioDA2.Services
{
    public class EncounterService : IInnerEncounterService, IEncounterService
    {
        private IEncounterRepository encountersStorage;
        private ITeamRepository teamsStorage;
        private ISportRepository sportsStorage;
        private IUserRepository usersStorage;
        private IAuthenticationService authenticator;
        private EncounterFactory factory;
        private EncounterDtoMapper encounterConverter;
        private CommentaryDtoMapper commentConverter;

        public EncounterService(IEncounterRepository encountersRepository, ITeamRepository teamsRepository,
            ISportRepository sportsRepository, IAuthenticationService authService)
        {
            factory = new EncounterFactory();
            encountersStorage = encountersRepository;
            teamsStorage = teamsRepository;
            sportsStorage = sportsRepository;
            authenticator = authService;
            encounterConverter = new EncounterDtoMapper(teamsStorage, encountersStorage,sportsStorage);
        }

        public EncounterService(IEncounterRepository encountersRepository, ITeamRepository teamsRepository,
            ISportRepository sportsRepository, IUserRepository usersRepository, IAuthenticationService authService)
            : this(encountersRepository, teamsRepository, sportsRepository, authService)
        {
            usersStorage = usersRepository;
            commentConverter = new CommentaryDtoMapper(usersStorage);

        }

        public EncounterDto AddEncounter(ICollection<int> teamsIds, string sportName, DateTime date)
        {
            return AddEncounter(0, teamsIds, sportName, date);
        }

        public EncounterDto AddEncounter(int idEncounter, ICollection<int> teamsIds, string sportName, DateTime encounterDate)
        {
            EncounterDto toAdd = new EncounterDto() { id = idEncounter,sportName = sportName , date = encounterDate, teamsIds = teamsIds };
            return AddEncounter(toAdd);
        }

        public EncounterDto AddEncounter(EncounterDto anEncounter)
        {
            Encounter toAdd = TryCreateEncounter(anEncounter);
            Encounter added = AddEncounter(toAdd);
            return encounterConverter.ToDto(added);
        }

        private Encounter TryCreateEncounter(EncounterDto anEncounter) {
            try
            {
                return encounterConverter.ToEncounter(anEncounter);
            }
            catch (InvalidEncounterDataException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
        }

        public Encounter AddEncounter(Encounter toAdd) {
            try
            {
                ValidateDate(toAdd);
                return encountersStorage.Add(toAdd);
            }
            catch (EncounterAlreadyExistsException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_ALREADY_EXISTS);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public EncounterDto ModifyEncounter(int idEncounter, ICollection<int> teamsIds, DateTime date, string sportName)
        {
            EncounterDto toModify = new EncounterDto() { id = idEncounter, sportName = sportName, date = date, teamsIds = teamsIds };
            return ModifyEncounter(toModify);
        }

        public EncounterDto ModifyEncounter(EncounterDto anEncounter)
        {
            Encounter toAdd;
            try
            {
                toAdd = encounterConverter.ToEncounter(anEncounter);
                ValidateDate(toAdd);
                encountersStorage.Modify(toAdd);
            }
            catch (InvalidEncounterDataException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);

            }
            catch (EncounterNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return encounterConverter.ToDto(toAdd);
        }

        private void ValidateDate(Encounter anEncounter) {
            if (anEncounter.GetParticipants().Any(t => DateOccupied(anEncounter.Id, t, anEncounter.Date)))
            {
                Team occupied = anEncounter.GetParticipants().First(t => DateOccupied(anEncounter.Id, t, anEncounter.Date));
                throw new TeamAlreadyHasEncounterException(occupied.Name + " already has an encounter on date " + anEncounter.Date);
            }
        }

        private bool DateOccupied(int encounterId, Team team, DateTime date)
        {
            if (encounterId > 0)
            {
                return encountersStorage.GetAll().Any(m => (m.Id != encounterId) && (m.GetParticipants().Any(t => t.Equals(team))) && SameDates(m.Date, date));
            }
            else
            {
                return encountersStorage.GetAll().Any(m => (m.GetParticipants().Any(t => t.Equals(team))) && SameDates(m.Date, date));
            }
        }

        private bool SameDates(DateTime date1, DateTime date2)
        {
            bool sameYear = date1.Year == date2.Year;
            bool sameMonth = date1.Month == date2.Month;
            bool sameDay = date1.Day == date2.Day;
            return sameYear && sameMonth && sameDay;
        }

        public ICollection<EncounterDto> GetAllEncounter()
        {
            try
            {
                return encountersStorage.GetAll()
                    .Select(e => encounterConverter.ToDto(e))
                    .ToList();
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public EncounterDto GetEncounter(int anId)
        {
            try
            {
                Encounter queried = encountersStorage.Get(anId);
                return encounterConverter.ToDto(queried);
            }
            catch (EncounterNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public void DeleteEncounter(int anId)
        {
            try
            {
                encountersStorage.Delete(anId);
            }
            catch (EncounterNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e)
            {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public ICollection<EncounterDto> GetAllEncounterDtos(string sportName)
        {
            try
            {
                return GetAllEncounters(sportName).Select(e => encounterConverter.ToDto(e)).ToList();
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
       }

        public ICollection<Encounter> GetAllEncounters(string sportName) {
            if (!sportsStorage.Exists(sportName))
            {
                throw new ServiceException("Sport not found", ErrorType.ENTITY_NOT_FOUND);
            }
            return encountersStorage.GetAll().Where(m => m.Sport.Name.Equals(sportName)).ToList();
        }

        public ICollection<EncounterDto> GetAllEncounterDtos(int idTeam)
        {
            if (!teamsStorage.Exists(idTeam))
            {
                throw new ServiceException("Team not found", ErrorType.ENTITY_NOT_FOUND);
            }
            return encountersStorage.GetAll()
                .Where(m => m.GetParticipants()
                .Any(t => t.Id == idTeam))
                .Select(e => encounterConverter.ToDto(e))
                .ToList();
        }

        public bool Exists(int id)
        {
            try
            {
                return encountersStorage.Exists(id);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }     


        public CommentaryDto CommentOnEncounter(int encounterId, string userName, string commentText)
        {
            CommentaryDto dto = new CommentaryDto() { makerUsername = userName, text = commentText };
            Commentary toAdd = commentConverter.ToCommentary(dto);
            Commentary added = AddComment(encounterId, toAdd); 
            dto.commentId = added.Id;
            return dto;
        }

        private Commentary AddComment(int encounterId, Commentary toAdd)
        {
            try
            {
                return encountersStorage.CommentOnEncounter(encounterId, toAdd);
            }
            catch (EncounterNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public ICollection<CommentaryDto> GetEncounterCommentaries(int encounterId)
        {
            try
            {
                Encounter stored = encountersStorage.Get(encounterId);
                return stored.GetAllCommentaries()
                    .Select(c => commentConverter.ToDto(c))
                    .ToList();
            }
            catch (EncounterNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public ICollection<CommentaryDto> GetAllCommentaries()
        {
            try
            {
                return encountersStorage.GetComments()
                    .Select(c => commentConverter.ToDto(c))
                    .ToList();
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public CommentaryDto GetComment(int id)
        {
            try
            {
                Commentary stored = encountersStorage.GetComment(id);
                return commentConverter.ToDto(stored);
            }
            catch (EntityNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public void SetResult(int id, ResultDto resultDto)
        {
            try
            {
                TrySetResult(id, resultDto);
            }
            catch (EntityNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);

            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);

            }

        }

        private void TrySetResult(int id, ResultDto resultDto)
        {
            Result result = LoadResult(resultDto.teams_positions);
            Encounter retrieved = encountersStorage.Get(id);
            try
            {
                retrieved.Result = result;
            }
            catch (InvalidEncounterDataException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
            encountersStorage.Modify(retrieved);
        }

        private Result LoadResult(ICollection<Tuple<int, int>> teamsPositions)
        {
            Result loaded = new Result();
            Team current;
            foreach (Tuple<int, int> standing in teamsPositions) {
                current = teamsStorage.Get(standing.Item1);
                loaded.Add(current, standing.Item2);
            }
            return loaded;
        }
    }
}
