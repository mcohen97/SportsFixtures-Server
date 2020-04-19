using System;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.Services
{
    public class LoggerService : ILoggerService
    {
        private ILogInfoRepository logRepo;
        private IAuthenticationService authenticator;

        public LoggerService(ILogInfoRepository logRepo, IAuthenticationService authService)
        {
            this.logRepo = logRepo;
            authenticator = authService;
        }

        public void Delete(int id)
        {
            authenticator.AuthenticateAdmin();
            logRepo.Delete(id);
        }

        public bool Exists(int id)
        {
            authenticator.AuthenticateAdmin();
            return logRepo.Exists(id);
        }


        public ICollection<LogInfoDto> GetAllLogs(DateTime from, DateTime to)
        {
            return GetAllLogs().Where(l => l.date >= from && l.date <= to).ToList();
        }

        public ICollection<LogInfoDto> GetAllLogs()
        {
            authenticator.AuthenticateAdmin();
            return logRepo.GetAll()
                .Select(l => CreateDto(l))
                .ToList();
        }

        public LogInfoDto GetLog(int id)
        {
            authenticator.AuthenticateAdmin();
            LogInfo stored = logRepo.Get(id);
            return CreateDto(stored);
        }

        public int Log(string logType, string messagge, string username, DateTime date)
        {
            LogInfo newLog = new LogInfo()
            {
                Id = 0,
                LogType = logType,
                Message = messagge,
                Username = username,
                Date = date
            };
            try
            {
                newLog = logRepo.Add(newLog);
            }
            catch (DataInaccessibleException e) {
                throw new ServiceException(e.Message, ErrorType.DATA_INACCESSIBLE);
            }
            return newLog.Id;
        }

        private LogInfoDto CreateDto(LogInfo log) {
            LogInfoDto dto = new LogInfoDto()
            {
                id = log.Id,
                date = log.Date,
                logType = log.LogType,
                message = log.Message,
                username = log.Username
            };
            return dto;
        }
    }
}