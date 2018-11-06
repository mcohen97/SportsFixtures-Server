﻿using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.BusinessLogic.Exceptions;
using ObligatorioDA2.Data.Repositories.Interfaces;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Interfaces;
using ObligatorioDA2.Services.Interfaces.Dtos;
using ObligatorioDA2.Services.Mappers;

namespace ObligatorioDA2.Services
{
    public class TeamService: ITeamService
    {
        private ISportRepository sports;
        private ITeamRepository teams;
        private IAuthenticationService authentication;
        private TeamDtoMapper mapper;

        public TeamService(ISportRepository sportsRepo, ITeamRepository teamsRepo, IAuthenticationService authService)
        {
            sports = sportsRepo;
            teams = teamsRepo;
            authentication = authService;
            mapper = new TeamDtoMapper();
        }

        public TeamDto AddTeam(TeamDto team)
        {
            AuthenticateAdmin();
            Team added= TryAddTeam(team);
            return mapper.ToDto(added);
        }

        private Team TryAddTeam(TeamDto team)
        {
            Sport sport = GetSport(team.sportName);
            Team toAdd = CreateTeam(team, sport);
            Team added;
            try
            {
                added = teams.Add(toAdd);
            }
            catch (TeamAlreadyExistsException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_ALREADY_EXISTS);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return added;
        }

        private Sport GetSport(string sportName)
        {
            Sport stored;
            try {
                stored = sports.Get(sportName);
            }
            catch (SportNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            return stored;
        }

        private Team CreateTeam(TeamDto team, Sport sport)
        {
            Team created;
            try
            {
                created = new Team(team.name, team.photo, sport);
            }
            catch (InvalidTeamDataException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
            return created;
        }

        public TeamDto Modify(TeamDto testDto)
        {
            AuthenticateAdmin();
            Team old = TryGetTeam(testDto.id);
            Team updated = Update(old, testDto);
            teams.Modify(updated);
            return mapper.ToDto(updated);
        }

        private Team Update(Team old, TeamDto testDto)
        {
            int id = old.Id;
            string name = string.IsNullOrWhiteSpace(testDto.name) ? old.Name : testDto.name;
            string photoPath = string.IsNullOrWhiteSpace(testDto.photo) ? old.PhotoPath : testDto.photo;
            return new Team(id, name, photoPath, old.Sport);
        }

        public TeamDto GetTeam(int id)
        {
            Authenticate();
            Team stored = TryGetTeam(id);
            return mapper.ToDto(stored);
        }

        private Team TryGetTeam(int id)
        {
            Team toReturn;
            try
            {
                toReturn = teams.Get(id);
            }
            catch (TeamNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return toReturn;
        }

        public void DeleteTeam(int id)
        {
            AuthenticateAdmin();
            try {
                teams.Delete(id);
            }
            catch (TeamNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public ICollection<TeamDto> GetAllTeams()
        {
            Authenticate();
            ICollection<TeamDto> allOfThem;
            try
            {
                allOfThem = teams.GetAll()
                    .Select(t => mapper.ToDto(t))
                    .ToList();
            }catch(DataInaccessibleException e)
            {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return allOfThem;
        }

        public ICollection<TeamDto> GetSportTeams(string name)
        {
            Authenticate();
            ICollection<Team> sportTeams;
            try
            {
                sportTeams = teams.GetTeams(name);
            }
            catch (SportNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return sportTeams
                .Select(t => mapper.ToDto(t))
                .ToList();
        }

        private void AuthenticateAdmin() {
            if (!authentication.IsLoggedIn())
            {
                throw new NotAuthenticatedException();
            }

            if (!authentication.HasAdminPermissions())
            {
                throw new NoPermissionsException();
            }
        }

        private void Authenticate() {
            if (!authentication.IsLoggedIn())
            {
                throw new NotAuthenticatedException();
            }
        }
    }
}
