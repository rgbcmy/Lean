# Security Audit Guide - OWASP Top 10
# 安全审计指南 - OWASP Top 10

## Overview
## 概览

This document provides a comprehensive security audit checklist based on the OWASP Top 10 security risks.
本文档提供基于 OWASP Top 10 安全风险的综合安全审计清单。

## OWASP Top 10 (2021) Checklist
## OWASP Top 10 (2021) 清单

### A01: Broken Access Control
### A01: 失效的访问控制

**Risk**: Users can access unauthorized resources or perform unauthorized actions.
**风险**: 用户可以访问未授权的资源或执行未授权的操作。

#### Checklist
#### 清单

- [ ] **JWT Token Validation**: Verify JWT tokens are validated on every protected endpoint
  - 验证每个受保护端点都验证 JWT Token
  - Check: All controllers have `[Authorize]` attribute where needed

- [ ] **Role-Based Access Control (RBAC)**: Verify role checks are implemented
  - 验证实施了角色检查
  - Check: Admin-only endpoints require admin role

- [ ] **Resource Ownership**: Verify users can only access their own data
  - 验证用户只能访问自己的数据
  - Check: Orders, positions, strategies filtered by UserId

- [ ] **CORS Configuration**: Verify CORS is properly configured
  - 验证 CORS 正确配置
  - Check file: `Program.cs` CORS policy

- [ ] **API Rate Limiting**: Verify rate limiting is enforced
  - 验证强制执行速率限制
  - Check: RateLimitingMiddleware is active

**Commands to Test**:
```bash
# Test unauthorized access
curl -X GET http://localhost:5000/api/v1/orders
# Should return 401 Unauthorized

# Test with valid token
curl -X GET http://localhost:5000/api/v1/orders \
  -H "Authorization: Bearer VALID_TOKEN"
# Should return 200 OK
```

---

### A02: Cryptographic Failures
### A02: 加密机制失效

**Risk**: Sensitive data exposed due to weak or missing encryption.
**风险**: 由于加密不足或缺失导致敏感数据暴露。

#### Checklist
#### 清单

- [ ] **HTTPS Enforcement**: Verify HTTPS is enforced in production
  - 验证生产环境强制 HTTPS
  - Check: `app.UseHttpsRedirection()` in Program.cs

- [ ] **Password Hashing**: Verify passwords are hashed with PBKDF2
  - 验证密码使用 PBKDF2 哈希
  - Check: `PasswordHasher.cs` implementation

- [ ] **Credential Encryption**: Verify IBKR credentials are encrypted
  - 验证 IBKR 凭证已加密
  - Check: `CredentialEncryptionService.cs`

- [ ] **JWT Secret Key**: Verify JWT secret is strong and stored securely
  - 验证 JWT 密钥强度且安全存储
  - Check: `appsettings.json` JWT configuration

- [ ] **Sensitive Data in Logs**: Verify no sensitive data in logs
  - 验证日志中无敏感数据
  - Check: Review log configuration

- [ ] **Database Connection Strings**: Verify encrypted or environment variables
  - 验证数据库连接字符串加密或使用环境变量
  - Check: Connection string storage

**Commands to Test**:
```bash
# Check password hash strength
dotnet test --filter "PasswordHasherTests"

# Verify HTTPS redirect
curl -I http://localhost:5000/api/v1/auth/login
# Should redirect to HTTPS in production
```

---

### A03: Injection
### A03: 注入

**Risk**: SQL injection, command injection, or other injection attacks.
**风险**: SQL 注入、命令注入或其他注入攻击。

#### Checklist
#### 清单

- [ ] **Parameterized Queries**: Verify all database queries use parameterized queries (EF Core does this by default)
  - 验证所有数据库查询使用参数化查询（EF Core 默认如此）
  - Check: No raw SQL queries with string concatenation

- [ ] **Input Validation**: Verify all user inputs are validated
  - 验证所有用户输入都经过验证
  - Check: FluentValidation validators for all DTOs

- [ ] **Command Injection**: Verify no system commands with user input
  - 验证没有使用用户输入的系统命令
  - Check: Strategy execution service

- [ ] **XSS Prevention**: Verify output encoding for user-generated content
  - 验证用户生成内容的输出编码
  - Check: React automatically escapes (built-in protection)

- [ ] **SQL Injection**: Test with malicious inputs
  - 使用恶意输入测试
  - Test: `' OR '1'='1` in search fields

**Commands to Test**:
```bash
# Test SQL injection
curl -X GET "http://localhost:5000/api/v1/market/quote?symbol=' OR '1'='1"
# Should be safely handled by EF Core

# Run security tests
dotnet test --filter "SecurityTests"
```

