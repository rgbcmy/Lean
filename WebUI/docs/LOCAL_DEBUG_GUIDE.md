# 本地调试启动指南

本文档说明如何在本地启动并验证 WebUI 的完整功能。

---

## 环境要求

| 工具 | 最低版本 | 说明 |
|------|---------|------|
| .NET SDK | 8.0+ | 后端 API |
| Node.js | 18+ | 前端开发服务器 |
| pnpm | 8+ | 前端包管理器 |

验证已安装：

```bash
dotnet --version
node --version
pnpm --version
```

---

## 一、启动后端 API

### 1. 进入 API 项目目录

```bash
cd f:\source\Lean\WebUI\WebUI.API
```

### 2. 运行

```bash
dotnet run
```

或指定 http profile（跳过 HTTPS 重定向，调试更方便）：

```bash
dotnet run --launch-profile http
```

### 3. 确认启动成功

控制台输出类似以下内容表示启动正常：

```
[DatabaseConfiguration] Starting to seed default data...
[DatabaseConfiguration] Found 0 existing users.
[DatabaseConfiguration] Creating default admin user...
[DatabaseConfiguration] Admin user created successfully!
info: Database initialized successfully
info: Now listening on: http://localhost:5041
```

> **首次启动**会自动在 `WebUI.API/` 目录下生成 `webui.db`（SQLite 数据库），并创建默认管理员账户。

**关键端口：**

| 服务 | 地址 |
|------|------|
| HTTP API | http://localhost:5041 |
| HTTPS API | https://localhost:7169 |
| 健康检查 | http://localhost:5041/health |
| API 文档 (Scalar) | http://localhost:5041/scalar/v1 |

---

## 二、启动前端

### 1. 进入前端目录并安装依赖（首次需要）

```bash
cd f:\source\Lean\WebUI\WebUI.Frontend
pnpm install
```

### 2. 启动开发服务器

```bash
pnpm dev
```

控制台输出示例：

```
  VITE v5.x.x  ready in xxx ms

  ➜  Local:   http://localhost:5173/
  ➜  Network: http://0.0.0.0:5173/
```

前端运行在 **http://localhost:5173**，已配置代理将 `/api` 和 `/hubs` 请求转发到后端 `http://localhost:5041`。

---

## 三、登录系统

打开浏览器访问 **http://localhost:5173**，使用默认管理员账户登录：

| 字段 | 值 |
|------|----|
| 用户名 | `admin` |
| 密码 | `admin123` |

> 登录后建议立即到 **设置 → 修改密码** 更改默认密码。

---

## 四、功能验证清单

按以下顺序逐项验证，确保各模块正常工作：

### 4.1 基础功能

- [ ] **登录/登出**：使用 `admin / admin123` 登录，确认跳转到 Dashboard
- [ ] **Dashboard**：页面正常渲染，无红色报错

### 4.2 交易功能

- [ ] **股票交易** (`/stock-trading`)：页面加载，输入股票代码可搜索行情
- [ ] **ETF 交易** (`/etf-trading`)：页面加载正常
- [ ] **订单管理** (`/orders`)：订单列表加载，可查看历史订单
- [ ] **持仓管理** (`/positions`)：持仓列表加载正常

### 4.3 策略管理

- [ ] **策略列表** (`/strategies`)：列表加载，显示空列表或已有策略
- [ ] **创建策略** (`/strategies/create`)：填写表单并提交，策略出现在列表
- [ ] **策略详情**：点击策略，查看详情页，含启动/停止按钮
- [ ] **启动策略**：点击启动，状态变为运行中（需要 Lean 引擎支持，见注意事项）
- [ ] **策略日志**：策略运行时日志实时显示（通过 SignalR）

### 4.4 回测功能

- [ ] **回测列表** (`/backtests`)：列表加载正常
- [ ] **创建回测** (`/backtests/config`)：配置参数（策略、时间范围）并提交
- [ ] **回测结果** (`/backtests/:id`)：结果页显示绩效指标和收益曲线
- [ ] **参数优化** (`/backtests/optimize`)：配置参数网格并提交优化任务
- [ ] **对比回测** (`/backtests/compare`)：选择多个回测结果进行对比

### 4.5 持仓与风控

- [ ] **投资组合分析** (`/portfolio`)：图表正常渲染
- [ ] **风险仪表板** (`/risk/dashboard`)：风险指标显示
- [ ] **风险配置** (`/risk/config`)：配置止损、仓位限制并保存

### 4.6 其他功能

- [ ] **定投计划** (`/recurring`)：创建定投计划
- [ ] **通知中心** (`/notifications`)：通知列表页加载
- [ ] **修改密码** (`/change-password`)：成功修改密码后需重新登录
- [ ] **帮助页面** (`/help`)：内容正常显示

---

## 五、SignalR 实时推送验证

以下功能依赖 WebSocket（SignalR），需要在策略运行时验证：

1. 打开策略详情页
2. 点击"启动策略"
3. **日志面板**应实时滚动显示输出（无刷新）
4. **订单/持仓**数量应在交易成交时自动更新（无刷新）

如果 SignalR 连接失败，浏览器控制台（F12）会显示 WebSocket 错误，检查后端是否正在运行。

---

## 六、API 文档（Scalar）

后端启动后，可直接在浏览器中测试 API：

**http://localhost:5041/scalar/v1**

常用测试接口：

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/v1/auth/login` | 登录获取 Token |
| GET | `/api/v1/auth/me` | 获取当前用户信息 |
| GET | `/api/v1/strategies` | 获取策略列表 |
| GET | `/api/v1/backtests` | 获取回测列表 |
| GET | `/health` | 健康检查 |
| GET | `/health/detailed` | 详细健康状态 |

在 Scalar 页面登录流程：
1. 调用 `POST /api/v1/auth/login`，Body 输入 `{"username":"admin","password":"admin123"}`
2. 复制响应中的 `accessToken`
3. 点击页面右上角 "Authenticate"，粘贴 Token
4. 即可调用需要认证的接口

---

## 七、注意事项

### 策略启动需要 Lean 引擎

策略的实际执行依赖 Lean 引擎进程，纯前后端启动时策略可以创建和配置，但点击"启动"后 Lean 进程不可用会报错。如需完整验证策略执行：

```bash
# 先构建 Lean
cd f:\source\Lean
dotnet build /p:Configuration=Debug

# 运行时策略执行引擎由 WebUI 通过 Process.Start 自动调用 Lean Launcher
```

### IBKR 连接为可选项

`appsettings.json` 中已配置 TWS 连接参数，默认指向 `127.0.0.1:4002`（模拟盘端口）。未连接 TWS 时，实时行情和下单功能不可用，但其他功能正常。

### HTTPS 重定向

开发环境默认会将 HTTP 重定向到 HTTPS。如需关闭：

在 `appsettings.json` 中添加：
```json
"Security": {
  "EnableHTTPSRedirect": false
}
```

### 数据库重置

如需从空数据库重新开始：

```bash
# 停止后端后执行
cd f:\source\Lean\WebUI\WebUI.API
del webui.db
# 重新启动后端会自动重建数据库和 admin 用户
```

---

## 八、快速参考

```
后端:   http://localhost:5041
前端:   http://localhost:5173
API文档: http://localhost:5041/scalar/v1
账号: admin / admin123
数据库: WebUI.API/webui.db (SQLite, 自动创建)
日志: WebUI.API/logs/webui-YYYYMMDD.log
```
