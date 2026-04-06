using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace FileShareApp.Infrastructure;

public static class MtlsExtensions
{
    /// <summary>
    /// Loads the mTLS client certificate from configuration when mTLS is enabled.
    /// Returns null when disabled or when the cert cannot be loaded.
    /// </summary>
    public static X509Certificate2? LoadMtlsClientCert(IConfiguration config, bool isDevelopment)
    {
        if (!config.GetValue<bool>("Mtls:Enabled")) return null;

        var path = config["Mtls:ClientCertPath"];
        var pw   = config["DEV_CERT_PASSWORD"] ?? string.Empty;

        if (string.IsNullOrEmpty(path))
        {
            Console.WriteLine("[mTLS] WARNING: Mtls:Enabled=true but Mtls:ClientCertPath is not configured");
            return null;
        }

        try
        {
            return X509CertificateLoader.LoadPkcs12FromFile(path, pw);
        }
        catch (Exception ex) when (isDevelopment)
        {
            Console.WriteLine($"[mTLS] Client cert failed to load: {ex.Message}");
            return null;
        }
    }
}
