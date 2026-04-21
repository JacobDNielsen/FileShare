using System.Security.Cryptography.X509Certificates;
using FileShareApp.Infrastructure;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

using var startupLoggerFactory = LoggerFactory.Create(logging => logging.AddConsole());
var startupLogger = startupLoggerFactory.CreateLogger("Startup");
var clientCert = MtlsExtensions.LoadMtlsClientCert(builder.Configuration, startupLogger);

//Add YARP from configuration
var reverseProxy = builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Attach client cert on every forwarded request when mTLS is active
if (clientCert is not null)
{
    reverseProxy.ConfigureHttpClient((_, handler) =>
    {
        handler.SslOptions.ClientCertificates ??= new X509CertificateCollection();
        handler.SslOptions.ClientCertificates.Add(clientCert);
    });
}

var app = builder.Build();

app.MapReverseProxy();

app.Run();
