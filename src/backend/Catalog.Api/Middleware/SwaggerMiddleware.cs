// Catalog.Api/Middleware/SwaggerMiddleware.cs
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Catalog.Api.Middleware;

public class SwaggerMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            await next(context);
            return;
        }

        var request = httpContext.Request;
        if (request.Path.StartsWithSegments("/swagger") || request.Path == "/swagger")
        {
            // Let ASP.NET Core handle it
            await next(context);
            return;
        }

        await next(context);
    }
}