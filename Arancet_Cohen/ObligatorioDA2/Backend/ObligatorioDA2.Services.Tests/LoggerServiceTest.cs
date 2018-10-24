﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.Repositories.Interfaces;
using ObligatorioDA2.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObligatorioDA2.Services.Tests
{
    [TestClass]
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
                Messagge = "Logged using API",
                Username = "SomeUsername"
            };
            logRepo = new Mock<ILogInfoRepository>().Object;
            Mock.Get(logRepo).Setup(l => l.Get(aLog.Id)).Throws(new LogNotFoundException());
            Mock.Get(logRepo).Setup(l => l.GetAll()).Returns(new List<LogInfo>());
            Mock.Get(logRepo).Setup(l => l.Exists(aLog.Id)).Returns(false);
            Mock.Get(logRepo).Setup(l => l.Delete(aLog.Id)).Callback(DeleteALog);
            Mock.Get(logRepo).Setup(l => l.Add(It.IsAny<LogInfo>())).Returns(aLog).Callback(AddALog);
            logger = new LoggerService(logRepo);
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
            aLog.Id = logger.Log(aLog.LogType, aLog.Messagge, aLog.Username, aLog.Date);
            Assert.IsTrue(logger.Exists(aLog.Id));
        }

        [TestMethod]
        public void NonExistentTest()
        {
            Assert.IsFalse(logger.Exists(aLog.Id));
        }

        [TestMethod]
        public void GetLogTest()
        {
            int id = logger.Log(aLog.LogType, aLog.Messagge, aLog.Username, aLog.Date);
            LogInfo logRetrieved = logger.GetLog(id);
            Assert.AreEqual(aLog.LogType, logRetrieved.LogType);
            Assert.AreEqual(aLog.Messagge, logRetrieved.Messagge);
            Assert.AreEqual(aLog.Date, logRetrieved.Date);
            Assert.AreEqual(aLog.Username, logRetrieved.Username);
        }

        [TestMethod]
        public void GetAllTest()
        {
            logger.Log(aLog.LogType, aLog.Messagge, aLog.Username, aLog.Date);
            ICollection<LogInfo> logs = logger.GetAllLogs();
            Assert.AreEqual(1, logs.Count);
        }

        [TestMethod]
        public void DeleteLogTest()
        {
            int id = logger.Log(aLog.LogType, aLog.Messagge, aLog.Username, aLog.Date);
            logger.Delete(id);
            Assert.IsFalse(logger.Exists(id));
        }

        [TestMethod]
        [ExpectedException(typeof(LogNotFoundException))]
        public void GetDeletedTest()
        {
            int id = logger.Log(aLog.LogType, aLog.Messagge, aLog.Username, aLog.Date);
            logger.Delete(id);
            logger.GetLog(id);
        }
    }
}
