# Design: Personal Trading WebUI

## Context

当前 Lean 是一个命令行驱动的量化交易引擎，所有操作都通过配置文件和代码完成。虽然功能强大，但缺乏可视化界面，导致：
- 用户需要编写代码才能执行交易
- 实时监控需要查看日志文件
- 策略管理和参数调整门槛高
- 对非技术用户不友好

本设计旨在为 Lean 构建一个现代化的 Web 界面系统，使其能够：
- 通过浏览器进行实盘交易操作
- 实时监控账户、持仓、订单状态
- 可视化管理策略和回测结果
- 特别针对 IBKR 券商和美股市场优化

**技术约束**：
- 必须与现有 Lean 引擎无缝集成，不破坏现有功能
- 支持 Windows/Linux/macOS 跨平台运行
- 本地部署优先（安全性），云端部署可选
- 数据库需支持 PostgreSQL 和 SQLite 切换

**用户群体**：
- 个人量化交易者（主要）
- 小型量化团队
- 算法交易爱好者

## Goals / Non-Goals

**Goals:**
- ✅ 提供完整的 Web 界面，覆盖交易、监控、策略管理全流程
- ✅ 支持 IBKR 实盘交易，专注美股股票和ETF
- ✅ 实时推送行情和订单状态（亚秒级延迟）
- ✅ 数据库抽象层，支持 PostgreSQL（生产）和 SQLite（开发）无缝切换
- ✅ 前后端分离架构，便于独立开发和部署
- ✅ 中文界面和中英双语文档
- ✅ 安全的本地部署方案（HTTPS、JWT、操作审计）
- ✅ 响应式设计，支持桌面和平板访问

**Non-Goals:**
- ❌ 多用户/多租户系统（首版仅支持单用户）
- ❌ 移动端原生 App（仅支持移动浏览器）
- ❌ 社交功能（策略分享、社区讨论）
- ❌ 支付系统或订阅服务
- ❌ 云端托管服务（首版仅本地部署）
- ❌ 支持除 IBKR 外的其他券商（后续版本）
- ❌ 期货、期权、外汇交易（首版专注股票和ETF）

## Decisions

### D1: 架构模式 - 前后端分离 + API Gateway

**决策**：采用前后端完全分离的架构，后端提供 RESTful API + SignalR，前端为独立的 SPA 应用。

**理由**：
- ✅ 前后端独立开发、测试、部署
- ✅ API 可被其他客户端复用（移动端、命令行工具）
- ✅ SignalR 提供双向实时通信，适合推送行情和订单更新
- ✅ 便于进行负载均衡和水平扩展

**备选方案**：
- ❌ Server-Side Rendering (SSR)：增加复杂度，不适合实时交互场景
- ❌ 桌面应用 (Electron)：打包体积大，部署更新复杂

**架构图**：
```
┌─────────────┐      HTTPS/WSS     ┌──────────────────┐
│   Browser   │ ◄──────────────► │   ASP.NET Core   │
│  (React)    │                    │   Web API        │
└─────────────┘                    │   + SignalR      │
                                   └────────┬─────────┘
                                           │
                         ┌─────────────────┼─────────────────┐
                         │                 │                 │
                    ┌────▼────┐      ┌────▼────┐      ┌────▼────┐
                    │  Lean   │      │ Database│      │  IBKR   │
                    │ Engine  │      │(PG/SQLite)     │ TWS/GW  │
                    └─────────┘      └─────────┘      └─────────┘
```

---

### D2: 后端技术栈 - ASP.NET Core (.NET 10)

**决策**：使用 ASP.NET Core (.NET 10) 构建 Web API 和 SignalR Hub。

**理由**：
- ✅ 与 Lean 现有 C# 代码库完美集成
- ✅ 高性能异步 I/O，适合实时交易场景
- ✅ 内置 SignalR 支持，双向实时通信
- ✅ 跨平台支持（Windows/Linux/macOS）
- ✅ .NET 10 的最新性能优化和特性
- ✅ 丰富的中间件生态（JWT、CORS、日志等）

