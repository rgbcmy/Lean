# Security Configuration Guide for Lean WebUI

This document covers the security configurations implemented in Lean WebUI, including HTTPS, HSTS, Content Security Policy, and other security headers.

## Table of Contents

- [Overview](#overview)
- [HTTPS Configuration](#https-configuration)
- [HSTS (HTTP Strict Transport Security)](#hsts-http-strict-transport-security)
- [Security Headers](#security-headers)
- [Content Security Policy](#content-security-policy)
- [CORS Configuration](#cors-configuration)
- [JWT Security](#jwt-security)
- [Production Security Checklist](#production-security-checklist)

---

## Overview

Lean WebUI implements multiple layers of security to protect against common web vulnerabilities:

1. **Transport Layer Security**: HTTPS with TLS 1.2+
2. **HSTS**: Force browsers to always use HTTPS
3. **Security Headers**: Prevent XSS, clickjacking, and content sniffing
4. **CSP**: Content Security Policy to prevent injection attacks
5. **CORS**: Strict cross-origin resource sharing policies
6. **JWT Authentication**: Secure token-based authentication
7. **Password Security**: PBKDF2 hashing with 10,000 iterations
8. **Audit Logging**: All security-relevant events are logged

---

## HTTPS Configuration

### Configuration in appsettings.Production.json

```json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://*:5001",
        "Certificate": {
          "Path": "${CERT_PATH:/etc/ssl/certs/leanwebui.pfx}",
          "Password": "${CERT_PASSWORD}"
        }
      }
    }
  },
  "Security": {
    "EnableHTTPSRedirect": true,
    "RequireHTTPS": true
  }
}
```

### Environment Variables

Set these in `.env.production`:

```bash
CERT_PATH=/etc/ssl/certs/leanwebui.pfx
CERT_PASSWORD=your-secure-password
```

### Redirect HTTP to HTTPS

Enabled by default in production. To disable (not recommended):

```json
{
  "Security": {
    "EnableHTTPSRedirect": false
  }
}
```

---

## HSTS (HTTP Strict Transport Security)

HSTS forces browsers to always use HTTPS, even if users type `http://` in the address bar.

### Configuration

```json
{
  "Security": {
    "EnableHSTS": true,
    "HSTSMaxAge": 31536000
  }
}
```

### Parameters

- **EnableHSTS**: Enable/disable HSTS (enabled by default in production)
- **HSTSMaxAge**: Duration in seconds (default: 31536000 = 1 year)
- **IncludeSubDomains**: Always enabled
- **Preload**: Enabled (allows inclusion in browser HSTS preload lists)

### How It Works

When a browser receives the HSTS header:

```
Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

It will:
1. Automatically convert all HTTP requests to HTTPS for the specified duration
2. Prevent users from bypassing certificate warnings
3. Apply to all subdomains

### HSTS Preload

To add your domain to the HSTS preload list:

1. Ensure HSTS is enabled with `max-age` ≥ 31536000 (1 year)
2. Submit your domain at https://hstspreload.org/
3. Wait for inclusion in browser updates

⚠️ **Warning**: HSTS preload is permanent and cannot be easily undone. Only enable for production domains.

---

## Security Headers

Lean WebUI automatically adds the following security headers to all responses:

### X-Content-Type-Options

Prevents browsers from MIME-sniffing responses away from the declared content-type.

```
X-Content-Type-Options: nosniff
```

**Protection**: Prevents content-sniffing attacks

### X-Frame-Options

Prevents the page from being embedded in iframes, protecting against clickjacking.

```
X-Frame-Options: DENY
```

**Options**:
- `DENY`: Never allow framing
- `SAMEORIGIN`: Allow framing only from same origin

**Configuration**:
```json
{
  "Security": {
    "EnableXFrameOptions": true,
    "XFrameOptionsValue": "DENY"
  }
}
```

### X-XSS-Protection

Enables the browser's built-in XSS filter.

```
X-XSS-Protection: 1; mode=block
```

**Protection**: Blocks pages when XSS attack is detected

### Referrer-Policy

Controls how much referrer information is sent with requests.

```
Referrer-Policy: strict-origin-when-cross-origin
```

**Behavior**:
- Same-origin: Full URL
- Cross-origin: Origin only
- Downgrade (HTTPS → HTTP): No referrer

### Permissions-Policy

Controls which browser features can be used.

```
Permissions-Policy: camera=(), microphone=(), geolocation=()
```

**Effect**: Disables camera, microphone, and geolocation APIs

---

## Content Security Policy

CSP prevents XSS attacks by controlling which resources can be loaded.

### Default Policy

```
Content-Security-Policy: default-src 'self'; 
  script-src 'self' 'unsafe-inline' 'unsafe-eval'; 
  style-src 'self' 'unsafe-inline'; 
  img-src 'self' data: https:; 
  font-src 'self' data:; 
  connect-src 'self' wss:
```

### Directive Breakdown

| Directive | Allowed Sources | Purpose |
|-----------|----------------|---------|
| `default-src` | `'self'` | Default for all resource types |
| `script-src` | `'self' 'unsafe-inline' 'unsafe-eval'` | JavaScript sources (allows React inline scripts) |
| `style-src` | `'self' 'unsafe-inline'` | CSS sources (allows inline styles) |
| `img-src` | `'self' data: https:` | Images from same origin, data URIs, or HTTPS |
| `font-src` | `'self' data:` | Fonts from same origin or data URIs |
| `connect-src` | `'self' wss:` | API and WebSocket connections |

### Configuration

```json
{
  "Security": {
    "EnableCSP": true,
    "CSPPolicy": "default-src 'self'; script-src 'self' 'unsafe-inline'"
  }
}
```

### Customizing CSP

To allow external resources (e.g., CDN):

```json
{
  "CSPPolicy": "default-src 'self'; script-src 'self' https://cdn.example.com; style-src 'self' https://cdn.example.com"
}
```

### CSP Reporting

To receive violation reports, add `report-uri` or `report-to`:

```
Content-Security-Policy: default-src 'self'; report-uri /api/csp-report
```

⚠️ **Note**: `'unsafe-inline'` and `'unsafe-eval'` are less secure but required for React. Consider using nonces or hashes for stricter policies.

---

## CORS Configuration

CORS (Cross-Origin Resource Sharing) controls which domains can access the API.

### Production Configuration

```json
{
  "CORS": {
    "AllowedOrigins": [
      "${WEBUI_FRONTEND_URL:https://yourdomain.com}"
    ],
    "AllowCredentials": true
  }
}
```

### Environment Variable

```bash
WEBUI_FRONTEND_URL=https://yourdomain.com
```

### Development vs Production

| Environment | Allowed Origins |
|-------------|----------------|
| Development | `http://localhost:3000`, `http://localhost:5173` |
| Production | Only specified frontend URL |

### Security Implications

- **AllowCredentials: true**: Cookies and Authorization headers are allowed
- **Wildcard (`*`) not allowed**: When credentials are enabled, must specify exact origins

---

## JWT Security

### Token Configuration

```json
{
  "WebUI": {
    "JwtSecret": "${JWT_SECRET}",
    "JwtExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7,
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 30
  }
}
```

### Generating a Secure Secret

```bash
# Generate 64-character base64 secret
openssl rand -base64 64
```

Set in `.env.production`:

```bash
JWT_SECRET=your-generated-secret-here
```

⚠️ **Requirements**:
- Minimum 32 characters
- Use strong random generation
- Never commit to version control
- Rotate periodically (requires re-authentication)

### Token Expiration

- **Access Token**: 30 minutes (short-lived)
- **Refresh Token**: 7 days (allows silent renewal)

### Brute Force Protection

- **Max attempts**: 5 failed logins
- **Lockout duration**: 30 minutes
- **Audit logging**: All failed attempts logged

---

## Production Security Checklist

### ✅ Before Deployment

- [ ] HTTPS certificate is valid and trusted
- [ ] JWT secret is strong (≥64 characters) and unique
- [ ] Certificate password is secure and stored securely
- [ ] `.env.production` is not in version control
- [ ] CORS allowed origins are restricted to frontend domain only
- [ ] HSTS is enabled with appropriate max-age
- [ ] Security headers are enabled
- [ ] Database connection uses strong password
- [ ] IBKR credentials are encrypted
- [ ] Audit logging is enabled
- [ ] Firewall rules allow only necessary ports (443, database port)
- [ ] Server is updated with latest security patches

### ✅ Post-Deployment

- [ ] Test HTTPS connection and certificate validity
- [ ] Verify HSTS header is present
- [ ] Verify security headers using https://securityheaders.com
- [ ] Test CSP doesn't break functionality
- [ ] Verify CORS only allows intended origins
- [ ] Test JWT authentication and expiration
- [ ] Review audit logs for suspicious activity
- [ ] Set up monitoring and alerts

### ✅ Ongoing Maintenance

- [ ] Monitor certificate expiration (renew 30 days before)
- [ ] Review and update dependencies monthly
- [ ] Rotate JWT secret every 6 months
- [ ] Review audit logs weekly
- [ ] Update CSP as needed for new resources
- [ ] Test disaster recovery procedures quarterly

---

## Testing Security Configuration

### Test HTTPS

```bash
curl -I https://yourdomain.com/health
```

Expected headers:
```
HTTP/2 200
strict-transport-security: max-age=31536000; includeSubDomains; preload
x-content-type-options: nosniff
x-frame-options: DENY
x-xss-protection: 1; mode=block
content-security-policy: default-src 'self'; ...
```

### Test HTTPS Redirect

```bash
curl -I http://yourdomain.com/health
```

Expected: `HTTP/1.1 307 Temporary Redirect` to HTTPS URL

### Test CORS

```bash
curl -I -H "Origin: https://malicious.com" https://yourdomain.com/api/v1/health
```

Expected: No `Access-Control-Allow-Origin` header

### Online Security Scanners

- https://securityheaders.com - Analyze security headers
- https://www.ssllabs.com/ssltest/ - Test SSL/TLS configuration
- https://csp-evaluator.withgoogle.com/ - Evaluate CSP policy

---

## Troubleshooting

### HSTS Clearing (Development)

If you need to clear HSTS settings in browser:

**Chrome**: `chrome://net-internals/#hsts` → Delete domain security policies
**Firefox**: `about:preferences#privacy` → Clear site data
**Edge**: `edge://net-internals/#hsts`

### CSP Violations

Check browser console for CSP violation errors:
```
Refused to load the script 'https://example.com/script.js' because it violates the Content Security Policy directive: "script-src 'self'"
```

**Solution**: Update CSP policy to allow the resource.

### CORS Errors

```
Access to fetch at 'https://api.example.com' from origin 'https://app.example.com' has been blocked by CORS policy
```

**Solution**: Add frontend origin to `CORS:AllowedOrigins` in appsettings.Production.json

---

## References

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [MDN: HSTS](https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Strict-Transport-Security)
- [MDN: CSP](https://developer.mozilla.org/en-US/docs/Web/HTTP/CSP)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [HSTS Preload List](https://hstspreload.org/)

For additional help, refer to the [Operations Manual](./OPERATIONS_MANUAL.md) or open an issue.
