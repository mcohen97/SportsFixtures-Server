using ObligatorioDA2.BusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ObligatorioDA2.BusinessLogic.Data.Exceptions;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.Services.Contracts;
using ObligatorioDA2.WebAPI.Controllers;
using ObligatorioDA2.WebAPI.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using ObligatorioDA2.Services.Contracts.Dtos;
using ObligatorioDA2.Services.Mappers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ObligatorioDA2.WebAPI.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class MatchesControllerTest{

        private Mock<IEncounterService> matchService;
        private Mock<IAuthenticationService> auth;
        private Mock<EncounterDtoMapper> mapper;
        private MatchesController controller;
        private EncounterDto testEncounter;

        [TestInitialize]
        public void SetUp() {
            matchService = new Mock<IEncounterService>();
            auth = new Mock<IAuthenticationService>();
            mapper = new Mock<EncounterDtoMapper>();
            testEncounter = BuildFakeMatch();
            controller = new MatchesController(matchService.Object, auth.Object);
            controller.ControllerContext = GetFakeControllerContext();
        }

        private EncounterDto BuildFakeMatch()
        {
            EncounterDto dto = new EncounterDto()
            {
                id = 1,
                sportName = "Football",
                date = DateTime.Now,
                teamsIds = new List<int>() { 1, 2 },
                hasResult = false
            };
            return dto;
        }

        [TestMethod]
        public void GetByGroupTest()
        {
            //Arrange.
            matchService.Setup(s => s.GetAllEncounter()).Returns(GetFakeEncounters());

            //Act.
            IActionResult result = controller.Get(true);
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<EncounterCalendarModelOut> matches = okResult.Value as ICollection<EncounterCalendarModelOut>;
            EncounterCalendarModelOut cal1 = ((List<EncounterCalendarModelOut>)matches)[0];
            EncountersGroupedByDate cal1Date = cal1.EncountersByDate.ToList()[0];
            EncounterCalendarModelOut cal2 = ((List<EncounterCalendarModelOut>)matches)[1];

            //Assert.
            matchService.Verify(s => s.GetAllEncounter(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(matches);
            Assert.AreEqual(matches.Count, 2);
            Assert.AreEqual(1, cal1.EncountersByDate.Count);
            Assert.AreEqual("Basketball", cal1.SportName);
            Assert.AreEqual(2, cal2.EncountersByDate.Count);
            Assert.AreEqual("Football", cal2.SportName);
            Assert.AreEqual(new DateTime(2018, 6, 27), cal1Date.Date);
            Assert.AreEqual(1, cal1Date.Encounters.Count);
        }

        private ICollection<EncounterDto> GetFakeEncounters()
        {
            EncounterDto dto2 = new EncounterDto()
            {
                id = 2,
                sportName = "Football",
                date = new DateTime(2018,6,27),
                teamsIds = new List<int>() { 1, 2 },
                hasResult = false
            };
            EncounterDto dto3 = new EncounterDto()
            {
                id = 3,
                sportName = "Basketball",
                date = new DateTime(2018, 6, 27),
                teamsIds = new List<int>() { 1, 2 },
                hasResult = false
            };
            return new List<EncounterDto>() { testEncounter, dto2, dto3 };
        }

        [TestMethod]
        public void GetTest() {
            //Arrange.
            matchService.Setup(s => s.GetAllEncounter()).Returns(new List<EncounterDto>() { testEncounter });

            //Act.
            IActionResult result = controller.Get(false);
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<EncounterModelOut> matches = okResult.Value as ICollection<EncounterModelOut>;

            //Assert.
            matchService.Verify(s => s.GetAllEncounter(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(matches);
            Assert.AreEqual(matches.Count, 1);
            Assert.AreEqual(matches.ToList()[0].TeamIds.Count, 2);
        }

        [TestMethod]
        public void CreateValidMatchTest() {
            matchService.Setup(ms => ms.AddEncounter(It.IsAny<int>(),It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>())).Returns(testEncounter);
            MatchModelIn input = BuildMatchModelIn(testEncounter);

            IActionResult result = controller.Post(input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            EncounterModelOut created = createdResult.Value as EncounterModelOut;

            matchService.Verify(ms => ms.AddEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual("GetMatchById", createdResult.RouteName);
            Assert.IsNotNull(created);
            Assert.AreEqual(testEncounter.date, testEncounter.date);
        }

        private MatchModelIn BuildMatchModelIn(EncounterDto anEncounter)
        {
            MatchModelIn built = new MatchModelIn()
            {
                SportName = anEncounter.sportName,
                TeamIds = anEncounter.teamsIds,
                Date = anEncounter.date
            };
            return built;
        }

        [TestMethod]
        public void CreateInvalidMatchTest() {
            //Arrange.
            MatchModelIn input = new MatchModelIn() { };
            //We need to force the error in de ModelState.
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.Post(input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;

            matchService.Verify(ms => ms.AddEncounter(testEncounter), Times.Never);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void CreateMatchSameDayForTeamTest() {
            //Arrange.
            Exception toThrow = new TeamAlreadyHasEncounterException();
            matchService.Setup(ms => ms.AddEncounter(It.IsAny<int>(),It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>())).Throws(toThrow);
            MatchModelIn input = BuildMatchModelIn(testEncounter);

            //Act.
            IActionResult result = controller.Post(input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;


            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage,toThrow.Message);
        }

        [TestMethod]
        public void GetMatchTest() {
            //Arrange.
            matchService.Setup(ms => ms.GetEncounter(It.IsAny<int>())).Returns(testEncounter);

            //Act.
            IActionResult result =controller.Get(1);
            OkObjectResult foundResult = result as OkObjectResult;
            EncounterModelOut match = foundResult.Value as EncounterModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(foundResult);
            Assert.AreEqual(200, foundResult.StatusCode);
            Assert.IsNotNull(match);
            Assert.AreEqual(1, match.Id);
            Assert.IsFalse(match.CommentsIds.Any());
        }

        [TestMethod]
        public void GetNotExistingMatchTest() {
            //Arrange.
            Exception internalEx = new EncounterNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.GetEncounter(It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.Get(1);
            NotFoundObjectResult notFoundresult = result as NotFoundObjectResult;
            ErrorModelOut error = notFoundresult.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFoundresult);
            Assert.AreEqual(404, notFoundresult.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void PutModifyMatchTest() {
            //Arrange.
            MatchModelIn input = BuildMatchModelIn(testEncounter);
            matchService.Setup(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(), It.IsAny<string>())).Returns(testEncounter);

            //Act.
            IActionResult result = controller.Put(1, input);
            OkObjectResult okResult = result as OkObjectResult;
            EncounterModelOut modified = okResult.Value as EncounterModelOut;

            //Assert.
            matchService.Verify(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);

            matchService.Verify(ms => ms.AddEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(modified);
            Assert.AreEqual(modified.Id, testEncounter.id);
        }

        [TestMethod]
        public void PutAdd() {
            //Arrange.
            Exception internalEx = new EncounterNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(),It.IsAny<string>())).Throws(toThrow);
            matchService.Setup(ms => ms.AddEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>())).Returns(testEncounter);

            MatchModelIn input = BuildMatchModelIn(testEncounter);

            //Act.
            IActionResult result = controller.Put(1, input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            EncounterModelOut modified = createdResult.Value as EncounterModelOut;

            //Assert.
            matchService.Verify(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);

            matchService.Verify(ms => ms.AddEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);

            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual("GetMatchById", createdResult.RouteName);
            Assert.IsNotNull(modified);
            Assert.AreEqual(modified.Id, testEncounter.id);
        }

        [TestMethod]
        public void PutNotValidTest() {
            //Arrange.
            MatchModelIn modelIn = new MatchModelIn() { };
            //We need to force the error in de ModelState.
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.Put(1, modelIn);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;

            //Assert.
            matchService.Verify(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(), It.IsAny<string>()), Times.Never);

            matchService.Verify(ms => ms.AddEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);

            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void PutDateOccupiedTest() {
            //Arrange.
            Exception toThrow = new TeamAlreadyHasEncounterException();
            matchService.Setup(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(), It.IsAny<string>())).Throws(toThrow);
            MatchModelIn input = BuildMatchModelIn(testEncounter);

            //Act.
            IActionResult result = controller.Put(1, input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            matchService.Verify(ms => ms.ModifyEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<DateTime>(), It.IsAny<string>()), Times.Once);

            matchService.Verify(ms => ms.AddEncounter(It.IsAny<int>(), It.IsAny<ICollection<int>>(),
                It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void DeleteTest() {
            //Act.
            IActionResult result =controller.Delete(3);
            OkObjectResult okResult = result as OkObjectResult;

            //Assert.
            matchService.Verify(ms => ms.DeleteEncounter(3), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
        }

        [TestMethod]
        public void DeleteNotExistentTest() {
            //Arrange.
            Exception internalEx = new EncounterNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.DeleteEncounter(3)).Throws(toThrow);

            //Act.
            IActionResult result = controller.Delete(3);
            NotFoundObjectResult badRequest = result as NotFoundObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.IsNotNull(error);
            Assert.AreEqual(404, badRequest.StatusCode);
            Assert.AreEqual(error.ErrorMessage,toThrow.Message);
        }

        [TestMethod]
        public void CommentOnMatchTest() {
            //Arrange.
            User commentarist = GetFakeUser();
            CommentaryDto made = new CommentaryDto() { makerUsername = commentarist.UserName, text = "this is a comment" };

            CommentModelIn input = new CommentModelIn() {
                Text = "this is a comment"
            };
            matchService.Setup(ms => ms.CommentOnEncounter(3, "username", input.Text)).Returns(made);


            //Act.
            IActionResult result = controller.CommentOnMatch(3,input);
            CreatedAtRouteResult createdResult = result as CreatedAtRouteResult;
            CommentModelOut comment = createdResult.Value as CommentModelOut;

            //Assert.
            matchService.Verify(ms => ms.CommentOnEncounter(3, "username", input.Text),Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);
            Assert.AreEqual("GetCommentById", createdResult.RouteName);
            Assert.IsNotNull(comment);
            Assert.AreEqual(comment.Text, input.Text);
            Assert.AreEqual("username", comment.MakerUsername);
        }

        [TestMethod]
        public void CommentWithBadFormatTest() {
            //Arrange.
            CommentModelIn input = new CommentModelIn()
            {
                Text = "this is a comment",
            };
            controller.ModelState.AddModelError("", "Error");

            //Act.
            IActionResult result = controller.CommentOnMatch(3,input);
            BadRequestObjectResult badRequest = result as BadRequestObjectResult;

            //Assert.
            matchService.Verify(ms => ms.CommentOnEncounter(It.IsAny<int>(), It.IsAny<string>(), input.Text), Times.Never);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public void CreateCommentInNoExistingMatch() {
            //Arrange.
            ControllerContext fakeContext = GetFakeControllerContext();
            controller.ControllerContext = fakeContext;
            CommentModelIn input = new CommentModelIn()
            {
                Text = "this is a comment"
            };
            Exception internalEx = new EncounterNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.CommentOnEncounter(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.CommentOnMatch(3,input);
            NotFoundObjectResult badRequest = result as NotFoundObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            matchService.Verify(ms => ms.CommentOnEncounter(3, "username", input.Text), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(404, badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void CreateCommentByNoExistingUser() {
            //Arrange.
            ControllerContext fakeContext = GetFakeControllerContext();
            controller.ControllerContext = fakeContext;
            CommentModelIn input = new CommentModelIn()
            {
                Text = "this is a comment"
            };
            Exception internalEx = new UserNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.CommentOnEncounter(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.CommentOnMatch(3,input);
            NotFoundObjectResult badRequest = result as NotFoundObjectResult;
            ErrorModelOut error = badRequest.Value as ErrorModelOut;

            //Assert.
            matchService.Verify(ms => ms.CommentOnEncounter(3, "username", input.Text), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(404, badRequest.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);

        }

        [TestMethod]
        public void ViewAllMatchCommentsTest() {
            //Arrange.
            User dummyUser = GetFakeUser();
            CommentaryDto dummyComment = new CommentaryDto() { text="Comment", makerUsername= dummyUser.UserName };
            ICollection<CommentaryDto> fakeList = new List<CommentaryDto>() { dummyComment, dummyComment, dummyComment };
            matchService.Setup(ms => ms.GetEncounterCommentaries(It.IsAny<int>())).Returns(fakeList);

            //Act.
            IActionResult result = controller.GetMatchComments(3);
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<CommentModelOut> comments = okResult.Value as ICollection<CommentModelOut>;

            //Assert.
            matchService.Verify(ms => ms.GetEncounterCommentaries(It.IsAny<int>()),Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(comments);
            Assert.AreEqual(fakeList.Count, comments.Count);
        }

       [TestMethod]
        public void ViewCommentTest() {
            //Arrange.
            User dummyUser = GetFakeUser();
            CommentaryDto dummyComment = new CommentaryDto() { text="Comment", makerUsername=dummyUser.UserName };
            matchService.Setup(ms => ms.GetComment(3)).Returns(dummyComment);

            //Act.
            IActionResult result = controller.GetComment(3);
            OkObjectResult okResult = result as OkObjectResult;
            CommentModelOut comment = okResult.Value as CommentModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(comment);
            Assert.AreEqual(dummyComment.text,comment.Text);
        }

        [TestMethod]
        public void ViewNotExistingCommentTest() {
            //Arrange.
            Exception internalEx = new CommentNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.GetComment(3)).Throws(toThrow);

            //Act.
            IActionResult result = controller.GetComment(3);
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(error.ErrorMessage, toThrow.Message);
        }

        [TestMethod]
        public void ViewAllTheCommentsTest() {
            //Arrange.
            User dummyUser = GetFakeUser();
            CommentaryDto dummyComment = new CommentaryDto() { text="Comment", makerUsername= dummyUser.UserName };
            ICollection<CommentaryDto> fakeList = new List<CommentaryDto>() { dummyComment, dummyComment, dummyComment };
            matchService.Setup(ms => ms.GetAllCommentaries()).Returns(fakeList);

            //Act.
            IActionResult result = controller.GetAllComments();
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<CommentModelOut> comments =okResult.Value as ICollection<CommentModelOut>;

            //Do.
            matchService.Verify(ms => ms.GetAllCommentaries(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(comments);
            Assert.AreEqual(fakeList.Count, comments.Count);
        }

        [TestMethod]
        public void GetSportMatchesTest() {
            //Arrange.
            ICollection<EncounterDto> dummies = new List<EncounterDto>() { testEncounter, testEncounter, testEncounter };
            matchService.Setup(ms => ms.GetAllEncounterDtos(It.IsAny<string>())).Returns(dummies);

            //Act.
            IActionResult result = controller.GetBySport("Soccer");
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<EncounterModelOut> matches = okResult.Value as ICollection<EncounterModelOut>;

            //Assert.
            matchService.Verify(ms => ms.GetAllEncounterDtos(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(matches);
            Assert.AreEqual(dummies.Count, matches.Count);
        }

        [TestMethod]
        public void GetSportMatchesNotFoundTest() {
            //Arrange.
            Exception internalEx = new SportNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.GetAllEncounterDtos(It.IsAny<string>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.GetBySport("Soccer");
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            matchService.Verify(ms => ms.GetAllEncounterDtos(It.IsAny<string>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        [TestMethod]
        public void GetTeamMatchesTest() {
            //Arrange.
            ICollection<EncounterDto> dummies = new List<EncounterDto>() { testEncounter, testEncounter, testEncounter };
            matchService.Setup(ms => ms.GetAllEncounterDtos(It.IsAny<int>())).Returns(dummies);

            //Act.
            IActionResult result = controller.GetByTeam(5);
            OkObjectResult okResult = result as OkObjectResult;
            ICollection<EncounterModelOut> matches = okResult.Value as ICollection<EncounterModelOut>;

            //Assert.
            matchService.Verify(ms => ms.GetAllEncounterDtos(It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNotNull(matches);
            Assert.AreEqual(dummies.Count, matches.Count);

        }

        [TestMethod]
        public void GetTeamMatchesNotFoundTest()
        {
            //Arrange.
            Exception internalEx = new TeamNotFoundException();
            Exception toThrow = new ServiceException(internalEx.Message, ErrorType.ENTITY_NOT_FOUND);
            matchService.Setup(ms => ms.GetAllEncounterDtos(It.IsAny<int>())).Throws(toThrow);

            //Act.
            IActionResult result = controller.GetByTeam(5);
            NotFoundObjectResult notFound = result as NotFoundObjectResult;
            ErrorModelOut error = notFound.Value as ErrorModelOut;

            //Assert.
            matchService.Verify(ms => ms.GetAllEncounterDtos(It.IsAny<int>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
            Assert.IsNotNull(error);
            Assert.AreEqual(toThrow.Message, error.ErrorMessage);
        }

        private User GetFakeUser()
        {
            UserId identity = new UserId() { Name = "name", Surname = "surname", UserName = "username", Password = "password", Email = "email@email.com" };
            return new User(identity, true);
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

        [TestMethod]
        public void SetResultTest() {
            //Arrange.
            matchService.Setup(ms => ms.SetResult(It.IsAny<int>(), It.IsAny<ResultDto>()));
            matchService.Setup(ms => ms.GetEncounter(It.IsAny<int>())).Returns(testEncounter);
            ResultModel resultModel = GetFakeResult();

            //Act.
            IActionResult result = controller.SetResult(1, resultModel);
            OkObjectResult okResult = result as OkObjectResult;
            EncounterModelOut matchWithResult= okResult.Value as EncounterModelOut;

            //Assert.
            matchService.VerifyAll();
            Assert.IsNotNull(result);
            Assert.IsNotNull(okResult);
            Assert.IsNotNull(matchWithResult);
            Assert.AreEqual(matchWithResult.Id, testEncounter.id);
        }

        private ResultModel GetFakeResult()
        {
            StandingModelIn pos1 = new StandingModelIn() { TeamId = 3, Position = 1 };
            StandingModelIn pos2 = new StandingModelIn() { TeamId = 4, Position = 2 };
            StandingModelIn pos3 = new StandingModelIn() { TeamId = 1, Position = 3 };
            ICollection<StandingModelIn> standings= new List<StandingModelIn>() {pos1,pos2,pos3};
            ResultModel fake = new ResultModel() {Team_Position=standings };
            return fake;
        }
    }
   
}
