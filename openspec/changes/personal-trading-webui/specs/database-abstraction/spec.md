# Spec: Database Abstraction（数据库抽象层）

## ADDED Requirements

### Requirement: Support multiple database providers
系统必须支持 PostgreSQL 和 SQLite 数据库。

#### Scenario: Configure PostgreSQL
- **WHEN** 配置文件中设置 `Database.Provider = "PostgreSQL"`
- **THEN** 系统使用 Npgsql 提供程序连接到 PostgreSQL 数据库

#### Scenario: Configure SQLite
- **WHEN** 配置文件中设置 `Database.Provider = "SQLite"`
- **THEN** 系统使用 SQLite 提供程序连接到本地 SQLite 文件

#### Scenario: Default to SQLite for development
- **WHEN** 未指定数据库提供程序
- **THEN** 系统默认使用 SQLite（适合本地开发）

#### Scenario: Invalid provider error
- **WHEN** 配置文件中指定不支持的数据库（如"MySQL"）
- **THEN** 系统启动失败并显示"不支持的数据库提供程序"

### Requirement: Use Entity Framework Core
系统必须使用 EF Core 作为 ORM 框架。

#### Scenario: Define database models
- **WHEN** 系统初始化
- **THEN** 使用 EF Core DbContext 定义实体模型（User, Strategy, Order, Position 等）

#### Scenario: Generate migrations
- **WHEN** 开发者修改数据模型
- **THEN** 使用 `dotnet ef migrations add` 生成迁移脚本

#### Scenario: Apply migrations automatically
- **WHEN** 系统启动时
- **THEN** 自动执行 `context.Database.Migrate()` 应用未应用的迁移

#### Scenario: Downgrade migration
- **WHEN** 需要回滚数据库版本
- **THEN** 使用 `dotnet ef database update <migration>` 回滚到指定版本

### Requirement: Implement Repository pattern
系统必须使用 Repository 模式封装数据访问。

#### Scenario: Generic repository
- **WHEN** 需要访问数据库
- **THEN** 通过 `IRepository<T>` 接口执行 CRUD 操作（不直接使用 DbContext）

#### Scenario: Specific repository
- **WHEN** 需要复杂查询（如订单筛选）
- **THEN** 创建专用 Repository（如 `IOrderRepository`）实现特定查询方法

#### Scenario: Unit of Work pattern
- **WHEN** 执行多个数据库操作
- **THEN** 使用 `IUnitOfWork` 确保事务一致性（全部成功或全部回滚）

### Requirement: Database connection management
系统必须高效管理数据库连接。

#### Scenario: Connection pooling
- **WHEN** 系统运行时
- **THEN** 使用连接池管理数据库连接（EF Core 默认启用）

#### Scenario: Connection string validation
- **WHEN** 系统启动时
- **THEN** 验证数据库连接字符串格式正确并能成功连接

#### Scenario: Retry on transient errors
- **WHEN** 数据库操作因临时错误失败（如网络抖动）
- **THEN** 系统自动重试（最多 3 次，间隔 1 秒）

#### Scenario: Connection timeout
- **WHEN** 数据库连接超过 30 秒未响应
- **THEN** 系统超时并抛出异常

### Requirement: Schema versioning
系统必须管理数据库架构版本。

#### Scenario: Track migration history
- **WHEN** 应用迁移
- **THEN** EF Core 在 `__EFMigrationsHistory` 表中记录已应用的迁移

#### Scenario: Check pending migrations
- **WHEN** 系统启动时
- **THEN** 检查是否有未应用的迁移并记录日志

#### Scenario: Block startup on migration failure
- **WHEN** 迁移应用失败
- **THEN** 系统拒绝启动并显示错误详情

### Requirement: Data seeding
系统必须支持初始化数据。

#### Scenario: Seed default user
- **WHEN** 数据库为空（首次启动）
- **THEN** 系统创建默认管理员账户（用户名: admin, 密码: 提示用户修改）

