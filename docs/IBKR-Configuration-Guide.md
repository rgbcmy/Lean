# IBKR Connection Configuration Guide
# Interactive Brokers 连接配置教程

[English](#english) | [中文](#中文)

---

## English

### Table of Contents

- [Overview](#overview)
- [Prerequisites](#prerequisites)
- [Step 1: Set Up IBKR Account](#step-1-set-up-ibkr-account)
- [Step 2: Install TWS or IB Gateway](#step-2-install-tws-or-ib-gateway)
- [Step 3: Configure API Settings in TWS/Gateway](#step-3-configure-api-settings-in-twsgateway)
- [Step 4: Configure Lean WebUI](#step-4-configure-lean-webui)
- [Step 5: Test Connection](#step-5-test-connection)
- [Troubleshooting](#troubleshooting-english)
- [Advanced Configuration](#advanced-configuration)

---

### Overview

Lean WebUI connects to Interactive Brokers (IBKR) through the TWS API or IB Gateway API to enable live trading. This guide will walk you through the complete configuration process from account setup to first connection.

**What You'll Need:**
- An active Interactive Brokers account
- TWS (Trader Workstation) or IB Gateway installed
- Lean WebUI installed and running

---

### Prerequisites

#### 1. IBKR Account Requirements

- **Account Type**: Individual, Joint, or Entity account
- **Account Status**: Must be approved and funded (for live trading)
- **Market Data Subscriptions**: US Securities Snapshot and Futures Value Bundle (for real-time quotes)
- **API Access**: Must be enabled (automatically enabled for most accounts)

#### 2. Supported Trading Modes

| Mode | Port (TWS) | Port (Gateway) | Purpose |
|------|------------|----------------|---------|
| Paper Trading | 7497 | 4002 | Testing without real money |
| Live Trading | 7496 | 4001 | Real trading with actual funds |

---

### Step 1: Set Up IBKR Account

#### 1.1 Apply for an Account

If you don't have an IBKR account yet:

1. Visit [Interactive Brokers](https://www.interactivebrokers.com)
2. Click **Open Account**
3. Choose account type:
   - **Individual**: Personal trading account
   - **Joint**: Shared account (2+ people)
   - **Entity**: Business/trust/LLC account
4. Complete the application (15-30 minutes)
5. Wait for approval (1-3 business days)

#### 1.2 Fund Your Account

For live trading:
- Minimum deposit: $0 (but $25,000 recommended to avoid PDT rule)
- Transfer methods: Bank wire, ACH, check
- Processing time: 1-3 business days

For paper trading:
- No funding required
- Virtual cash: $1,000,000 (default)

#### 1.3 Subscribe to Market Data (Optional but Recommended)

For real-time quotes:

1. Log in to [Client Portal](https://www.interactivebrokers.com/portal)
2. Navigate to **Settings → User Settings → Market Data Subscriptions**
3. Subscribe to:
   - **US Securities Snapshot and Futures Value Bundle** ($10/month, waived if you trade)
   - **NASDAQ Level I** ($1.50/month, waived if you trade)

Without subscriptions, you'll receive delayed quotes (15-20 minutes).

---

### Step 2: Install TWS or IB Gateway

#### Option A: Trader Workstation (TWS) - Full-Featured

**Pros**: Complete trading interface, charting, research tools
**Cons**: Heavier resource usage (~500MB RAM)

**Installation**:

1. Download from [IBKR TWS Download](https://www.interactivebrokers.com/en/index.php?f=16040)
2. Choose your OS: Windows, macOS, or Linux
3. Run the installer
4. Launch TWS and log in with your IBKR credentials

#### Option B: IB Gateway - Lightweight

**Pros**: Minimal UI, lower resource usage (~200MB RAM)
**Cons**: No charting or research tools

**Installation**:

1. Download from [IB Gateway Download](https://www.interactivebrokers.com/en/index.php?f=16457)
2. Choose your OS
3. Run the installer
4. Launch IB Gateway and log in

**Recommended**: Use IB Gateway for production setups (always-on servers).

---

### Step 3: Configure API Settings in TWS/Gateway

#### For TWS:

1. **Launch TWS** and log in
2. Navigate to **File → Global Configuration**
3. Go to **API → Settings**
4. Configure the following:

   **Enable API Access**:
   - ☑ Enable ActiveX and Socket Clients
   - ☐ Read-Only API (uncheck if you want to trade)
   
   **Socket Port**:
   - Paper Trading: `7497`
   - Live Trading: `7496`
   
   **Trusted IP Addresses**:
   - Click **+** to add `127.0.0.1` (localhost)
   - If running WebUI on another machine, add its IP address
   
   **Master API Client ID**:
   - Leave blank (or set to `0` for auto-accept)
   
   **Create API Message Log File**:
   - ☑ Check this for debugging (logs saved to `~/Jts/`)

5. Click **OK** to save
6. **Restart TWS** for changes to take effect

#### For IB Gateway:

1. **Launch IB Gateway** and log in
2. Click **Configure → Settings**
3. Follow the same steps as TWS above

---

### Step 4: Configure Lean WebUI

#### 4.1 Edit Configuration File

Open `WebUI.API/appsettings.json` and locate the `IBKR` section:

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
    },
    "MarketData": {
      "SubscribeRealtime": true,
      "DelayedData": false
    },
    "Trading": {
      "PaperTrading": true,
      "DefaultOrderType": "Limit",
      "EnableShortSelling": false
    }
  }
}
```

#### 4.2 Configuration Parameters Explained

| Parameter | Description | Default | Notes |
|-----------|-------------|---------|-------|
| `Host` | TWS/Gateway hostname | `localhost` | Use `127.0.0.1` or remote IP |
| `Port` | TWS/Gateway port | `7497` | See port reference above |
| `ClientId` | Unique client identifier | `1` | Use different IDs for multiple connections |
| `AccountId` | Your IBKR account number | (empty) | Find in TWS: Account → Account ID |
| `SubscribeRealtime` | Enable real-time data | `true` | Requires market data subscriptions |
| `DelayedData` | Use delayed quotes | `false` | Set `true` if no subscriptions |
| `PaperTrading` | Enable paper trading mode | `true` | Set `false` for live trading |
| `DefaultOrderType` | Default order type | `Limit` | `Market`, `Limit`, `Stop`, etc. |
| `EnableShortSelling` | Allow short sales | `false` | Set `true` if you have margin account |

#### 4.3 Find Your Account ID

**Method 1: In TWS**
1. Open TWS
2. Go to **Account → Account Window**
3. Your account ID is displayed at the top (e.g., `U1234567`)

**Method 2: In Client Portal**
1. Log in to [Client Portal](https://www.interactivebrokers.com/portal)
2. Your account ID is shown in the top-right corner

#### 4.4 Example Configurations

**Example 1: Paper Trading with TWS (default)**
```json
{
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    },
    "Trading": {
      "PaperTrading": true
    }
  }
}
```

**Example 2: Live Trading with IB Gateway**
```json
{
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 4001,
      "ClientId": 1
    },
    "Account": {
      "AccountId": "U1234567"
    },
    "Trading": {
      "PaperTrading": false
    }
  }
}
```

**Example 3: Remote IB Gateway**
```json
{
  "IBKR": {
    "TWS": {
      "Host": "192.168.1.100",
      "Port": 4001,
      "ClientId": 1
    }
  }
}
```

---

### Step 5: Test Connection

#### 5.1 Start TWS/Gateway

1. Launch TWS or IB Gateway
2. Log in with your credentials
3. Ensure the API settings are correct (Step 3)
4. Keep TWS/Gateway running (do not close)

#### 5.2 Start Lean WebUI

```bash
# Docker Compose
docker-compose up -d

# Local deployment
cd WebUI.API
dotnet run
```

#### 5.3 Check Connection Status

1. Open WebUI in browser: `https://localhost:5001`
2. Log in
3. Navigate to **Account → Connection Status**
4. Verify:
   - **Status**: `Connected` (green)
   - **Last Heartbeat**: Recent timestamp
   - **Account ID**: Your IBKR account number
   - **Connection Type**: `Paper Trading` or `Live Trading`

#### 5.4 Test Market Data

1. Go to **Trading → Stock Search**
2. Enter `AAPL`
3. Verify:
   - Real-time price updates
   - Bid/Ask spread
   - Volume data

If prices don't update, check:
- Market is open (9:30-16:00 ET)
- Market data subscriptions are active
- `SubscribeRealtime` is `true` in config

#### 5.5 Test Order Placement (Paper Trading)

**⚠️ Warning**: Only test in paper trading mode first!

1. Go to **Trading → Place Order**
2. Enter:
   - Symbol: `AAPL`
   - Type: `Limit Order`
   - Quantity: `1`
   - Price: Current market price + $1 (won't fill immediately)
   - Direction: `Buy`
3. Click **Submit Order**
4. Verify:
   - Order appears in **Orders → Today's Orders**
   - Status: `Submitted`
   - No error messages

5. Cancel the test order

---

### Troubleshooting (English)

#### Problem 1: "Connection Refused" Error

**Symptoms**: WebUI shows `Disconnected` status

**Causes & Solutions**:

1. **TWS/Gateway not running**
   - Solution: Start TWS or IB Gateway

2. **Wrong port number**
   - Solution: Double-check port in `appsettings.json` matches TWS settings
   - Paper Trading TWS: 7497
   - Live Trading TWS: 7496
   - Paper Trading Gateway: 4002
   - Live Trading Gateway: 4001

3. **API not enabled in TWS/Gateway**
   - Solution: Enable "ActiveX and Socket Clients" (Step 3)

4. **Firewall blocking connection**
   - Solution (Windows): Add exception for Lean WebUI and TWS
   - Solution (Linux): `sudo ufw allow 7497`

5. **TWS using different port**
   - Solution: In TWS, go to `File → Global Configuration → API → Settings` and verify Socket Port

#### Problem 2: "Not Authorized" Error

**Causes & Solutions**:

1. **IP not in trusted list**
   - Solution: Add `127.0.0.1` to trusted IPs in TWS API settings

2. **Read-Only API enabled**
   - Solution: Uncheck "Read-Only API" if you want to trade

#### Problem 3: No Market Data / Delayed Quotes

**Causes & Solutions**:

1. **No market data subscriptions**
   - Solution: Subscribe to US Securities Snapshot (~$10/month, waived with trading activity)

2. **Market closed**
   - Solution: Wait for market hours (9:30-16:00 ET, Mon-Fri)

3. **DelayedData enabled**
   - Solution: Set `DelayedData: false` in `appsettings.json`

#### Problem 4: Orders Rejected

**Causes & Solutions**:

1. **Insufficient buying power**
   - Solution: Check account balance in TWS

2. **Market closed**
   - Solution: Wait for market hours or use "Good till Canceled" orders

3. **Invalid order parameters**
   - Solution: Check order quantity, price, symbol

4. **Account restrictions**
   - Solution: Verify account permissions in Client Portal

#### Problem 5: Frequent Disconnections

**Causes & Solutions**:

1. **TWS auto-logout**
   - Solution: Disable auto-logout in TWS settings
   - TWS: `File → Global Configuration → Lock and Exit` → Set to "Never"

2. **Network instability**
   - Solution: Check internet connection stability

3. **Multiple API connections**
   - Solution: Use unique ClientId for each connection

---

### Advanced Configuration

#### Multi-Account Setup

If you have multiple IBKR accounts:

```json
{
  "IBKR": {
    "Accounts": [
      {
        "AccountId": "U1234567",
        "ClientId": 1,
        "Description": "Main Trading Account"
      },
      {
        "AccountId": "U7654321",
        "ClientId": 2,
        "Description": "Paper Trading Account"
      }
    ]
  }
}
```

#### Auto-Reconnect Configuration

```json
{
  "IBKR": {
    "Connection": {
      "AutoReconnect": true,
      "ReconnectIntervalSeconds": 30,
      "MaxReconnectAttempts": 10
    }
  }
}
```

#### Order Defaults

```json
{
  "IBKR": {
    "Trading": {
      "DefaultOrderType": "Limit",
      "DefaultTimeInForce": "Day",
      "EnableOutsideRegularHours": false,
      "RequireOrderConfirmation": true
    }
  }
}
```

---

## 中文

### 目录

- [概述](#概述)
- [前置条件](#前置条件)
- [第一步：设置 IBKR 账户](#第一步设置-ibkr-账户)
- [第二步：安装 TWS 或 IB Gateway](#第二步安装-tws-或-ib-gateway)
- [第三步：配置 TWS/Gateway API 设置](#第三步配置-twsgateway-api-设置)
- [第四步：配置 Lean WebUI](#第四步配置-lean-webui)
- [第五步：测试连接](#第五步测试连接)
- [故障排查](#故障排查中文)
- [高级配置](#高级配置中文)

---

### 概述

Lean WebUI 通过 TWS API 或 IB Gateway API 连接到 Interactive Brokers (IBKR) 以实现实盘交易。本指南将引导您完成从账户设置到首次连接的完整配置过程。

**您需要准备：**
- 激活的 Interactive Brokers 账户
- 已安装的 TWS（Trader Workstation）或 IB Gateway
- 已安装并运行的 Lean WebUI

---

### 前置条件

#### 1. IBKR 账户要求

- **账户类型**：个人、联名或实体账户
- **账户状态**：必须已批准并注资（实盘交易）
- **市场数据订阅**：美股快照和期货价值套餐（实时行情需要）
- **API 访问权限**：必须已启用（大多数账户默认启用）

#### 2. 支持的交易模式

| 模式 | 端口 (TWS) | 端口 (Gateway) | 用途 |
|------|------------|----------------|------|
| 纸交易（Paper Trading） | 7497 | 4002 | 无真实资金测试 |
| 实盘交易（Live Trading） | 7496 | 4001 | 真实资金交易 |

---

### 第一步：设置 IBKR 账户

#### 1.1 申请账户

如果您还没有 IBKR 账户：

1. 访问 [Interactive Brokers 中国](https://www.interactivebrokers.com.cn)
2. 点击 **开户**
3. 选择账户类型：
   - **个人账户**：个人交易账户
   - **联名账户**：共享账户（2人以上）
   - **实体账户**：企业/信托/有限责任公司账户
4. 完成申请（15-30分钟）
5. 等待审核（1-3个工作日）

#### 1.2 为账户注资

实盘交易：
- 最低存款：$0（但建议 $25,000 以避免 PDT 规则）
- 转账方式：电汇、ACH、支票
- 处理时间：1-3个工作日

纸交易：
- 无需注资
- 虚拟资金：$1,000,000（默认）

#### 1.3 订阅市场数据（可选但推荐）

获取实时行情：

1. 登录 [客户端门户](https://www.interactivebrokers.com/portal)
2. 导航至 **设置 → 用户设置 → 市场数据订阅**
3. 订阅：
   - **美股快照和期货价值套餐**（$10/月，交易活跃可免费）
   - **纳斯达克 Level I**（$1.50/月，交易活跃可免费）

不订阅的话，您将收到延迟行情（15-20分钟）。

---

### 第二步：安装 TWS 或 IB Gateway

#### 选项 A：Trader Workstation (TWS) - 功能完整

**优点**：完整的交易界面、图表、研究工具
**缺点**：资源占用较高（~500MB 内存）

**安装步骤**：

1. 从 [IBKR TWS 下载](https://www.interactivebrokers.com/cn/index.php?f=16040) 下载
2. 选择您的操作系统：Windows、macOS 或 Linux
3. 运行安装程序
4. 启动 TWS 并使用 IBKR 凭证登录

#### 选项 B：IB Gateway - 轻量级

**优点**：最小化界面，资源占用低（~200MB 内存）
**缺点**：无图表或研究工具

**安装步骤**：

1. 从 [IB Gateway 下载](https://www.interactivebrokers.com/cn/index.php?f=16457) 下载
2. 选择您的操作系统
3. 运行安装程序
4. 启动 IB Gateway 并登录

**推荐**：生产环境使用 IB Gateway（始终在线的服务器）。

---

### 第三步：配置 TWS/Gateway API 设置

#### 在 TWS 中：

1. **启动 TWS** 并登录
2. 导航至 **File → Global Configuration**（文件 → 全局配置）
3. 进入 **API → Settings**（API → 设置）
4. 配置以下内容：

   **启用 API 访问**：
   - ☑ Enable ActiveX and Socket Clients（启用 ActiveX 和 Socket 客户端）
   - ☐ Read-Only API（只读 API）- 如果要交易请取消勾选
   
   **Socket 端口**：
   - 纸交易：`7497`
   - 实盘交易：`7496`
   
   **受信任的 IP 地址**：
   - 点击 **+** 添加 `127.0.0.1`（本地主机）
   - 如果 WebUI 在另一台机器上运行，添加其 IP 地址
   
   **Master API Client ID**（主 API 客户端 ID）：
   - 留空（或设为 `0` 自动接受）
   
   **创建 API 消息日志文件**：
   - ☑ 勾选此项用于调试（日志保存到 `~/Jts/`）

5. 点击 **OK** 保存
6. **重启 TWS** 使更改生效

#### 在 IB Gateway 中：

1. **启动 IB Gateway** 并登录
2. 点击 **Configure → Settings**（配置 → 设置）
3. 按照上述 TWS 的相同步骤操作

---

### 第四步：配置 Lean WebUI

#### 4.1 编辑配置文件

打开 `WebUI.API/appsettings.json` 并找到 `IBKR` 部分：

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
    },
    "MarketData": {
      "SubscribeRealtime": true,
      "DelayedData": false
    },
    "Trading": {
      "PaperTrading": true,
      "DefaultOrderType": "Limit",
      "EnableShortSelling": false
    }
  }
}
```

#### 4.2 配置参数说明

| 参数 | 说明 | 默认值 | 备注 |
|------|------|--------|------|
| `Host` | TWS/Gateway 主机名 | `localhost` | 使用 `127.0.0.1` 或远程 IP |
| `Port` | TWS/Gateway 端口 | `7497` | 参见上方端口参考 |
| `ClientId` | 唯一客户端标识符 | `1` | 多个连接使用不同 ID |
| `AccountId` | 您的 IBKR 账户号 | (空) | 在 TWS 中查找：Account → Account ID |
| `SubscribeRealtime` | 启用实时数据 | `true` | 需要市场数据订阅 |
| `DelayedData` | 使用延迟行情 | `false` | 无订阅时设为 `true` |
| `PaperTrading` | 启用纸交易模式 | `true` | 实盘交易设为 `false` |
| `DefaultOrderType` | 默认订单类型 | `Limit` | `Market`、`Limit`、`Stop` 等 |
| `EnableShortSelling` | 允许做空 | `false` | 有保证金账户时设为 `true` |

#### 4.3 查找您的账户 ID

**方法 1：在 TWS 中**
1. 打开 TWS
2. 进入 **Account → Account Window**（账户 → 账户窗口）
3. 您的账户 ID 显示在顶部（例如：`U1234567`）

**方法 2：在客户端门户中**
1. 登录 [客户端门户](https://www.interactivebrokers.com/portal)
2. 您的账户 ID 显示在右上角

#### 4.4 配置示例

**示例 1：TWS 纸交易（默认）**
```json
{
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 7497,
      "ClientId": 1
    },
    "Trading": {
      "PaperTrading": true
    }
  }
}
```

**示例 2：IB Gateway 实盘交易**
```json
{
  "IBKR": {
    "TWS": {
      "Host": "localhost",
      "Port": 4001,
      "ClientId": 1
    },
    "Account": {
      "AccountId": "U1234567"
    },
    "Trading": {
      "PaperTrading": false
    }
  }
}
```

**示例 3：远程 IB Gateway**
```json
{
  "IBKR": {
    "TWS": {
      "Host": "192.168.1.100",
      "Port": 4001,
      "ClientId": 1
    }
  }
}
```

---

### 第五步：测试连接

#### 5.1 启动 TWS/Gateway

1. 启动 TWS 或 IB Gateway
2. 使用凭证登录
3. 确保 API 设置正确（第三步）
4. 保持 TWS/Gateway 运行（不要关闭）

#### 5.2 启动 Lean WebUI

```bash
# Docker Compose
docker-compose up -d

# 本地部署
cd WebUI.API
dotnet run
```

#### 5.3 检查连接状态

1. 在浏览器中打开 WebUI：`https://localhost:5001`
2. 登录
3. 导航至 **账户 → 连接状态**
4. 验证：
   - **状态**：`已连接`（绿色）
   - **最后心跳**：最近的时间戳
   - **账户 ID**：您的 IBKR 账户号
   - **连接类型**：`纸交易` 或 `实盘交易`

#### 5.4 测试市场数据

1. 进入 **交易 → 股票搜索**
2. 输入 `AAPL`
3. 验证：
   - 实时价格更新
   - 买卖价差
   - 成交量数据

如果价格不更新，检查：
- 市场是否开盘（9:30-16:00 ET）
- 市场数据订阅是否激活
- 配置中 `SubscribeRealtime` 是否为 `true`

#### 5.5 测试下单（纸交易）

**⚠️ 警告**：请先在纸交易模式下测试！

1. 进入 **交易 → 下单**
2. 输入：
   - 股票代码：`AAPL`
   - 类型：`限价单`
   - 数量：`1`
   - 价格：当前市价 + $1（不会立即成交）
   - 方向：`买入`
3. 点击 **提交订单**
4. 验证：
   - 订单出现在 **订单 → 今日订单**
   - 状态：`已提交`
   - 无错误消息

5. 取消测试订单

---

### 故障排查（中文）

#### 问题 1："连接被拒绝"错误

**症状**：WebUI 显示 `未连接` 状态

**原因和解决方法**：

1. **TWS/Gateway 未运行**
   - 解决：启动 TWS 或 IB Gateway

2. **端口号错误**
   - 解决：仔细检查 `appsettings.json` 中的端口是否与 TWS 设置匹配
   - 纸交易 TWS：7497
   - 实盘 TWS：7496
   - 纸交易 Gateway：4002
   - 实盘 Gateway：4001

3. **TWS/Gateway 中未启用 API**
   - 解决：启用"ActiveX and Socket Clients"（第三步）

4. **防火墙阻止连接**
   - 解决（Windows）：为 Lean WebUI 和 TWS 添加例外
   - 解决（Linux）：`sudo ufw allow 7497`

5. **TWS 使用不同端口**
   - 解决：在 TWS 中，进入 `File → Global Configuration → API → Settings` 并验证 Socket 端口

#### 问题 2："未授权"错误

**原因和解决方法**：

1. **IP 不在受信任列表中**
   - 解决：在 TWS API 设置中添加 `127.0.0.1` 到受信任 IP

2. **启用了只读 API**
   - 解决：如果要交易，取消勾选"Read-Only API"

#### 问题 3：无市场数据 / 延迟行情

**原因和解决方法**：

1. **未订阅市场数据**
   - 解决：订阅美股快照（~$10/月，交易活跃可免费）

2. **市场休市**
   - 解决：等待市场开盘时间（9:30-16:00 ET，周一至周五）

3. **启用了延迟数据**
   - 解决：在 `appsettings.json` 中设置 `DelayedData: false`

#### 问题 4：订单被拒绝

**原因和解决方法**：

1. **购买力不足**
   - 解决：在 TWS 中检查账户余额

2. **市场休市**
   - 解决：等待市场开盘或使用"取消前有效"订单

3. **订单参数无效**
   - 解决：检查订单数量、价格、股票代码

4. **账户限制**
   - 解决：在客户端门户中验证账户权限

#### 问题 5：频繁断开连接

**原因和解决方法**：

1. **TWS 自动登出**
   - 解决：在 TWS 设置中禁用自动登出
   - TWS：`File → Global Configuration → Lock and Exit` → 设为"Never"

2. **网络不稳定**
   - 解决：检查互联网连接稳定性

3. **多个 API 连接**
   - 解决：为每个连接使用唯一的 ClientId

---

### 高级配置（中文）

#### 多账户设置

如果您有多个 IBKR 账户：

```json
{
  "IBKR": {
    "Accounts": [
      {
        "AccountId": "U1234567",
        "ClientId": 1,
        "Description": "主交易账户"
      },
      {
        "AccountId": "U7654321",
        "ClientId": 2,
        "Description": "纸交易账户"
      }
    ]
  }
}
```

#### 自动重连配置

```json
{
  "IBKR": {
    "Connection": {
      "AutoReconnect": true,
      "ReconnectIntervalSeconds": 30,
      "MaxReconnectAttempts": 10
    }
  }
}
```

#### 订单默认值

```json
{
  "IBKR": {
    "Trading": {
      "DefaultOrderType": "Limit",
      "DefaultTimeInForce": "Day",
      "EnableOutsideRegularHours": false,
      "RequireOrderConfirmation": true
    }
  }
}
```

---

**配置完成！开始您的量化交易之旅吧！** 🚀
