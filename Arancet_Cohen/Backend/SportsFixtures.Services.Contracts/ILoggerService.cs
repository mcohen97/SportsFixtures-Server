using ObligatorioDA2.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;

namespace ObligatorioDA2.Services.Contracts
{
    public interface ILoggerService
    {
        int Log(string logType, string messagge, string username, DateTime date);
        LogInfoDto GetLog(int id);
        bool Exists(int id);
        void Delete(int id);
        ICollection<LogInfoDto> GetAllLogs();
        ICollection<LogInfoDto> GetAllLogs(DateTime from, DateTime to);
    }
}