**备选方案**：
- ❌ Node.js/Express：需要 C# 和 Node.js 双语言，增加团队复杂度
- ❌ Python/FastAPI：与 Lean C# 代码集成困难，序列化开销大

---

### D3: 前端技术栈 - React + TypeScript + Ant Design

**决策**：
- **框架**：React 18+
- **语言**：TypeScript 4.9+
- **UI 库**：Ant Design 5.x（中文友好）
- **状态管理**：Zustand（轻量级）
- **图表库**：ECharts（K线）+ Lightweight Charts（实时行情）
- **HTTP 客户端**：Axios
- **实时通信**：@microsoft/signalr

**理由**：
- ✅ React 生态成熟，组件化开发效率高
- ✅ TypeScript 提供类型安全，减少运行时错误
- ✅ Ant Design 中文文档完善，组件丰富，适合企业级应用
- ✅ ECharts 金融图表功能强大，支持K线、技术指标
- ✅ Zustand 比 Redux 简单，适合中小型应用

**备选方案**：
- ❌ Vue.js：团队更熟悉 React
- ❌ Material-UI：中文支持不如 Ant Design
- ❌ Redux：对本项目来说过于复杂

---

### D4: 数据库策略 - EF Core + Repository 模式 + Provider 切换

**决策**：
- 使用 Entity Framework Core 作为 ORM
- 实现 Repository 和 Unit of Work 模式
- 配置文件切换 PostgreSQL 和 SQLite

**理由**：
- ✅ EF Core 原生支持多数据库（PG、SQLite、SQL Server）
- ✅ Code First 迁移，数据库版本管理清晰
- ✅ Repository 模式隔离数据访问层，易于测试
- ✅ 切换数据库只需修改连接字符串和 Provider 配置

**配置示例**：
```json
{
  "Database": {
    "Provider": "PostgreSQL",  // 或 "SQLite"
    "ConnectionString": "Host=localhost;Database=leanui;Username=postgres;Password=xxx"
    // SQLite: "Data Source=lean.db"
  }
}
```

**数据模型**（主要实体）：
- `User` - 用户账户
- `BrokerAccount` - 券商账户配置
- `Strategy` - 策略定义
- `StrategyExecution` - 策略执行记录
- `Order` - 订单历史（持久化）
- `Position` - 持仓快照
- `AuditLog` - 操作审计日志

**备选方案**：
- ❌ Dapper（Micro-ORM）：需要手写 SQL，多数据库兼容困难
- ❌ 直接使用 ADO.NET：代码冗长，维护成本高

---

### D5: Lean 引擎集成方式 - Process + IPC

**决策**：WebUI 作为独立进程运行，通过以下方式与 Lean 引擎交互：
1. **启动/停止策略**：通过 `Process.Start()` 启动 Lean CLI，传递配置文件
2. **实时状态查询**：通过扩展 Lean 暴露 Named Pipe 或 TCP Socket IPC 接口
3. **历史数据**：从数据库读取（Lean 同步写入）

**理由**：
- ✅ 解耦：Lean 崩溃不影响 WebUI，WebUI 崩溃不影响 Lean
- ✅ 灵活：可独立升级 WebUI 或 Lean
- ✅ 安全：进程隔离，避免内存共享风险
- ✅ 可扩展：未来可支持多个 Lean 实例

**IPC 方案选择**：
- **生产环境**：Named Pipe（本地进程间通信，高性能）
- **开发环境**：HTTP API（便于调试）

**备选方案**：
- ❌ 内嵌 Lean 作为库：Lean 引擎运行在 WebUI 进程内，耦合度高，崩溃风险大
- ❌ 共享内存：跨平台兼容性差，复杂度高

---

### D6: IBKR 集成策略 - 复用 Lean 现有 IBrokerageHandler

**决策**：
- 使用 Lean 现有的 `InteractiveBrokers` Brokerage 实现
- WebUI 不直接连接 IBKR TWS/Gateway，而是通过 Lean 引擎间接交互
- WebUI 提供配置界面（TWS端口、账号等），生成 Lean 配置文件

