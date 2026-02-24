# Proposal: Personal Trading WebUI（个人量化交易Web界面）

## Why（为什么）

当前 Lean 引擎虽然功能强大，但缺少一个友好的 Web 界面来进行日常交易操作和策略管理。用户需要通过编写代码或配置文件来操作，对于个人量化交易者来说门槛较高。本项目旨在构建一个完整的 WebUI 系统，让用户能够通过浏览器进行实盘交易、策略管理、数据可视化和回测分析，特别针对 IBKR（Interactive Brokers）账户和美股市场（股票、ETF）进行优化，快速投入实际使用。

## What Changes（有什么变化）

本变更将为 Lean 引擎添加完整的 Web 界面系统，包括：

- **新增 Web API 后端服务**：基于 ASP.NET Core，提供 RESTful API 和 SignalR 实时通信
- **新增前端 Web 应用**：基于 React + TypeScript + Ant Design，支持中文界面
- **IBKR 券商集成模块**：账户认证、连接管理、实时数据订阅
- **美股交易功能**：股票和 ETF 的实时行情、下单、持仓管理、订单历史
- **策略管理系统**：策略列表、启动/停止控制、参数配置、执行监控
- **回测系统界面**：历史回测执行、结果展示、报告生成
- **数据可视化组件**：K线图、持仓盈亏图表、策略收益曲线
- **风险控制模块**：止损止盈、仓位限制、交易频率控制
- **数据持久化层**：支持 PostgreSQL（生产环境）和 SQLite（本地开发）切换
- **用户认证系统**：登录、权限管理、操作审计
- **中英双语文档**：所有用户文档和 API 文档提供中英文版本

**实施分阶段**：
- **Phase 1 (MVP)**：IBKR连接 + 股票交易 + 基础界面（2-3周）
- **Phase 2 (增强)**：ETF支持 + 实时行情 + 策略管理（1个月）
- **Phase 3 (完整)**：回测系统 + 高级图表 + 风险控制（2-3个月）

## Capabilities（功能模块）

### New Capabilities（新功能模块）

- `ibkr-integration`: IBKR 券商账户集成，包括认证、连接管理、TWS/Gateway 通信
- `us-stock-trading`: 美股股票交易功能，包括实时下单（市价单、限价单）、撤单、查询
- `etf-trading`: ETF 和指数基金交易功能，包括搜索、筛选、定投支持
- `realtime-market-data`: 实时行情数据订阅和推送，支持股票和 ETF 的 Level 1 行情
- `portfolio-management`: 持仓管理界面，包括持仓查询、盈亏计算、仓位分析
- `order-management`: 订单管理系统，包括订单历史、状态追踪、执行详情
- `strategy-execution`: 策略执行引擎控制，启动/停止 Lean 策略、参数传递
- `strategy-management`: 策略管理界面，策略列表、配置编辑、日志查看
- `backtesting-system`: 回测系统 Web 界面，历史数据回测、参数优化、结果对比
- `chart-visualization`: 图表可视化组件，K线图、技术指标、收益曲线、持仓分布图
- `risk-control`: 风险控制模块，止损止盈设置、仓位限制、日内交易频率控制
- `user-authentication`: 用户认证和授权系统，登录、JWT Token、角色权限
- `database-abstraction`: 数据库抽象层，支持 PostgreSQL 和 SQLite 无缝切换
- `webapi-backend`: Web API 后端服务，RESTful API + SignalR 实时推送
- `web-frontend`: Web 前端应用，React SPA，响应式设计，支持中文

### Modified Capabilities（修改的功能模块）

_无现有功能需求层面的修改（纯新增功能）_

## Impact（影响范围）

**新增代码**：
- `Algorithm/WebUI/` - WebUI 相关的算法扩展
- `WebUI/` - 新的 WebUI 项目根目录
  - `WebUI.API/` - ASP.NET Core Web API 项目
  - `WebUI.Frontend/` - React 前端项目
  - `WebUI.Core/` - 核心业务逻辑库
  - `WebUI.Data/` - 数据访问层
  - `WebUI.Tests/` - 单元测试和集成测试

**依赖新增**：
- `Microsoft.AspNetCore.SignalR` - 实时通信
- `Microsoft.EntityFrameworkCore` - ORM 框架
- `Npgsql.EntityFrameworkCore.PostgreSQL` - PostgreSQL 提供程序
- `Microsoft.EntityFrameworkCore.Sqlite` - SQLite 提供程序
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT 认证
- NPM 前端依赖：`react`, `antd`, `typescript`, `echarts`, `axios`

**现有代码影响**：
- `Brokerages/InteractiveBrokers/` - 可能需要扩展以支持 WebUI 的实时状态查询
- `Launcher/` - 可能需要添加 WebUI 启动模式
- `Engine/` - 可能需要暴露更多运行时状态接口

**配置变更**：
- 新增 `webui-config.json` 配置文件（数据库连接、API端口、IBKR设置）
- 新增环境变量支持（`WEBUI_DB_PROVIDER`, `WEBUI_CONNECTION_STRING`）

**文档新增**：
- `docs/WebUI用户指南.md` - 中文用户文档
- `docs/WebUI-User-Guide.md` - 英文用户文档
- `docs/WebUI开发指南.md` - 中文开发者文档
- `docs/WebUI-Developer-Guide.md` - 英文开发者文档
- `docs/WebUI-API-Reference.md` - API 接口文档

**部署影响**：
- 需要运行额外的 Web 服务进程
- 需要配置数据库（PostgreSQL 或 SQLite）
- 需要开放 Web 端口（默认 5000/5001）
- IBKR TWS/Gateway 需要配置 API 访问权限

**安全考虑**：
- 需要实施 HTTPS（生产环境）
- API Key 和交易凭证的加密存储
- 操作审计日志
- CORS 策略配置
- 交易操作二次确认机制
