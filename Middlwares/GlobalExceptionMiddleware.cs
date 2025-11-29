using EdgePMO.API.Dtos;
using Serilog.Context;
using System.Net;
using System.Text.Json;

namespace EdgePMO.API.Middlwares
{
    public class GlobalExceptionMiddleware : IMiddleware
    {
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                PathString path = context.Request.Path;
                string? query = context.Request.QueryString.ToString();

                using (LogContext.PushProperty("Path", path))
                using (LogContext.PushProperty("QueryString", query))
                {
                    _logger.LogError(ex, "Unhandled exception occurred at {Path}", path);
                }

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                Response response = new Response()
                {
                    IsSuccess = false,
                    Message = "Something went wrong, please try again later",
                    Code = HttpStatusCode.InternalServerError,
                    Result = null
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
        }
    }
}