---

### A04: Insecure Design
### A04: 不安全设计

**Risk**: Missing or ineffective security controls in design.
**风险**: 设计中缺失或无效的安全控制。

#### Checklist
#### 清单

- [ ] **Secure Architecture**: Review architecture for security best practices
  - 审查架构的安全最佳实践
  - Check: Separation of concerns, minimal privilege

- [ ] **Threat Modeling**: Perform threat modeling for critical flows
  - 对关键流程执行威胁建模
  - Review: Trading flow, authentication, data access

- [ ] **Security Requirements**: Verify security requirements are documented
  - 验证安全需求已文档化
  - Check: Security section in design.md

- [ ] **Error Handling**: Verify errors don't leak sensitive information
  - 验证错误不泄露敏感信息
  - Check: Exception handling middleware

- [ ] **Audit Logging**: Verify comprehensive audit logging
  - 验证全面的审计日志
  - Check: AuditLogService implementation

---

### A05: Security Misconfiguration
### A05: 安全配置错误

**Risk**: Insecure default configurations or misconfigured security settings.
**风险**: 不安全的默认配置或错误配置的安全设置。

#### Checklist
#### 清单

- [ ] **Default Credentials**: Verify no default admin passwords in production
  - 验证生产环境无默认管理员密码
  - Check: Seed data configuration

- [ ] **Error Messages**: Verify production error messages don't expose stack traces
  - 验证生产错误消息不暴露堆栈跟踪
  - Check: Environment-specific error handling

- [ ] **Security Headers**: Verify security headers are configured
  - 验证安全头已配置
  - Check: HSTS, X-Content-Type-Options, X-Frame-Options

- [ ] **Unnecessary Features**: Verify Swagger is disabled in production
  - 验证生产环境禁用 Swagger
  - Check: `if (app.Environment.IsDevelopment())` wrapper

- [ ] **File Permissions**: Verify proper file permissions on server
  - 验证服务器上的正确文件权限
  - Check: Configuration files, log files

- [ ] **Dependency Versions**: Verify all dependencies are up-to-date
  - 验证所有依赖项都是最新的
  - Run: `dotnet list package --outdated`

**Commands to Test**:
```bash
# Check security headers
curl -I https://localhost:5001/api/v1/health

# Check for outdated packages
dotnet list package --outdated
npm audit
```

---

### A06: Vulnerable and Outdated Components
### A06: 易受攻击和过时的组件

**Risk**: Using components with known vulnerabilities.
**风险**: 使用已知漏洞的组件。

#### Checklist
#### 清单

- [ ] **NuGet Packages**: Scan for vulnerable packages
  - 扫描易受攻击的包
  - Run: `dotnet list package --vulnerable`

- [ ] **npm Packages**: Scan for vulnerable packages
  - 扫描易受攻击的包
  - Run: `npm audit`

- [ ] **Automated Scanning**: Set up automated vulnerability scanning in CI/CD
  - 在 CI/CD 中设置自动漏洞扫描
  - Use: GitHub Dependabot, Snyk, or OWASP Dependency-Check

- [ ] **Update Schedule**: Establish regular update schedule
  - 建立定期更新计划
  - Review dependencies monthly

**Commands to Test**:
```bash
# Backend vulnerabilities
dotnet list package --vulnerable --include-transitive

# Frontend vulnerabilities
cd WebUI.Frontend
npm audit
npm audit fix

# Generate audit report
npm audit --json > audit-report.json
```

---

### A07: Identification and Authentication Failures
### A07: 识别和身份验证失败

**Risk**: Weak authentication or session management.
**风险**: 弱身份验证或会话管理。

#### Checklist
#### 清单

- [ ] **Password Policy**: Verify strong password requirements
  - 验证强密码要求
  - Check: Minimum 8 characters, complexity requirements

- [ ] **Brute Force Protection**: Verify account lockout after failed attempts
  - 验证失败尝试后的账户锁定
  - Check: 5 failed attempts = 30-minute lockout

- [ ] **Session Management**: Verify secure JWT token handling
  - 验证安全的 JWT Token 处理
  - Check: Token expiration, refresh token rotation

- [ ] **Multi-Factor Authentication (MFA)**: Consider implementing MFA
  - 考虑实施多因素身份验证
  - Status: Not in v1.0, planned for future

- [ ] **Credential Recovery**: Verify secure password reset process
  - 验证安全的密码重置流程
  - Check: Password reset implementation

