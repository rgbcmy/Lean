# Tasks: Personal Trading WebUI Implementation

## 1. Project Setup and Infrastructure

- [x] 1.1 创建项目目录结构（WebUI/WebUI.API, WebUI.Frontend, WebUI.Core, WebUI.Data, WebUI.Tests）
- [x] 1.2 初始化 ASP.NET Core Web API 项目（.NET 10）
- [x] 1.3 初始化 React + TypeScript 前端项目（使用 Vite）
- [x] 1.4 配置 Solution 文件，添加所有项目引用
- [x] 1.5 添加后端 NuGet 包（EF Core, SignalR, JWT Authentication, Npgsql, SQLite）
- [x] 1.6 添加前端 npm 包（antd, axios, @microsoft/signalr, zustand, echarts, react-router-dom）
- [x] 1.7 配置 Git 忽略文件（.gitignore）
- [x] 1.8 创建 Docker 和 docker-compose.yml 配置文件
- [x] 1.9 设置 CI/CD 基础配置（GitHub Actions 或 Azure DevOps）
- [x] 1.10 创建配置文件模板（webui-config.json, appsettings.json）

## 2. Database Layer (数据库层)

- [x] 2.1 定义 EF Core DbContext（WebUIDbContext）
- [x] 2.2 创建实体模型（User, BrokerAccount, Strategy, StrategyExecution, Order, Position, AuditLog）
- [x] 2.3 配置实体关系（外键、导航属性）
- [x] 2.4 实现数据库提供程序抽象层（PostgreSQL 和 SQLite 切换逻辑）
- [x] 2.5 创建初始 EF Core 迁移（InitialCreate）
- [x] 2.6 实现 Repository 接口（IRepository<T>, IOrderRepository, IStrategyRepository）
- [x] 2.7 实现 Unit of Work 模式（IUnitOfWork）
- [x] 2.8 实现数据种子逻辑（默认管理员账户、示例策略）
- [x] 2.9 编写数据库连接验证和健康检查代码
- [x] 2.10 配置数据库连接字符串管理（支持环境变量）
- [x] 2.11 添加数据库索引优化（UserId, StrategyId, Timestamp 等）
- [x] 2.12 编写数据库迁移单元测试（PostgreSQL 和 SQLite）

## 3. Authentication & Authorization (认证授权)

- [x] 3.1 实现密码哈希服务（PBKDF2, 10000 迭代）
- [x] 3.2 实现 JWT Token 生成和验证服务
- [x] 3.3 实现 Refresh Token 机制（存储、轮换、撤销）
- [x] 3.4 创建登录 API 端点（POST /api/v1/auth/login）
- [x] 3.5 创建登出 API 端点（POST /api/v1/auth/logout）
- [x] 3.6 创建刷新 Token API 端点（POST /api/v1/auth/refresh）
- [x] 3.7 实现 JWT 认证中间件配置
- [x] 3.8 实现操作审计日志记录（AuditLog 表）
- [x] 3.9 实现登录失败锁定机制（5 次失败锁定 30 分钟）
- [x] 3.10 实现密码强度验证规则
- [x] 3.11 创建修改密码 API 端点（POST /api/v1/auth/change-password）
- [x] 3.12 编写认证相关单元测试

## 4. Web API Core (API 核心功能)

- [x] 4.1 配置 ASP.NET Core 中间件（CORS, 日志, 错误处理）
- [x] 4.2 实现统一错误响应格式（ErrorResponse model）
- [x] 4.3 实现全局异常处理中间件
- [x] 4.4 配置 Swagger/OpenAPI（中英文注释）
- [x] 4.5 实现 API 版本控制（/api/v1/...）
- [x] 4.6 实现请求验证（FluentValidation）
- [x] 4.7 实现速率限制中间件（每用户 60 req/min）
- [x] 4.8 配置结构化日志（Serilog）
- [x] 4.9 实现健康检查端点（/health, /health/detailed, /health/ready）
- [x] 4.10 配置响应压缩（Gzip）
- [x] 4.11 实现优雅关闭逻辑
- [x] 4.12 编写 API 核心功能集成测试

