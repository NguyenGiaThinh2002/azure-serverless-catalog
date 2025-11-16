// Catalog.Api/Middleware/SwaggerUIMiddleware.cs
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.OpenApi.Readers;

namespace Catalog.Api.Middleware;

public class SwaggerUIMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var httpContext = context.GetHttpContext()!;

        if (httpContext.Request.Path.StartsWithSegments("/swagger"))
        {
            await httpContext.Response.SendFileAsync("swagger/index.html");
            return;
        }

        if (httpContext.Request.Path == "/swagger/v1/swagger.json")
        {
            var swaggerProvider = httpContext.RequestServices.GetRequiredService<Microsoft.OpenApi.Readers.OpenApiStringReader>();
            // Or simpler: redirect to the generated endpoint when we add it
            httpContext.Response.Redirect("/api/swagger/v1/swagger.json");
            return;
        }

        await next(context);
    }
}