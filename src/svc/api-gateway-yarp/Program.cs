using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var mtlsEnabled = builder.Configuration.GetValue<bool>("Mtls:Enabled");
var allowStartupWithoutClientCertInDevelopment = builder.Configuration.GetValue<bool>("Mtls:AllowStartupWithoutClientCertInDevelopment");
X509Certificate2? clientCert = null;
if (mtlsEnabled)
{
    var certPath = builder.Configuration["Mtls:ClientCertPath"]!;
    var certPassword = builder.Configuration["Mtls:ClientCertPassword"]!;
    try
    {
        clientCert = X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword, X509KeyStorageFlags.EphemeralKeySet);
    }
    catch (Exception ex) when (builder.Environment.IsDevelopment() && allowStartupWithoutClientCertInDevelopment)
    {
        Console.WriteLine($"[WARN] Could not load mTLS client certificate from '{certPath}'. Continuing without outbound client certificate in Development. {ex.GetType().Name}: {ex.Message}");
    }
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