## 5. IBKR Integration (IBKR 集成)

- [x] 5.1 扩展 Lean IBrokerageHandler 暴露 IPC 接口（Named Pipe 或 TCP）
- [x] 5.2 实现 IBKR 连接管理服务（连接、断开、健康检查）
- [x] 5.3 实现 IBKR 凭证加密存储（DPAPI on Windows, AES-GCM on Linux/macOS）
- [x] 5.4 创建 IBKR 账户查询 API（GET /api/v1/ibkr/account）
- [x] 5.5 实现账户余额和购买力查询
- [x] 5.6 实现连接状态监控（每 10 秒心跳）
- [x] 5.7 实现自动重连逻辑（指数退避：1s → 2s → 4s → ... → 60s）
- [x] 5.8 创建连接诊断 API（GET /api/v1/ibkr/diagnostics）
- [x] 5.9 实现纸面交易和实盘账户区分
- [x] 5.10 编写 IBKR 集成单元测试（使用 Mock）

## 6. Market Data (实时行情)

- [x] 6.1 实现行情数据订阅服务（通过 Lean 引擎）
- [x] 6.2 实现 Level 1 行情数据解析（最新价、买卖价、成交量）
- [x] 6.3 实现行情数据缓存（Redis, 5 分钟过期）
- [x] 6.4 创建行情订阅 API（POST /api/v1/market/subscribe）
- [x] 6.5 创建行情查询 API（GET /api/v1/market/quote/:symbol）
- [x] 6.6 实现市场状态检查（开盘、休市、盘前、盘后）
- [x] 6.7 实现延迟行情标识（15 分钟延迟）
- [x] 6.8 创建市场概览 API（GET /api/v1/market/summary）
- [x] 6.9 实现行情数据降采样（限制推送频率为 1次/秒）
- [x] 6.10 编写行情数据单元测试

## 7. SignalR Real-time Communication (实时通信)

- [x] 7.1 创建 SignalR Hub（MarketDataHub, OrderHub, StrategyHub）
- [x] 7.2 实现客户端订阅管理（Topic-based Groups）
- [x] 7.3 实现行情数据推送（MarketData topic）
- [x] 7.4 实现订单更新推送（OrderUpdate topic）
- [x] 7.5 实现持仓更新推送（PositionUpdate topic）
- [x] 7.6 实现策略日志推送（StrategyLog topic）
- [x] 7.7 配置 SignalR JWT 认证
- [x] 7.8 实现推送频率限制（合并更新）
- [x] 7.9 实现客户端重连处理
- [x] 7.10 编写 SignalR Hub 单元测试

## 8. Trading APIs (交易接口)

- [x] 8.1 创建股票搜索 API（GET /api/v1/stocks/search?q=AAPL）
- [x] 8.2 创建市价单下单 API（POST /api/v1/orders/market）
- [x] 8.3 创建限价单下单 API（POST /api/v1/orders/limit）
- [x] 8.4 实现订单参数验证（数量、价格、购买力检查）
- [x] 8.5 创建取消订单 API（DELETE /api/v1/orders/:id）
- [x] 8.6 创建订单查询 API（GET /api/v1/orders）
- [x] 8.7 创建订单详情 API（GET /api/v1/orders/:id）
- [x] 8.8 实现订单历史查询（支持日期范围、筛选、分页）
- [x] 8.9 创建订单成本预估 API（POST /api/v1/orders/estimate）
- [x] 8.10 编写交易 API 单元测试

## 9. Portfolio & Position Management (持仓管理)

