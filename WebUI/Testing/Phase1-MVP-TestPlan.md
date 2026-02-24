# Phase 1 MVP Verification Test Plan
# Personal Trading WebUI - 第一阶段验收测试计划

## Overview / 概述

This document outlines the test plan for Phase 1 MVP verification of the Personal Trading WebUI. These tests ensure that the core functionality required for basic trading operations is working correctly.

本文档概述了个人交易WebUI第一阶段MVP验证的测试计划。这些测试确保基本交易操作所需的核心功能正常工作。

**Test Period / 测试周期**: 2-3 days  
**Target Environment / 目标环境**: Paper Trading Account (IBKR)  
**Testing Team / 测试团队**: Development team + Pilot users

---

## Test Checklist / 测试清单

### 27.1 IBKR Paper Trading Account Connection / IBKR纸面交易账户连接

**Objective / 目标**: Verify successful connection to IBKR paper trading account through TWS/IB Gateway

**Prerequisites / 前提条件**:
- IBKR paper trading account credentials
- TWS or IB Gateway installed and configured
- API access enabled (Socket Port: 7497 for paper, 7496 for live)
- WebUI backend service running

**Test Steps / 测试步骤**:

1. **Start IB Gateway or TWS**
   - Launch TWS/Gateway and login with paper trading credentials
   - Ensure "Enable ActiveX and Socket Clients" is checked
   - Note the socket port number (default 7497 for paper)

2. **Configure WebUI Connection**
   - Navigate to Settings → IBKR Configuration
   - Enter connection details:
     - Host: localhost (or 127.0.0.1)
     - Port: 7497
     - Client ID: 1 (or unique number)
     - Account Type: Paper Trading
   - Save configuration

3. **Initiate Connection**
   - Click "Connect to IBKR" button
   - Monitor connection status in UI

4. **Verify Connection Success**
   - Check connection status indicator shows "Connected" (green)
   - Verify account ID is displayed correctly
   - Check connection timestamp is current
   - Review connection logs for any errors

**Expected Results / 期望结果**:
- ✅ Connection status: "Connected"
- ✅ Account information displayed (Account ID, Type)
- ✅ No error messages in logs
- ✅ Connection remains stable for at least 5 minutes

**Pass Criteria / 通过标准**:
- Connection established within 30 seconds
- No disconnections during 5-minute observation period
- Account details correctly retrieved and displayed

---

### 27.2 US Stock Trading - Market and Limit Orders / 美股股票下单功能

**Objective / 目标**: Verify ability to place market and limit orders for US stocks

**Prerequisites / 前提条件**:
- IBKR account connected successfully
- Sufficient buying power in paper account
- Market hours (9:30 AM - 4:00 PM EST) or extended hours

**Test Steps - Market Order / 市价单测试**:

1. **Navigate to Trading Page**
   - Go to Trading → Place Order
   
2. **Search for Stock**
   - Search for "AAPL" (Apple Inc.)
   - Verify stock information loads (current price, volume, etc.)

3. **Create Market Buy Order**
   - Order Type: Market
   - Action: Buy
   - Quantity: 10 shares
   - Review order details

4. **Submit Order**
   - Click "Submit Order"
   - Confirm in dialog
   - Note order ID

5. **Verify Order Placement**
   - Check Orders page for new order
   - Verify status transitions: Submitted → Filled
   - Confirm execution price is near market price at submission time

**Test Steps - Limit Order / 限价单测试**:

1. **Create Limit Sell Order**
   - Symbol: AAPL (or use previously purchased stock)
   - Order Type: Limit
   - Action: Sell
   - Quantity: 10 shares
   - Limit Price: Current ask + $5.00 (to prevent immediate fill)

2. **Submit Order**
   - Review order details
   - Submit and confirm

3. **Verify Limit Order Behavior**
   - Check order status shows "Submitted" or "PreSubmitted"
   - Verify order appears in open orders list
   - Confirm limit price is correct

4. **Cancel Limit Order**
   - Select the limit order
   - Click "Cancel Order"
   - Verify cancellation success

**Expected Results / 期望结果**:
- ✅ Market order fills within seconds
- ✅ Limit order submits successfully and remains open
- ✅ Order details display correctly (symbol, quantity, type, price)
- ✅ Order cancellation works properly

**Pass Criteria / 通过标准**:
- Market order execution time < 5 seconds
- Limit order successfully placed and appears in open orders
- Order cancellation confirmed within 3 seconds
- All order details accurate in UI

