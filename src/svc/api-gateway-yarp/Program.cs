using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Load client cert for mTLS outbound when enabled
var mtlsEnabled = builder.Configuration.GetValue<bool>("Mtls:Enabled");
X509Certificate2? clientCert = null;
if (mtlsEnabled)
{
    var path = builder.Configuration["Mtls:ClientCertPath"];
    var pw   = builder.Configuration["DEV_CERT_PASSWORD"];
    if (!string.IsNullOrEmpty(path))
    {
        try
        {
            clientCert = X509CertificateLoader.LoadPkcs12FromFile(path, pw ?? string.Empty);
        }
        catch (Exception ex) when (builder.Environment.IsDevelopment())
        {
            Console.WriteLine($"[mTLS] Client cert failed to load: {ex.Message}");
        }
    }
}

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