- [x] 9.1 创建持仓查询 API（GET /api/v1/positions）
- [x] 9.2 实现持仓盈亏计算（未实现盈亏、百分比）
- [x] 9.3 创建持仓详情 API（GET /api/v1/positions/:symbol）
- [x] 9.4 创建平仓 API（POST /api/v1/positions/:symbol/close）
- [x] 9.5 实现持仓排序和筛选
- [x] 9.6 创建持仓配置查询 API（GET /api/v1/portfolio/allocation）
- [x] 9.7 实现持仓历史记录（买入/卖出记录）
- [x] 9.8 创建持仓导出 API（GET /api/v1/positions/export）
- [x] 9.9 实现账户收益曲线数据 API（GET /api/v1/portfolio/equity-curve）
- [x] 9.10 编写持仓管理单元测试

## 10. ETF Trading (ETF 交易)

- [x] 10.1 创建 ETF 搜索 API（GET /api/v1/etfs/search）
- [x] 10.2 创建 ETF 详情 API（GET /api/v1/etfs/:symbol）
- [x] 10.3 复用股票交易接口支持 ETF 交易
- [x] 10.4 实现定投计划 CRUD API（POST/GET/PUT/DELETE /api/v1/etfs/recurring-plans）
- [x] 10.5 实现定投自动执行服务（定时任务）
- [x] 10.6 创建 ETF 对比 API（POST /api/v1/etfs/compare）
- [x] 10.7 创建 ETF 分红信息 API（GET /api/v1/etfs/:symbol/dividends）
- [x] 10.8 编写 ETF 交易单元测试

## 11. Strategy Management (策略管理)

- [x] 11.1 创建策略 CRUD API（POST/GET/PUT/DELETE /api/v1/strategies）
- [x] 11.2 实现策略配置文件生成（JSON/XML for Lean）
- [x] 11.3 实现策略代码文件上传和存储
- [x] 11.4 创建策略克隆 API（POST /api/v1/strategies/:id/clone）
- [x] 11.5 创建策略版本管理 API
- [x] 11.6 实现策略标签管理
- [x] 11.7 创建策略导出/导入 API
- [x] 11.8 实现策略性能摘要查询 API
- [x] 11.9 编写策略管理单元测试

## 12. Strategy Execution (策略执行)

- [x] 12.1 实现 Lean 进程启动服务（Process.Start）
- [x] 12.2 实现 Lean 进程停止服务（优雅关闭 + 强制终止）
- [x] 12.3 实现策略进程健康监控（IPC 心跳检查）
- [x] 12.4 创建策略启动 API（POST /api/v1/strategies/:id/start）
- [x] 12.5 创建策略停止 API（POST /api/v1/strategies/:id/stop）
- [x] 12.6 实现策略参数传递（命令行参数或配置文件）
- [x] 12.7 实现策略日志流式传输（File Watcher + SignalR）
- [x] 12.8 实现策略崩溃检测和自动重启
- [x] 12.9 实现策略定时执行（调度服务）
- [x] 12.10 创建策略执行历史 API（GET /api/v1/strategies/:id/executions）
- [x] 12.11 编写策略执行单元测试

## 13. Backtesting (回测系统)

- [x] 13.1 创建回测配置 API（POST /api/v1/backtests）
- [x] 13.2 实现回测执行服务（调用 Lean 回测）
- [x] 13.3 实现回测结果解析和存储
- [x] 13.4 创建回测结果查询 API（GET /api/v1/backtests/:id）
- [x] 13.5 实现回测结果对比 API（POST /api/v1/backtests/compare）
- [x] 13.6 实现参数优化服务（网格搜索）
- [x] 13.7 创建参数优化 API（POST /api/v1/backtests/optimize）
- [x] 13.8 实现回测报告生成（PDF/JSON）
- [x] 13.9 创建回测导出 API（GET /api/v1/backtests/:id/export）
- [x] 13.10 编写回测系统单元测试

## 14. Risk Control (风险控制)

