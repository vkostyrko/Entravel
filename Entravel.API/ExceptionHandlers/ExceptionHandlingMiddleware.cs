using System.Net;
using System.Text.Json;
using Entravel.Application.Exceptions;
using FluentValidation;

namespace Entravel.API.ExceptionHandlers;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                OrderSubmissionException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };

            object response;

            if (ex is ValidationException validationException)
            {
                response = new
                {
                    message = validationException.Message,
                    errors = validationException.Errors.Select(validationError => new
                    {
                        property = validationError.PropertyName,
                        error = validationError.ErrorMessage
                    })
                };
            }
            else
            {
                response = new { message = context.Response.StatusCode == 500 ? "Something went wrong" : ex.Message };
            }

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

