using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.WebAPI.Models;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private ILoggerService logger;
        private IAuthenticationService authenticator;
        private ErrorActionResultFactory errors;

        public LogsController(ILoggerService logService, IAuthenticationService authService)
        {
            logger = logService;
            authenticator = authService;
            errors = new ErrorActionResultFactory(this);
        }

        [HttpGet]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult GetAll([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGet(from, to);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }

            return result;
        }

        private IActionResult TryGet(DateTime from, DateTime to)
        {
            ICollection<LogInfoDto> logs;
            if (from != new DateTime() && to != new DateTime())
            {
                logs = logger.GetAllLogs(from, to);
            }
            else if (from != new DateTime())
            {
                logs = logger.GetAllLogs(from, DateTime.Now);
            }
            else if (to != new DateTime())
            {
                logs = logger.GetAllLogs(DateTime.MinValue, to);
            }
            else {
                logs = logger.GetAllLogs();
            }
            IEnumerable<LogModelOut> output = logs.Select(l => new LogModelOut {
                Id = l.id,
                Date = l.date,
                LogType = l.logType,
                Message = l.message,
                Username = l.username
            });
            return Ok(output);
        }

        private void SetSession()
        {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            authenticator.SetSession(username);
        }
    }
}