# Exports the trusted ASP.NET Core HTTPS dev certificate for Angular (Vite) dev server.
$ErrorActionPreference = 'Stop'

function ConvertTo-PemBlock {
    param(
        [string]$Label,
        [byte[]]$Bytes
    )

    $b64     = [Convert]::ToBase64String($Bytes)
    $wrapped = ($b64 -split '(.{64})' | Where-Object { $_ }) -join [Environment]::NewLine
    return "-----BEGIN $Label-----$([Environment]::NewLine)$wrapped$([Environment]::NewLine)-----END $Label-----$([Environment]::NewLine)"
}

$certDir = Join-Path $PSScriptRoot 'certs'
$pfxPath = Join-Path $certDir 'dev-cert.pfx'
$pemPath = Join-Path $certDir 'dev-cert.pem'
$keyPath = Join-Path $certDir 'dev-cert.key'
$pfxPass = 'dev'

New-Item -ItemType Directory -Force -Path $certDir | Out-Null

Write-Host 'Trusting ASP.NET HTTPS development certificate (if needed)...'
dotnet dev-certs https --trust | Out-Null

Write-Host 'Exporting development certificate...'
if (Test-Path $pfxPath) { Remove-Item $pfxPath -Force }
dotnet dev-certs https --export-path $pfxPath --password $pfxPass | Out-Null

Write-Host 'Converting to PEM for Angular...'
$cert = [System.Security.Cryptography.X509Certificates.X509Certificate2]::new(
    $pfxPath,
    $pfxPass,
    [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

$certPem = ConvertTo-PemBlock -Label 'CERTIFICATE' -Bytes $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
[System.IO.File]::WriteAllText($pemPath, $certPem)

$rsa = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($cert)
if ($null -eq $rsa) {
    throw 'Could not read private key from development certificate.'
}

$keyPem = ConvertTo-PemBlock -Label 'PRIVATE KEY' -Bytes $rsa.ExportPkcs8PrivateKey()
[System.IO.File]::WriteAllText($keyPath, $keyPem)

Write-Host "Done. Certificate files:"
Write-Host "  $pemPath"
Write-Host "  $keyPath"
Write-Host ''
Write-Host 'Restart Angular: npm start'
