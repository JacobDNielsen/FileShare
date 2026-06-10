using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;

namespace FileShareApp.Infrastructure;

public static class AuthLoggingExtensions
{
    /// <summary>
    /// Configures JwtBearerOptions with detailed logging for token validation and network requests.
    /// </summary>
    public static void AddAuthLogging(this JwtBearerOptions options, ILogger logger)
    {
        // 1. Hook into Authentication Events
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                logger.LogInformation("JWT Token validated successfully for {User}.", 
                    context.Principal?.Identity?.Name ?? "unknown user");
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                logger.LogWarning(context.Exception, "JWT Authentication failed. Reason: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnForbidden = context =>
            {
                logger.LogWarning("JWT Forbidden. User is authenticated but not authorized for this resource.");
                return Task.CompletedTask;
            }
        };

        // 2. Intercept the Backchannel HTTP Handler to log OIDC metadata and JWKS retrieval.
        var innerHandler = options.BackchannelHttpHandler ?? new HttpClientHandler();
        options.BackchannelHttpHandler = new LoggingHandler(innerHandler, logger);
    }

    private sealed class LoggingHandler : DelegatingHandler
    {
        private readonly ILogger _logger;

        public LoggingHandler(HttpMessageHandler innerHandler, ILogger logger)
            : base(innerHandler)
        {
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Auth Network Request: {Method} {Uri}", request.Method, request.RequestUri);

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                sw.Stop();

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Auth Network Response: {StatusCode} for {Uri} ({Elapsed}ms)", 
                        (int)response.StatusCode, request.RequestUri, sw.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning("Auth Network Response Error: {StatusCode} for {Uri} ({Elapsed}ms)", 
                        (int)response.StatusCode, request.RequestUri, sw.ElapsedMilliseconds);
                }

                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Auth Network Request Failed: {Uri} ({Elapsed}ms)", request.RequestUri, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
