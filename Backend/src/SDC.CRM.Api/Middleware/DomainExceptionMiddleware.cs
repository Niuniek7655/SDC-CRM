using Microsoft.AspNetCore.Mvc;
using SDC.CRM.Domain.Common;

namespace SDC.CRM.Api.Middleware;

/// <summary>Translates domain rule violations into HTTP 400 problem details.</summary>
public sealed class DomainExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Title = "Business rule violated",
                Detail = exception.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
