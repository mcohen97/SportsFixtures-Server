using System;
using System.Collections.Generic;
using System.Linq;
using ObligatorioDA2.Data.Repositories.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.WebAPI.Models;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private ISportService sports;
        private ITeamService teams;
        private IFixtureService fixture;
        private ISportTableService tableService;
        private IAuthenticationService authenticator;
        private IImageService images;
        private ErrorActionResultFactory errors;

        public SportsController(ISportService sportRepo, ITeamService teamRepo, 
            IFixtureService fixtureService,ISportTableService tableGenerator, 
            IAuthenticationService authService, IImageService imageService)
        {
            sports = sportRepo;
            fixture = fixtureService;
            teams = teamRepo;
            tableService = tableGenerator;
            images = imageService;
            authenticator = authService;
            errors = new ErrorActionResultFactory(this);
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
                SetSession();
                result = TryAddSport(modelIn);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryAddSport(SportModelIn modelIn)
        {
            SportDto data = BuildSportDto(modelIn);
            SportDto added = sports.AddSport(data);
            SportModelOut modelOut = CreateModelOut(added);
            IActionResult result = CreatedAtRoute("GetSportById",new {name = added.name },modelOut);
            return result;
        }

        private SportDto BuildSportDto(SportModelIn modelIn)
        {
            return new SportDto() { name = modelIn.Name, isTwoTeams = modelIn.IsTwoTeams };
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetAll();
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGetAll()
        {
            ICollection<SportDto> allOfThem = sports.GetAllSports();
            IEnumerable<SportModelOut> output = allOfThem.Select(s => CreateModelOut(s));
            return Ok(output);
        }

        [HttpGet("{name}", Name = "GetSportById")]
        [Authorize]
        public IActionResult Get(string name)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGet(name);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGet(string name)
        {
            SportDto retrieved = sports.GetSport(name);
            SportModelOut output = CreateModelOut(retrieved);
            return Ok(output);
        }

        [HttpDelete("{name}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Delete(string name)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryDelete(name);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryDelete(string name)
        {
            sports.DeleteSport(name);
            OkModelOut okMessage = new OkModelOut() { OkMessage = "The sport has been deleted successfully" };
            return Ok(okMessage);
        }


        [HttpGet("{name}/teams")]
        [Authorize]
        public IActionResult GetTeams(string name)
        {
            IActionResult result;
            try {
                SetSession();
                ICollection<TeamDto> sportTeams = teams.GetSportTeams(name);
                ICollection<TeamModelOut> output = sportTeams.Select(t => CreateModelOut(t)).ToList();
                result = Ok(output);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        [HttpGet("{sportName}/table")]
        [Authorize]
        public IActionResult CalculateSportTable(string sportName)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryCalculateTable(sportName);

            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryCalculateTable(string sportName)
        {
            ICollection<Tuple<TeamDto, int>> standings = tableService.GetScoreTable(sportName);
            IEnumerable<StandingModelOut> output = standings.Select(s => CreateStanding(s));
            return Ok(output);
        }

        private StandingModelOut CreateStanding(Tuple<TeamDto, int> standing)
        {
            TeamDto team = standing.Item1;
            return new StandingModelOut() { TeamId = team.id, Points = standing.Item2 };
        }
        private void SetSession()
        {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            authenticator.SetSession(username);
        }

        private SportModelOut CreateModelOut(SportDto aSport) {
            return new SportModelOut() { Name = aSport.name, IsTwoTeams = aSport.isTwoTeams };
        }

        private TeamModelOut CreateModelOut(TeamDto aTeam)
        {
            return new TeamModelOut()
            {
                Id = aTeam.id,
                SportName = aTeam.sportName,
                Name = aTeam.name,
                Photo = Convert.ToBase64String(images.ReadImage(aTeam.photo))
            };
        }
    }
}