**理由**：
- ✅ 避免重复开发 IBKR 协议
- ✅ 利用 Lean 成熟的连接管理和错误处理
- ✅ 保持代码一致性

**数据流**：
```
WebUI → Lean Engine → IBKR TWS/Gateway
          ↓
       Database (订单、持仓历史)
```

**WebUI 需要扩展的功能**：
- 实时查询连接状态
- 获取账户余额和持仓
- 订阅实时行情推送

---

### D7: 实时数据推送 - SignalR Hub

**决策**：使用 SignalR Hub 推送实时数据，Topics 包括：
- `MarketData` - 实时行情（价格、成交量）
- `OrderUpdate` - 订单状态变化
- `PositionUpdate` - 持仓变化
- `AccountUpdate` - 账户余额变化
- `StrategyLog` - 策略执行日志

**理由**：
- ✅ SignalR 自动处理 WebSocket/长轮询降级
- ✅ 支持客户端订阅特定 Topic（避免推送无关数据）
- ✅ 可扩展（支持 Redis Backplane 做集群）

**频率限制**：
- 行情推送：最高 1次/秒（避免前端渲染压力）
- 订单更新：实时推送（状态变化立即通知）
- 持仓更新：5秒合并推送

**备选方案**：
- ❌ Server-Sent Events (SSE)：单向推送，不支持客户端发送消息
- ❌ 轮询：延迟高，服务器压力大

---

### D8: 认证授权 - JWT + 本地存储

**决策**：
- **首版**：单用户模式，简化认证（配置文件预设用户名/密码哈希）
- **认证方式**：JWT Token（15分钟过期）+ Refresh Token（7天）
- **授权**：基于角色的访问控制（RBAC），预留扩展空间
- **密码存储**：PBKDF2 哈希（Salt + 10000 迭代）

**理由**：
- ✅ JWT 无状态，易于横向扩展
- ✅ Refresh Token 减少频繁登录
- ✅ 单用户模式降低首版复杂度

**流程**：
```
1. 用户登录 → 验证密码 → 返回 Access Token + Refresh Token
2. 前端请求携带 Access Token（Header: Authorization: Bearer xxx）
3. Token 过期 → 使用 Refresh Token 刷新 → 获取新 Access Token
```

**未来扩展**：多用户模式（数据库存储用户表）

---

### D9: 安全策略

**决策**：
- **HTTPS 强制**：生产环境必须启用（自签名证书或 Let's Encrypt）
- **CORS 策略**：仅允许前端域名访问（默认 `http://localhost:3000`）
- **API Key 加密**：IBKR API 凭证使用 DPAPI（Windows）或 AES-GCM（Linux/macOS）加密存储
- **操作审计**：所有交易操作写入 `AuditLog` 表（用户、时间、操作、IP）
- **二次确认**：危险操作（清仓、删除策略）前端二次确认

**理由**：
- ✅ HTTPS 防止中间人攻击
- ✅ 加密存储防止凭证泄露
- ✅ 审计日志可追溯
- ✅ 二次确认防止误操作

---

### D10: 部署策略 - Docker Compose（推荐）

**决策**：提供两种部署方式：

1. **Docker Compose（推荐）**：
   - 容器化 WebUI API、前端（Nginx）、PostgreSQL
   - 一键启动：`docker-compose up`
   
2. **本地部署**：
   - WebUI API：`dotnet WebUI.API.dll`
   - 前端：`npm run build` + 静态文件服务器
   - 数据库：外部 PostgreSQL 或 SQLite 文件

**docker-compose.yml 示例**：
```yaml
services:
  api:
    build: ./WebUI.API
    ports:
      - "5000:5000"
    environment:
      - Database__Provider=PostgreSQL
      - Database__ConnectionString=Host=db;...
  
  frontend:
    build: ./WebUI.Frontend
    ports:
      - "3000:80"
  
  db:
    image: postgres:15
    environment:
      POSTGRES_DB: leanui
```

**理由**：
- ✅ Docker 确保环境一致性
- ✅ 一键启动，降低部署门槛
- ✅ 方便版本回滚

---

