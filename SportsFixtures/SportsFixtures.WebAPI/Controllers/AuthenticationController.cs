using System;
using System.Text;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.WebAPI.Models;
using System.Net;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private ILogInService loginService;

        public AuthenticationController(ILogInService aService)
        {
            loginService = aService;
        }

        [HttpPost]
        public IActionResult Authenticate([FromBody]LoginModelIn user)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = AuthenticateWithValidModel(user);
            }
            else {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult AuthenticateWithValidModel(LoginModelIn user)
        {
            IActionResult result;
            try
            {
                UserDto logged = loginService.Login(user.Username, user.Password);
                string tokenString = GenerateJSONWebToken(logged);
                result = Ok(new { Token = tokenString });
            }
            catch (WrongPasswordException e2)
            {
                ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e2.Message };
                result = BadRequest(error);
            }
            catch (ServiceException e)
            {
                result = GenerateResponse(e);
            }

            return result;
        }

        private IActionResult GenerateResponse(ServiceException e)
        {
            IActionResult errorResult;
            ErrorModelOut error = new ErrorModelOut() { ErrorMessage = e.Message };
            if (e.Error.Equals(ErrorType.ENTITY_NOT_FOUND))
            {
                errorResult = BadRequest(error);
            }
            else
            {
                errorResult = StatusCode((int)HttpStatusCode.InternalServerError, error);
            }
            return errorResult;
        }

        private string GenerateJSONWebToken(UserDto userInfo)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
            SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
               issuer: "http://localhost:5000",
               audience: "http://localhost:5000",
               claims: new List<Claim>{
                        new Claim(ClaimTypes.Role, AdminOrFollower(userInfo)),
                        new Claim(AuthenticationConstants.USERNAME_CLAIM, userInfo.username),
                        },
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string AdminOrFollower(UserDto aUser)
        {
            return aUser.isAdmin ? AuthenticationConstants.ADMIN_ROLE : AuthenticationConstants.FOLLOWER_ROLE;
        }
    }
}

