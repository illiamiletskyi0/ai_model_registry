using Application.AIModels.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Api.Modules.Errors;

public static class AIModelErrorFactory
{
    public static ObjectResult ToObjectResult(this AIModelException error)
    {
        return new ObjectResult(error.Message)
        {
            StatusCode = error switch
            {
                AIModelAlreadyExistsException => StatusCodes.Status409Conflict,
                AIModelNotFoundException => StatusCodes.Status404NotFound,
                AIModelUnhandledException => StatusCodes.Status500InternalServerError,
                _ => throw new NotImplementedException("AIModel error handler is not implemented.")
            }
        };
    }
}