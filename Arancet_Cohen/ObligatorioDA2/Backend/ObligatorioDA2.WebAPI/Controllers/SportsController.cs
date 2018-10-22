using System;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.BusinessLogic;
using ObligatorioDA2.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.WebAPI.Models;
using ObligatorioDA2.Services.Interfaces;
using ObligatorioDA2.Services.Exceptions;
using System.Net;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private ISportRepository sports;
        private ITeamRepository teams;
        private IFixtureService fixture;


        public SportsController(ISportRepository sportRepo, ITeamRepository teamRepo, IFixtureService fixtureService)
        {
            sports = sportRepo;
            fixture = fixtureService;
            teams = teamRepo;
        }

        [HttpPost]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Post([FromBody]SportModelIn modelIn)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = CreateValidSport(modelIn);
            }
            else
            {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult CreateValidSport(SportModelIn modelIn)
        {
            IActionResult result;
            try {
                result = TryAddSport(modelIn);
            }
            catch (SportAlreadyExistsException e) {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
                result = BadRequest(error);
            }
            catch (DataInaccessibleException e) {
                result = NoDataAccess(e);
            }
            return result;
        }

        private IActionResult TryAddSport(SportModelIn modelIn)
        {
            Sport toAdd = new Sport(modelIn.Name);
            sports.Add(toAdd);
            SportModelOut modelOut = new SportModelOut(){Name = toAdd.Name};
            IActionResult result = CreatedAtRoute("GetSportById",new {name = toAdd.Name },modelOut);
            return result;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            IActionResult result;
            try
            {
                result = TryGetAll();
            }
            catch (DataInaccessibleException e) {
                result = NoDataAccess(e);
            }
            return result;
        }

        private IActionResult TryGetAll()
        {
            ICollection<Sport> allOfThem = sports.GetAll();
            IEnumerable<SportModelOut> output = allOfThem.Select(s => new SportModelOut { Name = s.Name });
            return Ok(output);
        }

        [HttpGet("{name}", Name = "GetSportById")]
        [Authorize]
        public IActionResult Get(string name)
        {

            IActionResult result;
            try
            {
                result = TryGet(name);
            }
            catch (SportNotFoundException e)
            {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
                result = NotFound(error);
            }
            catch (DataInaccessibleException e) {
                result = NoDataAccess(e);
            }
            return result;

        }

        private IActionResult TryGet(string name)
        {
            Sport retrieved = sports.Get(name);
            SportModelOut output = new SportModelOut() { Name = retrieved.Name };
            return Ok(output);
        }

        [HttpDelete("{name}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Delete(string name)
        {
            IActionResult result;
            try
            {
                result = TryDelete(name);
            }
            catch (SportNotFoundException e)
            {
                result = NotFound(e.Message);
            }
            catch (DataInaccessibleException e) {
                result = NoDataAccess(e);
            }
            return result;
        }

        private IActionResult TryDelete(string name)
        {
            sports.Delete(name);
            OkModelOut okMessage = new OkModelOut() { OkMessage = "Sport was deleted" };
            return Ok(okMessage);
        }

        [HttpPut("{name}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Put(string name, [FromBody] SportModelIn modelIn)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = ModifyOrAdd(name, modelIn);
            }
            else
            {
                result = BadRequest(ModelState);
            }
            return result;
        }


        private IActionResult ModifyOrAdd(string name, SportModelIn modelIn)
        {
            IActionResult result;
            Sport toAdd = new Sport(modelIn.Name);
            try
            {
                sports.Modify(toAdd);
                UserModelOut modelOut = new UserModelOut()
                {
                    Name = modelIn.Name
                };
                result = Ok(modelOut);
            }
            catch (SportNotFoundException)
            {
                sports.Add(toAdd);
                SportModelOut modelOut = new SportModelOut() { Name = toAdd.Name };
                result = CreatedAtRoute("GetSportById",new { name= toAdd.Name} ,modelOut);
            }
            catch (DataInaccessibleException e)
            {
                result = NoDataAccess(e);
            }
            return result;
        }


        [HttpGet("{name}/teams")]
        [Authorize]
        public IActionResult GetTeams(string name)
        {
            IActionResult result;
            try { 
                ICollection<Team> sportTeams = teams.GetTeams(name);
                ICollection<TeamModelOut> output = sportTeams.Select(t => CreateModelOut(t)).ToList();
                result = Ok(output);
            }
            catch (SportNotFoundException e) {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
                result = NotFound(error);
            }
            catch (DataInaccessibleException e)
            {
                result = NoDataAccess(e);
            }
            return result;
        }

        private TeamModelOut CreateModelOut(Team aTeam)
        {
            return new TeamModelOut()
            {
                Id = aTeam.Id,
                SportName = aTeam.Sport.Name,
                Name = aTeam.Name,
                Photo = aTeam.Photo
            };
        }

        [HttpPost("fixtures")]
        public IActionResult GetFixtureAlgorithms()
        {
            return Ok(new List<string>());
        }

        [HttpPost("{sportName}/OneMatchFixture")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult CreateOneMatchFixture(string sportName, [FromBody] FixtureModelIn input)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                DateTime initialDate;
                if (ValidDate(input))
                {
                    initialDate = new DateTime(input.Year, input.Month, input.Day);
                    fixture.FixtureAlgorithm = new OneMatchFixture(new DateTime(input.Year, input.Month, input.Day), 1, 7);
                    result = CreateValid(input, sportName, fixture);
                }
                else
                {
                    ErrorModelOut error = new ErrorModelOut() { ErrorMessage = "Invalid date format" };
                    result = BadRequest(error);
                }
                
            }
            else
            {
                result = BadRequest(ModelState);
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

        [HttpPost("{sportName}/HomeAwayFixture")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult CreateHomeAwayFixture(string sportName, FixtureModelIn input)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                DateTime initialDate;
                if (ValidDate(input))
                {
                    initialDate = new DateTime(input.Year, input.Month, input.Day);
                    fixture.FixtureAlgorithm = new HomeAwayFixture(new DateTime(input.Year, input.Month, input.Day), 1, 7);
                    result = CreateValid(input, sportName, fixture);
                }
                else
                {
                    ErrorModelOut error = new ErrorModelOut() { ErrorMessage = "Invalid date format" };
                    result = BadRequest(error);
                }

            }
            else
            {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult CreateValid(FixtureModelIn input, string sportName, IFixtureService fixture)
        {
            IActionResult result;
            try {
                result = TryCreate(input, sportName,fixture);
            }
            catch (WrongFixtureException e)
            {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
                result = BadRequest(error);
            }
            catch (TeamNotFoundException e)
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

        private IActionResult TryCreate(FixtureModelIn input, string sportName, IFixtureService fixture)
        {
            IActionResult result;
            Sport sport = sports.Get(sportName);
            ICollection<Match> added = fixture.AddFixture(sport);
            ICollection<MatchModelOut> addedModelOut = new List<MatchModelOut>();
            foreach (Match match in added)
            {
                addedModelOut.Add(new MatchModelOut()
                {
                    Id = match.Id,
                    AwayTeamId = match.AwayTeam.Id,
                    HomeTeamId = match.HomeTeam.Id,
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