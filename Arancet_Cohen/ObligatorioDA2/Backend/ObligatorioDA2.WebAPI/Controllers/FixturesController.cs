﻿using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.Services.Interfaces;
using Microsoft.Extensions.Options;
using ObligatorioDA2.Data.Repositories.Interfaces;
using ObligatorioDA2.WebAPI.Models;
using ObligatorioDA2.BusinessLogic;
using System.Collections.Generic;
using System.Linq;
using System;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using System.Net;
using System.Reflection;
using System.IO;
using System.Text;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FixturesController : ControllerBase
    {
        private IFixtureService fixtureService;
        private IOptions<FixtureStrategies> fixtureConfig;
        private ISportRepository sports;
        private const string DLL_EXTENSION = "*.dll";

        public FixturesController(IFixtureService service, IOptions<FixtureStrategies> config, ISportRepository sportsRepo) {
            fixtureService = service;
            fixtureConfig = config;
            sports = sportsRepo;
        }

        [HttpGet]
        public IActionResult GetFixtureAlgorithms()
        {
            string algorithmsPath = fixtureConfig.Value.DllPath;        
            ICollection<Type> algorithms = fixtureService.GetAlgorithms(algorithmsPath);
            ICollection<string> toReturn = algorithms.Select(t => t.ToString()).ToList();
            return Ok(toReturn);
        }

        private string GetTypeString(Type t)
        {
            string fullName = t.ToString();
            string[] tokens = fullName.Split(".");
            return tokens[tokens.Length - 1];
        }

        [HttpPost("{sportName}")]
        public IActionResult CreateFixture(string sportName, FixtureModelIn input) {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = TryCreateFixture(sportName, input);
            }
            else
            {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult TryCreateFixture(string sportName, FixtureModelIn input)
        {
            IActionResult result;
            if (ValidDate(input))
            {
                result = CreateValid(input, sportName);
            }
            else
            {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = "Invalid date format" };
                result = BadRequest(error);
            }
            return result;
        }

        private bool ValidDate(FixtureModelIn input)
        {
            bool result = true;
            try
            {
                DateTime date = new DateTime(input.Year, input.Month, input.Day);
                result = date != null;
            }
            catch (ArgumentOutOfRangeException exp)
            {
                result = false;
            }
            return result;
        }

        private IActionResult CreateValid(FixtureModelIn input, string sportName)
        {
            IActionResult result;
            try
            {
                fixtureService.FixtureAlgorithm = BuildFixtureAlgorithm(new DateTime(input.Year, input.Month, input.Day), input.FixtureName);
                result = TryCreate(input, sportName);
            }
            catch (WrongFixtureException e)
            {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
                result = BadRequest(error);
            }
            catch (EntityNotFoundException e)
            {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
                result = NotFound(error);
            }
            catch (DataInaccessibleException e)
            {
                result = NoDataAccess(e);
            }

            return result;
        }
        private IFixtureGenerator BuildFixtureAlgorithm(DateTime date, string fixtureName)
        {
            string algorithmsPath = fixtureConfig.Value.DllPath;
            Type algortihmType = GetAlgorithmType(algorithmsPath, fixtureName);
            object fromDll = Activator.CreateInstance(algortihmType, new object[] { date, 1, 7 });
            IFixtureGenerator algorithm = fromDll as IFixtureGenerator;
            return algorithm;
        }

        private Type GetAlgorithmType(string algorithmsPath, string fixtureName)
        {
            bool found = false;
            string[] files = Directory.GetFiles(algorithmsPath, DLL_EXTENSION);
            Type first2comply = null;

            for (int i=0; i< files.Length && !found; i++) {
                Assembly actual =Assembly.LoadFrom(files[i]);
                first2comply = actual.GetType(fixtureName);
                if (first2comply != null) {
                    found = true;
                }
            }

            if (first2comply == null) {
                throw new WrongFixtureException("Fixture not found");
            }
            return first2comply;
        }

        private IActionResult TryCreate(FixtureModelIn input, string sportName)
        {
            IActionResult result;
            Sport sport = sports.Get(sportName);
            ICollection<Encounter> added = fixtureService.AddFixture(sport);
            ICollection<MatchModelOut> addedModelOut = new List<MatchModelOut>();
            foreach (Encounter match in added)
            {
                addedModelOut.Add(new MatchModelOut()
                {
                    Id = match.Id,
                    TeamsIds = match.GetParticipants().Select(p => p.Id).ToList(),
                    SportName = match.Sport.Name,
                    Date = match.Date,
                    CommentsIds = match.GetAllCommentaries().Select(c => c.Id).ToList()
                });
            }
            result = Created("fixture-generator", addedModelOut);
            return result;
        }

        private IActionResult NoDataAccess(DataInaccessibleException e)
        {
            ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
            IActionResult internalError = StatusCode((int)HttpStatusCode.InternalServerError, error);
            return internalError;
        }
    }
}
