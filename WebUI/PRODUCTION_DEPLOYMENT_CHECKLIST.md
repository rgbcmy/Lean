# Production Deployment Checklist

Complete pre-deployment checklist for Lean WebUI production release.

**Deployment Date**: _______________  
**Deployment By**: _______________  
**Reviewed By**: _______________  
**Version**: _______________

---

## Phase 1: Pre-Deployment Preparation

### Infrastructure Setup

- [ ] **Server provisioning complete**
  - OS: Ubuntu 22.04 LTS or Windows Server 2022
  - CPU: 4+ cores
  - RAM: 8+ GB
  - Disk: 100+ GB SSD
  - Network: Static IP assigned

- [ ] **DNS configured**
  - A record for domain pointing to server IP
  - SSL certificate domain matches
  - TTL set appropriately (3600 or less)

- [ ] **Firewall rules configured**
  - Port 80 (HTTP) open
  - Port 443 (HTTPS) open
  - Port 5432 (PostgreSQL) restricted to localhost or VPN
  - All other ports blocked

- [ ] **Docker installed and running**
  ```bash
  docker --version
  # Should be 20.10+ or higher
  docker-compose version
  # Should be 2.0+ or higher
  ```

---

## Phase 2: Application Configuration

### Backend Configuration

- [ ] **Environment variables configured**
  - [ ] `.env.production` file created
  - [ ] `POSTGRES_PASSWORD` set (strong, 32+ characters)
  - [ ] `JWT_SECRET` generated (64+ characters, base64)
    ```bash
    openssl rand -base64 64
    ```
  - [ ] `CERT_PASSWORD` set
  - [ ] `IBKR_TWS_HOST`, `IBKR_TWS_PORT`, `IBKR_CLIENT_ID` configured
  - [ ] `GRAFANA_PASSWORD` set

- [ ] **Database configuration reviewed**
  - [ ] Connection string correct for PostgreSQL
  - [ ] Database name: `leanwebui`
  - [ ] Username: `leanuser`
  - [ ] Max connections: 100
  - [ ] Backup retention: 30 days

- [ ] **HTTPS certificate generated**
  - [ ] Certificate file exists: `certs/leanwebui.pfx`
  - [ ] Certificate not expired (valid for at least 30 days)
  - [ ] Certificate password matches `CERT_PASSWORD`
  - [ ] Certificate includes all required domains

- [ ] **CORS configuration**
  - [ ] Allowed origins set to production frontend URL only
  - [ ] No wildcards (*) in production

- [ ] **Logging configuration**
  - [ ] Log level: Warning in production
  - [ ] Log path: `/var/log/leanwebui/`
  - [ ] Log rotation enabled (30 days retention)
  - [ ] Seq URL configured (if using)

- [ ] **Security configuration**
  - [ ] HSTS enabled
  - [ ] Security headers enabled
  - [ ] CSP policy configured
  - [ ] Rate limiting enabled (60 req/min)
  - [ ] Login lockout enabled (5 attempts, 30min lockout)

### Frontend Configuration

- [ ] **Environment variables configured**
  - [ ] `.env.production` created in `WebUI.Frontend/`
  - [ ] `VITE_API_URL` points to production API
  - [ ] Feature flags configured

- [ ] **Build configuration verified**
  - [ ] `vite.config.ts` optimized for production
  - [ ] Source maps disabled
  - [ ] Console logs stripped
  - [ ] Asset optimization enabled

- [ ] **Build completed successfully**
  ```bash
  cd WebUI.Frontend
  npm install
  npm run build:prod
  # Verify dist/ folder created
  # Verify total size <3 MB
  ```

---

## Phase 3: Code and Testing

### Version Control

- [ ] **Code ready for deployment**
  - [ ] All code committed and pushed
  - [ ] Working on `main` or `release` branch
  - [ ] No uncommitted local changes
  - [ ] Git tag created for release
    ```bash
    git tag -a v1.0.0 -m "Production release 1.0.0"
    git push origin v1.0.0
    ```

### Testing

- [ ] **All automated tests passing**
  ```bash
  cd WebUI.Tests
  dotnet test
  # All tests should PASS
  ```

- [ ] **Manual testing completed**
  - [ ] Login/logout works
  - [ ] IBKR connection successful (paper trading)
  - [ ] Place order works
  - [ ] View positions works
  - [ ] Real-time updates work (SignalR)
  - [ ] Strategy execution works
  - [ ] Backtesting works
  - [ ] Charts render correctly
  - [ ] Mobile responsive design verified

- [ ] **Security testing**
  - [ ] SQL injection tested
  - [ ] XSS vulnerability tested
  - [ ] CSRF protection verified
  - [ ] Authentication bypass tested
  - [ ] Authorization rules verified

- [ ] **Performance testing**
  - [ ] Load testing completed
  - [ ] Response time <500ms for API calls
  - [ ] Concurrent users tested (target: 10+ simultaneous)
  - [ ] Memory leaks checked

---

## Phase 4: Backup and Recovery

### Backup Configuration