- [x] 14.1 实现止损规则配置和监控服务
- [x] 14.2 实现止盈规则配置和监控服务
- [x] 14.3 实现仓位限制检查（单个股票、总持仓数、现金比例）
- [x] 14.4 实现日内交易频率限制
- [x] 14.5 实现保证金使用监控和告警
- [x] 14.6 实现投资组合集中度检查
- [x] 14.7 实现熔断机制（价格异常波动、订单量异常）
- [x] 14.8 创建风险指标计算服务（VaR, Sharpe Ratio, Max Drawdown）
- [x] 14.9 实现 PDT（Pattern Day Trader）规则检查
- [x] 14.10 创建风险配置 API（POST/GET/PUT /api/v1/risk/config）
- [x] 14.11 创建风险报告生成 API（GET /api/v1/risk/report）
- [x] 14.12 编写风险控制单元测试

## 15. Frontend - Project Setup (前端基础)

- [x] 15.1 配置 React Router（路由配置）
- [x] 15.2 配置 Ant Design 主题（中文语言包）
- [x] 15.3 配置 Axios（baseURL, 拦截器）
- [x] 15.4 配置 SignalR 客户端连接
- [x] 15.5 设置 Zustand 全局状态管理（用户、连接状态）
- [x] 15.6 实现 JWT Token 管理（存储、刷新、自动续期）
- [x] 15.7 实现 HTTP 请求拦截器（添加 Authorization Header）
- [x] 15.8 实现响应拦截器（错误处理、Token 刷新）
- [x] 15.9 配置深色/浅色主题切换
- [x] 15.10 配置环境变量（API baseURL）

## 16. Frontend - Authentication Pages (认证页面)

- [x] 16.1 创建登录页面组件（LoginPage）
- [x] 16.2 实现登录表单（用户名、密码、记住我）
- [x] 16.3 实现登录表单验证
- [x] 16.4 集成登录 API 调用
- [x] 16.5 实现登录错误提示（Toast）
- [x] 16.6 实现登录后跳转
- [x] 16.7 创建修改密码页面（ChangePasswordPage）
- [x] 16.8 实现修改密码表单和验证
- [x] 16.9 实现登出功能（清除 Token + 跳转）
- [x] 16.10 实现登录状态持久化（Page Refresh）

## 17. Frontend - Layout & Navigation (布局导航)

- [x] 17.1 创建主布局组件（MainLayout: Sidebar + Header + Content）
- [x] 17.2 实现侧边栏导航菜单
- [x] 17.3 实现顶部导航栏（用户菜单、通知、IBKR 状态）
- [x] 17.4 实现面包屑导航
- [x] 17.5 实现响应式布局（Desktop/Tablet/Mobile）
- [x] 17.6 实现汉堡菜单（移动端）
- [x] 17.7 创建首页/仪表板（Dashboard）
- [x] 17.8 实现快速操作区（快速下单、查看持仓）
- [x] 17.9 实现通知中心（Notification Center）
- [x] 17.10 实现帮助和文档链接

## 18. Frontend - Trading Pages (交易页面)

- [x] 18.1 创建股票搜索组件（StockSearch）
- [x] 18.2 创建股票详情页（StockDetailPage）
- [x] 18.3 创建下单表单组件（OrderForm: 市价单/限价单）
- [x] 18.4 实现下单表单验证（数量、价格、购买力）
- [x] 18.5 实现订单确认对话框
- [x] 18.6 集成下单 API 调用
- [x] 18.7 创建订单列表页（OrdersPage）
- [x] 18.8 实现订单筛选和排序
- [x] 18.9 实现取消订单功能
- [x] 18.10 实现订单实时状态更新（SignalR）
- [x] 18.11 创建 ETF 搜索和交易页面
- [x] 18.12 创建定投计划管理页面

## 19. Frontend - Portfolio Pages (持仓页面)

- [x] 19.1 创建持仓列表页（PositionsPage）
- [x] 19.2 实现持仓表格（股票、数量、成本、当前价、盈亏）
- [x] 19.3 实现持仓实时价格更新（SignalR）
- [x] 19.4 实现持仓排序和筛选
- [x] 19.5 创建持仓详情页（PositionDetailPage）
- [x] 19.6 实现一键平仓功能
- [x] 19.7 创建持仓配置饼图（Position Allocation Chart）
- [x] 19.8 创建账户收益曲线图（Equity Curve）
- [x] 19.9 实现持仓导出功能（CSV/Excel）
- [x] 19.10 创建投资组合分析页面

