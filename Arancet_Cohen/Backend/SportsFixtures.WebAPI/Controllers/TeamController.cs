using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.WebAPI.Models;
using System.Text;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;
using System;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeamsController : ControllerBase
    {
        private ITeamService teamService;
        private IImageService images;
        private IAuthenticationService authenticator;
        private ErrorActionResultFactory errors;
        private const string IMG_EXTENSION = ".jpg";

        public TeamsController(ITeamService aService, IImageService imageManager, IAuthenticationService authService)
        {
            teamService = aService;
            errors = new ErrorActionResultFactory(this);
            images = imageManager;
            authenticator = authService;

        }

        [HttpGet]
        [Authorize]
        public IActionResult Get()
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGet();
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGet()
        {
            ICollection<TeamDto> allOfThem = teamService.GetAllTeams();
            ICollection<TeamModelOut> conversion = allOfThem.Select(t => BuildTeamModelOut(t)).ToList();
            return Ok(conversion);
        }

        [HttpGet("{id}", Name = "GetTeamById")]
        [Authorize]
        public IActionResult Get(int id)
        {
            IActionResult result;
            try
            {
                SetSession();
                TeamDto fetched = teamService.GetTeam(id);
                TeamModelOut output = BuildTeamModelOut(fetched);
                result = Ok(output);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }

            return result;
        }

        [HttpPost]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Post([FromBody] TeamModelIn team)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                SetSession();
                result = AddValidTeam(team);
            }
            else
            {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult AddValidTeam(TeamModelIn team)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryAddTeam(team);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        private IActionResult TryAddTeam(TeamModelIn team)
        {
            string imgData = team.Photo;
            team.Photo = team.Name + "_" + team.SportName + IMG_EXTENSION;

            TeamDto dto = BuildTransferObject(team);
            TeamDto added =teamService.AddTeam(dto);
            //if team could be added without exception thrown, then save its image.
            images.SaveImage(team.Photo, imgData);

            TeamModelOut modelOut = BuildTeamModelOut(added);
            return CreatedAtRoute("GetTeamById",new {id =added.id } ,modelOut);
        }

        private TeamDto BuildTransferObject(TeamModelIn team)
        {
            return new TeamDto() { id = team.Id, name = team.Name, photo = team.Photo, sportName = team.SportName };
        }

        [HttpPut("{teamId}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Put(int teamId, [FromBody] TeamModelIn value)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = PutValidFormat(teamId, value);
            }
            else
            {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult PutValidFormat(int teamId, TeamModelIn team)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryPut(teamId, team);
            }
            catch (ServiceException e)
            {
                if (e.Error.Equals(ErrorType.ENTITY_NOT_FOUND))
                {
                    result = Post(team);
                }
                else {
                    result = errors.GenerateError(e);
                }
            }

            return result;
        }

        private IActionResult TryPut(int teamId, TeamModelIn team)
        {
            string imgData = team.Photo;
            team.Id = teamId;

            TeamDto dto = BuildTransferObject(team);
            dto.photo = team.Name + "_" + team.SportName + IMG_EXTENSION;

            TeamDto modified = teamService.Modify(dto);
            //if team could be added without exception thrown, then save its image.
            images.SaveImage(team.Photo, imgData);

            TeamModelOut output = BuildTeamModelOut(modified);
            return Ok(output);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Delete(int id)
        {
            IActionResult result;
            try
            {
                SetSession();
                teamService.DeleteTeam(id);
                OkModelOut message = new OkModelOut { OkMessage = "The team was deleted succesfully" };
                result = Ok(message);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }

            return result;
        }

        private TeamModelOut BuildTeamModelOut(TeamDto toReturn)
        {
            TeamModelOut output = new TeamModelOut()
            {
                Id = toReturn.id,
                SportName = toReturn.sportName,
                Name = toReturn.name,
                Photo = Convert.ToBase64String(images.ReadImage(toReturn.photo))
            };
            return output;
        }

        private void SetSession() {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            authenticator.SetSession(username);
        }
    }
}