- [ ] **Backup strategy implemented**
  - [ ] Automated daily backups configured
  - [ ] Backup schedule: 2 AM daily
  - [ ] Backup retention: 30 days
  - [ ] Backup location: `/backups` or off-site storage
  - [ ] Backup script tested: `scripts/backup-db.sh`

- [ ] **Backup verification**
  - [ ] Manual backup successful
    ```bash
    ./scripts/backup-db.sh
    ```
  - [ ] Restore from backup tested
  - [ ] Backup file integrity verified

### Disaster Recovery

- [ ] **DR plan documented**
  - [ ] RTO defined: 4 hours
  - [ ] RPO defined: 24 hours
  - [ ] Recovery procedures documented
  - [ ] Off-site backup location configured

- [ ] **DR drill completed**
  - [ ] Database restore tested
  - [ ] Full system restore tested
  - [ ] RTO achieved in drill
  - [ ] RPO verified

---

## Phase 5: Monitoring and Alerting

### Monitoring Setup

- [ ] **Health checks configured**
  - [ ] `/health` endpoint accessible
  - [ ] `/health/detailed` working
  - [ ] `/health/ready` for load balancer

- [ ] **Prometheus configured**
  - [ ] Metrics endpoint `/metrics` accessible
  - [ ] Scrape interval: 15s
  - [ ] Data retention: 30 days

- [ ] **Grafana dashboards**
  - [ ] System overview dashboard
  - [ ] Application metrics dashboard
  - [ ] Database performance dashboard
  - [ ] Trading activity dashboard
  - [ ] Admin user created
  - [ ] Alerts configured

- [ ] **Seq logging configured**
  - [ ] Application logs flowing to Seq
  - [ ] Log retention: 7 days
  - [ ] Error alerts configured
  - [ ] API key secured

### Alerting Rules

- [ ] **Critical alerts configured**
  - [ ] Service down alert
  - [ ] Database connection failure
  - [ ] High error rate (>1%)
  - [ ] Disk space <10%
  - [ ] Memory usage >90%
  - [ ] Certificate expiring <30 days

- [ ] **Alert destinations configured**
  - [ ] Email notifications
  - [ ] Slack/Teams webhook (optional)
  - [ ] PagerDuty integration (optional)

---

## Phase 6: Security

### SSL/TLS Configuration

- [ ] **HTTPS enforced**
  - [ ] HTTP to HTTPS redirect enabled
  - [ ] HSTS header present
  - [ ] TLS 1.2 minimum version
  - [ ] Strong cipher suites only

- [ ] **Certificate verification**
  - [ ] Certificate chain complete
  - [ ] Certificate trusted by browsers
  - [ ] No certificate warnings
  - [ ] Certificate matches domain
  ```bash
  openssl s_client -connect yourdomain.com:443 -showcerts
  ```

### Authentication & Authorization

- [ ] **JWT configuration secure**
  - [ ] Secret key strong (64+ characters)
  - [ ] Token expiration: 30 minutes
  - [ ] Refresh token: 7 days
  - [ ] HTTPS required for token transmission

- [ ] **Password policy enforced**
  - [ ] Minimum length: 8 characters
  - [ ] Complexity requirements
  - [ ] Password hashing: PBKDF2, 10,000 iterations

- [ ] **Audit logging enabled**
  - [ ] All authentication events logged
  - [ ] Failed login attempts tracked
  - [ ] Account lockout after 5 failures

### Data Protection

- [ ] **Sensitive data encrypted**
  - [ ] IBKR credentials encrypted at rest
  - [ ] Database backups encrypted (if off-site)
  - [ ] Environment variables not in version control
  - [ ] `.env.production` in `.gitignore`

- [ ] **Access control**
  - [ ] File permissions restrictive (600 for secrets)
  - [ ] Database accessible only from localhost
  - [ ] SSH key-based authentication (no passwords)
  - [ ] Firewall rules reviewed

---

## Phase 7: Documentation

### User Documentation

- [ ] **User guide complete**
  - [ ] Installation instructions
  - [ ] Getting started guide
  - [ ] Feature documentation
  - [ ] FAQ section

- [ ] **API documentation**
  - [ ] Swagger/Scalar accessible
  - [ ] All endpoints documented
  - [ ] Authentication explained
  - [ ] Example requests/responses

### Operations Documentation

- [ ] **Operations manual complete**
  - [ ] System architecture documented
  - [ ] Deployment procedures
  - [ ] Troubleshooting guide
  - [ ] Rollback procedures
  - [ ] Disaster recovery plan

- [ ] **Runbooks created**
  - [ ] Daily operations checklist
  - [ ] Incident response procedures
  - [ ] Maintenance procedures
  - [ ] Emergency contacts list

---

## Phase 8: Deployment Execution

### Pre-Deployment

- [ ] **Maintenance window scheduled**
  - [ ] Stakeholders notified
  - [ ] Maintenance banner posted
  - [ ] Backup window allowed

- [ ] **Rollback plan ready**
  - [ ] Previous version backed up
  - [ ] Rollback tested in staging
  - [ ] Rollback steps documented

