using System.Net;
using Microsoft.AspNetCore.Mvc;
using ObligatorioDA2.Services.Exceptions;
using ObligatorioDA2.WebAPI.Models;


namespace ObligatorioDA2.WebAPI.Controllers
{
    public class ErrorActionResultFactory
    {
        ControllerBase sender;
        public ErrorActionResultFactory(ControllerBase controller) {
            sender = controller;
        }

        public IActionResult GenerateError(ServiceException e) {
            IActionResult errorResponse=null;
            ErrorModelOut errorMessage = new ErrorModelOut() { ErrorMessage = e.Message };
            switch (e.Error) {
                case ErrorType.ENTITY_ALREADY_EXISTS:
                    errorResponse = GenerateBadRequest(e);
                break;
                case ErrorType.INVALID_DATA:
                    errorResponse = GenerateBadRequest(e);
                    break;
                case ErrorType.ENTITY_NOT_FOUND:          
                    errorResponse = sender.NotFound(errorMessage);
                    break;
                case ErrorType.DATA_INACCESSIBLE:
                    errorResponse  = sender.StatusCode((int)HttpStatusCode.InternalServerError, errorMessage);
                    break;
                case ErrorType.NOT_AUTHENTICATED:
                    errorResponse = sender.Unauthorized();
                        break;
                case ErrorType.NO_PERMISSION:
                    errorResponse = sender.Forbid();
                    break;

            }
            return errorResponse;
        }

        private IActionResult GenerateBadRequest(ServiceException e)
        {
            ErrorModelOut errorMessage = new ErrorModelOut() { ErrorMessage = e.Message };
            return sender.BadRequest(errorMessage);
        }
    }
}
