using EdgePMO.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Net;
using System.Text.Json;

namespace EdgePMO.API.Middlwares
{
    public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            if (authorizeResult.Challenged)
            {
                // 401 Unauthorized - Authentication required
                await WriteCustomResponseAsync(context, HttpStatusCode.Unauthorized, "Authentication required");
                return;
            }

            if (authorizeResult.Forbidden)
            {
                // 403 Forbidden - Authenticated but not authorized
                var user = context.User.Identity?.Name ?? "Unknown";
                await WriteCustomResponseAsync(context, HttpStatusCode.Forbidden, "Access denied");
                return;
            }

            // Authorization successful, continue pipeline
            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static async Task WriteCustomResponseAsync(HttpContext context, HttpStatusCode statusCode, string error)
        {
            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            Response response = new Response()
            {
                IsSuccess = false,
                Message = error,
                Code = statusCode
            };

            string? json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
