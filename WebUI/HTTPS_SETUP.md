# HTTPS Setup Guide for Lean WebUI

This guide covers HTTPS configuration for Lean WebUI in both development and production environments.

## Table of Contents

- [Development (Self-Signed Certificate)](#development-self-signed-certificate)
- [Production (Let's Encrypt)](#production-lets-encrypt)
- [Production (Custom Certificate)](#production-custom-certificate)
- [Certificate Renewal](#certificate-renewal)
- [Troubleshooting](#troubleshooting)

---

## Development (Self-Signed Certificate)

For local development and testing, you can use self-signed certificates.

### Linux/macOS

```bash
cd WebUI/scripts
chmod +x generate-cert.sh
./generate-cert.sh localhost ./certs
```

This will create:
- `certs/leanwebui.pfx` - Certificate bundle for .NET
- `certs/leanwebui.crt` - Public certificate
- `certs/leanwebui.key` - Private key
- `certs/.cert_password` - Certificate password

### Windows (PowerShell as Administrator)

```powershell
cd WebUI\scripts
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\generate-cert.ps1 -Domain "localhost" -OutputPath ".\certs"
```

### Trust the Certificate (Development Only)

**Linux:**
```bash
sudo cp certs/leanwebui.crt /usr/local/share/ca-certificates/
sudo update-ca-certificates
```

**macOS:**
```bash
sudo security add-trusted-cert -d -r trustRoot -k /Library/Keychains/System.keychain certs/leanwebui.crt
```

**Windows:**
```powershell
Import-Certificate -FilePath "certs\leanwebui.crt" -CertStoreLocation Cert:\LocalMachine\Root
```

### Configure appsettings.json

Update `appsettings.Development.json`:

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://localhost:5001",
        "Certificate": {
          "Path": "certs/leanwebui.pfx",
          "Password": "YOUR_PASSWORD_FROM_.cert_password"
        }
      }
    }
  }
}
```

---

## Production (Let's Encrypt)

For production deployments with a public domain, use Let's Encrypt for free, automatically-renewing certificates.

### Prerequisites

- A public domain name pointing to your server
- Port 80 and 443 open on your firewall
- Certbot installed

### Install Certbot

**Ubuntu/Debian:**
```bash
sudo apt update
sudo apt install certbot
```

**RHEL/CentOS:**
```bash
sudo yum install certbot
```

**macOS:**
```bash
brew install certbot
```

### Generate Certificate (Standalone)

If you don't have a web server running on port 80:

```bash
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com
```

### Generate Certificate (Webroot)

If you have Nginx or Apache running:

```bash
sudo certbot certonly --webroot -w /var/www/html -d yourdomain.com -d www.yourdomain.com
```

Certificates will be saved to:
- Certificate: `/etc/letsencrypt/live/yourdomain.com/fullchain.pem`
- Private Key: `/etc/letsencrypt/live/yourdomain.com/privkey.pem`

### Convert to PFX for .NET

Let's Encrypt provides PEM files, but .NET requires PFX format:

```bash
sudo openssl pkcs12 -export \
  -out /etc/ssl/certs/leanwebui.pfx \
  -inkey /etc/letsencrypt/live/yourdomain.com/privkey.pem \
  -in /etc/letsencrypt/live/yourdomain.com/fullchain.pem \
  -password pass:YOUR_SECURE_PASSWORD

sudo chmod 600 /etc/ssl/certs/leanwebui.pfx
```

### Configure appsettings.Production.json

Update environment variables in `.env.production`:

```bash
CERT_PATH=/etc/ssl/certs/leanwebui.pfx
CERT_PASSWORD=YOUR_SECURE_PASSWORD
WEBUI_FRONTEND_URL=https://yourdomain.com
```

### Auto-Renewal Setup

Create a renewal script at `/etc/letsencrypt/renewal-hooks/deploy/leanwebui-renew.sh`:

```bash
#!/bin/bash
# Auto-convert renewed certificate to PFX

DOMAIN="yourdomain.com"
PFX_PASSWORD="YOUR_SECURE_PASSWORD"

openssl pkcs12 -export \
  -out /etc/ssl/certs/leanwebui.pfx \
  -inkey /etc/letsencrypt/live/$DOMAIN/privkey.pem \
  -in /etc/letsencrypt/live/$DOMAIN/fullchain.pem \
  -password pass:$PFX_PASSWORD

chmod 600 /etc/ssl/certs/leanwebui.pfx

# Restart Lean WebUI service
systemctl restart leanwebui
```

Make it executable:

```bash
sudo chmod +x /etc/letsencrypt/renewal-hooks/deploy/leanwebui-renew.sh
```

Test renewal (dry run):

```bash
sudo certbot renew --dry-run
```

Certbot will automatically run daily and renew certificates when needed.

---

## Production (Custom Certificate)

If you have a certificate from a commercial CA (e.g., DigiCert, GlobalSign):

### Convert to PFX

If you have separate certificate and key files:

```bash
openssl pkcs12 -export \
  -out leanwebui.pfx \
  -inkey yourdomain.key \
  -in yourdomain.crt \
  -certfile ca-bundle.crt \
  -password pass:YOUR_SECURE_PASSWORD
```

### Install Certificate

```bash
sudo cp leanwebui.pfx /etc/ssl/certs/
sudo chmod 600 /etc/ssl/certs/leanwebui.pfx
```

### Configure

Update `.env.production`:

```bash
CERT_PATH=/etc/ssl/certs/leanwebui.pfx
CERT_PASSWORD=YOUR_SECURE_PASSWORD
```

---

## Certificate Renewal

### Let's Encrypt (Automatic)

Certbot auto-renews certificates. Check status:

```bash
sudo systemctl status certbot.timer
```

Manual renewal:

```bash
sudo certbot renew
```

### Self-Signed Certificate

Self-signed certificates expire in 365 days. Re-run the generation script:

```bash
./generate-cert.sh yourdomain.com ./certs
```

### Commercial Certificate

Follow your CA's renewal process, then re-import the new certificate.

---

## Troubleshooting

### "Certificate not trusted" Error

**Development:** Trust the self-signed certificate (see instructions above)

**Production:** Ensure your certificate chain is complete and the root CA is trusted.

### "Unable to load certificate" Error

Check:
1. File path is correct
2. Password is correct
3. File permissions are readable (e.g., 600)
4. PFX file is not corrupted

Test certificate:

```bash
openssl pkcs12 -info -in leanwebui.pfx -passin pass:YOUR_PASSWORD
```

### Port 443 Already in Use

Check what's using the port:

```bash
sudo netstat -tlnp | grep :443
```

Stop conflicting service or change Kestrel port.

### Let's Encrypt Rate Limits

Let's Encrypt has rate limits (50 certificates per domain per week). If you hit the limit, wait or use the staging environment for testing:

```bash
sudo certbot certonly --staging --standalone -d yourdomain.com
```

### Certificate Expired

**Let's Encrypt:** Check if auto-renewal is working:

```bash
sudo certbot renew --dry-run
```

**Self-Signed:** Regenerate the certificate.

**Commercial:** Renew through your CA.

---

## Security Best Practices

1. **Use Strong Passwords:** Generate certificate passwords with at least 32 random characters
2. **Restrict File Permissions:** `chmod 600` on private keys and PFX files
3. **Enable HSTS:** Force HTTPS for all connections (configured in appsettings.Production.json)
4. **Use TLS 1.2+:** Disable older protocols (configured in Kestrel)
5. **Regular Updates:** Keep certificates renewed before expiration
6. **Secure Storage:** Store `.env.production` securely, never commit to version control
7. **Monitor Expiration:** Set up alerts 30 days before certificate expiry

---

## Quick Reference

| Environment | Certificate Type | Validity | Auto-Renew |
|-------------|-----------------|----------|------------|
| Development | Self-Signed | 365 days | No |
| Production | Let's Encrypt | 90 days | Yes |
| Production | Commercial CA | 1-2 years | Manual |

For more help, see [WebUI Documentation](../docs/) or open an issue.
