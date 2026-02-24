# Disaster Recovery Drill Procedure

This document outlines the disaster recovery drill procedure to validate recovery capabilities and ensure operational readiness.

## Objectives

- Verify backup and restore procedures work correctly
- Validate Recovery Time Objective (RTO): 4 hours
- Validate Recovery Point Objective (RPO): 24 hours
- Train operations team on recovery procedures
- Identify gaps in disaster recovery plan

---

## Prerequisites

### Before the Drill

- [ ] Schedule drill during low-activity period (weekend/holiday)
- [ ] Notify all stakeholders 1 week in advance
- [ ] Maintain current production system (no downtime)
- [ ] Prepare secondary environment for recovery testing
- [ ] Ensure all participants have access and permissions
- [ ] Prepare observation checklist

### Required Resources

- Secondary server or VM for recovery testing
- Access to backup storage
- Copy of production configuration files
- Team members:
  - Operations lead
  - Database administrator
  - Application engineer
  - Observer (timing and documentation)

---

## Drill Scenarios

## Scenario 1: Database Failure

**Simulated Event**: PostgreSQL database becomes corrupted and unrecoverable

### Timeline and Steps

#### T+0: Incident Detected

**Observer starts timer**

```bash
# Simulate database corruption (on test system)
docker exec lean-webui-db-test psql -U leanuser -d leanwebui -c "DROP TABLE Users CASCADE;"
```

**Expected Detection Time**: <5 minutes via monitoring

**Actions**:
- [ ] Monitor alerts (Grafana/Seq)
- [ ] Verify database unavailable
- [ ] Notify team via incident channel

#### T+5min: Initial Response

```bash
# Stop application to prevent further damage
docker-compose -f docker-compose.prod.yml stop webui-api

# Verify backup status
ls -lth /backups/leanwebui_*.sql.gz | head -1
```

**Expected**: Latest backup from past 24 hours

#### T+15min: Recovery Initiation

```bash
# Start recovery process
cd /recovery

# 1. Find latest backup
LATEST_BACKUP=$(ls -t /backups/leanwebui_*.sql.gz | head -1)
echo "Restoring from: $LATEST_BACKUP"

# 2. Stop database
docker-compose -f docker-compose.prod.yml stop postgres

# 3. Clear database volume
docker volume rm lean-postgres-data

# 4. Recreate database
docker-compose -f docker-compose.prod.yml up -d postgres

# Wait for database to be ready
until docker exec lean-webui-db pg_isready -U leanuser; do
    echo "Waiting for database..."
    sleep 2
done
```

**Expected Time**: 5 minutes

#### T+20min: Database Restore

```bash
# Restore backup
gunzip < $LATEST_BACKUP | docker exec -i lean-webui-db psql -U leanuser -d leanwebui

# Verify restore
docker exec -it lean-webui-db psql -U leanuser -d leanwebui -c "SELECT COUNT(*) FROM Users;"
```

**Expected Time**: 10-30 minutes (depending on database size)

#### T+50min: Application Restart

```bash
# Start application
docker-compose -f docker-compose.prod.yml start webui-api

# Verify health
curl http://localhost/health
```

**Expected Time**: 5 minutes

#### T+55min: Verification

```bash
# Test critical functions
# 1. Login
curl -X POST http://localhost/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","password":"testpass"}'

# 2. Query data
curl http://localhost/api/v1/portfolio/positions

# 3. Check data integrity
# Compare record counts before and after
```

**Expected Time**: 5-15 minutes

#### T+70min: Recovery Complete

**Total Recovery Time**: ~70 minutes (well within 4-hour RTO)

**Post-Recovery Actions**:
- [ ] Verify all services operational
- [ ] Check data integrity
- [ ] Monitor for errors
- [ ] Update incident log
- [ ] Notify stakeholders

---

## Scenario 2: Complete System Failure

**Simulated Event**: Primary server becomes unavailable (hardware failure, data center outage)

### Timeline and Steps

#### T+0: Incident Detected

**Observer starts timer**

```bash
# Simulate server failure
# Shutdown primary server OR use secondary/DR environment
```

**Expected Detection Time**: <5 minutes

#### T+5min: DR Activation Decision

**Actions**:
- [ ] Confirm primary server unrecoverable
- [ ] Activate disaster recovery plan
- [ ] Assemble recovery team
- [ ] Begin DR site provisioning