#### Scenario: Seed example strategy
- **WHEN** 数据库为空
- **THEN** 系统导入示例策略模板供用户参考

#### Scenario: Skip seeding if data exists
- **WHEN** 数据库已有数据
- **THEN** 系统跳过数据初始化

### Requirement: Cross-database compatibility
系统必须确保查询在 PostgreSQL 和 SQLite 上都能正常工作。

#### Scenario: Avoid provider-specific SQL
- **WHEN** 编写查询时
- **THEN** 使用 LINQ 而非原生 SQL（确保跨数据库兼容）

#### Scenario: Handle date/time differences
- **WHEN** 查询涉及日期时间
- **THEN** 使用 UTC 时间并通过 EF Core 函数转换（避免数据库特定函数）

#### Scenario: Test on both providers
- **WHEN** 开发新功能
- **THEN** 单元测试同时在 PostgreSQL 和 SQLite 上运行

### Requirement: Optimize query performance
系统必须优化数据库查询性能。

#### Scenario: Use eager loading
- **WHEN** 查询需要关联数据（如订单及其详情）
- **THEN** 使用 `.Include()` 一次性加载，避免 N+1 查询

#### Scenario: Use pagination
- **WHEN** 查询大量数据（如订单历史）
- **THEN** 使用 `.Skip()` 和 `.Take()` 分页加载

#### Scenario: Index frequently queried columns
- **WHEN** 配置数据模型
- **THEN** 为常用查询字段（如 UserId、StrategyId、Timestamp）添加索引

#### Scenario: Use async queries
- **WHEN** 执行数据库操作
- **THEN** 使用异步方法（`ToListAsync()`, `FirstOrDefaultAsync()`）避免阻塞

### Requirement: Database backup and restore
系统必须支持数据库备份（针对 SQLite）。

#### Scenario: Backup SQLite database
- **WHEN** 用户点击"备份数据库"
- **THEN** 系统复制 SQLite 文件到备份目录（带时间戳）

#### Scenario: Restore SQLite database
- **WHEN** 用户选择备份文件恢复
- **THEN** 系统停止系统、替换数据库文件、重启

#### Scenario: PostgreSQL backup note
- **WHEN** 使用 PostgreSQL
- **THEN** 系统提示"请使用 pg_dump 进行数据库备份"

### Requirement: Handle concurrent access
系统必须处理并发数据库访问。

#### Scenario: Optimistic concurrency control
- **WHEN** 多个请求同时修改同一记录
- **THEN** 使用 RowVersion/Timestamp 字段检测冲突并拒绝过期更新

#### Scenario: Retry on deadlock
- **WHEN** PostgreSQL 发生死锁
- **THEN** 系统捕获异常并重试事务

#### Scenario: Use appropriate isolation level
- **WHEN** 执行事务
- **THEN** 默认使用 Read Committed 隔离级别（平衡性能和一致性）

### Requirement: Secure database credentials
系统必须安全存储数据库连接凭证。

#### Scenario: Encrypt connection string in config
- **WHEN** 配置文件包含数据库密码
- **THEN** 使用配置加密（DPAPI on Windows, 环境变量或 Key Vault）

#### Scenario: Use environment variables
- **WHEN** 部署到生产环境
- **THEN** 从环境变量读取数据库连接字符串（不硬编码）

#### Scenario: Prompt if credentials missing
- **WHEN** 系统启动时缺少数据库凭证
- **THEN** 显示错误"数据库连接字符串未配置"并提供配置指南

### Requirement: Database health monitoring
系统必须监控数据库健康状态。

#### Scenario: Health check endpoint
- **WHEN** 外部监控系统调用 `/health` 端点
- **THEN** 系统测试数据库连接并返回状态（Healthy/Unhealthy）

#### Scenario: Log database errors
- **WHEN** 数据库操作失败
- **THEN** 系统记录详细错误日志（查询、参数、异常）

#### Scenario: Alert on connection failure
- **WHEN** 数据库连接持续失败超过 1 分钟
- **THEN** 系统发送告警通知
