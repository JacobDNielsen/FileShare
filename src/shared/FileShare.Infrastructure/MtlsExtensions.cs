using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace FileShareApp.Infrastructure;

public static class MtlsExtensions
{
    /// <summary>
    /// Loads the mTLS client certificate from configuration when mTLS is enabled.
    /// Returns null when mTLS is disabled.
    /// </summary>
    public static X509Certificate2? LoadMtlsClientCert(IConfiguration config)
    {
        if (!config.GetValue<bool>("Mtls:Enabled")) return null;
        var path = config["Mtls:ClientCertPath"];
        var pw = config["DEV_CERT_PASSWORD"];
        return X509CertificateLoader.LoadPkcs12FromFile(path!, pw!);
    }
}
