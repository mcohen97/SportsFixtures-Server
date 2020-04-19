using System;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.BusinessLogic.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Mappers;

namespace ObligatorioDA2.Services
{
    public class SportService : ISportService
    {
        private ISportRepository sports;
        private ITeamRepository teams;
        private IAuthenticationService authenticator;
        private SportDtoMapper mapper;

        public SportService(ISportRepository sportRepository, ITeamRepository teamRepository, IAuthenticationService authService)
        {
            sports = sportRepository;
            teams = teamRepository;
            authenticator = authService;
            mapper = new SportDtoMapper();
        }

        public ICollection<SportDto> GetAllSports()
        {
            authenticator.Authenticate();
            ICollection<Sport> allOfThem;
            try
            {
                allOfThem = sports.GetAll();
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return allOfThem
                .Select(s => mapper.ToDto(s))
                .ToList();
        }

        public SportDto GetSport(string name)
        {
            authenticator.Authenticate();
            Sport fromStorage;
            try
            {
                fromStorage = sports.Get(name);
            }
            catch (SportNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return mapper.ToDto(fromStorage);
        }

        public SportDto AddSport(SportDto dto)
        {
            authenticator.AuthenticateAdmin();
            Sport toAdd = TryCreate(dto);
            try {
                sports.Add(toAdd);
            }
            catch (SportAlreadyExistsException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_ALREADY_EXISTS);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return dto;
        }

        private Sport TryCreate(SportDto dto)
        {
            Sport built;
            try {
                built = new Sport(dto.name, dto.isTwoTeams);
            }
            catch (InvalidSportDataException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
            return built;
        }

        public void DeleteSport(string name)
        {
            authenticator.AuthenticateAdmin();
            try {
                sports.Delete(name);
            }
            catch (SportNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }




    }
}
