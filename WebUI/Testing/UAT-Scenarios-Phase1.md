# User Acceptance Testing (UAT) Scenarios
# Personal Trading WebUI - Phase 1 MVP
# 用户验收测试场景

## Introduction / 介绍

This document provides detailed step-by-step scenarios for User Acceptance Testing (UAT) of the Personal Trading WebUI Phase 1 MVP. These scenarios simulate real-world usage patterns and help validate that the system meets user requirements.

本文档为个人交易WebUI第一阶段MVP的用户验收测试(UAT)提供详细的分步场景。这些场景模拟真实世界的使用模式，帮助验证系统满足用户需求。

**Target Testers / 目标测试人员**: 3-5 users with varying levels of trading experience  
**Duration / 持续时间**: 2-3 hours per tester  
**Environment / 环境**: IBKR Paper Trading Account  
**Prerequisites / 前提条件**:
- Access to WebUI (URL and credentials provided)
- IBKR paper trading account set up
- TWS or IB Gateway running
- Modern web browser (Chrome, Firefox, Edge, or Safari)
- Stable internet connection

---

## Tester Information / 测试人员信息

**Please fill out before starting:**

- **Tester Name / 姓名**: ___________________________
- **Date / 日期**: ___________________________
- **Trading Experience / 交易经验**: 
  - [ ] Beginner (< 1 year)
  - [ ] Intermediate (1-3 years)
  - [ ] Advanced (> 3 years)
- **Technical Proficiency / 技术水平**:
  - [ ] Low (basic computer use)
  - [ ] Medium (comfortable with web apps)
  - [ ] High (power user, developer)
- **Testing Device / 测试设备**:
  - Device Type: [ ] Desktop [ ] Laptop [ ] Tablet [ ] Mobile
  - Operating System: _______________________
  - Browser: _______________________
  - Screen Size: _______________________

---

## Scenario 1: New User Onboarding
## 场景1：新用户入门

**Goal / 目标**: Validate that a first-time user can successfully set up and start using the WebUI.

**User Persona / 用户画像**: Sarah, a retail investor new to algorithmic trading platforms. She has basic trading knowledge but hasn't used Lean before.

**Estimated Time / 预估时间**: 15-20 minutes

---

### Step 1.1: Initial Access

1. Open your web browser
2. Navigate to the WebUI URL: `_______________________` (provided by test coordinator)
3. **Observe**: Does the login page load correctly?
   - [ ] Yes - Page loads quickly and displays login form
   - [ ] No - Describe issue: _______________________

**Time to load (seconds)**: ___________