## 20. Frontend - Strategy Pages (策略页面)

- [x] 20.1 创建策略列表页（StrategiesPage）
- [x] 20.2 实现策略卡片/表格展示
- [x] 20.3 创建新建策略页面（CreateStrategyPage）
- [x] 20.4 实现策略模板选择
- [x] 20.5 创建策略编辑页面（EditStrategyPage）
- [x] 20.6 实现策略参数配置表单
- [x] 20.7 创建策略详情页（StrategyDetailPage）
- [x] 20.8 实现策略启动/停止按钮
- [x] 20.9 实现策略日志实时展示（SignalR + 虚拟滚动）
- [x] 20.10 实现策略克隆和删除功能
- [x] 20.11 创建策略版本历史页面
- [x] 20.12 实现策略导出/导入功能

## 21. Frontend - Backtesting Pages (回测页面)

- [x] 21.1 创建回测配置页面（BacktestConfigPage）
- [x] 21.2 实现回测参数表单（日期范围、初始资金、基准）
- [x] 21.3 创建回测结果页（BacktestResultPage）
- [x] 21.4 实现回测指标展示（收益率、夏普比率、最大回撤）
- [x] 21.5 集成收益曲线图（ECharts）
- [x] 21.6 集成回撤图（Drawdown Chart）
- [x] 21.7 创建回测交易列表
- [x] 21.8 创建回测对比页面（CompareBacktestsPage）
- [x] 21.9 创建参数优化页面（OptimizationPage）
- [x] 21.10 实现回测报告导出（PDF）

## 22. Frontend - Chart Components (图表组件)

- [x] 22.1 创建 K线图组件（CandlestickChart - ECharts）
- [x] 22.2 实现时间周期切换（日线、周线、月线）
- [x] 22.3 实现图表缩放和平移
- [x] 22.4 集成技术指标（MA, MACD, RSI）
- [x] 22.5 创建实时行情图组件（LivePriceChart - Lightweight Charts）
- [x] 22.6 创建持仓配置饼图组件（AllocationPieChart）
- [x] 22.7 创建盈亏柱状图组件（P&L Bar Chart）
- [x] 22.8 创建收益曲线图组件（Equity Curve）
- [x] 22.9 实现图表导出功能（PNG/SVG）
- [x] 22.10 实现图表响应式设计

## 23. Frontend - Risk & Settings (风险和设置)

- [x] 23.1 创建风险控制配置页面（RiskConfigPage）
- [x] 23.2 实现止损止盈规则配置表单
- [x] 23.3 实现仓位限制配置表单
- [x] 23.4 实现交易频率限制配置
- [x] 23.5 创建风险指标展示页面（Risk Dashboard）
- [x] 23.6 创建系统设置页面（SettingsPage）
- [x] 23.7 实现 IBKR 连接配置表单
- [x] 23.8 实现数据库配置界面（Provider 切换）
- [x] 23.9 实现主题和语言偏好设置
- [x] 23.10 实现通知偏好设置

## 24. Testing & Quality Assurance (测试与质量)

- [x] 24.1 编写后端单元测试（覆盖率 > 70%）
- [x] 24.2 编写后端集成测试（API 端点）
- [x] 24.3 编写前端组件单元测试（Jest + React Testing Library）
- [x] 24.4 编写 E2E 测试（Playwright 或 Cypress）
- [x] 24.5 执行性能测试（API 响应时间、并发压力）
- [x] 24.6 执行安全审计（OWASP Top 10）
- [x] 24.7 代码质量扫描（SonarQube 或 CodeQL）
- [x] 24.8 修复所有 Critical 和 High 优先级问题
- [x] 24.9 跨浏览器测试（Chrome, Firefox, Edge, Safari）
- [x] 24.10 跨平台测试（Windows, Linux, macOS）

## 25. Documentation (文档编写)

