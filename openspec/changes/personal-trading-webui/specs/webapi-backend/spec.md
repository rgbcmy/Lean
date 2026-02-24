# Spec: Web API Backend（Web API 后端服务）

## ADDED Requirements

### Requirement: RESTful API endpoints
系统必须提供符合 REST 规范的 API 端点。

#### Scenario: Standard HTTP methods
- **WHEN** 客户端调用 API
- **THEN** 系统使用标准 HTTP 方法（GET 查询、POST 创建、PUT/PATCH 更新、DELETE 删除）

#### Scenario: Resource-based URLs
- **WHEN** API 设计时
- **THEN** 使用资源名词作为路径（如 `/api/v1/orders`、`/api/v1/strategies`）

#### Scenario: Return proper status codes
- **WHEN** API 处理请求
- **THEN** 返回正确的 HTTP 状态码（200 成功、201 创建、400 参数错误、401 未授权、404 未找到、500 服务器错误）

#### Scenario: JSON response format
- **WHEN** API 返回数据
- **THEN** 使用 JSON 格式，结构统一（如 `{ "data": {...}, "error": null }`）

### Requirement: API versioning
系统必须支持 API 版本管理。

#### Scenario: Version in URL path
- **WHEN** 客户端调用 API
- **THEN** URL 包含版本号（如 `/api/v1/orders`）

#### Scenario: Support multiple versions
- **WHEN** 系统升级 API
- **THEN** 同时支持 v1 和 v2，确保向后兼容

#### Scenario: Deprecation warning
- **WHEN** 客户端使用旧版本 API
- **THEN** 响应头包含 `X-API-Deprecated: true` 和弃用日期

### Requirement: Request validation
系统必须验证所有输入参数。

#### Scenario: Validate required fields
- **WHEN** 客户端请求缺少必需参数
- **THEN** 系统返回 400 错误："缺少必需参数: [字段名]"

#### Scenario: Validate data types
- **WHEN** 客户端传递错误类型（如字符串传给数字字段）
- **THEN** 系统返回 400 错误："参数 [字段名] 必须为数字"

#### Scenario: Validate value ranges
- **WHEN** 客户端传递超出范围的值（如负数数量）
- **THEN** 系统返回 400 错误："参数 [字段名] 必须大于 0"

#### Scenario: Sanitize input
- **WHEN** 客户端提交数据
- **THEN** 系统清理输入防止 SQL 注入和 XSS 攻击

### Requirement: Error handling and responses
系统必须提供一致的错误响应格式。

#### Scenario: Structured error response
- **WHEN** API 发生错误
- **THEN** 返回 JSON 格式：`{ "error": { "code": "INVALID_PARAMETER", "message": "详细描述", "details": {...} } }`

#### Scenario: Hide sensitive errors in production
- **WHEN** 生产环境发生服务器错误
- **THEN** 仅返回通用错误信息"服务器内部错误"，不暴露堆栈跟踪

#### Scenario: Log all errors
- **WHEN** API 发生错误
- **THEN** 系统记录详细日志（请求 ID、用户、端点、参数、异常）

### Requirement: Rate limiting
系统必须实施 API 速率限制。

#### Scenario: Limit requests per user
- **WHEN** 单个用户短时间内频繁调用 API
- **THEN** 系统限制为每分钟最多 60 次请求

#### Scenario: Return rate limit headers
- **WHEN** API 响应时
- **THEN** 包含速率限制头（`X-RateLimit-Limit`, `X-RateLimit-Remaining`, `X-RateLimit-Reset`）

#### Scenario: Reject exceeded requests
- **WHEN** 用户超过速率限制
- **THEN** 系统返回 429 Too Many Requests 错误

#### Scenario: Exempt admin users (future)
- **WHEN** 管理员用户调用 API
- **THEN** 系统不施加速率限制（预留功能）

### Requirement: CORS configuration
系统必须配置跨域资源共享（CORS）。

#### Scenario: Allow frontend origin
- **WHEN** 配置 CORS
- **THEN** 允许前端域名（如 `http://localhost:3000`）访问 API

#### Scenario: Allow credentials
- **WHEN** 前端需要携带 Cookie
- **THEN** CORS 配置 `Allow-Credentials: true`

#### Scenario: Restrict in production
- **WHEN** 生产环境
- **THEN** 仅允许白名单域名访问，拒绝其他来源

#### Scenario: Preflight requests
- **WHEN** 浏览器发送 OPTIONS 预检请求
- **THEN** 系统正确响应 CORS 头并返回 200

### Requirement: SignalR real-time communication
系统必须提供 SignalR Hub 用于实时推送。

#### Scenario: Client connects to hub
- **WHEN** 前端建立 SignalR 连接
- **THEN** 系统验证 Token 并建立 WebSocket 连接

#### Scenario: Subscribe to topics
- **WHEN** 客户端订阅特定 Topic（如"MarketData:AAPL"）
- **THEN** 系统将客户端加入对应的 Group

#### Scenario: Push updates to subscribers
- **WHEN** 行情数据更新
- **THEN** 系统通过 SignalR 推送到所有订阅该股票的客户端

#### Scenario: Unsubscribe on disconnect
- **WHEN** 客户端断开连接
- **THEN** 系统自动移除所有订阅