**First Impression (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.2: First Login

1. Enter your username: `_______________________`
2. Enter your password: `_______________________`
3. (Optional) Check "Remember Me" if available
4. Click "Login" or "Sign In" button

**Questions to answer:**
- Did the login succeed immediately? [ ] Yes [ ] No
- If no, what error message appeared? _______________________
- How long did it take to log in? _________ seconds
- Did you feel the login process was secure? [ ] Yes [ ] No [ ] Not sure

**Rate the login experience (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.3: First Impressions of Dashboard

After logging in, you should see the main dashboard.

1. **Observe the layout**:
   - [ ] Sidebar navigation menu visible
   - [ ] Top navigation bar visible
   - [ ] Main content area visible
   - [ ] Footer visible (if applicable)

2. **Identify the following menu items** (check all you see):
   - [ ] Dashboard / Home
   - [ ] Trading / Orders
   - [ ] Positions / Portfolio
   - [ ] Account / Balance
   - [ ] Settings / Configuration
   - [ ] Help / Documentation

3. **Quick orientation test**: Without clicking, try to guess where you would go to:
   - Place a trade: _______________________
   - View your current positions: _______________________
   - Check account balance: _______________________
   - Connect to IBKR: _______________________

**Rate dashboard clarity (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.4: Configure IBKR Connection

1. Navigate to **Settings** (or IBKR Configuration page)
2. Locate the IBKR connection settings section
3. Fill in the connection details:
   - **Host**: `localhost` (or `127.0.0.1`)
   - **Port**: `7497` (for paper trading)
   - **Client ID**: `1` (or any unique number)
   - **Account Type**: Select "Paper Trading"
4. Click "Save" or "Save Configuration"

**Questions:**
- Were the fields clearly labeled? [ ] Yes [ ] Somewhat [ ] No
- Did you understand what each field meant? [ ] Yes [ ] Needed help [ ] No
- Was there helpful tooltip or documentation? [ ] Yes [ ] No

5. Click "Connect" button (if separate from Save)
6. **Observe** the connection attempt:
   - [ ] Connection status updates in real-time
   - [ ] Progress indicator shown
   - [ ] Clear success/failure message displayed

**Connection Result:**
- [ ] Connected successfully
- [ ] Failed to connect - Error: _______________________

**Time to connect (seconds)**: ___________

**Rate connection setup process (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.5: Verify Account Information

Once connected, you should see your IBKR account information.

1. Navigate to **Account** or **Dashboard** page
2. **Verify the following information is displayed**:
   - [ ] Account ID / Number
   - [ ] Account Type (e.g., "Paper Trading")
   - [ ] Total Account Value / Net Liquidation
   - [ ] Cash Available
   - [ ] Buying Power
   - [ ] Connection Status (e.g., "Connected" with green indicator)

3. **Check for reasonableness**:
   - Does the account value look correct? [ ] Yes [ ] No [ ] Don't know
   - Is buying power greater than zero? [ ] Yes [ ] No

**Rate account info display (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.6: Place First Test Order

**Important**: This is a paper trading account with virtual money - no real money at risk.

1. Navigate to **Trading** or **Place Order** page
2. Search for stock symbol: `SPY` (S&P 500 ETF)
   - [ ] Search box easy to find
   - [ ] Search results appear quickly
   - [ ] Stock information displayed (price, name, etc.)

3. Select **SPY** from search results (if applicable)
4. Configure the order:
   - **Action**: BUY
   - **Order Type**: MARKET
   - **Quantity**: 10 shares
5. Review the estimated cost (should show estimate)
6. **Before submitting**, answer:
   - Does the order summary show all details clearly? [ ] Yes [ ] No
   - Is there a confirmation step? [ ] Yes [ ] No
   - Do you feel confident this order is correct? [ ] Yes [ ] No [ ] Unsure

7. Click "Submit Order" or "Place Order"
8. If confirmation dialog appears, review and click "Confirm"

**Order Submission:**
- [ ] Order submitted successfully
- [ ] Received order ID or confirmation number: _______________________
- [ ] Error occurred - describe: _______________________

**Time from clicking "Submit" to confirmation**: _________ seconds

**Rate order placement experience (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.7: Monitor Order Status

1. Navigate to **Orders** page (or stay on current page if order appears there)
2. Locate your SPY market order
3. **Watch the status** for 30-60 seconds:
   - Initial status: _______________________
   - Status after 30 seconds: _______________________
   - Final status: _______________________

**Questions:**
- Did the status update automatically without refreshing? [ ] Yes [ ] No
- Did you receive any notification when order filled? [ ] Yes [ ] No [ ] N/A
- Can you easily see order details (quantity, price, time)? [ ] Yes [ ] No

**Time for order to fill**: _________ seconds  
**Fill price**: $___________

**Rate order monitoring experience (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.8: View Position

After the order fills, you should have a position in SPY.

1. Navigate to **Positions** or **Portfolio** page
2. **Verify SPY position is shown**:
   - [ ] Symbol: SPY
   - [ ] Quantity: 10 shares
   - [ ] Average cost shown
   - [ ] Current market value shown
   - [ ] Unrealized P&L shown
   - [ ] Current price displayed

3. **Watch for real-time updates**:
   - Wait 30 seconds and observe if the current price updates
   - [ ] Price updated automatically
   - [ ] P&L recalculated automatically
   - [ ] No updates observed

**Rate position display experience (1-10)**: ___________  
Comments: _________________________________________________

---

### Step 1.9: Overall Assessment - Scenario 1

**Time to complete entire scenario**: _________ minutes

**Overall Experience Rating (1-10)**: ___________

**What did you like most?**
_________________________________________________________________
_________________________________________________________________

**What was most confusing or difficult?**
_________________________________________________________________
_________________________________________________________________

**Would you feel comfortable using this for real trading?**
- [ ] Yes, definitely
- [ ] Yes, with more practice
- [ ] Maybe
- [ ] No, needs improvement
- [ ] Absolutely not

**Additional comments:**
_________________________________________________________________
_________________________________________________________________

---

## Scenario 2: Daily Trading Workflow
## 场景2：日常交易流程

**Goal / 目标**: Validate typical daily trading activities.

**User Persona / 用户画像**: Mike, an active day trader who checks his account multiple times per day and makes several trades.

**Estimated Time / 预估时间**: 30-40 minutes

---

### Step 2.1: Morning Routine - Check Account

Imagine it's 9:30 AM EST, market just opened.

1. Log in to the WebUI
2. Navigate to Dashboard (if not already there)
3. **Quick account overview**:
   - Note your total account value: $___________
   - Note your buying power: $___________
   - Note number of current positions: ___________

4. Navigate to **Positions** page
5. Review your holdings (from Scenario 1, you should have SPY)
6. **Check current P&L**:
   - Is your SPY position showing profit or loss? ___________
   - Amount: $___________
   - Percentage: ___________%

**Rate the ease of getting a quick account overview (1-10)**: ___________

---

### Step 2.2: Research Opportunity - View Stock Information

You want to research Tesla (TSLA) before trading.

1. Navigate to **Trading** or **Market** page
2. Search for: `TSLA`
3. **Review available information**:
   - [ ] Current price displayed
   - [ ] Bid/Ask spread shown
   - [ ] Volume information
   - [ ] Daily high/low
   - [ ] Previous close
   - [ ] Chart or price history (if available)

**Questions:**
- Is there enough information to make a trading decision? [ ] Yes [ ] No
- What information is missing that you would want? _______________________

**Rate stock information display (1-10)**: ___________

---

### Step 2.3: Place Limit Buy Order

You want to buy TSLA, but only at a specific price.

1. While viewing TSLA information, locate current ask price: $___________
2. Calculate your limit price: Current ask - $5.00 = $___________
3. Create a limit buy order:
   - Symbol: TSLA
   - Action: BUY
   - Order Type: **LIMIT**
   - Quantity: 20 shares
   - Limit Price: $___________ (your calculated price)

4. Review order details
5. Submit the order

**Questions:**
- Was it easy to switch from Market to Limit order type? [ ] Yes [ ] No
- Did the UI clearly show the limit price you entered? [ ] Yes [ ] No
- Did it warn you that the order might not fill immediately? [ ] Yes [ ] No

**Order submitted successfully?** [ ] Yes [ ] No  
**Order ID**: _______________________

**Rate limit order placement (1-10)**: ___________

---

### Step 2.4: Place Limit Sell Order (Take Profit)

You already own SPY (from Scenario 1). Set a take-profit limit sell order.

1. Navigate to **Positions** page
2. Click on your SPY position
3. Find "Sell" or "Close Position" option
4. Create limit sell order:
   - Symbol: SPY
   - Action: SELL
   - Order Type: LIMIT
   - Quantity: 10 shares (full position)
   - Limit Price: Current ask + $10.00 = $___________

5. Submit the order

**Questions:**
- Could you initiate the sell order directly from the position? [ ] Yes [ ] No
- Did it auto-fill the quantity based on your position? [ ] Yes [ ] No

**Order ID**: _______________________

**Rate selling from position experience (1-10)**: ___________

---

### Step 2.5: Monitor Multiple Open Orders

You now have 2 open limit orders (TSLA buy and SPY sell).

1. Navigate to **Orders** page
2. **Verify both orders are shown**:
   - [ ] TSLA buy limit order visible
   - [ ] SPY sell limit order visible
   - [ ] Status shows "Open" or "Submitted" or similar
   - [ ] Limit prices displayed correctly

3. **Apply filters or sorting**:
   - Try filtering by "Open Orders" (if filter available)
   - Try sorting by Symbol
   - Try sorting by Date

**Questions:**
- Can you easily distinguish between open and filled orders? [ ] Yes [ ] No
- Is it clear which orders are active? [ ] Yes [ ] No

**Rate order list display (1-10)**: ___________

---

### Step 2.6: Cancel a Limit Order

You've changed your mind about buying TSLA.

1. On the **Orders** page, locate your TSLA limit buy order
2. Find the "Cancel" button or action
3. Click Cancel
4. Confirm cancellation (if confirmation dialog appears)

**Questions:**
- How many clicks to cancel? ___________
- Was there a confirmation step? [ ] Yes [ ] No
- Did the cancellation happen quickly? [ ] Yes (< 3 sec) [ ] Moderate [ ] Slow
- Did the order disappear from open orders or change status? [ ] Yes [ ] No

**Cancellation successful?** [ ] Yes [ ] No

**Rate order cancellation experience (1-10)**: ___________

---

### Step 2.7: Real-time Updates During Active Trading

Test the real-time update functionality.

1. Keep **Positions** or **Orders** page open
2. Do NOT refresh the page manually
3. Wait for 2-3 minutes and observe:
   - [ ] Prices updated automatically
   - [ ] P&L recalculated in real-time
   - [ ] Order statuses updated (if any orders fill)
   - [ ] No updates observed (note: might be slow if market quiet)

**Questions:**
- Did you see real-time updates happening? [ ] Yes [ ] No
- If yes, approximately how often? Every _________ seconds
- Were the updates smooth or did the page flicker? [ ] Smooth [ ] Some flicker [ ] Very jumpy
- Did you experience any lag or freezing? [ ] Yes [ ] No

**Rate real-time update experience (1-10)**: ___________

---

### Step 2.8: Check Order History

1. Navigate to **Orders** page
2. Switch to "All Orders" or "History" view (if separate)
3. **Verify you can see**:
   - [ ] Your filled SPY market buy order (from Scenario 1)
   - [ ] Your canceled TSLA limit order
   - [ ] Your open SPY sell limit order
   - [ ] Timestamps for each order
   - [ ] Order statuses (Filled, Canceled, Open, etc.)

4. **Try searching or filtering**:
   - Filter by Symbol: "SPY"
   - Filter by Status: "Filled"
   - Filter by Date: Today

**Questions:**
- Can you easily find past orders? [ ] Yes [ ] No
- Is the order history complete? [ ] Yes [ ] No [ ] Can't verify

**Rate order history functionality (1-10)**: ___________

---

### Step 2.9: End of Day Review

It's 4:00 PM EST, market just closed. Review your day.

1. Navigate to **Dashboard** or **Account** page
2. **Find end-of-day summary**:
   - Today's P&L: $___________
   - Total account value: $___________
   - Number of trades today: ___________
   - Current positions: ___________

**Questions:**
- Can you easily see today's performance? [ ] Yes [ ] No
- Is there a summary of today's activity? [ ] Yes [ ] No
- Can you see a chart of account value over time? [ ] Yes [ ] No

**Rate end-of-day review experience (1-10)**: ___________

---

### Step 2.10: Logout

1. Find the logout button/link (usually in top-right corner or user menu)
2. Click Logout
3. **Verify**:
   - [ ] Redirected to login page
   - [ ] Cannot access protected pages by URL navigation
   - [ ] Session is fully cleared

**Logout successful?** [ ] Yes [ ] No

---

### Overall Assessment - Scenario 2

**Time to complete entire scenario**: _________ minutes

**Overall Experience Rating (1-10)**: ___________

**How efficient was the daily trading workflow?**
- [ ] Very efficient - everything I need is accessible
- [ ] Mostly efficient - minor improvements needed
- [ ] Somewhat efficient - several pain points
- [ ] Not efficient - major redesign needed

**What would make the daily workflow faster?**
_________________________________________________________________
_________________________________________________________________

**Additional comments:**
_________________________________________________________________
_________________________________________________________________

---

## Scenario 3: Error Handling and Recovery
## 场景3：错误处理与恢复

**Goal / 目标**: Test how well the system handles errors and unexpected situations.

**User Persona / 用户画像**: Any user encountering various error conditions.

**Estimated Time / 预估时间**: 15-20 minutes

---

### Step 3.1: Insufficient Buying Power

Attempt to place an order larger than your buying power allows.

1. Navigate to **Account** page and note your buying power: $___________
2. Calculate impossible order: Buying Power / $500 = _________ shares (round up)
3. Go to **Trading** page
4. Try to buy: _________ shares of a $500+ stock (e.g., GOOGL or BRK.B)
5. Submit the order

**Expected Behavior**: System should reject the order

**What happened?**
- [ ] Order rejected with clear error message
- [ ] Order accepted (unexpected!)
- [ ] System crashed or froze

**If error shown:**
- Error message: "_______________________________________"
- Was the error message clear and helpful? [ ] Yes [ ] No
- Did it explain how to fix the issue? [ ] Yes [ ] No

**Rate error handling (1-10)**: ___________

---

### Step 3.2: Invalid Stock Symbol

Try to order a stock with an invalid symbol.

1. Navigate to **Trading** page
2. Search for: `INVALIDXYZ123`
3. Try to place an order (if system allows)

**What happened?**
- [ ] Search returned "No results" or "Invalid symbol"
- [ ] System showed an error message
- [ ] System allowed order to proceed (unexpected!)

**Error message (if any)**: "_______________________________________"

**Was it clear that the symbol doesn't exist?** [ ] Yes [ ] No

**Rate invalid symbol handling (1-10)**: ___________

---

### Step 3.3: Order with Invalid Parameters

Try to submit an order with missing or invalid data.

1. Navigate to **Trading** page
2. Search for valid symbol: `AAPL`
3. Try to submit order WITHOUT entering quantity (leave it blank or 0)

**What happened?**
- [ ] Submit button disabled (good UX)
- [ ] Error message: "_______________________________________"
- [ ] Order accepted with 0 shares (unexpected!)

4. Now enter negative quantity: `-10`
5. Try to submit

**What happened?**
- [ ] Error shown: "_______________________________________"
- [ ] Field turned red or showed validation error
- [ ] Order rejected

**Rate form validation (1-10)**: ___________

---

### Step 3.4: Cancel Already-Filled Order

Try to cancel an order that has already been filled.

1. Navigate to **Orders** page
2. Find a filled order (from previous scenarios)
3. Look for Cancel button
   - [ ] Cancel button NOT shown (good - correct behavior)
   - [ ] Cancel button shown but disabled
   - [ ] Cancel button shown and clickable

4. If Cancel button is available, try clicking it

**What happened?**
- [ ] Error message: "_______________________________________"
- [ ] Action not allowed (correct)
- [ ] Order actually canceled (unexpected!)

**Rate filled order handling (1-10)**: ___________

---

### Step 3.5: Connection Loss Simulation

**Note**: This requires disconnecting from internet temporarily. Skip if not possible.

1. While logged in and viewing a page with real-time data
2. Disconnect your internet (turn off WiFi, unplug ethernet, or use browser DevTools to simulate offline)
3. Wait 10-15 seconds
4. **Observe**:
   - [ ] Connection status indicator shows "Disconnected" or "Offline"
   - [ ] Warning message displayed
   - [ ] Real-time updates stopped
   - [ ] No indication of connection loss (poor UX)

5. Reconnect to internet
6. **Observe recovery**:
   - [ ] Auto-reconnected within _________ seconds
   - [ ] Required manual page refresh
   - [ ] Data is current after reconnection
   - [ ] Data seems stale or incorrect

**Rate connection loss handling (1-10)**: ___________

---

### Step 3.6: IBKR Disconnection

Test what happens when IBKR connection is lost.

**Note**: Only perform this if you can easily reconnect IBKR.

1. Close TWS or IB Gateway (disconnect from IBKR)
2. Return to WebUI and wait 30 seconds
3. **Observe**:
   - [ ] Connection status changes to "Disconnected"
   - [ ] Warning/alert message shown
   - [ ] Real-time data stops updating
   - [ ] No indication of disconnection (poor)

4. Restart TWS/Gateway and reconnect
5. Return to WebUI
6. **Check reconnection**:
   - [ ] Auto-reconnected without user action
   - [ ] Required clicking "Reconnect" button
   - [ ] Required full page refresh
   - [ ] Required logging out and back in

**Time to detect disconnection**: _________ seconds  
**Recovery method**: _______________________

**Rate IBKR disconnection handling (1-10)**: ___________

---

### Step 3.7: Session Timeout

Test session timeout behavior.

**Note**: This may take a long time depending on timeout setting. Skip if timeout is very long.

1. Log in
2. Leave the browser tab open but do not interact for timeout period
   - Estimated timeout: _________ minutes (if known)
3. After timeout period, try to perform an action (e.g., navigate to another page or click a button)

**What happened?**
- [ ] Automatically logged out and redirected to login
- [ ] Session refreshed automatically (still logged in)
- [ ] Error message shown but stayed on page
- [ ] Nothing - still seems to work

**If logged out:**
- [ ] Warning shown before timeout
- [ ] Session expired message clear
- [ ] Easy to log back in

**Rate session timeout handling (1-10)**: ___________

---

### Overall Assessment - Scenario 3

**Overall Error Handling Rating (1-10)**: ___________

**Which error case was handled best?**
_________________________________________________________________

**Which error case was handled worst?**
_________________________________________________________________

**Do you feel the system handles errors gracefully?**
- [ ] Yes, very well
- [ ] Mostly well
- [ ] Could be better
- [ ] Poor error handling

**Additional comments:**
_________________________________________________________________
_________________________________________________________________

---

## Final Assessment / 最终评估

### Overall UAT Summary

**Total time spent on all scenarios**: _________ hours

**Overall Satisfaction Rating (1-10)**: ___________

---

### Strengths (What worked well?)
1. _________________________________________________________________
2. _________________________________________________________________
3. _________________________________________________________________

---

### Weaknesses (What needs improvement?)
1. _________________________________________________________________
2. _________________________________________________________________
3. _________________________________________________________________

---

### Critical Issues (Blocking issues that must be fixed)
1. _________________________________________________________________
2. _________________________________________________________________
3. _________________________________________________________________

---

### Feature Requests (Nice to have)
1. _________________________________________________________________
2. _________________________________________________________________
3. _________________________________________________________________

---

### Would you use this system for real trading?

- [ ] Yes, definitely - it's ready
- [ ] Yes, after minor improvements
- [ ] Maybe, after significant improvements
- [ ] No, too many issues

**Explanation**: _________________________________________________________________
_________________________________________________________________

---

### Recommendation

**Should this proceed to Phase 2 development?**
- [ ] Yes, approve Phase 1 MVP
- [ ] Yes, but fix critical issues first
- [ ] No, needs major rework
- [ ] Unsure

---

### Additional Comments / 其他意见

Please share any additional thoughts, suggestions, or observations:

_________________________________________________________________
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________
_________________________________________________________________

---

**Thank you for your valuable feedback! / 感谢您宝贵的反馈！**

---

**Tester Signature**: _______________________  
**Date Completed**: _______________________

**Reviewed By**: _______________________  
**Date Reviewed**: _______________________
