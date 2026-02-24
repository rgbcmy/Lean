# Generate Self-Signed HTTPS Certificate for Lean WebUI (Windows/PowerShell)
# Usage: .\generate-cert.ps1 [-Domain "localhost"] [-OutputPath "./certs"] [-Password "auto"]
# Example: .\generate-cert.ps1 -Domain "myapp.local" -OutputPath "C:\certs"

param(
    [string]$Domain = "localhost",
    [string]$OutputPath = "./certs",
    [string]$Password = ""
)

Write-Host "=== Lean WebUI Certificate Generator (Windows) ===" -ForegroundColor Cyan
Write-Host "Domain: $Domain" -ForegroundColor Yellow
Write-Host "Output Path: $OutputPath" -ForegroundColor Yellow
Write-Host ""

# Create output directory if it doesn't exist
if (-not (Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
}

# Generate random password if not provided
if ([string]::IsNullOrEmpty($Password)) {
    $Password = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
}

# Convert password to SecureString
$SecurePassword = ConvertTo-SecureString -String $Password -Force -AsPlainText

try {
    Write-Host "[1/3] Generating self-signed certificate..." -ForegroundColor Green
    
    # Create certificate
    $cert = New-SelfSignedCertificate `
        -DnsName $Domain, "www.$Domain", "localhost" `
        -CertStoreLocation "cert:\LocalMachine\My" `
        -NotAfter (Get-Date).AddYears(1) `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -KeyUsage DigitalSignature, KeyEncipherment `
        -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.1") `
        -FriendlyName "Lean WebUI Development Certificate"

    Write-Host "[2/3] Exporting certificate to PFX..." -ForegroundColor Green
    
    # Export to PFX
    $pfxPath = Join-Path $OutputPath "leanwebui.pfx"
    Export-PfxCertificate `
        -Cert $cert `
        -FilePath $pfxPath `
        -Password $SecurePassword | Out-Null

    Write-Host "[3/3] Exporting certificate to CRT..." -ForegroundColor Green
    
    # Export to CRT (public certificate)
    $crtPath = Join-Path $OutputPath "leanwebui.crt"
    Export-Certificate `
        -Cert $cert `
        -FilePath $crtPath | Out-Null

    # Save password to file
    $passwordFile = Join-Path $OutputPath ".cert_password"
    $Password | Out-File -FilePath $passwordFile -NoNewline -Encoding ASCII

    Write-Host ""
    Write-Host "✓ Certificate generated successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Files created:" -ForegroundColor Cyan
    Write-Host "  - PFX Bundle:  $pfxPath"
    Write-Host "  - Certificate: $crtPath"
    Write-Host ""
    Write-Host "Certificate Thumbprint: $($cert.Thumbprint)" -ForegroundColor Yellow
    Write-Host "Certificate Password:   $Password" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "⚠️  SAVE THIS PASSWORD - You'll need it for appsettings.Production.json" -ForegroundColor Red
    Write-Host ""
    Write-Host "To use this certificate:" -ForegroundColor Cyan
    Write-Host "1. Import the certificate into Trusted Root Certification Authorities (optional, for development):"
    Write-Host "   Import-Certificate -FilePath '$crtPath' -CertStoreLocation Cert:\LocalMachine\Root"
    Write-Host "2. Set CERT_PATH=$pfxPath in .env.production"
    Write-Host "3. Set CERT_PASSWORD=$Password in .env.production"
    Write-Host ""
    Write-Host "Password also saved to: $passwordFile" -ForegroundColor Gray
    Write-Host ""
    Write-Host "⚠️  For production use, consider using Let's Encrypt (see HTTPS_SETUP.md)" -ForegroundColor Red

    # Remove certificate from store (optional - keep it if you want to trust it locally)
    Write-Host ""
    $removeCert = Read-Host "Remove certificate from local machine store? (y/N)"
    if ($removeCert -eq 'y' -or $removeCert -eq 'Y') {
        Remove-Item -Path "cert:\LocalMachine\My\$($cert.Thumbprint)" -Force
        Write-Host "Certificate removed from store." -ForegroundColor Green
    } else {
        Write-Host "Certificate remains in store at: cert:\LocalMachine\My\$($cert.Thumbprint)" -ForegroundColor Yellow
    }

} catch {
    Write-Host "Error generating certificate: $_" -ForegroundColor Red
    exit 1
}
