# Lean WebUI - Demo Video Tutorial Script

This document provides a comprehensive script for creating a video tutorial demonstrating the Lean WebUI quick start guide.

> **Purpose**: Guide users through installation, configuration, and basic usage of Lean WebUI in a 10-15 minute video tutorial.

---

## Video Information

**Title**: Lean WebUI - Quick Start Guide  
**Duration**: 12-15 minutes  
**Target Audience**: Quantitative traders, developers interested in algorithmic trading  
**Language**: Bilingual (English primary, Chinese subtitles)  
**Resolution**: 1920x1080 (1080p)  
**Format**: MP4 (H.264)

---

## Pre-Production Checklist

### Environment Setup

- [ ] Clean Windows/Linux desktop with minimal distractions
- [ ] Docker Desktop installed and running
- [ ] VS Code or Visual Studio installed
- [ ] IBKR TWS Paper Trading account configured
- [ ] Chrome browser with dev tools available
- [ ] Screen recording software (OBS Studio / Camtasia)
- [ ] Microphone tested (clear audio, no background noise)
- [ ] Sample strategy code prepared
- [ ] Test database with sample data

### Recording Settings

- **Screen Resolution**: 1920x1080
- **Frame Rate**: 30 FPS
- **Bitrate**: 5000 kbps
- **Audio**: 192 kbps, 48 kHz
- **Cursor Highlight**: Enabled
- **Keystroke Display**: Enabled (for shortcuts)

---

## Video Structure

### Intro (0:00 - 1:00)

**Visual**: Animated title screen with Lean + IBKR logos

**Narration (English)**:
> "Welcome to the Lean WebUI Quick Start Guide. In this tutorial, we'll walk you through installing and running Lean WebUI, a modern web interface for quantitative trading with the QuantConnect Lean engine and Interactive Brokers. By the end of this video, you'll be able to deploy the system, connect to IBKR paper trading, and run your first trading strategy."

**Narration (Chinese - displayed as subtitles)**:
> "欢迎观看 Lean WebUI 快速启动教程。在本视频中,我们将指导您安装和运行 Lean WebUI——一个用于量化交易的现代化Web界面,支持QuantConnect Lean引擎和Interactive Brokers。视频结束时,您将能够部署系统、连接IBKR模拟交易账户,并运行您的第一个交易策略。"

**On-Screen Text**:
- Lean WebUI v1.0
- QuantConnect Lean Engine + IBKR Integration
- Quick Start Guide

**Timestamps**: 0:00 - 1:00

---

### Section 1: Prerequisites (1:00 - 2:30)

**Visual**: Split screen showing system requirements checklist

**Narration**:
> "Before we start, let's make sure you have everything ready. You'll need: a computer with Windows 10 or later, Linux, or macOS; Docker Desktop installed; an IBKR paper trading account; and at least 8GB of RAM. Don't worry if you don't have everything yet—we'll show you where to get each component."