- [x] 25.1 编写用户安装指南（中文版 - WebUI用户指南.md）
- [x] 25.2 编写用户安装指南（英文版 - WebUI-User-Guide.md）
- [x] 25.3 编写 IBKR 连接配置教程
- [x] 25.4 编写数据库配置教程（PostgreSQL 和 SQLite）
- [x] 25.5 编写 Docker 部署教程
- [x] 25.6 编写开发者指南（中文版 - WebUI开发者完整指南.md）
- [x] 25.7 编写开发者指南（英文版 - WebUI-Developer-Guide.md）
- [x] 25.8 编写 API 参考文档（WebUI-API-Reference.md）
- [x] 25.9 创建架构图和流程图 (Architecture-Diagrams.md)
- [x] 25.10 录制演示视频（快速开始教程）(Video-Tutorial-Script.md)

## 26. Deployment & Production Readiness (部署与生产就绪)

- [x] 26.1 配置生产环境 appsettings.json
- [x] 26.2 生成自签名 HTTPS 证书（或 Let's Encrypt）
- [x] 26.3 配置 HTTPS 重定向和 HSTS
- [x] 26.4 构建前端生产版本（npm run build）
- [x] 26.5 配置静态文件服务（Nginx 或 ASP.NET Static Files）
- [x] 26.6 测试 Docker Compose 部署
- [x] 26.7 配置日志聚合（ELK Stack 或 Seq）
- [x] 26.8 配置监控和告警（Prometheus + Grafana 或 Application Insights）
- [x] 26.9 配置数据库备份策略（自动备份脚本）
- [x] 26.10 编写运维手册（故障排查、回滚流程）
- [x] 26.11 执行灾难恢复演练
- [x] 26.12 准备生产环境发布清单

## 27. Phase 1 MVP Verification (第一阶段验收)

- [x] 27.1 验证 IBKR 纸面交易账户连接成功
- [x] 27.2 验证美股股票手动下单功能（市价单、限价单）
- [x] 27.3 验证订单状态实时更新
- [x] 27.4 验证持仓查询和显示
- [x] 27.5 验证账户余额查询
- [x] 27.6 验证基础 Web 界面可用性（登录、导航、响应式）
- [x] 27.7 用户验收测试（UAT - 至少 3 个真实场景）
- [x] 27.8 收集第一阶段用户反馈

## 28. Phase 2 Enhancement (第二阶段增强)

- [x] 28.1 验证实时行情订阅和显示
- [x] 28.2 验证 ETF 搜索和交易功能
- [x] 28.3 验证定投计划创建和执行
- [x] 28.4 验证策略启动/停止功能
- [x] 28.5 验证策略日志实时展示
- [x] 28.6 验证 K线图显示和技术指标
- [x] 28.7 优化 UI/UX（根据用户反馈）
- [x] 28.8 性能优化（API 响应时间 < 500ms）

## 29. Phase 3 Complete (第三阶段完整功能)

- [x] 29.1 验证回测系统正常工作
- [x] 29.2 验证参数优化功能
- [x] 29.3 验证回测报告生成
- [x] 29.4 验证风险控制规则生效（止损、仓位限制）
- [x] 29.5 验证风险指标计算准确性
- [x] 29.6 验证所有图表组件正常渲染
- [x] 29.7 最终性能调优（负载测试 100 订单/秒）
- [x] 29.8 最终安全加固审查

## 30. Release Preparation (发布准备)

- [x] 30.1 创建 v1.0.0 发布分支
- [x] 30.2 更新版本号（所有项目文件）
- [x] 30.3 生成完整的 CHANGELOG.md
- [x] 30.4 创建 GitHub Release（Tag + Release Notes）
- [x] 30.5 发布 Docker 镜像到 Docker Hub
- [x] 30.6 发布用户文档到网站
- [x] 30.7 准备社区公告（Reddit, Twitter, Discord）
- [x] 30.8 建立用户反馈渠道（GitHub Issues, Discord）
- [x] 30.9 规划 v1.1.0 路线图（根据用户反馈）
- [x] 30.10 庆祝项目成功发布 🎉
