using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.Services.Contracts;
using Microsoft.Extensions.Options;
using ObligatorioDA2.WebAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System;
using ObligatorioDA2.Services.Exceptions;
using Microsoft.AspNetCore.Authorization;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FixturesController : ControllerBase
    {
        private IFixtureService fixtureService;
        private IOptions<FixtureStrategies> fixtureConfig;
        private IAuthenticationService authenticator;
        private EncounterModelFactory factory;
        private ErrorActionResultFactory errors;

        public FixturesController(IFixtureService service, IOptions<FixtureStrategies> config,  IAuthenticationService authService) {
            fixtureService = service;
            fixtureConfig = config;
            authenticator = authService;
            factory = new EncounterModelFactory();
            errors = new ErrorActionResultFactory(this);
        }

        [HttpGet]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult GetFixtureAlgorithms()
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetAlgorithms();
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGetAlgorithms()
        {
            string algorithmsPath = fixtureConfig.Value.DllPath;
            ICollection<Type> algorithms = fixtureService.GetAlgorithms(algorithmsPath);
            ICollection<string> toReturn = algorithms.Select(t => t.ToString()).ToList();
            return Ok(toReturn);
        }

        [HttpPost("{sportName}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult CreateFixture(string sportName, FixtureModelIn input)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = TryCreateFixture(sportName, input);
            }
            else
            {
                //we call the function anyways, because we want the attempt to be logged.
                TryCreateFixture(sportName, input);
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult TryCreateFixture(string sportName, FixtureModelIn input)
        {
            IActionResult result;
            try
            {
                SetSession();
                FixtureDto fixture = BuildDto(input);
                fixtureService.SetFixtureAlgorithm(fixture, fixtureConfig.Value.DllPath);
                result = TryCreate(input, sportName);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryCreate(FixtureModelIn input, string sportName)
        {
            IActionResult result;
            ICollection<EncounterDto> added = fixtureService.AddFixture(sportName);
            ICollection<EncounterModelOut> addedModelOut = new List<EncounterModelOut>();
            foreach (EncounterDto match in added)
            {
                addedModelOut.Add(factory.CreateModelOut(match));
            }
            result = Created("fixture-generator", addedModelOut);
            return result;
        }

        private void SetSession()
        {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            authenticator.SetSession(username);
        }

        private FixtureDto BuildDto(FixtureModelIn modelIn) {
            return new FixtureDto()
            {
                fixtureName = modelIn.FixtureName,
                initialDate = modelIn.InitialDate,
                daysBetweenRounds = modelIn.DaysBetweenRounds,
                roundLength = modelIn.RoundLength
            };
        }
    }
}
