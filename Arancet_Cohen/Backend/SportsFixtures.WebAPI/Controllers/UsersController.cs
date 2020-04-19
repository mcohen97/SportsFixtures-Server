using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;
using System;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private IUserService userService;
        private IAuthenticationService authenticator;
        private IImageService images;
        private ErrorActionResultFactory errors;


        public UsersController(IUserService aService,IAuthenticationService authenticationService, IImageService imageService) {
            userService = aService;
            images = imageService;
            authenticator = authenticationService;
            errors = new ErrorActionResultFactory(this);
        }

        [HttpGet]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Get()
        {
            IActionResult result;
            try {
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
            ICollection<UserDto> users = userService.GetAllUsers();
            ICollection<UserModelOut> output = users.Select(u => CreateModelOut(u)).ToList();
            return Ok(output);
        }

        [HttpGet("{username}", Name = "GetUserById")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Get(string username)
        {
            IActionResult result;
            try
            {
                SetSession();
                UserModelOut toReturn = TryGetUser(username);
                result = Ok(toReturn);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private UserModelOut TryGetUser(string username) {
            UserDto queried = userService.GetUser(username);
            UserModelOut toReturn = new UserModelOut
            {
                Name = queried.name,
                Surname = queried.surname,
                Username = queried.username,
                Email = queried.email,
                IsAdmin = queried.isAdmin
            };
            return toReturn;
        }

        [HttpPost]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Post([FromBody] UserModelIn user)
        {
            IActionResult toReturn;
            if (ModelState.IsValid)
            {
                toReturn = AddValidUser(user);
            }
            else
            {
                toReturn = BadRequest(ModelState);
            }
            return toReturn;
        }

        private IActionResult AddValidUser(UserModelIn user) {

            IActionResult result;
            try
            {
                SetSession();
                result = TryAddUser(user);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }

            return result;
        }

        private IActionResult TryAddUser(UserModelIn user)
        {

            UserDto toAdd = BuildUser(user);
            UserDto added =userService.AddUser(toAdd);
            UserModelOut modelOut = CreateModelOut(added);
            return CreatedAtRoute("GetUserById", new { username = toAdd.username }, modelOut);
        }

        [HttpPut("{username}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Put(string username, [FromBody] UpdateUserModelIn input)
        {
            IActionResult result;
            UserDto toModify = BuildUser(username, input);
            try
            {
                SetSession();
                UserDto modified =userService.ModifyUser(toModify);
                result = Ok(CreateModelOut(modified));
            }
            catch (ServiceException e)
            {
                if (e.Error.Equals(ErrorType.ENTITY_NOT_FOUND))
                {
                    UserDto added = userService.AddUser(toModify);
                    result = CreatedAtRoute("GetUserById", new { username = toModify.username }, CreateModelOut(added));
                }
                else
                {
                    result = errors.GenerateError(e);
                }
            }
            return result;
        }

        [HttpDelete("{username}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Delete(string username)
        {
            IActionResult result;
            try
            {
                SetSession();
                userService.DeleteUser(username);
                OkModelOut okMessage = new OkModelOut() { OkMessage = "The user has been deleted successfully" };
                result = Ok(okMessage);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }

            return result;
        }
        private UserDto BuildUser(string username,UpdateUserModelIn modelIn)
        {
            UserDto built = new UserDto()
            {
                name = modelIn.Name,
                surname = modelIn.Surname,
                username = username,
                password = modelIn.Password,
                email = modelIn.Email,
                isAdmin = modelIn.IsAdmin
            };
            return built;
        }

        private UserDto BuildUser(UserModelIn modelIn)
        {
            UserDto built = new UserDto()
            {
                name = modelIn.Name,
                surname = modelIn.Surname,
                username = modelIn.Username,
                password = modelIn.Password,
                email = modelIn.Email,
                isAdmin = modelIn.IsAdmin
            };
            return built;
        }

        [HttpPost, Route("followed-teams/{teamId}")]
        [Authorize]
        public IActionResult FollowTeam(int teamId)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryFollowTeam(teamId);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryFollowTeam(int teamId)
        {
           IActionResult result;
           string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
           userService.FollowTeam(username, teamId);
           OkModelOut okMessage = new OkModelOut() { OkMessage = "You now follow the team" };
           result = Ok(okMessage);

            return result;
        }

        [HttpDelete, Route("followed-teams/{teamId}")]
        [Authorize]
        public IActionResult UnFollowTeam(int teamId)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryUnFollow(teamId);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryUnFollow(int teamId)
        {
            IActionResult result;
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            userService.UnFollowTeam(username, teamId);
            OkModelOut okMessage = new OkModelOut() { OkMessage = "Team unfollowed succesfully" };
            result = Ok(okMessage);
            return result;
        }

        [HttpGet("{username}/followed-teams")]
        [Authorize]
        public IActionResult GetFollowedTeams(string username)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetFollowedTeams(username);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }
        private void SetSession()
        {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            authenticator.SetSession(username);
        }
        private IActionResult TryGetFollowedTeams(string username)
        {
            ICollection<TeamDto> followed = userService.GetUserTeams(username);
            ICollection<TeamModelOut> converted = followed.Select(t => CreateModelOut(t)).ToList();
            return Ok(converted);
        }

        private UserModelOut CreateModelOut(UserDto user)
        {
            UserModelOut built = new UserModelOut()
            {
                Username = user.username,
                Name = user.name,
                Surname = user.surname,
                Email = user.email,
                IsAdmin = user.isAdmin
            };
            return built;
        }

        private TeamModelOut CreateModelOut(TeamDto stored)
        {
            TeamModelOut built = new TeamModelOut()
            {
                Id = stored.id,
                Name = stored.name,
                SportName = stored.sportName,
                Photo = Convert.ToBase64String(images.ReadImage(stored.photo))
            };
            return built;
        }
    }
}
