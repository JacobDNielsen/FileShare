using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FileShareApp.Infrastructure;

public static class MtlsExtensions
{
    /// <summary>
    /// Loads the mTLS client certificate from configuration when mTLS is enabled.
    /// Returns null when mTLS is disabled.
    /// </summary>
    public static X509Certificate2? LoadMtlsClientCert(IConfiguration config, ILogger? logger = null)
    {
        if (!config.GetValue<bool>("Mtls:Enabled")) return null;

        var path = config["Mtls:ClientCertPath"];
        var pw = config["Mtls:ClientCertPassword"];

        try
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new InvalidOperationException("Mtls:ClientCertPath is required when Mtls:Enabled is true.");

            if (string.IsNullOrWhiteSpace(pw))
                throw new InvalidOperationException("Mtls:ClientCertPassword is required when Mtls:Enabled is true.");

            var cert = X509CertificateLoader.LoadPkcs12FromFile(path, pw);
            logger?.LogInformation("Loaded mTLS client certificate from {ClientCertPath}.", path);
            return cert;
        }
        catch (Exception ex)
        {
            logger?.LogError(
                ex,
                "Failed to load mTLS client certificate. MtlsEnabled={MtlsEnabled}, ClientCertPath={ClientCertPath}",
                true,
                path ?? "<null>");
            throw;
        }
    }
}
