# Spec: User Authentication（用户认证）

## ADDED Requirements

### Requirement: User login
系统必须提供用户登录功能。

#### Scenario: Successful login
- **WHEN** 用户输入正确的用户名和密码
- **THEN** 系统返回 Access Token 和 Refresh Token，并跳转到首页

#### Scenario: Invalid credentials
- **WHEN** 用户输入错误的用户名或密码
- **THEN** 系统返回"用户名或密码错误"并拒绝登录

#### Scenario: Account locked after failed attempts
- **WHEN** 用户连续 5 次输入错误密码
- **THEN** 系统锁定账户 30 分钟并显示"账户已锁定，请 30 分钟后重试"

#### Scenario: Case-insensitive username
- **WHEN** 用户输入大小写不同的用户名（如"Admin" vs "admin"）
- **THEN** 系统识别为同一用户

### Requirement: JWT token-based authentication
系统必须使用 JWT Token 进行身份验证。

#### Scenario: Issue Access Token
- **WHEN** 用户登录成功
- **THEN** 系统签发 15 分钟有效期的 Access Token（包含用户ID、角色等信息）

#### Scenario: Issue Refresh Token
- **WHEN** 用户登录成功
- **THEN** 系统同时签发 7 天有效期的 Refresh Token

#### Scenario: Validate Access Token
- **WHEN** 客户端发送带有 Authorization Header 的 API 请求
- **THEN** 系统验证 Token 签名和有效期，通过后处理请求

#### Scenario: Reject expired Access Token
- **WHEN** Access Token 过期
- **THEN** 系统返回 401 Unauthorized 错误

### Requirement: Refresh Token mechanism
系统必须支持使用 Refresh Token 刷新 Access Token。

#### Scenario: Refresh Access Token
- **WHEN** 客户端使用有效的 Refresh Token 请求刷新
- **THEN** 系统签发新的 Access Token（15 分钟有效）

#### Scenario: Rotate Refresh Token
- **WHEN** 刷新 Access Token 时
- **THEN** 系统同时签发新的 Refresh Token 并撤销旧的

#### Scenario: Reject invalid Refresh Token
- **WHEN** Refresh Token 无效或已过期
- **THEN** 系统返回 401 错误并要求用户重新登录

### Requirement: User logout
系统必须提供登出功能。

#### Scenario: Logout and revoke tokens
- **WHEN** 用户点击"登出"
- **THEN** 系统将 Refresh Token 加入黑名单（Redis 存储）并清除客户端 Token

#### Scenario: Logout from all devices
- **WHEN** 用户选择"登出所有设备"
- **THEN** 系统撤销该用户的所有 Refresh Token

#### Scenario: Client-side cleanup
- **WHEN** 用户登出
- **THEN** 前端清除 localStorage/sessionStorage 中的 Token 并跳转到登录页

### Requirement: Password security
系统必须安全存储和验证密码。

#### Scenario: Hash password on registration
- **WHEN** 创建新用户账户
- **THEN** 系统使用 PBKDF2 算法（10000 迭代）对密码进行哈希并加盐存储

#### Scenario: Verify password on login
- **WHEN** 用户登录时
- **THEN** 系统使用相同算法哈希输入密码并与数据库比对

#### Scenario: Enforce strong password policy
- **WHEN** 用户设置密码
- **THEN** 系统要求密码至少 8 位、包含大小写字母、数字和特殊字符

#### Scenario: Prevent password reuse
- **WHEN** 用户修改密码
- **THEN** 系统检查新密码不与最近 3 次使用的密码相同

### Requirement: Change password
系统必须支持用户修改密码。

#### Scenario: Change password with old password
- **WHEN** 用户提供旧密码和新密码
- **THEN** 系统验证旧密码正确后更新为新密码

#### Scenario: Invalid old password
- **WHEN** 用户输入错误的旧密码
- **THEN** 系统拒绝修改并显示"旧密码错误"

#### Scenario: Logout after password change
- **WHEN** 密码修改成功
- **THEN** 系统撤销所有 Refresh Token 并要求重新登录

### Requirement: Password reset (future feature placeholder)
系统必须为密码重置预留接口（首版单用户模式暂不实现）。

#### Scenario: Request password reset
- **WHEN** 用户忘记密码（预留功能）
- **THEN** 系统发送重置链接到注册邮箱（首版不实现，直接修改配置文件）

#### Scenario: Single-user mode workaround
- **WHEN** 首版用户忘记密码
- **THEN** 用户通过修改配置文件中的密码哈希来重置（提供工具脚本）

### Requirement: Role-based access control (RBAC)
系统必须支持基于角色的权限控制（为多用户扩展预留）。

#### Scenario: Admin role
- **WHEN** 用户角色为 Admin
- **THEN** 系统允许访问所有功能（策略管理、系统设置等）

#### Scenario: Viewer role (future)
- **WHEN** 用户角色为 Viewer（预留）
- **THEN** 系统仅允许查看数据，不允许交易或修改

#### Scenario: Check permission before action
- **WHEN** 用户执行操作（如删除策略）
- **THEN** 系统验证用户角色是否有权限

### Requirement: Audit user actions
系统必须记录用户的关键操作。

#### Scenario: Log login attempts
- **WHEN** 用户尝试登录（成功或失败）
- **THEN** 系统记录日志（时间、用户名、IP 地址、结果）

#### Scenario: Log trading actions
- **WHEN** 用户下单、取消订单、平仓
- **THEN** 系统记录操作到 AuditLog 表（用户、操作类型、时间、详情）

#### Scenario: Log configuration changes
- **WHEN** 用户修改系统配置（如风险参数）
- **THEN** 系统记录修改前后的值

#### Scenario: View audit log
- **WHEN** 管理员查看审计日志
- **THEN** 系统显示最近 1000 条操作记录（可筛选、搜索）

### Requirement: Secure token storage
系统必须安全存储客户端的 Token。

#### Scenario: Store tokens in httpOnly cookie (recommended)
- **WHEN** 系统返回 Token
- **THEN** 将 Refresh Token 存储在 httpOnly cookie 中（防止 XSS 攻击）

#### Scenario: Store Access Token in memory
- **WHEN** 前端收到 Access Token
- **THEN** 存储在内存变量中（不存储到 localStorage，减少 XSS 风险）

#### Scenario: HTTPS only in production
- **WHEN** 系统运行在生产环境
- **THEN** 强制使用 HTTPS 传输 Token（防止中间人攻击）

### Requirement: Session timeout
系统必须实施会话超时机制。

#### Scenario: Idle timeout warning
- **WHEN** 用户 10 分钟无操作
- **THEN** 系统显示"即将超时"警告并提供"继续使用"按钮

#### Scenario: Auto logout on idle
- **WHEN** 用户 15 分钟无操作
- **THEN** 系统自动登出并清除 Token

#### Scenario: Extend session on activity
- **WHEN** 用户进行任何操作
- **THEN** 系统重置空闲计时器

### Requirement: Prevent brute force attacks
系统必须防止暴力破解攻击。

#### Scenario: Rate limit login attempts
- **WHEN** 同一 IP 地址短时间内多次登录失败
- **THEN** 系统限制该 IP 的登录频率（每 5 秒最多 1 次）

#### Scenario: CAPTCHA after failed attempts
- **WHEN** 用户连续 3 次登录失败
- **THEN** 系统要求输入验证码（未来功能，首版可选）

#### Scenario: Notify on suspicious activity
- **WHEN** 检测到异常登录（如来自新 IP）
- **THEN** 系统发送邮件通知"检测到来自新位置的登录"
