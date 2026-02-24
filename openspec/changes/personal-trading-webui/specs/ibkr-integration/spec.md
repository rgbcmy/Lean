# Spec: IBKR Integration（IBKR 券商集成）

## ADDED Requirements

### Requirement: Connect to IBKR TWS/Gateway
系统必须能够连接到 Interactive Brokers 的 TWS (Trader Workstation) 或 Gateway 应用程序。

#### Scenario: Successful connection
- **WHEN** 用户配置正确的 TWS 端口（默认 7496）和账户 ID
- **THEN** 系统成功建立连接并显示"已连接"状态

#### Scenario: Connection failure due to invalid port
- **WHEN** 用户配置错误的端口号
- **THEN** 系统显示连接失败错误，提示检查端口配置

#### Scenario: Connection timeout
- **WHEN** TWS/Gateway 未运行或防火墙阻止连接
- **THEN** 系统在 10 秒后超时，显示超时错误并提供重试选项

### Requirement: Authenticate IBKR account
系统必须验证 IBKR 账户的有效性并获取账户权限。

#### Scenario: Valid credentials
- **WHEN** 用户提供有效的账户 ID 和 TWS 配置
- **THEN** 系统成功验证账户并显示账户类型（纸面交易/实盘）

#### Scenario: Invalid account ID
- **WHEN** 用户提供无效的账户 ID
- **THEN** 系统拒绝连接并显示"账户 ID 无效"错误

### Requirement: Monitor connection status
系统必须实时监控与 IBKR 的连接状态。

#### Scenario: Connection is healthy
- **WHEN** 与 TWS/Gateway 的连接正常
- **THEN** 系统每 10 秒发送心跳检查并显示绿色状态指示器

#### Scenario: Connection lost
- **WHEN** TWS/Gateway 意外关闭或网络中断
- **THEN** 系统检测到连接断开，显示红色状态指示器，并触发自动重连

#### Scenario: Connection restored
- **WHEN** 自动重连成功
- **THEN** 系统显示"连接已恢复"通知并更新状态为绿色

### Requirement: Auto-reconnect on disconnection
系统必须在连接断开后自动尝试重新连接。

#### Scenario: First reconnection attempt
- **WHEN** 检测到连接断开
- **THEN** 系统等待 1 秒后尝试第一次重连

#### Scenario: Exponential backoff
- **WHEN** 重连失败多次
- **THEN** 系统使用指数退避策略（1s → 2s → 4s → 8s → ... → 最大 60s）

#### Scenario: Max reconnection attempts
- **WHEN** 连续重连失败超过 10 次
- **THEN** 系统停止自动重连并通知用户手动检查 TWS/Gateway

### Requirement: Retrieve account summary
系统必须能够查询 IBKR 账户的摘要信息。

#### Scenario: Query account balance
- **WHEN** 用户请求查看账户信息
- **THEN** 系统返回账户余额（现金、净值、可用资金）

#### Scenario: Query buying power
- **WHEN** 用户查询可用购买力
- **THEN** 系统返回当前账户的购买力（考虑保证金）

### Requirement: Subscribe to account updates
系统必须订阅账户变化的实时推送。

#### Scenario: Balance change notification
- **WHEN** 账户余额发生变化（如订单成交）
- **THEN** 系统通过 SignalR 推送更新的余额到前端

#### Scenario: Position update notification
- **WHEN** 持仓数量或价值变化
- **THEN** 系统实时推送持仓更新到前端

### Requirement: Handle IBKR API rate limits
系统必须遵守 IBKR API 的速率限制。

#### Scenario: Respect message rate limit
- **WHEN** 系统发送 API 请求
- **THEN** 确保每秒请求数不超过 50 次（IBKR 限制）

#### Scenario: Queue requests when exceeding limit
- **WHEN** 请求频率超过限制
- **THEN** 系统将请求放入队列并延迟发送

### Requirement: Store IBKR credentials securely
系统必须安全存储 IBKR 连接凭证。

#### Scenario: Encrypt credentials on Windows
- **WHEN** 在 Windows 平台上存储凭证
- **THEN** 使用 DPAPI (Data Protection API) 加密存储

#### Scenario: Encrypt credentials on Linux/macOS
- **WHEN** 在 Linux 或 macOS 平台上存储凭证
- **THEN** 使用 AES-256-GCM 加密存储，密钥由系统密钥派生

#### Scenario: Decrypt credentials on startup
- **WHEN** 系统启动时读取凭证
- **THEN** 自动解密并用于建立连接

### Requirement: Display connection diagnostics
系统必须提供连接诊断信息帮助用户排查问题。

#### Scenario: Show connection details
- **WHEN** 用户查看连接诊断
- **THEN** 系统显示 TWS 版本、API 版本、连接时长、最后心跳时间

#### Scenario: Show error history
- **WHEN** 用户查看错误日志
- **THEN** 系统显示最近 50 条 IBKR API 错误记录（时间、错误代码、描述）

### Requirement: Support paper trading and live accounts
系统必须同时支持纸面交易账户和实盘账户。

#### Scenario: Connect to paper trading account
- **WHEN** 用户配置纸面交易端口（默认 7497）
- **THEN** 系统连接到纸面交易账户并显示"纸面交易"标签

#### Scenario: Connect to live account
- **WHEN** 用户配置实盘交易端口（默认 7496）
- **THEN** 系统连接到实盘账户并显示醒目的"实盘"警告标签

#### Scenario: Prevent accidental live trading
- **WHEN** 用户首次连接实盘账户
- **THEN** 系统显示确认对话框："您正在连接实盘账户，确认继续？"
