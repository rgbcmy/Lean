# Lean WebUI 用户指南

## 目录

- [简介](#简介)
- [系统要求](#系统要求)
- [快速开始](#快速开始)
- [安装部署](#安装部署)
  - [Docker Compose 部署（推荐）](#docker-compose-部署推荐)
  - [本地直接部署](#本地直接部署)
- [配置指南](#配置指南)
  - [数据库配置](#数据库配置)
  - [IBKR 连接配置](#ibkr-连接配置)
  - [安全配置](#安全配置)
- [功能使用](#功能使用)
  - [登录系统](#登录系统)
  - [仪表盘](#仪表盘)
  - [账户管理](#账户管理)
  - [实盘交易](#实盘交易)
  - [持仓管理](#持仓管理)
  - [订单管理](#订单管理)
  - [策略管理](#策略管理)
  - [回测系统](#回测系统)
  - [数据可视化](#数据可视化)
  - [风险控制](#风险控制)
- [常见问题](#常见问题)
- [故障排查](#故障排查)

---

## 简介

Lean WebUI 是为 QuantConnect Lean 量化交易引擎打造的现代化 Web 管理界面，专为个人量化交易者设计。通过 WebUI，您可以：

- 🖥️ **可视化操作**：通过浏览器进行所有交易操作，无需编写代码
- 📊 **实时监控**：实时查看账户、持仓、订单状态和行情数据
- 🤖 **策略管理**：可视化管理和监控量化策略的执行
- 📈 **数据可视化**：K线图、技术指标、收益曲线等专业图表
- 🛡️ **风险控制**：内置止损止盈、仓位限制等风控功能
- 🌐 **中文支持**：完整的中文界面和文档

### 主要特性

- ✅ 支持 Interactive Brokers (IBKR) 实盘交易
- ✅ 专注美股市场（股票、ETF）
- ✅ 实时行情推送（亚秒级延迟）
- ✅ 前后端分离架构，响应式设计
- ✅ 支持 PostgreSQL 和 SQLite 数据库
- ✅ 本地部署，数据安全可控
- ✅ HTTPS 加密，JWT 认证
- ✅ 跨平台支持（Windows/Linux/macOS）

---

## 系统要求

### 硬件要求

- **CPU**：2核心及以上（推荐 4核心）
- **内存**：4GB 及以上（推荐 8GB）
- **存储**：10GB 可用空间（SSD 推荐）
- **网络**：稳定的互联网连接（低延迟，用于实时交易）

### 软件要求

#### Docker 部署（推荐）

- Docker 20.10+
- Docker Compose 2.0+
- 操作系统：Windows 10/11、Linux（Ubuntu 20.04+）、macOS 11+

#### 本地部署

- .NET 10 Runtime/SDK
- Node.js 18+ 和 npm 9+
- 数据库：
  - PostgreSQL 14+（生产环境推荐）
  - SQLite 3.35+（开发/测试环境）
- 操作系统：Windows 10/11、Linux、macOS

#### IBKR 要求

- Interactive Brokers 账户
- TWS (Trader Workstation) 或 IB Gateway 已安装并配置
- API 访问权限已启用

---

## 快速开始

如果您想快速体验 Lean WebUI，可以按照以下步骤操作：

### 1. 克隆仓库

```bash
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI
```

### 2. 使用 Docker Compose 启动（最简单）

```bash
docker-compose up -d
```

### 3. 访问 WebUI

打开浏览器访问：`https://localhost:5001`

默认登录凭证：
- 用户名：`admin`
- 密码：`ChangeMe123!`

⚠️ **安全提示**：首次登录后请立即修改默认密码！

---

## 安装部署

### Docker Compose 部署（推荐）

Docker Compose 是最简单的部署方式，适合生产环境使用。

#### 1. 准备配置文件

在 `WebUI` 目录下创建或编辑 `docker-compose.yml`：

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: lean-postgres
    environment:
      POSTGRES_DB: leanui
      POSTGRES_USER: leanuser
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    restart: unless-stopped

  api:
    build:
      context: ./WebUI.API
      dockerfile: Dockerfile
    container_name: lean-api
    depends_on:
      - postgres
    environment:
      Database__Provider: PostgreSQL
      Database__ConnectionString: Host=postgres;Database=leanui;Username=leanuser;Password=${DB_PASSWORD}
      JWT__SecretKey: ${JWT_SECRET}
      IBKR__TWS__Host: ${IBKR_HOST:-host.docker.internal}
      IBKR__TWS__Port: ${IBKR_PORT:-7497}
    ports:
      - "5000:5000"
      - "5001:5001"
    volumes:
      - ./data:/app/data
      - ./logs:/app/logs
    restart: unless-stopped

  frontend:
    build:
      context: ./WebUI.Frontend
      dockerfile: Dockerfile
    container_name: lean-frontend
    depends_on:
      - api
    ports:
      - "3000:80"
    restart: unless-stopped

volumes:
  postgres_data:
```

#### 2. 创建环境变量文件

创建 `.env` 文件：

```env
# 数据库密码
DB_PASSWORD=your_secure_password_here

# JWT 密钥（至少 32 字符）
JWT_SECRET=your_jwt_secret_key_at_least_32_characters_long

# IBKR TWS/Gateway 配置
IBKR_HOST=host.docker.internal
IBKR_PORT=7497
```

#### 3. 启动服务

```bash
# 构建并启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f

# 检查服务状态
docker-compose ps
```

#### 4. 初始化数据库

首次启动时，数据库会自动迁移并创建表结构。您也可以手动执行：

```bash
docker-compose exec api dotnet ef database update
```

#### 5. 访问应用

- **WebUI 前端**：http://localhost:3000
- **API 接口**：http://localhost:5000
- **API 文档（Swagger）**：http://localhost:5000/swagger

---

### 本地直接部署

如果您希望不使用 Docker，可以直接在本地运行。

#### 1. 安装依赖

确保已安装：
- .NET 10 SDK
- Node.js 18+
- PostgreSQL 或 SQLite

#### 2. 配置数据库

##### 使用 PostgreSQL（推荐）

```bash
# 安装 PostgreSQL
# Windows: 从 https://www.postgresql.org/download/windows/ 下载
# Linux: sudo apt install postgresql
# macOS: brew install postgresql

# 创建数据库
psql -U postgres
CREATE DATABASE leanui;
CREATE USER leanuser WITH PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;
\q
```

##### 使用 SQLite（开发环境）

无需额外配置，程序会自动创建 `lean.db` 文件。

#### 3. 配置后端

编辑 `WebUI.API/appsettings.json`：

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=leanui;Username=leanuser;Password=your_password"
  },
  "JWT": {
    "SecretKey": "your_jwt_secret_key_at_least_32_characters_long",
    "Issuer": "LeanWebUI",
    "Audience": "LeanWebUIClient",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    }
  },
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://localhost:5000"
      },
      "Https": {
        "Url": "https://localhost:5001"
      }
    }
  }
}
```

#### 4. 启动后端

```bash
cd WebUI.API
dotnet restore
dotnet ef database update
dotnet run
```

#### 5. 配置前端

编辑 `WebUI.Frontend/.env`：

```env
REACT_APP_API_URL=https://localhost:5001/api
REACT_APP_SIGNALR_URL=https://localhost:5001/hubs
```

#### 6. 启动前端

```bash
cd WebUI.Frontend
npm install
npm start
```

浏览器会自动打开 `http://localhost:3000`

---

## 配置指南

### 数据库配置

#### PostgreSQL 配置

适用于生产环境，提供更好的性能和并发支持。

```json
{
  "Database": {
    "Provider": "PostgreSQL",
    "ConnectionString": "Host=localhost;Database=leanui;Username=leanuser;Password=your_password;Port=5432"
  }
}
```

**连接字符串参数说明**：
- `Host`：数据库服务器地址
- `Database`：数据库名称
- `Username`：用户名
- `Password`：密码
- `Port`：端口（默认 5432）

#### SQLite 配置

适用于开发/测试环境，无需额外数据库服务。

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "Data Source=lean.db"
  }
}
```

#### 切换数据库

要切换数据库，只需修改 `Provider` 和 `ConnectionString`，然后重新运行迁移：

```bash
dotnet ef database update
```

---

### IBKR 连接配置

详细的 IBKR 配置教程请参考：[IBKR 连接配置教程](./IBKR-Configuration-Guide.md)

#### 基本配置

```json
{
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    },
    "Account": {
      "AccountId": "your_ibkr_account_id"
    }
  }
}
```

#### 端口说明

- **TWS 纸交易**：7497
- **TWS 实盘**：7496
- **IB Gateway 纸交易**：4002
- **IB Gateway 实盘**：4001

#### 前置条件

1. **启动 TWS 或 IB Gateway**
2. **启用 API 访问**：
   - TWS: `File → Global Configuration → API → Settings`
   - 勾选 `Enable ActiveX and Socket Clients`
   - 取消勾选 `Read-Only API`（如果需要交易）
   - 添加 `127.0.0.1` 到受信任 IP 列表
3. **配置端口**：确保端口号与配置文件一致

---

### 安全配置

#### 生成 JWT 密钥

```bash
# Linux/macOS
openssl rand -base64 32

# PowerShell (Windows)
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

将生成的密钥填入 `appsettings.json` 的 `JWT.SecretKey`。

#### HTTPS 证书配置

##### 开发环境（自签名证书）

```bash
dotnet dev-certs https --trust
```

##### 生产环境（Let's Encrypt）

参考 Docker 部署教程中的 Caddy 配置。

#### 修改默认密码

首次登录后，进入 `设置 → 账户安全` 修改密码。

或通过命令行工具修改：

```bash
dotnet WebUI.API.dll --reset-password
```

---

## 功能使用

### 登录系统

1. 访问 WebUI 地址（默认 `https://localhost:5001`）
2. 输入用户名和密码
3. 首次登录后请立即修改默认密码
4. 登录后会获得 15 分钟有效期的 Access Token

**多设备登录**：支持同一账户在多个设备登录（共享 Refresh Token）。

---

### 仪表盘

登录后首页为仪表盘，显示：

- **账户总览**：总资产、可用现金、持仓市值、今日盈亏
- **持仓概览**：持仓列表（前5个）、盈亏排行
- **今日订单**：最新订单状态
- **策略状态**：运行中的策略及其性能
- **市场热度**：热门股票涨跌榜
- **快捷操作**：快速下单、启动策略

---

### 账户管理

#### 查看账户信息

`账户 → 账户总览`

- 账户余额（现金、股票市值、总资产）
- 保证金使用情况
- 购买力
- 账户历史净值曲线

#### 连接状态监控

`账户 → 连接状态`

- IBKR 连接状态（已连接/断开）
- 最后心跳时间
- 数据延迟情况
- 重连按钮

---

### 实盘交易

#### 搜索股票

`交易 → 股票搜索`

1. 输入股票代码（如 `AAPL`）或公司名称
2. 查看实时行情、基本信息、技术指标
3. 点击 `买入` 或 `卖出`

#### 下单

`交易 → 下单`

1. **选择股票**：输入代码或从持仓选择
2. **选择订单类型**：
   - **市价单（Market）**：立即以市场价成交
   - **限价单（Limit）**：指定价格成交
   - **止损单（Stop）**：触发价格后转为市价单
   - **止损限价单（Stop Limit）**：触发后转为限价单
3. **输入数量**：整数股数
4. **设置有效期**：
   - 当日有效（Day）
   - 取消前有效（GTC）
5. **确认下单**：检查订单详情，点击确认

**示例**：

```
股票代码：AAPL
订单类型：限价单
价格：$150.00
数量：10 股
方向：买入
有效期：当日有效
```

#### ETF 交易

`交易 → ETF`

- 支持主流 ETF（SPY、QQQ、IWM 等）
- 提供定投计划功能（每日/每周定额买入）

---

### 持仓管理

`持仓 → 持仓列表`

#### 查看持仓

表格显示：
- 股票代码
- 持仓数量
- 平均成本
- 当前价格
- 未实现盈亏（金额、百分比）
- 市值

#### 持仓操作

- **一键平仓**：快速卖出全部持仓
- **部分平仓**：卖出指定数量
- **设置止损止盈**：自动触发卖出

#### 持仓分析

`持仓 → 投资组合分析`

- **持仓分布饼图**：按市值占比
- **行业分布**：按行业分类
- **收益曲线**：账户净值历史
- **风险指标**：夏普比率、最大回撤、波动率

---

### 订单管理

`订单 → 订单历史`

#### 查看订单

- **今日订单**：当天所有订单
- **历史订单**：所有订单历史（可筛选日期范围）
- **状态筛选**：待提交、已提交、部分成交、已成交、已取消

#### 订单详情

点击订单查看：
- 订单基本信息（股票、类型、价格、数量）
- 成交明细（多笔成交的明细）
- 订单状态流转历史
- 佣金和费用

#### 取消订单

对于未成交或部分成交的订单，可点击 `取消订单` 按钮。

---

### 策略管理

`策略 → 策略列表`

#### 创建新策略

1. 点击 `新建策略`
2. 选择策略模板或从空白开始
3. 配置策略参数：
   - **策略名称**
   - **策略代码**（上传 C# 或 Python 文件）
   - **初始资金**
   - **交易品种**
   - **时间范围**
4. 保存策略

#### 启动策略

1. 进入策略详情页
2. 检查配置参数
3. 点击 `启动策略`
4. 确认风险提示

#### 监控策略

策略运行时，可查看：
- **实时日志**：策略输出的日志信息（自动滚动）
- **当前持仓**：策略持有的股票
- **订单记录**：策略下的所有订单
- **性能指标**：收益率、夏普比率、胜率

#### 停止策略

点击 `停止策略` 按钮，策略会优雅关闭（完成当前交易后停止）。

#### 策略版本管理

`策略 → 版本历史`

- 查看策略的所有历史版本
- 回滚到旧版本
- 对比不同版本的差异

---

### 回测系统

`回测 → 新建回测`

#### 配置回测

1. **选择策略**：从已有策略中选择
2. **设置回测参数**：
   - **起止日期**：历史数据范围
   - **初始资金**：$100,000
   - **基准指数**：SPY（用于对比）
3. **点击运行回测**

#### 查看回测结果

`回测 → 回测结果`

- **收益曲线图**：账户净值变化
- **回撤图**：最大回撤和回撤周期
- **性能指标**：
  - 总收益率
  - 年化收益率
  - 夏普比率
  - 最大回撤
  - 胜率
  - 盈亏比
- **交易明细**：所有回测交易记录

#### 参数优化

`回测 → 参数优化`

- 选择要优化的参数（如移动平均周期）
- 设置参数范围（如 10-50，步长5）
- 运行网格搜索
- 查看最优参数组合

#### 回测对比

`回测 → 对比分析`

- 选择多个回测结果
- 并排对比性能指标
- 生成对比报告（PDF）

---

### 数据可视化

#### K线图

`图表 → K线图`

- 选择股票代码
- 切换时间周期（日线、周线、月线）
- 叠加技术指标（MA、MACD、RSI、布林带）
- 缩放和平移（鼠标滚轮、拖拽）
- 导出图表（PNG/SVG）

#### 实时行情图

`图表 → 实时行情`

- 轻量级实时价格图表
- 1秒更新频率
- 支持多股票对比

#### 持仓分布图

`持仓 → 分布图`

- 饼图显示各股票占比
- 柱状图显示盈亏排行

#### 收益曲线

`账户 → 收益曲线`

- 账户净值历史曲线
- 对比基准指数（SPY）
- 显示回撤区域

---

### 风险控制

`设置 → 风险控制`

#### 止损止盈规则

- **全局止损**：账户总资产跌破阈值时平仓（如 -5%）
- **个股止损**：单个股票跌破买入价一定比例时卖出（如 -3%）
- **止盈**：单个股票盈利达到目标时卖出（如 +10%）

#### 仓位限制

- **单个股票最大仓位**：不超过总资产的 20%
- **总持仓数限制**：最多持有 10 只股票
- **现金比例**：保持至少 10% 现金

#### 交易频率限制

- **日内交易次数**：每日最多 20 笔
- **Pattern Day Trader (PDT) 检查**：账户低于 $25,000 时限制日内交易

#### 熔断机制

- **价格异常波动**：单笔订单价格偏离市价超过 5% 时拒绝
- **订单量异常**：单笔订单超过日均成交量 1% 时预警

---

## 常见问题

### Q1: 无法连接到 IBKR TWS

**原因**：
- TWS 未运行
- API 设置未启用
- 端口配置错误
- 防火墙阻止

**解决方法**：
1. 确认 TWS 或 IB Gateway 已启动
2. TWS: `File → Global Configuration → API → Settings`，启用 API
3. 检查端口号（纸交易 7497，实盘 7496）
4. 添加 `127.0.0.1` 到受信任 IP
5. 临时关闭防火墙测试

---

### Q2: 登录后立即退出

**原因**：JWT Token 配置错误或密钥不一致

**解决方法**：
1. 检查 `appsettings.json` 中的 `JWT.SecretKey` 长度至少 32 字符
2. 清除浏览器缓存和 Cookies
3. 重启后端服务

---

### Q3: 实时行情不更新

**原因**：
- SignalR 连接断开
- IBKR 数据订阅未成功
- 市场休市

**解决方法**：
1. F12 打开浏览器开发者工具，查看 Console 是否有 WebSocket 错误
2. 检查 IBKR 连接状态（账户 → 连接状态）
3. 确认市场交易时间（美股：9:30-16:00 ET）
4. 重新订阅行情

---

### Q4: 订单提交失败

**原因**：
- 账户余额不足
- 超过仓位限制
- 风控规则触发
- 市场休市

**解决方法**：
1. 查看订单错误信息
2. 检查账户购买力（账户 → 账户总览）
3. 检查风控设置（设置 → 风险控制）
4. 确认市场交易时间

---

### Q5: 策略启动失败

**原因**：
- 策略代码语法错误
- Lean 引擎未正确安装
- 配置文件错误

**解决方法**：
1. 查看策略日志（策略详情页 → 日志）
2. 检查策略代码是否能编译
3. 确认 Lean 引擎路径配置正确
4. 查看后端日志：`docker-compose logs api`

---

### Q6: 数据库连接失败

**原因**：
- PostgreSQL 服务未启动
- 连接字符串错误
- 权限不足

**解决方法**：
1. 检查 PostgreSQL 服务：`sudo systemctl status postgresql`
2. 验证连接字符串参数（Host、Port、用户名、密码）
3. 测试数据库连接：`psql -h localhost -U leanuser -d leanui`
4. 检查用户权限：`GRANT ALL PRIVILEGES ON DATABASE leanui TO leanuser;`

---

## 故障排查

### 查看日志

#### Docker 部署

```bash
# 查看所有服务日志
docker-compose logs

# 查看特定服务日志
docker-compose logs api
docker-compose logs frontend

# 实时跟踪日志
docker-compose logs -f api
```

#### 本地部署

- **后端日志**：`WebUI.API/logs/` 目录
- **前端日志**：浏览器 Console（F12）
- **Lean 引擎日志**：`data/logs/` 目录

### 重启服务

#### Docker 部署

```bash
# 重启所有服务
docker-compose restart

# 重启特定服务
docker-compose restart api
```

#### 本地部署

```bash
# 停止后端（Ctrl+C），然后重新运行
cd WebUI.API
dotnet run

# 停止前端（Ctrl+C），然后重新运行
cd WebUI.Frontend
npm start
```

### 清理数据

```bash
# Docker 部署：删除数据库卷（警告：会删除所有数据！）
docker-compose down -v

# 本地部署：删除数据库文件
rm lean.db
```

### 健康检查

访问以下 URL 检查服务状态：
- API 健康检查：`http://localhost:5000/health`
- 数据库连接：`http://localhost:5000/health/db`
- IBKR 连接：`http://localhost:5000/health/ibkr`

---

## 联系支持

如果您遇到无法解决的问题，可以：

1. **查看文档**：[WebUI 开发指南](./WebUI开发指南.md)、[API 参考](./WebUI-API-Reference.md)
2. **提交 Issue**：[GitHub Issues](https://github.com/QuantConnect/Lean/issues)
3. **社区讨论**：[QuantConnect Forum](https://www.quantconnect.com/forum)

---

## 许可证

Lean WebUI 遵循 Apache 2.0 开源协议。详见 [LICENSE](../LICENSE)。

---

**祝您交易顺利！** 🚀
