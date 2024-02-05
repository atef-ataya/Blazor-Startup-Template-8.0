using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace Server.Middleware
{
    public class RateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private static Dictionary<string, (DateTime, int)> _requestCounts = new Dictionary<string, (DateTime, int)>();

        public RateLimitingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var key = $"{context.Request.Path}-{context.Connection.RemoteIpAddress}";
            var limit = 20; // Max requests
            var resetTime = TimeSpan.FromHours(1); // Reset time window

            if (_requestCounts.ContainsKey(key))
            {
                var (windowStart, count) = _requestCounts[key];
                if (windowStart + resetTime < DateTime.UtcNow)
                {
                    _requestCounts[key] = (DateTime.UtcNow, 1);
                }
                else if (count >= limit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    return;
                }
                else
                {
                    _requestCounts[key] = (windowStart, count + 1);
                }
            }
            else
            {
                _requestCounts.Add(key, (DateTime.UtcNow, 1));
            }

            await _next(context);
        }
    }

}
