using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Services.Logging;
using System;

namespace ObligatorioDA2.Services.Logging
{
    public class LogInfoMapper
    {
        public LogInfoEntity ToEntity(LogInfo log)
        {
            return new LogInfoEntity()
            {
                Id = log.Id,
                Messagge = log.Message,
                Date = log.Date,
                LogType = log.LogType,
                Username = log.Username
            };
        }

        public LogInfo ToLogInfo(LogInfoEntity entity)
        {
            return new LogInfo()
            {
                Id = entity.Id,
                Message = entity.Messagge,
                Date = entity.Date,
                LogType = entity.LogType,
                Username = entity.Username
            };
        }
    }
}