# Frontend Build and Deployment Guide

This guide covers building and deploying the Lean WebUI frontend for production.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Development Build](#development-build)
- [Production Build](#production-build)
- [Build Optimization](#build-optimization)
- [Environment Configuration](#environment-configuration)
- [Deployment](#deployment)
- [Troubleshooting](#troubleshooting)

---

## Prerequisites

- Node.js 18+ and npm
- TypeScript 5.9+
- Git

Verify installation:

```bash
node --version  # Should be v18 or higher
npm --version   # Should be 9 or higher
```

---

## Development Build

### Install Dependencies

```bash
cd WebUI/WebUI.Frontend
npm install
```

### Start Development Server

```bash
npm run dev
```

This starts Vite dev server at http://localhost:5173 with:
- Hot Module Replacement (HMR)
- Fast refresh
- Source maps
- API proxy to backend

### Development Environment Variables

Create `.env.development`:

```bash
VITE_API_URL=http://localhost:5000
VITE_ENABLE_MOCK=false
```

---

## Production Build

### Quick Build (Linux/macOS)

```bash
cd WebUI/scripts
chmod +x build-frontend.sh
./build-frontend.sh production
```

### Quick Build (Windows)

```powershell
cd WebUI\scripts
.\build-frontend.ps1 -Mode "production"
```

### Manual Build Steps

```bash
cd WebUI/WebUI.Frontend

# 1. Install dependencies
npm install

# 2. Run linter
npm run lint

# 3. Type check
npm run type-check

# 4. Run tests
npm test -- --run

# 5. Build for production
npm run build:prod
```

### Build Output

```
dist/
├── index.html            # Entry point
├── assets/              # Static assets
│   ├── images/
│   └── fonts/
└── js/                  # JavaScript bundles
    ├── index-[hash].js      # Main entry
    ├── react-vendor-[hash].js    # React chunks
    ├── antd-vendor-[hash].js     # Ant Design chunks
    ├── chart-vendor-[hash].js    # Chart libraries
    ├── signalr-vendor-[hash].js  # SignalR
    └── http-vendor-[hash].js     # Axios, Zustand
```

Total size: ~2-3 MB (gzipped: ~600-800 KB)

---

## Build Optimization

### Analyze Bundle Size

To visualize what's in your bundle:

```bash
npm run build:analyze
```

This opens an interactive treemap showing bundle composition.

**Bundle Splitting Strategy:**

Our build splits code into:
- **react-vendor**: React, React-DOM, React Router (~140 KB gzipped)
- **antd-vendor**: Ant Design (~300 KB gzipped)
- **chart-vendor**: ECharts, Lightweight Charts (~200 KB gzipped)
- **signalr-vendor**: SignalR client (~50 KB gzipped)
- **http-vendor**: Axios, Zustand (~40 KB gzipped)
- **App code**: Your application logic (~100-200 KB gzipped)

### Production Optimizations

The build automatically applies:

- **Minification**: Using Terser
- **Tree shaking**: Remove unused code
- **Code splitting**: Lazy load routes and vendors
- **Asset optimization**: Compress images, inline small assets
- **Source map removal**: No source maps in production (configurable)
- **Console removal**: All `console.log` calls stripped
- **CSS optimization**: Minify and extract CSS

### Environment-Specific Builds

#### Production Build

```bash
npm run build:prod
```

- Removes all console logs and debuggers
- No source maps
- Maximum minification
- Target: ES2020+

#### Staging Build

```bash
npm run build:staging
```

- Keeps console.warn and console.error
- Includes source maps
- Less aggressive minification

---

## Environment Configuration

### Production Environment Variables

Create `.env.production`:

```bash
# API Configuration
VITE_API_URL=https://api.yourdomain.com
VITE_WS_URL=wss://api.yourdomain.com

# Feature Flags
VITE_ENABLE_ANALYTICS=true
VITE_ENABLE_ERROR_TRACKING=true

# App Configuration
VITE_APP_VERSION=1.0.0
VITE_APP_NAME="Lean WebUI"
```

### Using Environment Variables

In your code:

```typescript
const apiUrl = import.meta.env.VITE_API_URL
const isDev = import.meta.env.DEV
const isProd = import.meta.env.PROD
```

**Note**: Only variables prefixed with `VITE_` are exposed to the client.

---

## Deployment

### Option 1: Nginx (Recommended for Production)

#### Install Nginx

**Ubuntu/Debian:**
```bash
sudo apt install nginx
```

**RHEL/CentOS:**
```bash
sudo yum install nginx
```

#### Configure Nginx

Create `/etc/nginx/sites-available/leanwebui`:

```nginx
server {
    listen 80;
    server_name yourdomain.com www.yourdomain.com;
    
    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name yourdomain.com www.yourdomain.com;

    # SSL Configuration
    ssl_certificate /etc/ssl/certs/leanwebui.crt;
    ssl_certificate_key /etc/ssl/certs/leanwebui.key;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # Root directory
    root /var/www/leanwebui/dist;
    index index.html;

    # Gzip compression
    gzip on;
    gzip_types text/plain text/css application/json application/javascript text/xml application/xml application/xml+rss text/javascript;
    gzip_min_length 1000;

    # Security headers
    add_header X-Frame-Options "DENY" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # SPA fallback - serve index.html for all routes
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Don't cache index.html
    location = /index.html {
        add_header Cache-Control "no-cache, no-store, must-revalidate";
    }

    # Proxy API requests to backend
    location /api/ {
        proxy_pass http://localhost:5000/api/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }

    # Proxy WebSocket connections
    location /hubs/ {
        proxy_pass http://localhost:5000/hubs/;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

#### Enable and Restart

```bash
sudo ln -s /etc/nginx/sites-available/leanwebui /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### Option 2: Apache

Create `/etc/apache2/sites-available/leanwebui.conf`:

```apache
<VirtualHost *:443>
    ServerName yourdomain.com
    DocumentRoot /var/www/leanwebui/dist

    # SSL Configuration
    SSLEngine on
    SSLCertificateFile /etc/ssl/certs/leanwebui.crt
    SSLCertificateKeyFile /etc/ssl/certs/leanwebui.key

    # Enable mod_rewrite for SPA routing
    <Directory /var/www/leanwebui/dist>
        Options -Indexes +FollowSymLinks
        AllowOverride All
        Require all granted

        # SPA fallback
        RewriteEngine On
        RewriteBase /
        RewriteRule ^index\.html$ - [L]
        RewriteCond %{REQUEST_FILENAME} !-f
        RewriteCond %{REQUEST_FILENAME} !-d
        RewriteRule . /index.html [L]
    </Directory>

    # Proxy API requests
    ProxyPass /api http://localhost:5000/api
    ProxyPassReverse /api http://localhost:5000/api

    # Proxy WebSocket
    ProxyPass /hubs ws://localhost:5000/hubs
    ProxyPassReverse /hubs ws://localhost:5000/hubs
</VirtualHost>
```

Enable:

```bash
sudo a2enmod rewrite proxy proxy_http proxy_wstunnel ssl
sudo a2ensite leanwebui
sudo systemctl restart apache2
```

### Option 3: ASP.NET Core Static Files

Serve frontend from the backend (simpler deployment):

**Program.cs**:

```csharp
// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Fallback to index.html for SPA routing
app.MapFallbackToFile("index.html");
```

Copy build to `WebUI.API/wwwroot`:

```bash
cp -r WebUI.Frontend/dist/* WebUI.API/wwwroot/
```

### Option 4: Docker

See `DOCKER_DEPLOYMENT.md` for containerized deployment.

---

## Troubleshooting

### Build Failures

#### "Cannot find module"

```bash
rm -rf node_modules package-lock.json
npm install
```

#### "Out of memory"

Increase Node memory:

```bash
export NODE_OPTIONS="--max-old-space-size=4096"
npm run build
```

#### TypeScript errors

```bash
npm run type-check
```

Fix errors before building.

### Runtime Issues

#### Blank page after deployment

Check:
1. Browser console for errors
2. Network tab for failed requests
3. Ensure API URL is correct in `.env.production`
4. Verify SPA fallback is configured

#### "404 Not Found" on refresh

SPA routing requires server fallback to `index.html`. Configure your web server (see Nginx/Apache examples above).

#### API calls fail

Check:
1. CORS is configured correctly on backend
2. API URL matches in `.env.production`
3. Network tab shows correct request URL
4. Backend is running and accessible

#### WebSocket connection fails

Check:
1. WSS (secure WebSocket) is used if site is HTTPS
2. Proxy configuration for `/hubs` path
3. SignalR connection URL is correct

---

## Production Checklist

### Before Building

- [ ] All tests pass (`npm test`)
- [ ] No TypeScript errors (`npm run type-check`)
- [ ] No linting errors (`npm run lint`)
- [ ] Environment variables configured in `.env.production`
- [ ] API URL points to production backend

### After Building

- [ ] Build completes without errors
- [ ] Bundle size is reasonable (<3 MB total)
- [ ] No sensitive data in build artifacts
- [ ] Source maps are disabled

### Before Deploying

- [ ] Test build locally (`npm run preview:prod`)
- [ ] Verify API integration works
- [ ] Check console for errors
- [ ] Test on target browsers (Chrome, Firefox, Safari, Edge)
- [ ] Verify mobile responsiveness

### After Deploying

- [ ] HTTPS is working
- [ ] SPA routing works (refresh on any route)
- [ ] API calls succeed
- [ ] WebSocket connections work
- [ ] Static assets load correctly
- [ ] Check browser console for errors
- [ ] Verify caching headers are set
- [ ] Test authentication flow

---

## Performance Tips

1. **Enable compression**: Gzip or Brotli on your web server
2. **Use CDN**: Serve static assets from CDN
3. **Cache aggressively**: Set long cache times for hashed assets
4. **Lazy load routes**: Use React.lazy() for route components
5. **Code split large libraries**: Import only what you need
6. **Optimize images**: Use WebP format, compress images
7. **Monitor bundle size**: Run `npm run build:analyze` regularly

---

## References

- [Vite Production Build](https://vitejs.dev/guide/build.html)
- [React Production Optimization](https://react.dev/learn/rendering-lists#optimizing-with-keys)
- [Nginx SPA Configuration](https://www.nginx.com/blog/deploying-nginx-nginx-plus-docker/)

For more help, see [WebUI Documentation](../docs/) or open an issue.