### D11: 开发工作流 - Monorepo

**决策**：将 WebUI 作为 Lean 仓库的子项目，目录结构：
```
Lean/
├── WebUI/
│   ├── WebUI.API/          # ASP.NET Core API
│   ├── WebUI.Frontend/     # React 前端
│   ├── WebUI.Core/         # 业务逻辑库
│   ├── WebUI.Data/         # 数据访问层
│   ├── WebUI.Tests/        # 测试
│   └── docker-compose.yml
```

**理由**：
- ✅ 单一仓库，版本同步
- ✅ 便于 CI/CD 集成
- ✅ 共享 Lean 代码库引用

**备选方案**：
- ❌ 独立仓库：版本同步困难，集成复杂

## Risks / Trade-offs

### R1: Lean 引擎稳定性依赖
**风险**：WebUI 依赖 Lean 引擎运行，Lean 崩溃会导致交易中断。  
**缓解**：
- 实现 Lean 进程健康检查（每10秒 ping IPC）
- 自动重启机制（崩溃后5秒重启）
- 前端显示连接状态（红色警告）

---

### R2: IBKR TWS/Gateway 连接断开
**风险**：IBKR TWS/Gateway 需保持运行，断开会导致无法交易。  
**缓解**：
- 自动重连逻辑（指数退避：1s → 2s → 4s → ...→ 60s）
- 连接状态监控（前端实时显示）
- 邮件/推送通知（连接失败超过5分钟）

---

### R3: 实时数据推送压力
**风险**：市场开盘时大量行情推送可能导致前端渲染卡顿或服务器压力。  
**缓解**：
- 限制推送频率（最高 1次/秒）
- 前端使用虚拟滚动（订单列表）
- SignalR 连接数限制（单实例最多100连接）

---

### R4: 数据库性能瓶颈
**风险**：订单和行情数据量大，SQLite 可能成为性能瓶颈。  
**缓解**：
- 生产环境强烈推荐 PostgreSQL
- 历史数据定期归档（6个月以上数据移至冷存储）
- 查询加索引（账户ID、时间戳、交易对）

---

### R5: 跨平台兼容性
**风险**：Windows/Linux/macOS 环境差异可能导致功能异常。  
**缓解**：
- 使用 .NET 跨平台 API（避免 P/Invoke）
- 配置路径使用 `Path.Combine()`（不硬编码斜杠）
- CI/CD 多平台测试（Windows + Ubuntu + macOS）

---

### R6: 安全性风险
**风险**：Web 界面暴露可能被未授权访问或攻击。  
**缓解**：
- 默认绑定 `localhost`（仅本地访问）
- 生产环境强制 HTTPS
- JWT Token 短期过期（15分钟）
- 登录失败锁定（5次失败锁定30分钟）
- 不暴露敏感错误信息（前端仅显示通用错误）

---

### R7: 前后端版本不兼容
**风险**：前后端独立部署可能导致 API 版本不匹配。  
**缓解**：
- API 版本化（`/api/v1/...`）
- 前端启动时检查 API 版本（`GET /api/version`）
- 版本不匹配时前端显示升级提示

## Migration Plan

### Phase 1: 开发环境搭建（第1-2周）

**目标**：搭建基础架构，验证技术路线。

**步骤**：
1. 创建项目结构（WebUI.API、WebUI.Frontend、WebUI.Core、WebUI.Data）
2. 配置 EF Core + SQLite（开发用）
3. 实现基础 API（用户登录、健康检查）
4. 实现前端框架（React + Ant Design + 路由）
5. 配置 SignalR Hub（测试连接）

**验收标准**：
- ✅ 前端能成功登录并获取 JWT Token
- ✅ SignalR 连接成功，能接收测试消息
- ✅ 数据库迁移正常运行（SQLite）

---

### Phase 2: IBKR 集成 + 基础交易（第3-5周）

**目标**：实现 IBKR 连接和美股股票交易功能。

**步骤**：
1. 扩展 Lean IBrokerage 暴露 IPC 接口
2. WebUI API 实现 IBKR 账户查询（余额、持仓）
3. 实现下单 API（市价单、限价单）
4. 前端实现交易界面（下单表单、订单列表）
5. 实时订单状态推送（SignalR）

