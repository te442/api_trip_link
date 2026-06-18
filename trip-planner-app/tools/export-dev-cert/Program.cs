using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var certDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "certs"));
var pfxPath = Path.Combine(certDir, "dev-cert.pfx");
const string password = "dev";

Directory.CreateDirectory(certDir);

Console.WriteLine("Trusting ASP.NET HTTPS development certificate...");
Run("dotnet", "dev-certs https --trust");

if (File.Exists(pfxPath))
    File.Delete(pfxPath);

Console.WriteLine("Exporting development certificate...");
Run("dotnet", $"dev-certs https --export-path \"{pfxPath}\" --password {password}");

var cert = X509CertificateLoader.LoadPkcs12FromFile(
    pfxPath,
    password,
    X509KeyStorageFlags.Exportable);

var pemPath = Path.Combine(certDir, "dev-cert.pem");
var keyPath = Path.Combine(certDir, "dev-cert.key");

await File.WriteAllTextAsync(pemPath, ToPem("CERTIFICATE", cert.Export(X509ContentType.Cert)));

var rsa = cert.GetRSAPrivateKey()
    ?? throw new InvalidOperationException("Private key not found in development certificate.");
await File.WriteAllTextAsync(keyPath, ToPem("PRIVATE KEY", rsa.ExportPkcs8PrivateKey()));

Console.WriteLine($"Created: {pemPath}");
Console.WriteLine($"Created: {keyPath}");
Console.WriteLine("Restart Angular: npm start");

static string ToPem(string label, byte[] bytes)
{
    var b64 = Convert.ToBase64String(bytes);
    var sb  = new StringBuilder();
    sb.AppendLine($"-----BEGIN {label}-----");
    for (var i = 0; i < b64.Length; i += 64)
        sb.AppendLine(b64.Substring(i, Math.Min(64, b64.Length - i)));
    sb.AppendLine($"-----END {label}-----");
    return sb.ToString();
}

static void Run(string file, string args)
{
    using var process = Process.Start(new ProcessStartInfo(file, args)
    {
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError  = true
    }) ?? throw new InvalidOperationException($"Failed to start: {file} {args}");

    process.WaitForExit();
    if (process.ExitCode != 0)
    {
        var err = process.StandardError.ReadToEnd();
        throw new InvalidOperationException($"{file} {args} failed: {err}");
    }
}