---

### 27.3 Real-time Order Status Updates / 订单状态实时更新

**Objective / 目标**: Verify that order status updates are received and displayed in real-time through SignalR

**Prerequisites / 前提条件**:
- IBKR connected
- SignalR connection established
- At least one active order

**Test Steps / 测试步骤**:

1. **Place Test Order**
   - Submit a market order for a liquid stock (e.g., SPY)
   - Keep Orders page open

2. **Monitor Status Transitions**
   - Watch for status updates without refreshing page
   - Expected transitions: 
     - PendingSubmit → PreSubmitted → Submitted → Filled

3. **Verify Update Timestamps**
   - Check that each status change shows timestamp
   - Verify timestamps are in correct timezone

4. **Test Multiple Orders**
   - Submit 3-5 orders simultaneously
   - Verify all orders update independently
   - Ensure no UI lag or missed updates

5. **Test Notifications**
   - Check notification center for order fill alerts
   - Verify sound/visual notification (if configured)

**Expected Results / 期望结果**:
- ✅ Status updates appear automatically without page refresh
- ✅ All status transitions captured and displayed
- ✅ Timestamps accurate for each update
- ✅ No delays > 2 seconds for status propagation

**Pass Criteria / 通过标准**:
- Real-time updates received within 2 seconds of actual state change
- No missed status transitions
- UI remains responsive during rapid updates
- SignalR connection remains stable

---

### 27.4 Position Query and Display / 持仓查询和显示

**Objective / 目标**: Verify accurate retrieval and display of current positions

**Prerequisites / 前提条件**:
- IBKR connected
- At least 2-3 positions held in account (from previous buy orders)

**Test Steps / 测试步骤**:

1. **Navigate to Positions Page**
   - Go to Portfolio → Positions
   - Allow page to load

2. **Verify Position Data**
   - Check each position displays:
     - Symbol
     - Quantity
     - Average cost
     - Current market price
     - Market value
     - Unrealized P&L
     - P&L percentage

3. **Test Real-time Price Updates**
   - Monitor current price updates (should update every 1-5 seconds)
   - Verify P&L recalculates with price changes
   - Check for smooth updates without flickering

4. **Test Sorting and Filtering**
   - Sort by: Symbol, Quantity, P&L, % Change
   - Filter by: Profit/Loss, Long/Short
   - Verify results correct after each action

5. **Test Position Details**
   - Click on a position to see details
   - Verify detailed view shows:
     - Trade history for that symbol
     - Average purchase price calculation
     - Total invested amount

6. **Test Export Functionality**
   - Export positions to CSV
   - Verify all data included and formatted correctly

**Expected Results / 期望结果**:
- ✅ All positions displayed with accurate data
- ✅ Real-time price and P&L updates working
- ✅ Calculations correct (market value, P&L)
- ✅ Sorting and filtering work as expected

**Pass Criteria / 通过标准**:
- Position data matches IBKR account portal (verify manually)
- P&L calculations accurate within $0.01
- Real-time updates within 5 seconds
- No missing positions

---

### 27.5 Account Balance Query / 账户余额查询

**Objective / 目标**: Verify accurate display of account balance and buying power

**Prerequisites / 前提条件**:
- IBKR connected
- Some trading activity completed

**Test Steps / 测试步骤**:

1. **View Account Summary**
   - Navigate to Dashboard or Account page
   - Locate account balance section

2. **Verify Balance Components**
   - Check following values displayed:
     - Total Account Value (Net Liquidation Value)
     - Cash Available
     - Buying Power
     - Equity (Stocks Value)
     - Unrealized P&L
     - Realized P&L (today/total)

3. **Compare with IBKR Portal**
   - Open IBKR Account Management portal
   - Compare key metrics with WebUI display
   - Verify values match (or explain discrepancies)

4. **Test Balance Updates**
   - Place and fill a trade
   - Monitor account balance updates
   - Verify buying power decreases appropriately
   - Check that cash and equity adjust correctly

5. **Verify Historical Balance**
   - Check account value chart (if available)
   - Verify end-of-day balance is logged

**Expected Results / 期望结果**:
- ✅ All account balance fields populated
- ✅ Values match IBKR portal data
- ✅ Buying power calculations correct
- ✅ Updates reflect recent trades

**Pass Criteria / 通过标准**:
- Account totals within $1.00 of IBKR portal values
- Buying power calculation accurate for margin rules
- Balance updates within 10 seconds of trade execution

