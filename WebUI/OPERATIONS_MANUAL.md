# Lean WebUI Operations Manual

This manual provides operational procedures, troubleshooting guides, and recovery processes for Lean WebUI in production.

## Table of Contents

1. [System Overview](#system-overview)
2. [Daily Operations](#daily-operations)
3. [Monitoring](#monitoring)
4. [Troubleshooting](#troubleshooting)
5. [Deployment and Rollback](#deployment-and-rollback)
6. [Disaster Recovery](#disaster-recovery)
7. [Security Incidents](#security-incidents)
8. [Performance Tuning](#performance-tuning)
9. [Maintenance Procedures](#maintenance-procedures)
10. [Emergency Contacts](#emergency-contacts)

---

## System Overview

### Architecture Components

```
┌─────────────────────────────────────────────────────────────┐
│                        Internet                             │
└───────────────────────┬─────────────────────────────────────┘
                        │
                  ┌─────▼─────┐
                  │   Nginx   │ (Port 80/443)
                  │  Proxy    │
                  └─────┬─────┘
                        │
         ┌──────────────┼──────────────┐
         │              │              │
    ┌────▼────┐   ┌────▼────┐   ┌────▼────┐
    │ WebUI   │   │  Seq    │   │Grafana  │
    │  API    │   │ Logs    │   │Metrics  │
    └────┬────┘   └─────────┘   └────┬────┘
         │                            │
    ┌────▼────┐              ┌────────▼───────┐
    │Postgres │              │  Prometheus    │
    │Database │              │    Metrics     │
    └─────────┘              └────────────────┘
         │
    ┌────▼────┐
    │  IBKR   │
    │ TWS/GW  │
    └─────────┘
```

### Key Services

| Service | Purpose | Critical | Port |
|---------|---------|----------|------|
| Nginx | Reverse proxy, SSL termination | Yes | 80, 443 |
| WebUI API | Backend application | Yes | 5000, 5001 |
| PostgreSQL | Database | Yes | 5432 |
| Seq | Log aggregation | No | 5341 |
| Prometheus | Metrics collection | No | 9090 |
| Grafana | Dashboards | No | 3000 |

---

## Daily Operations

### Morning Checklist

```bash
# 1. Check all services are running
docker ps | grep lean-webui

# 2. Check system health
curl http://localhost/health/detailed

# 3. Review overnight logs
docker logs --since 24h lean-webui-api | grep -i error

# 4. Check disk space
df -h | grep -E '/|/var/lib/docker'

# 5. Verify database backup
ls -lth /backups | head -5

# 6. Check monitoring alerts
# Visit Grafana: http://localhost:3000

# 7. Review Seq for critical errors
# Visit Seq: http://localhost:5341
```

### Evening Checklist

```bash
# 1. Review day's trading activity
# Check orders, executions, errors

# 2. Review performance metrics
# CPU, memory, disk usage

# 3. Check for any pending alerts
# Grafana + Seq

# 4. Verify backup completed
grep "Backup completed" /var/log/backup.log

# 5. Plan tomorrow's maintenance (if any)
```

---

## Monitoring

### Key Metrics to Watch

#### Application Health

- **Health endpoint**: http://localhost/health
  - Should return `200 OK` with `status: "Healthy"`
  - Check every 30 seconds (automatic)

#### Performance Metrics

| Metric | Normal Range | Warning | Critical |
|--------|--------------|---------|----------|
| CPU Usage | 0-60% | 60-80% | >80% |
| Memory Usage | 0-70% | 70-85% | >85% |
| Disk Usage | 0-70% | 70-85% | >85% |
| API Response Time | <200ms | 200-500ms | >500ms |
| Database Connections | <50 | 50-80 | >80 |
| Error Rate | <0.1% | 0.1-1% | >1% |

#### Business Metrics

- Active trading strategies: Monitor count
- Order fill rate: Should be >95%
- IBKR connection status: Should be "Connected"
- Failed logins: Should be <5/hour

### Monitoring Dashboards

**Grafana Dashboards**: http://localhost:3000

1. **System Overview**: CPU, memory, disk, network
2. **Application Metrics**: Request rate, errors, latency
3. **Database Performance**: Query time, connections, locks
4. **Trading Activity**: Orders, fills, rejections

**Seq Log Search**: http://localhost:5341

- Filter by level: Error, Warning
- Search recent errors: `@Level = 'Error' and @Timestamp > Now() - 1h`

---

## Troubleshooting

### Service Won't Start

**Symptom**: Docker container fails to start

**Diagnosis**:
```bash
# Check container logs
docker logs lean-webui-api

# Check service dependencies
docker-compose ps

# Check port conflicts
sudo netstat -tlnp | grep :5000
```

**Common Causes**:

1. **Port already in use**
   ```bash
   # Find process using port
   sudo lsof -i :5000
   
   # Kill process or change port in docker-compose.yml
   ```

2. **Database not ready**
   ```bash
   # Check database health
   docker exec lean-webui-db pg_isready -U leanuser
   
   # Restart database
   docker-compose restart postgres
   ```

3. **Certificate errors**
   ```bash
   # Verify certificate exists
   ls -la certs/leanwebui.pfx
   
   # Check permissions
   chmod 600 certs/leanwebui.pfx
   ```

### Database Connection Errors

**Symptom**: "Could not connect to database" errors

**Diagnosis**:
```bash
# Test database connection
docker exec lean-webui-api nc -zv postgres 5432

# Check database logs
docker logs lean-webui-db | tail -50

# Check connection string
docker exec lean-webui-api env | grep ConnectionStrings
```

**Solutions**:

1. **Database not running**
   ```bash
   docker-compose restart postgres
   ```

2. **Wrong credentials**
   ```bash
   # Verify in .env.production
   grep POSTGRES_PASSWORD .env.production
   
   # Update and restart
   docker-compose down
   docker-compose up -d
   ```

3. **Too many connections**
   ```sql
   -- Connect to database
   docker exec -it lean-webui-db psql -U leanuser -d leanwebui
   
   -- Check active connections
   SELECT count(*) FROM pg_stat_activity;
   
   -- Terminate idle connections
   SELECT pg_terminate_backend(pid) 
   FROM pg_stat_activity 
   WHERE state = 'idle' AND state_change < NOW() - INTERVAL '10 minutes';
   ```

### High CPU/Memory Usage

**Symptom**: System slow, high resource usage

**Diagnosis**:
```bash
# Check Docker stats
docker stats

# Check system resources
top
htop

# Check process details
docker exec lean-webui-api ps aux
```

**Solutions**:

1. **Memory leak** (restart service)
   ```bash
   docker-compose restart webui-api
   ```

2. **Too many concurrent requests** (rate limiting)
   - Check Seq for unusual request patterns
   - Verify rate limiting is enabled

3. **Database query performance**
   ```sql
   -- Find slow queries
   SELECT pid, now() - pg_stat_activity.query_start AS duration, query 
   FROM pg_stat_activity 
   WHERE state != 'idle' 
   ORDER BY duration DESC 
   LIMIT 10;
   ```

### IBKR Connection Issues

**Symptom**: "IBKR connection failed" errors

**Diagnosis**:
```bash
# Check IBKR status via API
curl http://localhost/api/v1/ibkr/diagnostics

# Check logs
docker logs lean-webui-api | grep IBKR
```

**Solutions**:

1. **TWS/Gateway not running**
   - Start IBKR TWS or Gateway
   - Verify it's listening on configured port

2. **Wrong port or host**
   ```bash
   # Verify configuration
   docker exec lean-webui-api env | grep IBKR
   
   # Test connection
   telnet $IBKR_HOST $IBKR_PORT
   ```

3. **API not enabled in TWS**
   - TWS → File → Global Configuration → API → Settings
   - Enable "Enable ActiveX and Socket Clients"
   - Add trusted IP addresses

### SSL/HTTPS Errors

**Symptom**: Certificate warnings or connection refused

**Diagnosis**:
```bash
# Test HTTPS
curl -I https://localhost

# Check certificate
openssl s_client -connect localhost:443 -showcerts

# Check Nginx logs
docker logs lean-webui-nginx | tail -50
```

**Solutions**:

1. **Certificate expired**
   ```bash
   # Check expiry
   openssl x509 -in certs/leanwebui.crt -noout -dates
   
   # Regenerate if needed
   ./scripts/generate-cert.sh yourdomain.com ./certs
   ```

2. **Certificate not trusted**
   - For development: Trust certificate in browser/OS
   - For production: Use Let's Encrypt

3. **Wrong certificate path**
   ```bash
   # Verify in docker-compose.prod.yml
   grep CERT_PATH docker-compose.prod.yml
   ```

---

## Deployment and Rollback

### Deployment Procedure

#### 1. Pre-Deployment Checks

```bash
# Run all tests
cd WebUI.Tests
dotnet test

# Build frontend
cd ../WebUI.Frontend
npm run build:prod

# Backup database
./scripts/backup-db.sh
```

#### 2. Deploy New Version

```bash
# Pull latest code
git pull origin main

# Build new images
docker-compose -f docker-compose.prod.yml build

# Stop old version
docker-compose -f docker-compose.prod.yml down

# Start new version
docker-compose -f docker-compose.prod.yml up -d

# Verify deployment
curl http://localhost/health
```

#### 3. Post-Deployment Verification

```bash
# Check all services
docker ps

# Check logs for errors
docker-compose logs --tail=100 webui-api

# Test critical functions
# - Login
# - Place order (paper trading)
# - View positions
# - Check real-time updates

# Monitor for 30 minutes
watch -n 10 'curl -s http://localhost/health | jq'
```

### Rollback Procedure

**When to Rollback**:
- Critical functionality broken
- Data corruption detected
- Security vulnerability introduced
- Performance degradation >50%

**Rollback Steps**:

```bash
# 1. Stop current version
docker-compose -f docker-compose.prod.yml down

# 2. Restore database (if needed)
gunzip < backups/pre-deployment_$(date +%Y%m%d).sql.gz | \
  docker exec -i lean-webui-db psql -U leanuser -d leanwebui

# 3. Checkout previous version
git checkout <previous-commit-hash>

# 4. Rebuild and restart
docker-compose -f docker-compose.prod.yml build
docker-compose -f docker-compose.prod.yml up -d

# 5. Verify rollback
curl http://localhost/health
docker logs lean-webui-api
```

### Database Migration Rollback

```bash
# List migrations
docker exec lean-webui-api dotnet ef migrations list

# Rollback to specific migration
docker exec lean-webui-api dotnet ef database update <MigrationName>
```

---

## Disaster Recovery

### Scenarios and Procedures

#### Complete System Failure

**Recovery Time Objective (RTO)**: 4 hours  
**Recovery Point Objective (RPO)**: 24 hours

**Procedure**:

```bash
# 1. Provision new server
# 2. Install Docker and Docker Compose
# 3. Clone repository
git clone <repository-url>
cd Lean/WebUI

# 4. Restore configuration
cp /backup/.env.production .env.production

# 5. Restore certificates
cp /backup/certs/* ./certs/

# 6. Start database
docker-compose -f docker-compose.prod.yml up -d postgres

# 7. Restore database from backup
gunzip < /backup/leanwebui_latest.sql.gz | \
  docker exec -i lean-webui-db psql -U leanuser -d leanwebui

# 8. Start all services
docker-compose -f docker-compose.prod.yml up -d

# 9. Verify system
curl http://localhost/health
```

#### Database Corruption

```bash
# 1. Stop application
docker-compose -f docker-compose.prod.yml stop webui-api

# 2. Backup current state
docker exec lean-webui-db pg_dump -U leanuser leanwebui > corrupted_$(date +%Y%m%d).sql

# 3. Restore from last good backup
gunzip < backups/leanwebui_$(date -d yesterday +%Y%m%d)_*.sql.gz | \
  docker exec -i lean-webui-db psql -U leanuser -d leanwebui

# 4. Restart application
docker-compose -f docker-compose.prod.yml start webui-api

# 5. Verify data integrity
# Check recent trades, orders, positions
```

#### Data Center Outage

**Prerequisites**:
- Off-site backups (S3, Azure Blob, etc.)
- Documented recovery procedure
- Secondary data center or cloud provider

**Procedure**:
1. Activate disaster recovery site
2. Restore from off-site backups
3. Update DNS to point to DR site
4. Verify all services operational
5. Monitor for issues

---

## Security Incidents

### Suspected Breach

**Immediate Actions**:

```bash
# 1. Isolate system
# Disconnect from internet or disable external access
sudo iptables -A INPUT -j DROP
sudo iptables -A OUTPUT -j DROP

# 2. Preserve evidence
docker logs lean-webui-api > incident_$(date +%Y%m%d_%H%M%S).log
docker exec lean-webui-db pg_dump -U leanuser leanwebui > db_snapshot_$(date +%Y%m%d_%H%M%S).sql

# 3. Review access logs
grep -i "401\|403\|500" /var/log/nginx/access.log > suspicious_access.log

# 4. Check for unauthorized access
SELECT * FROM AuditLogs 
WHERE Timestamp > NOW() - INTERVAL '24 hours' 
ORDER BY Timestamp DESC;

# 5. Contact security team
# Notify stakeholders
```

### Credential Compromise

**JWT Secret Compromised**:

```bash
# 1. Generate new secret
NEW_JWT_SECRET=$(openssl rand -base64 64)

# 2. Update configuration
echo "JWT_SECRET=$NEW_JWT_SECRET" >> .env.production

# 3. Restart application
docker-compose -f docker-compose.prod.yml restart webui-api

# 4. Force all users to re-authenticate
# Invalidate all refresh tokens in database
UPDATE RefreshTokens SET IsRevoked = TRUE;
```

**Database Password Compromised**:

```bash
# 1. Change password in PostgreSQL
docker exec -it lean-webui-db psql -U postgres
ALTER USER leanuser WITH PASSWORD 'new-secure-password';

# 2. Update configuration
# Edit .env.production with new password

# 3. Restart services
docker-compose -f docker-compose.prod.yml restart
```

---

## Performance Tuning

### Database Optimization

```sql
-- Analyze query performance
EXPLAIN ANALYZE SELECT * FROM Orders WHERE UserId = 1 ORDER BY Timestamp DESC LIMIT 100;

-- Update statistics
ANALYZE;

-- Reindex
REINDEX DATABASE leanwebui;

-- Vacuum
VACUUM ANALYZE;
```

### Application Tuning

```bash
# Increase worker threads (edit appsettings.Production.json)
"Kestrel": {
  "Limits": {
    "MaxConcurrentConnections": 100,
    "MaxConcurrentUpgradedConnections": 100
  }
}

# Enable response caching
# Already configured in Program.cs

# Monitor thread pool
docker exec lean-webui-api dotnet-counters monitor --process-id 1
```

---

## Maintenance Procedures

### Weekly Tasks

- [ ] Review error logs
- [ ] Check disk space
- [ ] Verify backups
- [ ] Review security alerts
- [ ] Update documentation if needed

### Monthly Tasks

- [ ] Update dependencies
- [ ] Review and rotate logs
- [ ] Test disaster recovery
- [ ] Performance review
- [ ] Security audit

### Quarterly Tasks

- [ ] Disaster recovery drill
- [ ] Capacity planning review
- [ ] SSL certificate renewal check
- [ ] Penetration testing
- [ ] Infrastructure cost review

---

## Emergency Contacts

### Escalation Path

| Level | Contact | Response Time |
|-------|---------|---------------|
| L1 | On-call Engineer | 15 minutes |
| L2 | Senior Engineer | 1 hour |
| L3 | System Architect | 4 hours |

### External Vendors

- **IBKR Support**: 1-877-442-2757
- **Cloud Provider**: [Provider support number]
- **Security Team**: [Security team contact]

---

## Appendix

### Useful Commands

```bash
# Quick health check
curl -s http://localhost/health | jq

# View recent logs
docker logs --tail=100 --follow lean-webui-api

# Database query
docker exec -it lean-webui-db psql -U leanuser -d leanwebui

# Restart service
docker-compose -f docker-compose.prod.yml restart webui-api

# View resource usage
docker stats --no-stream

# Export metrics
curl http://localhost:5000/metrics

# Check SSL expiry
echo | openssl s_client -servername localhost -connect localhost:443 2>/dev/null | openssl x509 -noout -dates
```

### Log Locations

| Component | Location |
|-----------|----------|
| Application | Docker logs or `/app/logs` |
| Nginx | `/var/log/nginx` |
| PostgreSQL | Docker logs |
| System | `/var/log/syslog` |
| Backup | `/var/log/backup.log` |

---

**Document Version**: 1.0  
**Last Updated**: 2024-02-19  
**Next Review**: 2024-05-19  
**Owner**: DevOps Team

For questions or updates to this manual, please contact the operations team.