#### T+30min: DR Environment Setup

```bash
# On DR server
# 1. Install Docker
curl -fsSL https://get.docker.com | sh

# 2. Clone repository
git clone <repository-url>
cd Lean/WebUI

# 3. Restore configuration files
scp backup-server:/backups/.env.production .
scp backup-server:/backups/certs/* ./certs/

# 4. Verify files
ls -la .env.production certs/
```

**Expected Time**: 30 minutes

#### T+60min: Database Restoration

```bash
# Start PostgreSQL
docker-compose -f docker-compose.prod.yml up -d postgres

# Wait for ready
until docker exec lean-webui-db pg_isready -U leanuser; do sleep 2; done

# Restore from off-site backup
aws s3 cp s3://leanwebui-backups/latest.sql.gz - | \
  gunzip | docker exec -i lean-webui-db psql -U leanuser -d leanwebui

# Or from network backup
scp backup-server:/backups/latest.sql.gz - | \
  gunzip | docker exec -i lean-webui-db psql -U leanuser -d leanwebui
```

**Expected Time**: 30-60 minutes

#### T+120min: Application Deployment

```bash
# Build and start all services
docker-compose -f docker-compose.prod.yml up -d

# Verify all services
docker ps
curl http://localhost/health
```

**Expected Time**: 15-30 minutes

#### T+150min: DNS Cutover

```bash
# Update DNS to point to DR site
# (This step depends on your DNS provider)

# Verify DNS propagation
dig yourdomain.com +short
```

**Expected Time**: 15-30 minutes (DNS propagation varies)

#### T+180min: Verification and Monitoring

**Actions**:
- [ ] Test all critical functions
- [ ] Verify user access
- [ ] Monitor for errors
- [ ] Enable production trading (if applicable)
- [ ] Continue monitoring for 24 hours

**Total Recovery Time**: ~3 hours (within 4-hour RTO)

---

## Scenario 3: Security Breach

**Simulated Event**: Unauthorized access detected, credentials potentially compromised

### Timeline and Steps

#### T+0: Breach Detected

**Observer starts timer**

```bash
# Simulate: Review audit logs for suspicious activity
docker exec -it lean-webui-db psql -U leanuser -d leanwebui << EOF
SELECT * FROM AuditLogs 
WHERE Action = 'Login' 
AND Success = false 
AND Timestamp > NOW() - INTERVAL '1 hour'
HAVING COUNT(*) > 10;
EOF
```

#### T+5min: Containment

```bash
# 1. Isolate system (block external access)
sudo iptables -A INPUT -p tcp --dport 443 -j DROP

# 2. Preserve evidence
docker logs lean-webui-api > security_incident_$(date +%Y%m%d_%H%M%S).log
docker exec lean-webui-db pg_dump -U leanuser leanwebui > incident_db_dump.sql

# 3. Review recent changes
SELECT * FROM AuditLogs ORDER BY Timestamp DESC LIMIT 100;
```

#### T+15min: Credential Rotation

```bash
# 1. Generate new JWT secret
NEW_JWT_SECRET=$(openssl rand -base64 64)
echo "JWT_SECRET=$NEW_JWT_SECRET" > .env.production.new

# 2. Change database password
docker exec -it lean-webui-db psql -U postgres << EOF
ALTER USER leanuser WITH PASSWORD '$(openssl rand -base64 32)';
EOF

# 3. Update configuration
mv .env.production .env.production.backup
mv .env.production.new .env.production

# 4. Restart services
docker-compose -f docker-compose.prod.yml restart
```

#### T+30min: Verification

```bash
# 1. Verify all users forced to re-authenticate
# Check active sessions, revoke refresh tokens

# 2. Review access patterns
# Analyze Seq logs

# 3. Enable external access (if safe)
sudo iptables -D INPUT -p tcp --dport 443 -j DROP
```

#### T+60min: Post-Incident

**Actions**:
- [ ] Document incident timeline
- [ ] Review what was accessed
- [ ] Identify root cause
- [ ] Implement additional security measures
- [ ] Prepare incident report

---

## Drill Execution Checklist

### Pre-Drill (1 week before)

- [ ] Schedule drill date and time
- [ ] Identify participants and observers
- [ ] Prepare secondary/test environment
- [ ] Notify stakeholders
- [ ] Verify backups are current
- [ ] Prepare timing spreadsheet
- [ ] Review procedures with team