---

### 27.6 Basic Web Interface Usability / 基础Web界面可用性

**Objective / 目标**: Verify that the web interface is functional, responsive, and user-friendly

**Test Areas / 测试区域**:

#### A. Login and Authentication / 登录认证

**Test Steps**:
1. Navigate to WebUI URL (e.g., http://localhost:5000)
2. Test login with valid credentials
3. Test login with invalid credentials
4. Test "Remember Me" functionality
5. Test logout functionality
6. Verify session timeout after inactivity (if configured)

**Expected Results**:
- ✅ Login successful with correct credentials
- ✅ Error message for invalid credentials
- ✅ Session persists with "Remember Me"
- ✅ Logout clears session and redirects to login

#### B. Navigation / 导航

**Test Steps**:
1. Verify sidebar menu displays all sections:
   - Dashboard
   - Trading
   - Orders
   - Positions
   - Account
   - Settings
2. Click each menu item and verify page loads
3. Test breadcrumb navigation
4. Test browser back/forward buttons
5. Test direct URL navigation to different pages

**Expected Results**:
- ✅ All menu items clickable and functional
- ✅ Page transitions smooth (< 1 second)
- ✅ Current page highlighted in menu
- ✅ Browser navigation works correctly

#### C. Responsive Design / 响应式设计

**Test Steps**:
1. Test on desktop (1920x1080, 1366x768)
2. Test on tablet (iPad, 768px width)
3. Test on mobile (iPhone, 375px width)
4. Resize browser window and check layout adapts
5. Test portrait and landscape orientations (mobile/tablet)

**Expected Results**:
- ✅ Layout adapts to different screen sizes
- ✅ Mobile menu (hamburger) works on small screens
- ✅ No horizontal scrolling on mobile
- ✅ Touch interactions work on mobile
- ✅ All features accessible on all screen sizes

#### D. Browser Compatibility / 浏览器兼容性

**Test Browsers**:
- Chrome (latest)
- Firefox (latest)
- Edge (latest)
- Safari (macOS/iOS)

**Test Steps**:
1. Open WebUI in each browser
2. Test core functionality:
   - Login
   - Place order
   - View positions
   - Real-time updates
3. Check for console errors
4. Verify visual consistency

**Expected Results**:
- ✅ All features work in all tested browsers
- ✅ Visual appearance consistent
- ✅ No critical console errors

#### E. Performance / 性能

**Test Steps**:
1. Measure page load times
2. Test with SignalR pushing rapid updates
3. Test with large order/position lists (50+ items)
4. Monitor browser memory usage over 30 minutes

**Expected Results**:
- ✅ Initial page load < 3 seconds
- ✅ Page transitions < 1 second
- ✅ UI remains responsive during real-time updates
- ✅ No memory leaks (stable memory after 30 min)

**Pass Criteria / 通过标准**:
- All core features accessible and working
- Responsive design works on 3 screen sizes
- No critical bugs blocking usage
- Performance acceptable (page loads < 3s)

---

### 27.7 User Acceptance Testing (UAT) / 用户验收测试

**Objective / 目标**: Validate real-world usage scenarios with actual users

**Test Scenarios / 测试场景**:

#### Scenario 1: New User Onboarding / 新用户入门

**User Profile**: First-time user, familiar with trading concepts

**Steps**:
1. User receives WebUI URL and credentials
2. User logs in for the first time
3. User navigates to Settings to configure IBKR connection
4. User enters IBKR credentials and connects account
5. User explores Dashboard to understand interface
6. User places first practice trade (10 shares SPY market order)
7. User monitors order execution
8. User views resulting position

**Success Criteria**:
- User completes all steps without assistance
- Time to first trade < 15 minutes
- User feels confident to continue using system

#### Scenario 2: Daily Trading Workflow / 日常交易流程

**User Profile**: Experienced trader, daily active user

**Steps**:
1. User logs in at market open (9:30 AM EST)
2. User checks account balance and buying power
3. User reviews current positions and P&L
4. User searches for TSLA and views current price
5. User places limit buy order: 20 TSLA @ $5 below current price
6. User places limit sell order on existing position (take profit)
7. User monitors open orders throughout day
8. User receives real-time notification when limit order fills
9. User reviews filled orders and updated positions
10. User checks end-of-day P&L before logout

**Success Criteria**:
- All steps complete without errors
- Real-time updates work smoothly
- User can efficiently manage multiple orders
- User satisfied with workflow efficiency

#### Scenario 3: Error Recovery / 错误处理与恢复

**User Profile**: Any user

**Steps**:
1. User places order with insufficient buying power
2. System displays clear error message
3. User corrects quantity and resubmits successfully
4. User loses internet connection mid-session
5. System shows connection lost warning
6. Internet reconnects automatically
7. User verifies data is current after reconnection
8. User attempts to cancel already-filled order
9. System shows appropriate error message

**Success Criteria**:
- All error messages clear and actionable
- System recovers gracefully from connection loss
- No data loss during disconnection
- User can continue working after errors

**Additional UAT Requirements**:
- Minimum 3 test users complete all scenarios
- Collect feedback on usability, performance, and missing features
- Document all issues encountered
- Rate overall satisfaction (1-10 scale)

**Pass Criteria / 通过标准**:
- 3/3 scenarios completed successfully by all testers
- Average satisfaction rating ≥ 7/10
- No critical blocking issues found
- All P1/P2 bugs documented for fixing

---

### 27.8 First Phase User Feedback Collection / 收集第一阶段用户反馈

**Objective / 目标**: Gather structured feedback from pilot users to guide Phase 2 development

**Feedback Collection Methods / 反馈收集方法**:

1. **In-App Feedback Form** (preferred)
   - Embedded feedback widget
   - Star rating system
   - Free-text comments
   - Issue type categorization (Bug/Feature Request/UX Issue)

2. **Post-UAT Survey**
   - Google Form or similar
   - Shared after each user completes UAT scenarios

3. **Interview Sessions**
   - 30-minute 1-on-1 sessions with each tester
   - Screen sharing to observe actual usage
   - Recorded for later review

**Feedback Questions / 反馈问题**:

#### A. Functionality / 功能性 (1-10 scale)
- How would you rate the order placement experience?
- How satisfied are you with the real-time data updates?
- How useful is the positions/portfolio view?
- How clear are the account balance displays?

#### B. Usability / 易用性 (1-10 scale)
- How intuitive is the navigation structure?
- How easy is it to find the features you need?
- How would you rate the overall user experience?
- How responsive is the interface?

#### C. Performance / 性能 (1-10 scale)
- How fast does the application load?
- How smooth are the real-time updates?
- Have you experienced any lag or delays?

#### D. Open-Ended Questions / 开放式问题
1. What did you like most about the WebUI?
2. What frustrated you the most?
3. What features are missing that you need?
4. What would you change about the current design?
5. Would you use this for actual trading? Why or why not?
6. Any additional comments or suggestions?

#### E. Feature Priority Voting / 功能优先级投票
Ask users to rank Phase 2/3 features by importance:
- Real-time candlestick charts with indicators
- Strategy management (start/stop/configure)
- Backtesting interface
- ETF screening and filtering
- Dollar-cost averaging (DCA) plans
- Mobile app
- Portfolio analytics and reports
- Risk management rules
- Multi-account support

**Feedback Analysis / 反馈分析**:
- Compile all responses into spreadsheet
- Calculate average scores for quantitative questions
- Categorize and tag qualitative feedback
- Identify top 5 issues/requests
- Create prioritized backlog for Phase 2

**Deliverables / 交付物**:
- Feedback summary report (中英文)
- Issue tracking tickets for bugs
- Feature request backlog
- User satisfaction metrics
- Recommendations for Phase 2 priorities

**Pass Criteria / 通过标准**:
- Feedback collected from ≥ 3 users
- All feedback documented and categorized
- Summary report completed
- Actionable backlog created for Phase 2

---

## Test Environment Setup / 测试环境设置

### Required Infrastructure / 所需基础设施

1. **IBKR Paper Trading Account**
   - Create account at: https://www.interactivebrokers.com/en/trading/tws-updateable-latest.php
   - Fund with virtual money (default $1M USD)
   - Enable API access in account settings

2. **TWS or IB Gateway**
   - Download: Trader Workstation or IB Gateway
   - Configure API settings:
     - Enable ActiveX and Socket Clients
     - Socket port: 7497 (paper) or 7496 (live)
     - Allow connections from localhost

3. **WebUI Application**
   - Backend: ASP.NET Core running on http://localhost:5000
   - Database: PostgreSQL or SQLite configured
   - Frontend: React app served from backend

4. **Test Data**
   - Multiple stock symbols for testing (AAPL, MSFT, TSLA, SPY, etc.)
   - Some positions already held for testing portfolio view

### Configuration Checklist / 配置检查清单

- [x] IBKR paper account created and funded
- [x] TWS/Gateway installed and configured
- [x] WebUI backend built and running
- [x] Database initialized with migrations
- [x] Frontend assets compiled and served
- [x] Admin user account created
- [x] IBKR connection settings configured
- [x] Test user accounts created (for multi-user testing)

---

## Test Execution Schedule / 测试执行计划

### Day 1: Infrastructure and Connection Tests
- 27.1: IBKR connection verification
- 27.5: Account balance query
- Environment stability checks

### Day 2: Trading Functionality Tests
- 27.2: Order placement (market & limit)
- 27.3: Real-time order status updates
- 27.4: Position display and updates
- 27.6: Web interface usability

### Day 3: User Acceptance Testing
- 27.7: UAT scenarios with pilot users
- 27.8: Feedback collection and analysis

---

## Issue Tracking / 问题跟踪

### Bug Severity Definitions / 缺陷严重程度定义

- **P0 - Critical**: System unusable, data loss, security issue
- **P1 - High**: Major feature broken, workaround exists
- **P2 - Medium**: Minor feature broken, cosmetic issue
- **P3 - Low**: Enhancement, nice-to-have

### Bug Template / 缺陷报告模板

```markdown
**Title**: [Brief description]
**Priority**: P0/P1/P2/P3
**Test Case**: [Which test case/scenario]
**Steps to Reproduce**:
1. 
2. 
3. 

**Expected Result**: 
**Actual Result**: 
**Screenshots**: [If applicable]
**Environment**: 
- Browser: 
- OS: 
- WebUI Version: 
**Notes**: 
```

---

## Test Completion Criteria / 测试完成标准

Phase 1 MVP is considered verified and ready for broader release when:

✅ All 8 test sections (27.1 - 27.8) passed  
✅ IBKR connection stable and reliable  
✅ Core trading workflow functional (buy/sell stocks)  
✅ Real-time updates working correctly  
✅ Web interface usable on desktop and mobile  
✅ UAT scenarios completed by ≥ 3 users  
✅ User feedback collected and analyzed  
✅ All P0 bugs fixed  
✅ All P1 bugs fixed or documented with workarounds  
✅ Known issues documented in release notes

---

## Sign-off / 签署确认

| Role | Name | Date | Signature |
|------|------|------|-----------|
| Test Lead | | | |
| Product Owner | | | |
| Dev Lead | | | |

---

## Appendix A: Test Data / 测试数据

### Sample Stock Symbols for Testing
- **Large Cap Tech**: AAPL, MSFT, GOOGL, AMZN, META
- **High Volume ETFs**: SPY, QQQ, VOO, IWM
- **Medium Cap**: TSLA, NVDA, AMD
- **Dividend Stocks**: JNJ, PG, KO

### Sample Test Orders
```json
[
  {
    "symbol": "AAPL",
    "action": "BUY",
    "orderType": "MARKET",
    "quantity": 10
  },
  {
    "symbol": "SPY",
    "action": "BUY",
    "orderType": "LIMIT",
    "quantity": 5,
    "limitPrice": "current - 5.00"
  },
  {
    "symbol": "MSFT",
    "action": "SELL",
    "orderType": "LIMIT",
    "quantity": 10,
    "limitPrice": "current + 10.00"
  }
]
```

---

## Appendix B: Common Issues and Solutions / 常见问题与解决方案

### Issue: Cannot connect to IBKR
**Solution**: 
- Verify TWS/Gateway is running
- Check API settings enabled
- Confirm port number (7497 for paper, 7496 for live)
- Disable firewall temporarily for testing

### Issue: Orders not filling
**Solution**:
- Verify market is open (9:30 AM - 4:00 PM EST)
- Check sufficient buying power
- For limit orders, check price is reasonable
- Verify stock symbol is valid

### Issue: Real-time updates not working
**Solution**:
- Check SignalR connection in browser console
- Verify WebSocket connection allowed (no proxy blocking)
- Restart backend service
- Clear browser cache

### Issue: Positions not displaying
**Solution**:
- Wait 30 seconds after order fill
- Refresh page
- Check IBKR account portal to verify position exists
- Review backend logs for errors

---

**Document Version**: 1.0  
**Last Updated**: 2026-02-19  
**Next Review**: After Phase 1 completion
