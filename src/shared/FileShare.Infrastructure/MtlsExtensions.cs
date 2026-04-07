using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

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
        var pw = ResolveCertificatePassword(config, isDevelopment);

        if (string.IsNullOrWhiteSpace(path))
            return FailOrNull(isDevelopment, "Mtls:Enabled=true but Mtls:ClientCertPath is missing");

        if (string.IsNullOrWhiteSpace(pw))
            return FailOrNull(isDevelopment, "Mtls:Enabled=true but client certificate password is missing");

        if (!File.Exists(path))
            return FailOrNull(isDevelopment, $"Client cert file not found at '{path}'");

        try
        {
            return X509CertificateLoader.LoadPkcs12FromFile(path, pw);
        }
        catch (CryptographicException ex)
        {
            return FailOrNull(isDevelopment, "Client cert failed to load (wrong password or invalid cert)", ex);
        }
        catch (IOException ex)
        {
            return FailOrNull(isDevelopment, "Client cert could not be read from disk", ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            return FailOrNull(isDevelopment, "Access denied while reading client cert file", ex);
        }
    }

    private static string? ResolveCertificatePassword(IConfiguration config, bool isDevelopment)
    {
        return config["Mtls:ClientCertPassword"]
               ?? (isDevelopment ? config["DEV_CERT_PASSWORD"] : null);
    }

    private static X509Certificate2? FailOrNull(bool isDevelopment, string message, Exception? ex = null)
    {
        if (isDevelopment)
        {
            Console.WriteLine($"[mTLS] WARNING: {message}");
            if (ex is not null) Console.WriteLine($"[mTLS] Detail: {ex.Message}");
            return null;
        }

        throw new InvalidOperationException($"[mTLS] {message}", ex);
    }
}