#### Scenario: Reconnect handling
- **WHEN** 客户端因网络中断断开连接
- **THEN** SignalR 自动重连并恢复订阅

### Requirement: API documentation
系统必须提供 API 文档。

#### Scenario: Swagger/OpenAPI integration
- **WHEN** 系统运行时
- **THEN** 提供 Swagger UI 界面（`/swagger`）供查看和测试 API

#### Scenario: Auto-generate from code
- **WHEN** API 端点更新
- **THEN** Swagger 文档自动同步更新

#### Scenario: Include examples
- **WHEN** 查看 API 文档
- **THEN** 每个端点包含请求/响应示例

#### Scenario: Chinese descriptions
- **WHEN** 查看 API 文档
- **THEN** 描述和注释提供中文版本

### Requirement: Logging and monitoring
系统必须记录 API 请求日志。

#### Scenario: Log all requests
- **WHEN** API 收到请求
- **THEN** 记录日志（时间、用户、端点、方法、状态码、耗时）

#### Scenario: Structured logging
- **WHEN** 记录日志
- **THEN** 使用结构化格式（JSON），便于解析和搜索

#### Scenario: Log sensitive data masking
- **WHEN** 日志包含敏感信息（如密码、Token）
- **THEN** 自动脱敏显示（如 `password: *****`）

#### Scenario: Performance monitoring
- **WHEN** API 响应时间超过 1 秒
- **THEN** 记录慢查询日志并告警

### Requirement: Health check endpoints
系统必须提供健康检查端点。

#### Scenario: Basic health check
- **WHEN** 调用 `/health` 端点
- **THEN** 系统返回 `{ "status": "Healthy" }` 和 200 状态码

#### Scenario: Detailed health check
- **WHEN** 调用 `/health/detailed` 端点
- **THEN** 系统返回各组件状态（数据库、IBKR连接、磁盘空间等）

#### Scenario: Readiness check
- **WHEN** 调用 `/health/ready` 端点
- **THEN** 系统检查是否准备好接受流量（数据库迁移完成、依赖服务可用）

### Requirement: Graceful shutdown
系统必须支持优雅关闭。

#### Scenario: Stop accepting new requests
- **WHEN** 收到终止信号（SIGTERM）
- **THEN** 系统停止接受新请求并返回 503 Service Unavailable

#### Scenario: Complete in-flight requests
- **WHEN** 开始关闭流程
- **THEN** 系统等待所有进行中的请求完成（最多等待 30 秒）

#### Scenario: Close connections
- **WHEN** 所有请求完成
- **THEN** 系统关闭数据库连接、SignalR 连接和其他资源

### Requirement: API security
系统必须实施 API 安全措施。

#### Scenario: HTTPS only in production
- **WHEN** 生产环境运行时
- **THEN** 强制使用 HTTPS，拒绝 HTTP 请求

#### Scenario: Validate JWT on every request
- **WHEN** API 收到请求
- **THEN** 验证 Authorization Header 中的 JWT Token

#### Scenario: Prevent CSRF attacks
- **WHEN** 使用 Cookie 存储 Token
- **THEN** 实施 SameSite Cookie 和 CSRF Token 验证

#### Scenario: SQL injection prevention
- **WHEN** 执行数据库查询
- **THEN** 使用参数化查询（EF Core 自动处理）

### Requirement: Caching strategy
系统必须实施缓存提高性能。

#### Scenario: Cache frequently accessed data
- **WHEN** 查询不常变化的数据（如策略列表）
- **THEN** 缓存到 Redis（5 分钟过期）

#### Scenario: Invalidate cache on update
- **WHEN** 数据被修改（如策略更新）
- **THEN** 立即清除相关缓存

#### Scenario: HTTP cache headers
- **WHEN** API 返回静态资源或稳定数据
- **THEN** 设置 `Cache-Control` 头（如 `max-age=3600`）

### Requirement: Pagination support
系统必须为列表 API 提供分页。

#### Scenario: Page-based pagination
- **WHEN** 客户端请求 `/api/v1/orders?page=2&pageSize=50`
- **THEN** 系统返回第 2 页的 50 条记录

#### Scenario: Return pagination metadata
- **WHEN** API 返回分页数据
- **THEN** 响应包含元数据（`totalCount`, `page`, `pageSize`, `totalPages`）

#### Scenario: Cursor-based pagination (future)
- **WHEN** 需要高性能分页（如实时日志）
- **THEN** 使用游标分页（`?cursor=abc123`）（预留功能）

### Requirement: API performance optimization
系统必须优化 API 性能。

#### Scenario: Async controllers
- **WHEN** 实现 API 端点
- **THEN** 使用异步方法（`async Task<IActionResult>`）

#### Scenario: Response compression
- **WHEN** API 返回大量数据
- **THEN** 启用 Gzip 压缩减少传输大小

#### Scenario: Lazy loading prevention
- **WHEN** 查询数据库
- **THEN** 显式使用 `Include()` 避免 N+1 查询

#### Scenario: Connection pooling
- **WHEN** 调用外部服务（如 IBKR）
- **THEN** 使用 HttpClient 连接池复用连接
