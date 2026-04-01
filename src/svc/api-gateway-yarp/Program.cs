using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var mtlsEnabled = builder.Configuration.GetValue<bool>("Mtls:Enabled");
X509Certificate2? clientCert = null;
if (mtlsEnabled)
{
    var certPath = builder.Configuration["Mtls:ClientCertPath"]!;
    var certPassword = builder.Configuration["Mtls:ClientCertPassword"]!;
    clientCert = new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.EphemeralKeySet);
}

//Add YARP from configuration
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .ConfigureHttpClient((context, handler) =>
    {
        if (clientCert != null)
            handler.SslOptions.ClientCertificates = new X509CertificateCollection { clientCert };
    });

var app = builder.Build();

app.MapReverseProxy();

app.Run();