### During Drill

- [ ] Observer records start time
- [ ] Team executes recovery procedures
- [ ] Observer records milestone times
- [ ] Document any issues or deviations
- [ ] Take screenshots of key steps
- [ ] Record all commands executed

### Post-Drill (same day)

- [ ] Calculate actual RTO
- [ ] Verify all systems restored
- [ ] Conduct hot debrief (15-30 minutes)
- [ ] Collect feedback from participants
- [ ] Document lessons learned
- [ ] Identify improvement areas

### Follow-Up (within 1 week)

- [ ] Write detailed drill report
- [ ] Update DR procedures based on findings
- [ ] Address identified gaps
- [ ] Update documentation
- [ ] Schedule training if needed
- [ ] Plan next drill (quarterly)

---

## Evaluation Criteria

### Success Criteria

- [ ] RTO achieved (<4 hours)
- [ ] RPO achieved (<24 hours data loss)
- [ ] All critical systems restored
- [ ] Data integrity verified
- [ ] No data loss beyond RPO
- [ ] All team members knew their roles
- [ ] Documentation accurate and complete

### Metrics to Track

| Metric | Target | Actual | Status |
|--------|--------|--------|--------|
| Detection Time | <5 min | | |
| Response Time | <15 min | | |
| Backup Retrieval | <30 min | | |
| Database Restore | <60 min | | |
| Application Start | <10 min | | |
| Verification | <30 min | | |
| **Total RTO** | **<4 hours** | | |
| Data Loss (in hours) | <24 hours | | |

---

## Common Issues and Solutions

### Issue: Backup Not Found

**Symptom**: Cannot locate recent backup file

**Solution**:
1. Check backup logs: `/var/log/backup.log`
2. Verify backup service running: `docker ps | grep backup`
3. Check off-site backups (S3, Azure, etc.)
4. Escalate to Level 2 support

### Issue: Database Restore Fails

**Symptom**: Database restore command errors

**Solution**:
1. Verify backup file integrity: `gunzip -t backup.sql.gz`
2. Check disk space: `df -h`
3. Try older backup
4. Review PostgreSQL logs: `docker logs lean-webui-db`

### Issue: Application Won't Start

**Symptom**: WebUI API fails to start after restore

**Solution**:
1. Check database connectivity: `docker exec webui-api nc -zv postgres 5432`
2. Verify configuration: `docker exec webui-api env | grep ConnectionStrings`
3. Review application logs: `docker logs webui-api`
4. Restart services: `docker-compose restart`

---

## Drill Report Template

```markdown
# Disaster Recovery Drill Report

**Date**: YYYY-MM-DD
**Scenario**: [Database Failure / System Failure / Security Breach]
**Participants**: [List names and roles]
**Observer**: [Name]

## Executive Summary

[Brief overview of drill results]

## Timeline

| Time | Event | Duration | Status |
|------|-------|----------|--------|
| T+0 | Incident detected | - | ✓ |
| T+X | [Milestone] | X min | ✓/✗ |
| ... | ... | ... | ... |

## Metrics

- **Total RTO**: X hours Y minutes (Target: <4 hours) - PASS/FAIL
- **Data Loss**: X hours (Target: <24 hours) - PASS/FAIL

## Successes

- [What went well]

## Issues Identified

1. [Issue description]
   - Impact: [High/Medium/Low]
   - Root cause: [...]
   - Action item: [...]

## Recommendations

1. [Recommendation]
2. [Recommendation]

## Action Items

| Action | Owner | Due Date | Status |
|--------|-------|----------|--------|
| [Action] | [Name] | YYYY-MM-DD | Open |

## Next Drill

**Date**: YYYY-MM-DD (quarterly)
**Scenario**: [To be determined]
```

---

## Continuous Improvement

### After Each Drill

1. Update procedures based on actual experience
2. Address identified gaps immediately
3. Improve automation where possible
4. Train team on any new procedures
5. Update documentation

### Annual Review

- Review all drill reports from the year
- Identify trends and recurring issues
- Update RTO/RPO targets if needed
- Evaluate and update technology stack
- Conduct full-scale disaster recovery exercise

---

**Document Version**: 1.0  
**Last Updated**: 2024-02-19  
**Next Review**: 2024-05-19  
**Owner**: Operations Team

For questions about disaster recovery drills, contact the operations lead.