**Commands to Test**:
```bash
# Test password policy
curl -X POST http://localhost:5000/api/v1/auth/change-password \
  -H "Content-Type: application/json" \
  -d '{"oldPassword":"test","newPassword":"weak"}'
# Should be rejected

# Test account lockout
for i in {1..6}; do
  curl -X POST http://localhost:5000/api/v1/auth/login \
    -H "Content-Type: application/json" \
    -d '{"username":"test","password":"wrong"}'
done
# 6th attempt should return lockout error
```

---

### A08: Software and Data Integrity Failures
### A08: 软件和数据完整性失效

**Risk**: Code or infrastructure vulnerabilities related to integrity.
**风险**: 与完整性相关的代码或基础设施漏洞。

#### Checklist
#### 清单

- [ ] **Code Signing**: Verify code signing for releases
  - 验证发布版本的代码签名
  - Check: Build pipeline configuration

- [ ] **Dependency Integrity**: Verify package integrity checks
  - 验证包完整性检查
  - Check: package-lock.json integrity hashes

- [ ] **CI/CD Security**: Verify secure CI/CD pipeline
  - 验证安全的 CI/CD 流水线
  - Check: No secrets in code, use secret management

- [ ] **Backup Integrity**: Verify database backup integrity
  - 验证数据库备份完整性
  - Check: Backup verification process

---

### A09: Security Logging and Monitoring Failures
### A09: 安全日志记录和监控失效

**Risk**: Insufficient logging and monitoring.
**风险**: 日志记录和监控不足。

#### Checklist
#### 清单

- [ ] **Audit Logging**: Verify all security events are logged
  - 验证所有安全事件都被记录
  - Check: Login/logout, failed auth, privilege changes

- [ ] **Log Protection**: Verify logs are protected from tampering
  - 验证日志受保护不被篡改
  - Check: Log file permissions, log aggregation

- [ ] **Monitoring Alerts**: Set up alerts for suspicious activity
  - 为可疑活动设置警报
  - Check: Multiple failed logins, unusual trading patterns

- [ ] **Log Retention**: Verify appropriate log retention policy
  - 验证适当的日志保留策略
  - Check: 90-day retention minimum

**Commands to Test**:
```bash
# Check audit logs
dotnet ef database query \
  "SELECT * FROM AuditLogs ORDER BY Timestamp DESC LIMIT 100"

# Verify logging configuration
cat appsettings.json | grep -A 10 "Logging"
```

---

### A10: Server-Side Request Forgery (SSRF)
### A10: 服务器端请求伪造 (SSRF)

**Risk**: Application fetches remote resources without validating URLs.
**风险**: 应用程序在未验证 URL 的情况下获取远程资源。

#### Checklist
#### 清单

- [ ] **URL Validation**: Verify external URLs are validated
  - 验证外部 URL 已验证
  - Check: Whitelist allowed domains

- [ ] **Network Segmentation**: Verify backend services are segmented
  - 验证后端服务已分段
  - Check: IBKR connections are isolated

- [ ] **Input Sanitization**: Verify user-provided URLs are sanitized
  - 验证用户提供的 URL 已清理
  - Check: Strategy code upload validation

---

## Security Tools
## 安全工具

### Recommended Tools
### 推荐工具

1. **OWASP ZAP**: Web application security scanner
   ```bash
   docker run -t owasp/zap2docker-stable zap-baseline.py \
     -t http://localhost:5173
   ```

2. **dotnet security scan**: .NET security analyzer
   ```bash
   dotnet tool install --global security-scan
   security-scan WebUI.sln
   ```

3. **npm audit**: npm vulnerability scanner
   ```bash
   cd WebUI.Frontend
   npm audit --production
   ```

4. **Snyk**: Comprehensive vulnerability scanner
   ```bash
   snyk test
   ```

## Security Testing Schedule
## 安全测试计划

- **Weekly**: Automated dependency scanning
- **每周**: 自动依赖扫描

- **Monthly**: Manual security review
- **每月**: 手动安全审查

- **Quarterly**: Full penetration testing
- **季度**: 完整渗透测试

- **Before Release**: Complete OWASP Top 10 audit
- **发布前**: 完整的 OWASP Top 10 审计

## Reporting
## 报告

Document all findings in:
在以下位置记录所有发现:

```
tests/security/audit-reports/
  ├── 2024-01-15-owasp-audit.md
  ├── 2024-01-15-vulnerability-scan.json
  └── remediation-tracking.md
```

## References
## 参考

- [OWASP Top 10 2021](https://owasp.org/www-project-top-ten/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [React Security Best Practices](https://reactjs.org/docs/dom-elements.html#dangerouslysetinnerhtml)
