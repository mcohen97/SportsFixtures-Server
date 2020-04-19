using System;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Mappers;

namespace ObligatorioDA2.Services
{
    public class AuthenticationService : IAuthenticationService, ILogInService
    {
        private IUserRepository users;
        private User current;
        private UserDtoMapper mapper;
        private ILogInfoRepository logger;

        public AuthenticationService(IUserRepository aRepo, ILogInfoRepository aLogger)
        {
            users = aRepo;
            logger = aLogger;
            mapper = new UserDtoMapper();
        }

        public UserDto Login(string aUsername, string aPassword)
        {
            User fetched = TryGetUser(aUsername);

            if (!aPassword.Equals(fetched.Password))
            {

                AddLog(LogType.LOGIN, LogMessage.LOGIN_WRONG_PASSWORD, aUsername, DateTime.Now);
                throw new WrongPasswordException();
            }
            AddLog(LogType.LOGIN, LogMessage.LOGIN_OK, aUsername, DateTime.Now);
            return mapper.ToDto(fetched);
        }

        private User TryGetUser(string aUsername)
        {
            try
            {
                return GetExistentUser(aUsername);
            }
            catch (UserNotFoundException e)
            {
                AddLog(LogType.LOGIN, LogMessage.LOGIN_USER_NOT_FOUND, aUsername, DateTime.Now);
                throw new ServiceException(e.Message, ErrorType.ENTITY_NOT_FOUND);
            }
        }

        private void AddLog(string logType, string message, string actor, DateTime date)
        {
            LogInfo log = new LogInfo()
            {
                LogType = logType,
                Message = message,
                Username = actor,
                Date = date
            };
            logger.Add(log);
        }

        public void SetSession(string userName)
        {
            try
            {
                current = GetExistentUser(userName);
            }
            catch (UserNotFoundException e)
            {
                throw new ServiceException(e.Message, ErrorType.NOT_AUTHENTICATED);
            }
        }

        private User GetExistentUser(string aUsername)
        {
            try
            {
                return users.Get(aUsername);
            }
            catch (DataInaccessibleException e)
            {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
        }
        public UserDto GetConnectedUser()
        {
            if (!IsLoggedIn()) {
                throw new NotAuthenticatedException();
            }
            return mapper.ToDto(current);
        }

        public void AuthenticateAdmin()
        {
            if (!IsLoggedIn())
            {
                throw new NotAuthenticatedException();
            }

            if (!HasAdminPermissions())
            {
                throw new NoPermissionsException();
            }
        }

        public void Authenticate()
        {
            if (!IsLoggedIn())
                throw new NotAuthenticatedException();
            
        }
        private bool HasAdminPermissions()
        {
            return current.IsAdmin;
        }

        private bool IsLoggedIn()
        {
            return (current != null);
        }

    }
}
