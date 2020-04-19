using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts.Dtos;

namespace ObligatorioDA2.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MatchesController : ControllerBase
    {
        private IEncounterService encounterService;
        private EncounterModelFactory factory;
        private ErrorActionResultFactory errors;
        private IAuthenticationService authenticator;

        public MatchesController(IEncounterService aService, IAuthenticationService authService)
        {
            encounterService = aService;
            factory = new EncounterModelFactory();
            errors = new ErrorActionResultFactory(this);
            authenticator = authService;
        }

        [HttpGet]
        [Authorize]
        public IActionResult Get([FromQuery] bool grouped)
        {
            ICollection<EncounterDto> matches = encounterService.GetAllEncounter();
            ICollection<EncounterModelOut> output = matches.Select(m => factory.CreateModelOut(m)).ToList();
            IActionResult result;
            if (grouped) {
                ICollection <EncounterCalendarModelOut> groupedBySportsAndDates = Group(output);
                result = Ok(groupedBySportsAndDates);
            }
            else
            {
                result = Ok(output);
            }
            return result;
        }

        private ICollection<EncounterCalendarModelOut> Group(ICollection<EncounterModelOut> matches)
        {
            IEnumerable<IGrouping<string, EncounterModelOut>> groupsByMatch= matches
                .OrderBy(m => m.SportName)
                .GroupBy(m => m.SportName);
            return groupsByMatch.Select(g => CreateSportCalendar(g)).ToList();
        }

        private EncounterCalendarModelOut CreateSportCalendar(IGrouping<string, EncounterModelOut> sportGroup)
        {
            EncounterCalendarModelOut calendar = new EncounterCalendarModelOut();
            calendar.SportName=sportGroup.Key;
            calendar.EncountersByDate = new List<EncountersGroupedByDate>();

            IEnumerable<EncounterModelOut> sportEncounters = sportGroup;
            IEnumerable<IGrouping<DateTime, EncounterModelOut>> encountersByDate = sportEncounters
                .OrderBy(e => e.Date)
                .GroupBy(e => e.Date);

            foreach (IGrouping<DateTime, EncounterModelOut> dateEncounters in encountersByDate) {
                EncountersGroupedByDate groupByDate = new EncountersGroupedByDate();
                groupByDate.Date = dateEncounters.Key;
                groupByDate.Encounters = dateEncounters.ToList();
                calendar.EncountersByDate.Add(groupByDate);
            }
            return calendar;
        }

        [HttpPost]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Post([FromBody] MatchModelIn input)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = TryPostMatch(0,input.TeamIds, input.SportName, input.Date);
            }
            else {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult TryPostMatch(int id, ICollection<int> teamIds, string sportName, DateTime date)
        {
            IActionResult result;
            try
            {
                SetSession();
                EncounterDto added = encounterService.AddEncounter(id,teamIds, sportName, date);
                EncounterModelOut output = factory.CreateModelOut(added);
                result = CreatedAtRoute("GetMatchById",new {matchId = added.id }, output);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }

            return result;
        }


        [HttpGet("{matchId}", Name = "GetMatchById")]
        [Authorize]
        public IActionResult Get(int matchId)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetMatch(matchId);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGetMatch(int matchId)
        {
            EncounterDto stored = encounterService.GetEncounter(matchId);
            EncounterModelOut modelOut = factory.CreateModelOut(stored);
            IActionResult result = Ok(modelOut);
            return result;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Put(int id, MatchModelIn aMatch)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = TryPut(id, aMatch);
            }
            else {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult TryPut(int id, MatchModelIn aMatch)
        {
            IActionResult result;
            try
            {
                SetSession();
                EncounterDto modified =encounterService.ModifyEncounter(id, aMatch.TeamIds, aMatch.Date, aMatch.SportName);
                modified.id = id;
                EncounterModelOut output = factory.CreateModelOut(modified);
                result = Ok(output);
            }
            catch (ServiceException e)
            {
                if (e.Error.Equals(ErrorType.ENTITY_NOT_FOUND))
                {
                    result = TryPostMatch(id, aMatch.TeamIds, aMatch.SportName, aMatch.Date);
                }
                else {
                    result = errors.GenerateError(e);
                }
            }
            return result;
        }

        private EncounterModelOut BuildModelout(int id,MatchModelIn aMatch)
        {

            EncounterModelOut output = new MatchModelOut()
            {
                Id = id,
                SportName = aMatch.SportName,
                TeamIds = aMatch.TeamIds,
                HasResult = false,
                Date = aMatch.Date
            };
            return output;
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult Delete(int id)
        {
            IActionResult result;
            try {
                SetSession();
                result = TryToDelete(id);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryToDelete(int id)
        {
            encounterService.DeleteEncounter(id);
            OkModelOut okMessage = new OkModelOut() { OkMessage = "The match was deleted successfully" };
            return Ok(okMessage);
        }

        [HttpPost("{matchId}/comments")]
        [Authorize]
        public IActionResult CommentOnMatch(int matchId,CommentModelIn input)
        {
            IActionResult result;
            if (ModelState.IsValid)
            {
                result = AddValidFormatComment(matchId,input);
            }
            else {
                result = BadRequest(ModelState);
            }
            return result;
        }

        private IActionResult AddValidFormatComment(int matchId,CommentModelIn input)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryAddComment(matchId, input);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryAddComment(int matchId,CommentModelIn input)
        {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            CommentaryDto created = encounterService.CommentOnEncounter(matchId, username, input.Text);
            CommentModelOut output = new CommentModelOut
            {
                Id = created.commentId,
                MakerUsername = username,
                Text = input.Text
            };
            return CreatedAtRoute("GetCommentById",new {id =output.Id }, output);
        }

        [HttpGet("sport/{sportName}")]
        [Authorize]
        public IActionResult GetBySport(string sportName)
        {
            IActionResult result;
            try {
                SetSession();
                ICollection<EncounterDto> matches = encounterService.GetAllEncounterDtos(sportName);
                ICollection<EncounterModelOut> output = matches.Select(m => factory.CreateModelOut(m)).ToList();
                result = Ok(output);
            }
            catch (ServiceException e)
            {
                result = errors.GenerateError(e);
            }
            return result;
        }

        [HttpGet("team/{teamId}")]
        [Authorize]
        public IActionResult GetByTeam(int teamId)
        {
            IActionResult result;
            try
            {
                SetSession();
                ICollection<EncounterDto> matches = encounterService.GetAllEncounterDtos(teamId);
                ICollection<EncounterModelOut> output = matches.Select(m => factory.CreateModelOut(m)).ToList();
                result = Ok(output);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }


        [HttpGet("{matchId}/comments", Name = "GetCommentMatchComments")]
        [Authorize]
        public IActionResult GetMatchComments(int matchId) {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetMatchComments(matchId);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGetMatchComments(int matchId)
        {
            ICollection<CommentaryDto> matchComments = encounterService.GetEncounterCommentaries(matchId);
            ICollection<CommentModelOut> output = matchComments.Select(c => BuildCommentModelOut(c)).ToList();
            return Ok(output);
        }

        [HttpGet("comments")]
        [Authorize]
        public IActionResult GetAllComments()
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetAllComments();
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGetAllComments()
        {
            ICollection<CommentaryDto> allComments = encounterService.GetAllCommentaries();
            ICollection<CommentModelOut> output = allComments.Select(c => BuildCommentModelOut(c)).ToList();
            return Ok(output);
        }

        private CommentModelOut BuildCommentModelOut(CommentaryDto aComment) {
            CommentModelOut comment = new CommentModelOut()
            {
                Id = aComment.commentId,
                MakerUsername = aComment.makerUsername,
                Text = aComment.text
            };
            return comment;
        }

        [HttpGet("comments/{id}", Name = "GetCommentById")]
        [Authorize]
        public IActionResult GetComment(int id)
        {
            IActionResult result;
            try
            {
                SetSession();
                result = TryGetComment(id);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TryGetComment(int id)
        {
            CommentaryDto comment = encounterService.GetComment(id);
            CommentModelOut output = new CommentModelOut
            {
                Id = comment.commentId,
                MakerUsername = comment.makerUsername,
                Text = comment.text
            };
            return Ok(output);
        }

        [HttpPost("{matchId}/result")]
        [Authorize(Roles = AuthenticationConstants.ADMIN_ROLE)]
        public IActionResult SetResult(int matchId, ResultModel resultModel)
        {
            IActionResult result;
            try {
                SetSession();
                result = TrySetResult(matchId, resultModel);
            }
            catch (ServiceException e) {
                result = errors.GenerateError(e);
            }
            return result;
        }

        private IActionResult TrySetResult(int matchId, ResultModel resultModel)
        {
            ICollection<Tuple<int, int>> team_positions = resultModel.Team_Position
                .Select(tp => new Tuple<int, int>(tp.TeamId, tp.Position)).ToList();
            ResultDto encounterResult = new ResultDto() { teams_positions = team_positions };
            encounterService.SetResult(matchId, encounterResult);
            EncounterDto matchWithResult = encounterService.GetEncounter(matchId);
            EncounterModelOut result = factory.CreateModelOut(matchWithResult);
            return Ok(result);
        }

        private void SetSession()
        {
            string username = HttpContext.User.Claims.First(c => c.Type.Equals(AuthenticationConstants.USERNAME_CLAIM)).Value;
            authenticator.SetSession(username);
        }
    }
}
