using Microsoft.AspNetCore.Mvc.Filters;

namespace backend.Filters;

public class CorsHeaderFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Add CORS headers before response is sent
        var origin = context.HttpContext.Request.Headers["Origin"].ToString();
        var allowedOrigin = string.IsNullOrEmpty(origin) ? "*" :
            (origin.Contains("netlify.app") || origin.Contains("localhost:4200") ? origin : "*");

        context.HttpContext.Response.Headers["Access-Control-Allow-Origin"] = allowedOrigin;
        context.HttpContext.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS, PATCH";
        context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type, Authorization, X-Requested-With";
        context.HttpContext.Response.Headers["Access-Control-Allow-Credentials"] = "false";
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Headers already added in OnResultExecuting
    }
}