**验收标准**：
- ✅ 连接 IBKR Paper Trading 账户成功
- ✅ 能查询账户余额和持仓
- ✅ 能下市价单和限价单，订单能成功执行
- ✅ 订单状态变化实时推送到前端

---

### Phase 3: 策略管理 + ETF 支持（第6-8周）

**目标**：策略启动/停止控制，支持 ETF 交易。

**步骤**：
1. 实现策略 CRUD API（创建、列表、删除）
2. 策略启动/停止控制（Process 管理）
3. ETF 搜索和交易（复用股票交易接口）
4. 前端策略管理界面
5. 策略执行日志实时展示

**验收标准**：
- ✅ 能创建、启动、停止策略
- ✅ 策略日志实时推送到前端
- ✅ 支持 ETF 交易（如 SPY、QQQ）

---

### Phase 4: 数据可视化（第9-10周）

**目标**：K线图、持仓图表、收益曲线。

**步骤**：
1. 集成 ECharts，实现 K线图组件
2. 持仓饼图/柱状图
3. 策略收益曲线
4. 实时行情图（Lightweight Charts）

**验收标准**：
- ✅ 能查看股票日K线图
- ✅ 持仓分布图表正常展示
- ✅ 策略收益曲线随时间更新

---

### Phase 5: 回测系统 + 风险控制（第11-12周）

**目标**：历史回测和风险管理功能。

**步骤**：
1. 回测 API（调用 Lean 回测）
2. 回测结果解析和存储
3. 前端回测界面（参数配置、结果展示）
4. 风险控制规则配置（止损、仓位限制）
5. 风险规则实时检查和告警

**验收标准**：
- ✅ 能执行历史回测并查看结果
- ✅ 能配置止损规则，触发时自动平仓
- ✅ 超过仓位限制时阻止下单

---

### Phase 6: 生产就绪（第13-14周）

**目标**：安全加固、性能优化、文档完善。

**步骤**：
1. HTTPS 配置（自签名证书生成脚本）
2. API Key 加密存储
3. PostgreSQL 支持测试
4. Docker Compose 部署测试
5. 编写用户文档和开发者文档（中英文）
6. 性能测试（100订单/秒压测）

**验收标准**：
- ✅ HTTPS 正常工作
- ✅ PostgreSQL 和 SQLite 都能正常运行
- ✅ Docker Compose 一键部署成功
- ✅ 文档完整（安装、配置、使用）

---

### 回滚策略

如果上线后出现严重问题：
1. **立即停止 WebUI 服务**（Lean 引擎独立运行不受影响）
2. **切换回命令行模式操作 Lean**
3. **查看审计日志排查问题**
4. **修复后重新部署**

**关键原则**：WebUI 是 Lean 的可选组件，崩溃不影响核心交易功能。

## Open Questions

### Q1: 多账户支持时机？
**问题**：首版仅支持单个 IBKR 账户，何时支持多账户切换？  
**待定**：根据用户反馈决定，可能在 Phase 2（V2.0）加入。

---

### Q2: 移动端适配程度？
**问题**：响应式设计在移动端能覆盖到什么程度？是否需要原生 App？  
**待定**：首版保证平板可用，手机端优先级低。原生 App 在用户量达到一定规模后考虑。

---

### Q3: 其他券商支持？
**问题**：除 IBKR 外，是否支持富途、老虎、盈透等券商？  
**待定**：首版专注 IBKR，其他券商根据用户投票决定优先级。

---

### Q4: 云端部署方案？
**问题**：是否提供官方云端托管服务（SaaS）？  
**待定**：首版专注本地部署，云端服务需考虑数据安全、合规、成本等因素。

---

### Q5: WebUI 性能目标？
**问题**：支持多少并发用户？多少订单/秒？  
**当前目标**：
- 并发用户：10（单机）
- 订单吞吐：50单/秒
- 行情推送延迟：< 500ms

**待优化**：根据实际使用情况调整。