**Screen Actions**:
1. Show Docker Desktop download page (https://www.docker.com/products/docker-desktop)
2. Show IBKR account registration page
3. Display system requirements checklist with checkmarks

**On-Screen Text**:
```
✓ System Requirements:
  - OS: Windows 10+, Linux, or macOS
  - RAM: 8GB minimum (16GB recommended)
  - Storage: 20GB free space
  - Docker Desktop 20.10+
  - IBKR Paper Trading Account
```

**Timestamps**: 1:00 - 2:30

---

### Section 2: Installation - Docker Compose Method (2:30 - 5:00)

**Visual**: Terminal/Command Prompt window

**Narration**:
> "The easiest way to get started is using Docker Compose. First, let's clone the Lean repository from GitHub. Open your terminal and run..."

**Screen Actions**:

**Step 1: Clone Repository (2:30 - 3:00)**
```bash
git clone https://github.com/QuantConnect/Lean.git
cd Lean/WebUI
```

**On-Screen Text**: 
- Command explanation: "Cloning Lean repository with WebUI components"
- Highlight: Repository includes frontend, backend, and configuration files

**Step 2: Review docker-compose.yml (3:00 - 3:30)**

**Narration**:
> "Let's take a quick look at the Docker Compose configuration. It defines five services: the React frontend, the ASP.NET Core API, PostgreSQL database, Redis cache, and Nginx reverse proxy."

**Screen Actions**:
- Open `docker-compose.yml` in VS Code
- Highlight each service section (5 seconds each):
  - `webui-frontend` (React)
  - `webui-api` (ASP.NET Core)
  - `postgres` (Database)
  - `redis` (Cache)
  - `nginx` (Reverse Proxy)

**Step 3: Configure Environment (3:30 - 4:15)**

**Narration**:
> "Before starting, we need to configure a few environment variables. Copy the sample environment file and edit it with your IBKR account details."

**Screen Actions**:
```bash
cp .env.example .env
```

**Edit `.env` file** (show in VS Code):
```env
# IBKR Configuration
IBKR_ACCOUNT_ID=DU1234567
IBKR_TWS_PORT=7497
IBKR_HOST=host.docker.internal

# Database
DATABASE_PROVIDER=PostgreSQL
DATABASE_CONNECTION=Host=postgres;Port=5432;Database=leanui;Username=leanuser;Password=secure_password_123

# JWT Secret (generate secure key)
JWT_SECRET=your_secure_32_character_secret_key_here_change_this
```

**On-Screen Text**:
- "Replace DU1234567 with your IBKR paper trading account ID"
- "Generate a secure JWT secret for production"

**Step 4: Start Services (4:15 - 5:00)**

**Narration**:
> "Now we're ready to start the services. Run docker-compose up, and Docker will download the required images and start all containers. This may take a few minutes on first run."

**Screen Actions**:
```bash
docker-compose up -d
```

**Show output** (sped up 2x):
- Pulling images
- Creating network
- Creating volumes
- Starting containers

**Verify services are running**:
```bash
docker-compose ps
```

**Expected output** (highlight "Up" status for all services):
```
NAME                STATUS              PORTS
webui-frontend      Up 30 seconds       0.0.0.0:3000->3000/tcp
webui-api           Up 30 seconds       0.0.0.0:5000->5000/tcp
postgres            Up 30 seconds       5432/tcp
redis               Up 30 seconds       6379/tcp
nginx               Up 30 seconds       0.0.0.0:80->80/tcp
```

**Timestamps**: 2:30 - 5:00

---

### Section 3: First Login (5:00 - 6:30)

**Visual**: Chrome browser

**Narration**:
> "Great! The system is now running. Let's access the web interface. Open your browser and navigate to localhost. You should see the Lean WebUI login page."

**Screen Actions**:

**Step 1: Open Browser (5:00 - 5:15)**
- Type `http://localhost` in Chrome address bar
- Page loads showing Login screen

**Step 2: Login with Default Credentials (5:15 - 5:45)**

**Narration**:
> "For first-time login, use the default admin credentials. Username is 'admin' and password is 'ChangeMe123!' Make sure to change this password immediately after logging in."

**Screen Actions**:
- Type `admin` in Username field
- Type `ChangeMe123!` in Password field
- Click "Login" button
- Show loading spinner briefly
- Redirect to Dashboard

**On-Screen Text**:
```
Default Credentials (CHANGE IMMEDIATELY):
Username: admin
Password: ChangeMe123!
```

**Step 3: Change Password (5:45 - 6:30)**

**Narration**:
> "As a security best practice, let's change the default password right away. Click on your username in the top-right corner, select 'Change Password', and enter a strong password."

**Screen Actions**:
- Click user menu (top-right avatar)
- Select "Change Password"
- Fill form:
  - Old Password: `ChangeMe123!`
  - New Password: `SecureP@ssw0rd2024`
  - Confirm Password: `SecureP@ssw0rd2024`
- Click "Save"
- Show success message: "Password changed successfully"

**Timestamps**: 5:00 - 6:30

---

### Section 4: Dashboard Tour (6:30 - 7:30)

**Visual**: Dashboard page

**Narration**:
> "Welcome to the Lean WebUI dashboard. Let's take a quick tour. At the top, you see your account summary showing buying power and portfolio value. The sidebar gives you access to all major features: Trading, Strategies, Positions, Orders, Backtesting, and Settings."

**Screen Actions** (highlight each section for 5-10 seconds):

1. **Account Summary Card** (top)
   - Account ID
   - Net Liquidation: $100,000.00
   - Buying Power: $400,000.00
   - Available Funds: $100,000.00
   - Unrealized P&L: $0.00

2. **Sidebar Navigation**
   - Dashboard (home icon)
   - Trading (chart icon)
   - Strategies (code icon)
   - Positions (briefcase icon)
   - Orders (list icon)
   - Backtest (history icon)
   - Settings (gear icon)

3. **Main Area**
   - Portfolio allocation chart (empty for now)
   - Recent orders table (empty)
   - Strategy status cards (no active strategies)

**On-Screen Text**:
- Pointer annotations highlighting each feature
- "Click any menu item to navigate"

**Timestamps**: 6:30 - 7:30

---

### Section 5: IBKR Connection Setup (7:30 - 9:00)

**Visual**: Split screen - IBKR TWS on left, WebUI Settings on right

**Narration**:
> "Before we can trade, we need to connect to Interactive Brokers. First, make sure IBKR Trader Workstation is running and connected in Paper Trading mode. Then, in the WebUI, navigate to Settings and enter your connection details."

**Screen Actions**:

**Step 1: Start IBKR TWS (7:30 - 8:00)**
- Open IBKR Trader Workstation
- Login with paper trading credentials
- **Important**: Show enabling API connection:
  - Go to File → Global Configuration → API → Settings
  - Check "Enable ActiveX and Socket Clients"
  - Set Socket Port: `7497` (paper trading)
  - Uncheck "Read-Only API"
  - Click "OK"
- Show TWS status: "Connected" (paper trading account)

**Step 2: Configure WebUI Connection (8:00 - 8:45)**

**Narration**:
> "Now, in the WebUI settings page, enter your IBKR connection details. Use localhost as the host, port 7497 for paper trading, and your account ID."

**Screen Actions**:
- Click "Settings" in sidebar
- Select "IBKR Configuration" tab
- Fill form:
  ```
  Host: localhost
  Port: 7497
  Client ID: 1
  Account ID: DU1234567
  ```
- Click "Test Connection" button
- Show success message with green checkmark: "✓ Connected to IBKR successfully"
- Click "Save Settings"

**Step 3: Verify Connection (8:45 - 9:00)**
- Return to Dashboard
- Show "IBKR Status" indicator changed to green "Connected"
- Account balance synced from IBKR

**Timestamps**: 7:30 - 9:00

---

### Section 6: Place First Order (9:00 - 10:30)

**Visual**: Trading page

**Narration**:
> "Let's place our first order. Navigate to the Trading page. Here you'll see a clean interface with real-time quotes, an order form, and an orderbook."

**Screen Actions**:

**Step 1: Navigate to Trading (9:00 - 9:10)**
- Click "Trading" in sidebar
- Page loads showing:
  - Symbol search bar (top)
  - Quote display area
  - Order form (center)
  - Order book (right)
  - Open orders table (bottom)

**Step 2: Search for Symbol (9:10 - 9:30)**

**Narration**:
> "Let's search for Apple stock. Type 'AAPL' in the search bar."

**Screen Actions**:
- Click symbol search box
- Type `AAPL`
- Dropdown shows suggestion: "AAPL - Apple Inc."
- Click on AAPL
- Quote loads showing:
  ```
  AAPL - Apple Inc.
  Last: $175.50
  Bid: $175.45 x 100
  Ask: $175.55 x 200
  Volume: 52,350,000
  Change: +$2.25 (+1.30%)
  ```

**Step 3: Fill Order Form (9:30 - 10:00)**

**Narration**:
> "Now let's create a limit buy order for 10 shares. Select 'Limit' as the order type, 'Buy' as the direction, enter 10 for quantity, and set a limit price slightly below the current market price."

**Screen Actions**:
- Order Type: Select "Limit" from dropdown
- Direction: Select "Buy" (button turns green)
- Quantity: Type `10`
- Limit Price: Type `175.00`
- Time In Force: Default "Day" (leave as is)
- Show order summary:
  ```
  Estimated Cost: $1,750.00
  Commission: $1.00
  Total: $1,751.00
  ```

**Step 4: Submit Order (10:00 - 10:30)**

**Narration**:
> "Review your order details, then click 'Place Order'. The system will ask for confirmation. After confirming, the order is submitted to IBKR."

**Screen Actions**:
- Click "Place Order" button
- Confirmation modal appears:
  ```
  Confirm Order
  Symbol: AAPL
  Direction: Buy
  Quantity: 10
  Price: $175.00 (Limit)
  Estimated Total: $1,751.00
  
  [Cancel] [Confirm]
  ```
- Click "Confirm"
- Show success notification: "✓ Order placed successfully (Order ID: 12345)"
- Order appears in "Open Orders" table with status "Submitted"

**On-Screen Text**:
- Arrow pointing to order in table: "Your order is now active"
- "Status updates automatically via WebSocket"

**Timestamps**: 9:00 - 10:30

---

### Section 7: Create and Run a Strategy (10:30 - 12:00)

**Visual**: Strategies page

**Narration**:
> "Now let's create a simple trading strategy. Navigate to the Strategies page and click 'Create New Strategy'."

**Screen Actions**:

**Step 1: Navigate to Strategies (10:30 - 10:40)**
- Click "Strategies" in sidebar
- Click "Create New Strategy" button

**Step 2: Fill Strategy Form (10:40 - 11:20)**

**Narration**:
> "We'll create a basic mean reversion strategy. Give it a name, select 'Intraday' as the type, and paste in our algorithm code. For this demo, we'll use a pre-written strategy that buys when RSI is oversold and sells when overbought."

**Screen Actions**:
- Fill form:
  ```
  Name: Mean Reversion - AAPL
  Description: RSI-based mean reversion strategy
  Type: Intraday
  Symbols: AAPL, MSFT
  Initial Balance: $100,000
  ```
- Algorithm Code (show in code editor):
  ```csharp
  public class MeanReversionAlgorithm : QCAlgorithm
  {
      private RelativeStrengthIndex _rsi;
      
      public override void Initialize()
      {
          SetStartDate(2024, 1, 1);
          SetCash(100000);
          
          AddEquity("AAPL", Resolution.Minute);
          _rsi = RSI("AAPL", 14);
      }
      
      public override void OnData(Slice data)
      {
          if (!_rsi.IsReady) return;
          
          if (_rsi < 30 && !Portfolio.Invested)
          {
              SetHoldings("AAPL", 0.9);
          }
          else if (_rsi > 70 && Portfolio.Invested)
          {
              Liquidate("AAPL");
          }
      }
  }
  ```
- Click "Save Strategy"
- Show success message: "Strategy created successfully"

**Step 3: Start Strategy (11:20 - 12:00)**

**Narration**:
> "Our strategy is created. To run it in live paper trading mode, click the 'Start' button next to the strategy."

**Screen Actions**:
- Strategy card appears in list
- Click "Start" button
- Modal appears:
  ```
  Start Strategy
  
  ⚠️ This will execute trades in your IBKR paper trading account
  
  Mode: ○ Paper Trading  ○ Live Trading
  Account: DU1234567
  
  [Cancel] [Start Strategy]
  ```
- Select "Paper Trading" radio button
- Click "Start Strategy"
- Strategy card updates:
  - Status badge: "Running" (green)
  - Shows live P&L: $0.00
  - Trade count: 0

**On-Screen Text**:
- "Strategy is now monitoring the market"
- "Trades will execute when conditions are met"

**Timestamps**: 10:30 - 12:00

---

### Section 8: Monitor Performance (12:00 - 13:00)

**Visual**: Strategy detail page

**Narration**:
> "Let's monitor our running strategy. Click on the strategy to view detailed performance metrics and logs."

**Screen Actions**:

**Step 1: Open Strategy Details (12:00 - 12:15)**
- Click on "Mean Reversion - AAPL" strategy card
- Strategy detail page loads

**Step 2: Show Live Data (12:15 - 12:45)**

**Narration**:
> "Here we can see real-time logs, performance charts, and trade history. The system automatically updates as the strategy runs."

**Screen Actions** (show each panel for 10 seconds):

1. **Performance Summary** (top cards):
   ```
   Total Return: +2.5%
   Sharpe Ratio: 1.85
   Win Rate: 65.0%
   Total Trades: 12
   ```

2. **Equity Curve Chart** (middle):
   - Line chart showing portfolio value over time
   - Starts at $100K, currently at $102,500

3. **Live Logs** (bottom):
   ```
   [10:30:15] INFO: Strategy started
   [10:35:22] INFO: RSI(14) = 28.5 (Oversold)
   [10:35:23] INFO: Buy signal: AAPL @ $175.20
   [10:35:24] INFO: Order placed: Buy 550 AAPL @ Market
   [10:35:25] INFO: Order filled: Avg price $175.22
   ```
- Show logs scrolling automatically (WebSocket updates)

**Step 3: View Trade History (12:45 - 13:00)**
- Scroll down to "Trade History" table
- Show executed trades:
  ```
  Time       Symbol  Direction  Qty  Price    P&L
  10:35:25   AAPL    Buy        550  175.22   -
  11:20:15   AAPL    Sell       550  179.50   +$2,354
  ```

**Timestamps**: 12:00 - 13:00

---

### Section 9: Run a Backtest (13:00 - 14:00)

**Visual**: Backtest page

**Narration**:
> "Before running strategies with real money, it's crucial to backtest them. Let's run a historical backtest on our strategy to see how it would have performed."

**Screen Actions**:

**Step 1: Navigate to Backtest (13:00 - 13:10)**
- Click "Backtest" in sidebar
- Click "New Backtest" button

**Step 2: Configure Backtest (13:10 - 13:30)**
- Select Strategy: "Mean Reversion - AAPL"
- Start Date: `2023-01-01`
- End Date: `2023-12-31`
- Initial Capital: `$100,000`
- Resolution: `Minute`
- Click "Run Backtest"

**Step 3: Show Progress (13:30 - 13:45)**

**Narration**:
> "The backtest is now running. You can see the progress bar as it processes a year of historical data. This typically takes a minute or two."

**Screen Actions**:
- Progress modal shows:
  ```
  Running Backtest...
  Progress: [████████░░] 75%
  Estimated Time Remaining: 30 seconds
  ```
- Progress updates in real-time

**Step 4: View Results (13:45 - 14:00)**

**Narration**:
> " Great! The backtest completed. Let's look at the results. Our strategy returned 15.2% over the year with a Sharpe ratio of 1.85, which indicates good risk-adjusted returns."

**Screen Actions**:
- Results page loads showing:
  ```
  Backtest Results
  
  Performance Metrics:
  Total Return: 15.2%
  Sharpe Ratio: 1.85
  Max Drawdown: -8.5%
  Win Rate: 67.5%
  Total Trades: 234
  Profit Factor: 2.15
  ```
- Show equity curve chart (full year)
- Show monthly returns table
- Show drawdown chart

**Timestamps**: 13:00 - 14:00

---

### Outro (14:00 - 15:00)

**Visual**: Back to Dashboard, then fade to closing screen

**Narration**:
> "Congratulations! You've successfully installed Lean WebUI, connected to IBKR, placed your first order, created and ran a trading strategy, and performed a backtest. This is just the beginning—Lean WebUI offers many more advanced features like custom indicators, multi-strategy portfolios, and risk management controls."

**Screen Actions**:
- Quick montage (3 seconds each) of:
  - Dashboard
  - Trading page with live charts
  - Strategy running
  - Backtest results
- Fade to closing screen

**Narration**:
> "For more tutorials and documentation, visit our GitHub repository and documentation site. Links are in the video description. If you found this helpful, please like and subscribe. Happy trading!"

**On-Screen Text**:
```
📚 Documentation: 
   https://github.com/QuantConnect/Lean/tree/master/docs

💬 Community Forum: 
   https://www.quantconnect.com/forum

🛠️ Report Issues: 
   https://github.com/QuantConnect/Lean/issues

⭐ Star on GitHub: 
   https://github.com/QuantConnect/Lean

Thank you for watching!
```

**Timestamps**: 14:00 - 15:00

---

## Post-Production Checklist

### Editing

- [ ] Remove long pauses and "um/uh" sounds
- [ ] Add zoom-in effects for small UI elements
- [ ] Add transition effects between sections (fade/slide)
- [ ] Add background music (low volume, non-distracting)
- [ ] Add Chinese subtitles for entire video
- [ ] Add chapter markers (YouTube chapters):
  - 0:00 Intro
  - 1:00 Prerequisites
  - 2:30 Installation
  - 5:00 First Login
  - 6:30 Dashboard Tour
  - 7:30 IBKR Setup
  - 9:00 Place First Order
  - 10:30 Create Strategy
  - 12:00 Monitor Performance
  - 13:00 Run Backtest
  - 14:00 Outro

### Graphics

- [ ] Add animated title at beginning (5 seconds)
- [ ] Add "Subscribe" button animation (bottom-right corner)
- [ ] Add tooltips/arrows to highlight UI elements
- [ ] Add error highlighting (red box) for important warnings
- [ ] Add success checkmarks for completed steps

### Audio

- [ ] Normalize audio levels
- [ ] Remove background noise
- [ ] Add subtle click sounds for button presses
- [ ] Add "whoosh" transition sounds (not too loud)
- [ ] Final audio mix: Voice (primary), Music (20% volume), SFX (30% volume)

### Export Settings

```
Format: MP4 (H.264)
Resolution: 1920x1080
Frame Rate: 30 FPS
Bitrate: 8000 kbps (high quality)
Audio: AAC, 192 kbps, 48 kHz
```

---

## YouTube Upload Details

**Title**: 
- English: "Lean WebUI - Complete Quick Start Guide | Algorithmic Trading with IBKR"
- Chinese: "Lean WebUI 完整快速入门教程 | 使用IBKR进行算法交易"

**Description**:
```
Learn how to set up Lean WebUI for algorithmic trading with Interactive Brokers in under 15 minutes!

🚀 What you'll learn:
✅ Install Lean WebUI using Docker Compose
✅ Connect to IBKR paper trading account
✅ Place your first order
✅ Create and run a trading strategy
✅ Perform backtesting on historical data

📚 Documentation:
- GitHub: https://github.com/QuantConnect/Lean
- User Guide: [link to docs]
- API Reference: [link to docs]

⏱️ Timestamps:
0:00 Introduction
1:00 Prerequisites
2:30 Installation with Docker
5:00 First Login & Password Change
6:30 Dashboard Tour
7:30 IBKR Connection Setup
9:00 Place First Order
10:30 Create a Trading Strategy
12:00 Monitor Strategy Performance
13:00 Run a Backtest
14:00 Conclusion & Next Steps

🔗 Useful Links:
- Lean Documentation: https://www.quantconnect.com/docs
- IBKR API Guide: [link]
- Community Forum: https://www.quantconnect.com/forum

#AlgorithmicTrading #QuantitativeFinance #IBKR #LeanAlgorithm #TradingBot #Python #CSharp
```

**Tags**:
```
algorithmic trading, quantitative finance, lean algorithm, interactive brokers, ibkr, trading bot, backtesting, algo trading, quantconnect, programming, c#, python, docker, web development, fintech
```

**Thumbnail Design**:
- Bold text: "Lean WebUI"
- Subtitle: "Quick Start Guide"
- Screenshots: Split view of code + chart
- Color scheme: Blue/green (professional)
- Include play button icon overlay

---

## Alternative Formats

### Short Version (5 minutes)

For social media (YouTube Shorts, TikTok):
- 0:00-0:30: Intro
- 0:30-2:00: Docker installation (quick)
- 2:00-3:30: Place order demo
- 3:30-4:30: Strategy creation
- 4:30-5:00: Results & CTA

### Tutorial Series (15-20 min each)

1. **Part 1**: Installation & Setup
2. **Part 2**: Trading Basics  & Order Management
3. **Part 3**: Strategy Development
4. **Part 4**: Backtesting & Optimization
5. **Part 5**: Risk Management & Monitoring
6. **Part 6**: Advanced Features & Production Deployment

---

## Additional Resources

### Supplementary Materials

Create and link in video description:
1. **Sample Code Repository**: GitHub gist with the mean reversion strategy
2. **Configuration Templates**: example .env file, docker-compose.yml
3. **Troubleshooting Guide PDF**: Common issues and solutions
4. **Cheat Sheet**: Quick reference for API endpoints and keyboard shortcuts

### Community Engagement

- Pin comment asking viewers to share their strategies
- Create poll: "What feature would you like to see next?"
- Respond to comments within 24 hours
- Create follow-up videos based on common questions

---

**Production Status**: Ready for recording  
**Last Updated**: January 2024  
**Version**: 1.0

