using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.WebAPI.Controllers;
using ObligatorioDA2.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class LogsControllerTest
    {
        private ILoggerService logger;
        private LogsController controller;
        private ICollection<LogInfoDto> someLogList;

        [TestInitialize]
        public void Initialize()
        {
            logger = Mock.Of<ILoggerService>();
            Mock<IAuthenticationService> auth = new Mock<IAuthenticationService>();
            controller = new LogsController(logger, auth.Object);
            controller.ControllerContext = GetFakeControllerContext();
            string someUsername = "SomePepitoFulanito";
            someLogList = new List<LogInfoDto>()
            {
                new LogInfoDto()
                {
                    id = 1,
                    date = new DateTime(2016, 02, 20),
                    logType = LogType.LOGIN,
                    message = LogMessage.LOGIN_OK,
                    username = someUsername
                },
                new LogInfoDto()
                {
                    id = 2,
                    date = new DateTime(2017, 05, 13),
                    logType = LogType.LOGIN,
                    message = LogMessage.LOGIN_USER_NOT_FOUND,
                    username = someUsername+"Roberto"
                },
                new LogInfoDto()
                {
                    id = 3,
                    date = new DateTime(2018, 01, 01),
                    logType = LogType.LOGIN,
                    message = LogMessage.LOGIN_OK,
                    username = someUsername
                }
            };
        }

        [TestMethod]
        public void GetAllLogsTest()
        {
            //Arrange.
            Mock.Get(logger).Setup(l => l.GetAllLogs()).Returns(someLogList);
            
            //Act.
            IActionResult result = controller.GetAll(new DateTime(), new DateTime());
            OkObjectResult okResult = result as OkObjectResult;
            IEnumerable<LogModelOut> logs = okResult.Value as IEnumerable<LogModelOut>;
            LogModelOut first = logs.First(l => l.Id == 1);

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(logs);
            Mock.Get(logger).Verify(s => s.GetAllLogs(), Times.Once);
            Assert.AreEqual(someLogList.Count, logs.ToList().Count);
            Assert.AreEqual(LogType.LOGIN,first.LogType);
            Assert.AreEqual(LogMessage.LOGIN_OK,first.Message);
            Assert.AreEqual("SomePepitoFulanito",first.Username);
            Assert.AreEqual(new DateTime(2016, 02, 20),first.Date);
        }

        [TestMethod]
        public void GetAllLogsFromTest()
        {
            //Arrange.
            Mock.Get(logger).Setup(l => l.GetAllLogs(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(someLogList);

            //Act.
            IActionResult result = controller.GetAll(new DateTime(2017, 04, 13), new DateTime());
            OkObjectResult okResult = result as OkObjectResult;
            IEnumerable<LogModelOut> logs = okResult.Value as IEnumerable<LogModelOut>;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(logs);
            Mock.Get(logger).Verify(s => s.GetAllLogs(new DateTime(2017, 04, 13), It.Is<DateTime>(d => d >=DateTime.Today)), Times.Once);
        }

        [TestMethod]
        public void GetAllLogsToTest()
        {
            //Arrange.
            Mock.Get(logger).Setup(l => l.GetAllLogs(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(someLogList);

            //Act.
            IActionResult result = controller.GetAll(new DateTime(), new DateTime(2017, 06, 13));
            OkObjectResult okResult = result as OkObjectResult;
            IEnumerable<LogModelOut> logs = okResult.Value as IEnumerable<LogModelOut>;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(logs);
            Mock.Get(logger).Verify(s => s.GetAllLogs(DateTime.MinValue, new DateTime(2017, 06, 13)), Times.Once);
        }

        [TestMethod]
        public void GetAllLogsBetweenDatesTest()
        {
            //Arrange.
            Mock.Get(logger).Setup(l => l.GetAllLogs(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(someLogList);

            //Act.
            IActionResult result = controller.GetAll(new DateTime(2017, 04, 13), new DateTime(2017, 06, 13));
            OkObjectResult okResult = result as OkObjectResult;
            IEnumerable<LogModelOut> logs = okResult.Value as IEnumerable<LogModelOut>;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(logs);
            Mock.Get(logger).Verify(s => s.GetAllLogs(new DateTime(2017, 04, 13), new DateTime(2017, 06, 13)), Times.Once);
        }

        [TestMethod]
        public void GetLogsNoDataAccessTest()
        {
            //Arrange.
            Exception internalEx = new DataInaccessibleException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.DATA_INACCESSIBLE);
            Mock.Get(logger).Setup(l => l.GetAllLogs()).Throws(toThrow);

            //Act.
            IActionResult result = controller.GetAll(new DateTime(), new DateTime());
            ObjectResult noData = result as ObjectResult;
            ErrorModelOut error = noData.Value as ErrorModelOut;

            //Assert.
            Mock.Get(logger).Verify(s => s.GetAllLogs(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(noData);
            Assert.AreEqual(500, noData.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        private ControllerContext GetFakeControllerContext()
        {
            ICollection<Claim> fakeClaims = new List<Claim>() { new Claim("Username", "username") };

            Mock<ClaimsPrincipal> cp = new Mock<ClaimsPrincipal>();
            cp.Setup(m => m.Claims).Returns(fakeClaims);
            Mock<HttpContext> contextMock = new Mock<HttpContext>();
            contextMock.Setup(ctx => ctx.User).Returns(cp.Object);

            Mock<ControllerContext> controllerContextMock = new Mock<ControllerContext>();
            controllerContextMock.Object.HttpContext = contextMock.Object;
            return controllerContextMock.Object;
        }
    }
}
