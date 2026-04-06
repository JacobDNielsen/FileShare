using System.Security.Cryptography.X509Certificates;
using FileShareApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var clientCert = MtlsExtensions.LoadMtlsClientCert(builder.Configuration, builder.Environment.IsDevelopment());

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
