using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.BusinessLogic.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Mappers;
using System.Collections.Generic;
using System.Linq;

namespace ObligatorioDA2.Services
{
    public class UserService : IUserService
    {
        private IUserRepository usersStorage;
        private ITeamRepository teamsStorage;
        private IAuthenticationService authenticator;
        private UserFactory factory;
        private UserDtoMapper userMapper;
        private TeamDtoMapper teamMapper;

        public UserService(IUserRepository usersRepository, ITeamRepository teamRepository, IAuthenticationService authService)
        {
            usersStorage = usersRepository;
            teamsStorage = teamRepository;
            authenticator = authService;
            factory = new UserFactory();
            userMapper = new UserDtoMapper();
            teamMapper = new TeamDtoMapper();
        }

        public UserDto AddUser(UserDto userData)
        {
            authenticator.AuthenticateAdmin();
            User toAdd= TryCreate(userData);
            TryAdd(toAdd);
            return userData;
        }

        private User TryCreate(UserDto userData)
        {
            User toCreate;
            try
            {
                toCreate = CreateUser(userData);
            }
            catch (InvalidUserDataException e)
            {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
            return toCreate;
        }

        private void TryAdd(User toAdd)
        {
            try
            {
                usersStorage.Add(toAdd);
            }
            catch (EntityAlreadyExistsException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_ALREADY_EXISTS);
            }
            catch (DataInaccessibleException e)
            {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        private User CreateUser(UserDto userData)
        {
            UserId identity = new UserId()
            {
                Name = userData.name,
                Surname = userData.surname,
                UserName = userData.username,
                Password = userData.password,
                Email = userData.email
            };

            User created = userData.isAdmin ? factory.CreateAdmin(identity) : factory.CreateFollower(identity);

            return created;
        }

        public void DeleteUser(string userName)
        {
            authenticator.AuthenticateAdmin();
            try
            {
                usersStorage.Delete(userName);
            }
            catch (EntityNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public UserDto GetUser(string username)
        {
            authenticator.AuthenticateAdmin();
            User stored = TryGetUser(username);
            return userMapper.ToDto(stored);
        }

        private User TryGetUser(string username) {
            try
            {
               return usersStorage.Get(username);
            }
            catch (EntityNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e)
            {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }

        public UserDto ModifyUser(UserDto toModify)
        {
            authenticator.AuthenticateAdmin();
            UserDto old = GetUser(toModify.username);
            User updated = TryUpdate(old, toModify);
            try
            {
                usersStorage.Modify(updated);
            }
            catch (EntityNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return userMapper.ToDto(updated);
       }

        private User TryUpdate(UserDto old, UserDto toModify)
        {
            User updated;
            try
            {
                updated = UpdateUserData(old, toModify);
            }
            catch (InvalidUserDataException e) {
                throw new ServiceException(e.Message, ErrorType.INVALID_DATA);
            }
            return updated;
        }

        private User UpdateUserData(UserDto old, UserDto toModify)
        {
            string newName = string.IsNullOrWhiteSpace(toModify.name) ? old.name : toModify.name;
            string newSurname = string.IsNullOrWhiteSpace(toModify.surname) ? old.surname : toModify.surname;
            string newUsername = old.username;
            string newPassword = string.IsNullOrWhiteSpace(toModify.password) ? old.password : toModify.password;
            string newEmail = string.IsNullOrWhiteSpace(toModify.email) ? old.email : toModify.email;

            UserId data = new UserId() {
                Name = newName,Surname = newSurname,UserName = newUsername,Email = newEmail,Password = newPassword};
            User updated = toModify.isAdmin ? factory.CreateAdmin(data) : factory.CreateFollower(data);
            return updated;
        }

        public void FollowTeam(string userName, int idTeam)
        {
            authenticator.Authenticate();
            Team toFollow = TryGetTeam(idTeam);
            try
            {
                TryFollowTeam(userName, toFollow);
            }
            catch (InvalidUserDataException)
            {
                throw new TeamAlreadyFollowedException();
            }
        }

        private void TryFollowTeam(string username, Team toFollow)
        {
            User follower = TryGetUser(username);
            follower.AddFavourite(toFollow);
            usersStorage.Modify(follower);
        }

        public void UnFollowTeam(string userName, int idTeam)
        {
            authenticator.Authenticate();
            Team toUnfollow = TryGetTeam(idTeam);
            
            TryUnFollow(userName, toUnfollow);
        }

        private Team TryGetTeam(int idTeam)
        {
            Team stored;
            try
            {
                stored = teamsStorage.Get(idTeam);
            }
            catch (TeamNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
            return stored;
        }


        private void TryUnFollow(string username, Team fake)
        {
            User follower = TryGetUser(username);
            try
            {
                follower.RemoveFavouriteTeam(fake);
            }
            catch (InvalidUserDataException e) {
                throw new TeamNotFollowedException();
            }
            usersStorage.Modify(follower);
        }


        public ICollection<TeamDto> GetUserTeams(string userName)
        {
            authenticator.Authenticate();
            ICollection<Team> userTeams;
            try
            {
                User fetched = usersStorage.Get(userName);
                userTeams = fetched.GetFavouriteTeams();
            }
            catch (EntityNotFoundException e) {
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }catch(DataInaccessibleException e)
            {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return userTeams
                .Select(ut => teamMapper.ToDto(ut))
                .ToList();
        }

        public ICollection<UserDto> GetAllUsers()
        {
            authenticator.AuthenticateAdmin();
            ICollection<User> allOfThem;
            try
            {
                allOfThem = usersStorage.GetAll();
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return allOfThem
                .Select(u => userMapper.ToDto(u))
                .ToList();
        }

    }
}
