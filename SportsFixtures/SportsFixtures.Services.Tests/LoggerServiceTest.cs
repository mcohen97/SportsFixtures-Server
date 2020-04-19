using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LoggerServiceTest
    {
        private ILoggerService logger;
        private ILogInfoRepository logRepo;
        private LogInfo aLog;

        [TestInitialize]
        public void Initialize()
        {
            aLog = new LogInfo()
            {
                Id = 1,
                Date = DateTime.Now,
                LogType = LogType.LOGIN,
                Message = "Logged using API",
                Username = "SomeUsername"
            };
            logRepo = new Mock<ILogInfoRepository>().Object;
            Mock.Get(logRepo).Setup(l => l.Get(aLog.Id)).Throws(new LogNotFoundException());
            Mock.Get(logRepo).Setup(l => l.GetAll()).Returns(new List<LogInfo>());
            Mock.Get(logRepo).Setup(l => l.Exists(aLog.Id)).Returns(false);
            Mock.Get(logRepo).Setup(l => l.Delete(aLog.Id)).Callback(DeleteALog);
            Mock.Get(logRepo).Setup(l => l.Add(It.IsAny<LogInfo>())).Returns(aLog).Callback(AddALog);
            Mock<IAuthenticationService> auth = new Mock<IAuthenticationService>();
            logger = new LoggerService(logRepo, auth.Object);
        }

        private void AddALog()
        {
            Mock.Get(logRepo).Setup(l => l.Get(aLog.Id)).Returns(aLog);
            Mock.Get(logRepo).Setup(l => l.GetAll()).Returns(new List<LogInfo>() { aLog });
            Mock.Get(logRepo).Setup(l => l.Exists(aLog.Id)).Returns(true);
            Mock.Get(logRepo).Setup(l => l.Delete(aLog.Id)).Callback(DeleteALog);
        }

        private void DeleteALog()
        {
            Mock.Get(logRepo).Setup(l => l.Get(aLog.Id)).Throws(new LogNotFoundException());
            Mock.Get(logRepo).Setup(l => l.GetAll()).Returns(new List<LogInfo>());
            Mock.Get(logRepo).Setup(l => l.Exists(aLog.Id)).Returns(false);
            Mock.Get(logRepo).Setup(l => l.Delete(aLog.Id)).Throws(new LogNotFoundException());
        }

        [TestMethod]
        public void AddLogTest()
        {
            aLog.Id = logger.Log(aLog.LogType, aLog.Message, aLog.Username, aLog.Date);
            Assert.IsTrue(logger.Exists(aLog.Id));
        }

        [TestMethod]
        [ExpectedException(typeof(ServiceException))]
        public void AddLogNoDataAccessTest() {
            Mock<ILogInfoRepository> fakeRepo = new Mock<ILogInfoRepository>();
            fakeRepo.Setup(r => r.Add(It.IsAny<LogInfo>())).Throws(new DataInaccessibleException());
            Mock<IAuthenticationService> auth = new Mock<IAuthenticationService>();
            logger = new LoggerService(fakeRepo.Object, auth.Object);
            logger.Log(aLog.LogType, aLog.Message, aLog.Username, aLog.Date);
        }

        [TestMethod]
        public void NonExistentTest()
        {
            Assert.IsFalse(logger.Exists(aLog.Id));
        }

        [TestMethod]
        public void GetLogTest()
        {
            int id = logger.Log(aLog.LogType, aLog.Message, aLog.Username, aLog.Date);
            LogInfoDto logRetrieved = logger.GetLog(id);
            Assert.AreEqual(aLog.LogType, logRetrieved.logType);
            Assert.AreEqual(aLog.Message, logRetrieved.message);
            Assert.AreEqual(aLog.Date, logRetrieved.date);
            Assert.AreEqual(aLog.Username, logRetrieved.username);
        }

        [TestMethod]
        public void GetAllTest()
        {
            logger.Log(aLog.LogType, aLog.Message, aLog.Username, aLog.Date);
            ICollection<LogInfoDto> logs = logger.GetAllLogs();
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void GetAllBetweenDatesTest() {
            Mock.Get(logRepo).Setup(l => l.GetAll()).Returns(GetFakeLogs());
            DateTime from = new DateTime(2000, 5, 27);
            DateTime to = new DateTime(2018, 1, 1);
            ICollection<LogInfoDto> logs = logger.GetAllLogs(from,to);
            Assert.AreEqual(2, logs.Count);
        }

        [TestMethod]
        public void DeleteLogTest()
        {
            int id = logger.Log(aLog.LogType, aLog.Message, aLog.Username, aLog.Date);
            logger.Delete(id);
            Assert.IsFalse(logger.Exists(id));
        }

        [TestMethod]
        [ExpectedException(typeof(LogNotFoundException))]
        public void GetDeletedTest()
        {
            int id = logger.Log(aLog.LogType, aLog.Message, aLog.Username, aLog.Date);
            logger.Delete(id);
            logger.GetLog(id);
        }

        public ICollection<LogInfo> GetFakeLogs() {
            LogInfo log1 = new LogInfo()
            {
                Id = 1,
                Date = new DateTime(1997,6,27),
                LogType = LogType.LOGIN,
                Message = "Logged using API",
                Username = "SomeUsername"
            };
            LogInfo log2 = new LogInfo()
            {
                Id = 2,
                Date = new DateTime(2001, 9, 11),
                LogType = LogType.LOGIN,
                Message = "Logged using API",
                Username = "SomeUsername"
            };
            LogInfo log3 = new LogInfo()
            {
                Id = 3,
                Date = new DateTime(2010, 5, 13),
                LogType = LogType.LOGIN,
                Message = "Logged using API",
                Username = "SomeUsername"
            };
            LogInfo log4 = new LogInfo()
            {
                Id = 4,
                Date = DateTime.Today,
                LogType = LogType.LOGIN,
                Message = "Logged using API",
                Username = "SomeUsername"
            };
            return new List<LogInfo>() { log1, log2, log3, log4 };
        }
    }
}