### Deployment Steps

- [ ] **Step 1: Final backup**
  ```bash
  ./scripts/backup-db.sh
  # Save backup location: _______________
  ```

- [ ] **Step 2: Stop existing services** (if upgrading)
  ```bash
  docker-compose down
  ```

- [ ] **Step 3: Pull latest code**
  ```bash
  git pull origin main
  git checkout v1.0.0  # Use specific tag
  ```

- [ ] **Step 4: Build images**
  ```bash
  cd WebUI
  docker-compose -f docker-compose.prod.yml build
  ```

- [ ] **Step 5: Start services**
  ```bash
  docker-compose -f docker-compose.prod.yml up -d
  ```

- [ ] **Step 6: Verify health**
  ```bash
  curl http://localhost/health
  # Should return status: "Healthy"
  ```

### Post-Deployment Verification

- [ ] **Services running**
  ```bash
  docker ps
  # All containers should be Up
  ```

- [ ] **Logs clean**
  ```bash
  docker logs lean-webui-api --tail=50
  # No critical errors
  ```

- [ ] **Health checks passing**
  - [ ] `/health` returns 200 OK
  - [ ] `/health/detailed` shows all checks healthy
  - [ ] Database connectivity verified

- [ ] **Application functionality**
  - [ ] Can access homepage
  - [ ] Can log in
  - [ ] API calls succeed
  - [ ] WebSocket connections work
  - [ ] Real-time data updates

- [ ] **Performance acceptable**
  - [ ] Page load time <2s
  - [ ] API response time <500ms
  - [ ] CPU usage <50%
  - [ ] Memory usage <70%

---

## Phase 9: Monitoring Period

### First 30 Minutes

- [ ] **Watch logs continuously**
  ```bash
  docker-compose logs -f
  ```

- [ ] **Monitor metrics**
  - [ ] CPU, memory, disk usage
  - [ ] Request rate
  - [ ] Error rate
  - [ ] Response time

- [ ] **Test critical paths**
  - [ ] Login flow
  - [ ] Place order (paper trading)
  - [ ] View positions
  - [ ] Real-time updates

### First 4 Hours

- [ ] **Monitor error logs**
  - [ ] Check Seq for errors
  - [ ] Review Grafana alerts
  - [ ] Check application logs

- [ ] **Performance monitoring**
  - [ ] No memory leaks detected
  - [ ] No abnormal CPU spikes
  - [ ] Database queries performant

### First 24 Hours

- [ ] **User feedback**
  - [ ] No critical bugs reported
  - [ ] Performance acceptable to users
  - [ ] All features working as expected

- [ ] **System stability**
  - [ ] No unexpected restarts
  - [ ] No data corruption
  - [ ] Backups completing successfully

---

## Phase 10: Sign-Off

### Deployment Approval

- [ ] **Technical sign-off**
  - [ ] All critical items completed
  - [ ] No blocking issues
  - [ ] Monitoring in place
  - [ ] Rollback plan ready

**Deployed By**: _____________________  Date: ___________

**Reviewed By**: _____________________  Date: ___________

**Approved By**: _____________________  Date: ___________

### Post-Deployment Report

- [ ] **Deployment summary written**
  - Deployment start time: ___________
  - Deployment end time: ___________
  - Total downtime: ___________
  - Issues encountered: ___________
  - Resolutions applied: ___________

- [ ] **Lessons learned documented**
  - What went well: ___________
  - What could be improved: ___________
  - Action items: ___________

---

## Rollback Procedure (If Needed)

### When to Rollback

Rollback immediately if:
- [ ] Critical functionality broken
- [ ] Data corruption detected
- [ ] Security vulnerability introduced
- [ ] Performance degradation >50%
- [ ] Unrecoverable errors

### Rollback Steps

1. **Stop current version**
   ```bash
   docker-compose -f docker-compose.prod.yml down
   ```

2. **Restore database** (if schema changed)
   ```bash
   gunzip < /backups/pre-deployment.sql.gz | \
     docker exec -i lean-webui-db psql -U leanuser -d leanwebui
   ```

3. **Checkout previous version**
   ```bash
   git checkout <previous-tag>
   ```

4. **Rebuild and start**
   ```bash
   docker-compose -f docker-compose.prod.yml build
   docker-compose -f docker-compose.prod.yml up -d
   ```

5. **Verify rollback**
   ```bash
   curl http://localhost/health
   docker logs lean-webui-api
   ```

**Rollback Time**: _________ minutes

---

## Emergency Contacts

| Role | Name | Phone | Email |
|------|------|-------|-------|
| Deployment Lead | _________ | _________ | _________ |
| Database Admin | _________ | _________ | _________ |
| System Admin | _________ | _________ | _________ |
| On-Call Engineer | _________ | _________ | _________ |
| Manager | _________ | _________ | _________ |

---

## Notes

_Use this section for deployment-specific notes, special configurations, or reminders._

---

**Checklist Version**: 1.0  
**Last Updated**: 2024-02-19  
**Next Review**: After each production deployment

For questions about this checklist, contact the deployment lead.
