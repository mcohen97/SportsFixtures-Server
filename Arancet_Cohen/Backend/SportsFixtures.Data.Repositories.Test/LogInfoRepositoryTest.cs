using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Data.DataAccess;
using ObligatorioDA2.Data.Repositories.Contracts;
using ObligatorioDA2.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace ObligatorioDA2.Data.Repositories.Test
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LogInfoRepositoryTest
    {
        LogInfo log1;
        LogInfo log2;
        ILogInfoRepository repo;

        [TestInitialize]
        public void Initialize()
        {
            LoggingContext context = CreateContext();
            CreateLogs();
            ClearDataBase(context);
        }

        private void CreateLogs()
        {
            log1 = new LogInfo()
            {
                Id = 0,
                Date = DateTime.Now,
                LogType = LogType.FIXTURE,
                Message = "HomeAway fixture generated",
                Username = "SomeUser"
            };
            log2 = new LogInfo()
            {
                Id = 0,
                Date = DateTime.Now,
                LogType = LogType.LOGIN,
                Message = "Logged using API",
                Username = "SomeUser"
            };
        }

        private LoggingContext CreateContext()
        {
            DbContextOptions<LoggingContext> options = new DbContextOptionsBuilder<LoggingContext>()
                            .UseInMemoryDatabase(databaseName: "LoggingRepository")
                            .Options;
            LoggingContext context = new LoggingContext(options);
            repo = new LogInfoRepository(context);
            return context;
        }

        private void CreateDisconnectedDatabase()
        {
            DbContextOptions<LoggingContext> options = new DbContextOptionsBuilder<LoggingContext>()
                .UseInMemoryDatabase(databaseName: "UserRepositoryTest")
                .Options;
            Mock<LoggingContext> contextMock = new Mock<LoggingContext>(options);
            Mock<DbException> toThrow = new Mock<DbException>();
            contextMock.Setup(c => c.Logs).Throws(toThrow.Object);
            repo = new LogInfoRepository(contextMock.Object);
        }

        private void ClearDataBase(LoggingContext context)
        {
            foreach (LogInfoEntity log in context.Logs)
            {
                context.Logs.Remove(log);
            }
            context.SaveChanges();
        }

        [TestMethod]
        public void NoLogsTest()
        {
            bool noLogs = repo.IsEmpty();
            Assert.IsTrue(noLogs);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void IsEmptyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.IsEmpty();
        }

        [TestMethod]
        public void AddLogTest()
        {
            repo.Add(log1);
            Assert.AreEqual(1, repo.GetAll().Count);
        }

        [TestMethod]
        [ExpectedException(typeof(LogAlreadyExistsException))]
        public void AddAlreadyExistentLogTest()
        {
            log1 = repo.Add(log1);
            repo.Add(log1);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void AddLogNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.Add(log1);
        }

        [TestMethod]
        public void GetLogTest()
        {
            log1 = repo.Add(log1);
            LogInfo logRetrieved = repo.Get(log1.Id);
            Assert.AreEqual(log1, logRetrieved);
        }

        [TestMethod]
        [ExpectedException(typeof(LogNotFoundException))]
        public void GetNotExistentTeamTest()
        {
            LogInfo logInDB = repo.Get(1);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetSportNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.Get(1);
        }

        [TestMethod]
        public void ExistsTeamTest()
        {
            log1 = repo.Add(log1);
            bool result = repo.Exists(log1.Id);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DoesNotExistTest()
        {

            repo.Add(log1);
            bool result = repo.Exists(log2.Id);
            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ExistsNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.Exists(log1.Id);
        }

        [TestMethod]
        public void DeleteTest()
        {
            log1 = repo.Add(log1);
            repo.Delete(log1.Id);
            Assert.IsTrue(repo.IsEmpty());
        }


        [TestMethod]
        [ExpectedException(typeof(LogNotFoundException))]
        public void DeleteNotExistentTest()
        {
            repo.Add(log1);
            repo.Delete(log2.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void DeleteNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.Delete(log1.Id);
        }

        [TestMethod]
        public void ModifySportTest()
        {
            log1 = repo.Add(log1);
            log1.Message = "New log info";
            CreateContext();
            repo.Modify(log1);
            LogInfo editedLog = repo.Get(log1.Id);
            Assert.AreEqual(log1, editedLog);
        }

        [TestMethod]
        [ExpectedException(typeof(LogNotFoundException))]
        public void ModifyNotExistentTest()
        {
            repo.Modify(log1);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ModifyNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.Modify(log1);
        }

        [TestMethod]
        public void ClearTest()
        {
            repo.Add(log1);
            repo.Add(log2);
            repo.Clear();
            Assert.IsTrue(repo.IsEmpty());
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void ClearNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.Clear();
        }

        [TestMethod]
        public void GetAllTest()
        {
            repo.Add(log1);
            repo.Add(log2);
            ICollection<LogInfo> logs = repo.GetAll();
            Assert.AreEqual(2, logs.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(DataInaccessibleException))]
        public void GetAllNoAccessTest()
        {
            CreateDisconnectedDatabase();
            repo.GetAll();
        }
    }
}

