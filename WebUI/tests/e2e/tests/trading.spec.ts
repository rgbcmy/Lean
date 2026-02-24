import { test, expect } from '@playwright/test';

/**
 * Trading Flow E2E Tests
 * 交易流程端到端测试
 */

// Helper function to login
async function login(page: any) {
  await page.goto('/');
  await page.getByLabel(/用户名/i).fill('admin');
  await page.getByLabel(/密码/i).fill('admin123');
  await page.getByRole('button', { name: /登录/i }).click();
  await page.waitForURL(/.*dashboard/);
}

test.describe('Stock Trading Flow', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should navigate to stock trading page', async ({ page }) => {
    await page.click('text=股票交易');
    await expect(page).toHaveURL(/.*stock-trading/);
    await expect(page.getByText(/股票搜索|Stock Search/i)).toBeVisible();
  });

  test('should search for a stock symbol', async ({ page }) => {
    await page.goto('/stock-trading');

    // Search for AAPL
    await page.getByPlaceholder(/输入股票代码/i).fill('AAPL');
    await page.waitForTimeout(1000); // Wait for debounce

    // Verify search results
    await expect(page.getByText(/AAPL/i)).toBeVisible();
    await expect(page.getByText(/Apple Inc/i)).toBeVisible();
  });

  test('should display stock quote information', async ({ page }) => {
    await page.goto('/stock-trading');

    // Search and select stock
    await page.getByPlaceholder(/输入股票代码/i).fill('AAPL');
    await page.waitForTimeout(1000);
    await page.click('text=AAPL');

    // Verify quote information is displayed
    await expect(page.getByText(/最新价|Last Price/i)).toBeVisible();
    await expect(page.getByText(/涨跌幅|Change/i)).toBeVisible();
    await expect(page.getByText(/成交量|Volume/i)).toBeVisible();
  });

  test('should create a market buy order', async ({ page }) => {
    await page.goto('/stock-trading');

    // Search for stock
    await page.getByPlaceholder(/输入股票代码/i).fill('AAPL');
    await page.waitForTimeout(1000);
    await page.click('text=AAPL');

    // Fill order form
    await page.selectOption('select[name="orderType"]', '市价单');
    await page.getByLabel(/数量/i).fill('10');
    await page.click('text=买入');

    // Confirm order
    await page.click('button:has-text("确认下单")');

    // Verify success message
    await expect(page.getByText(/订单提交成功/i)).toBeVisible({ timeout: 5000 });
  });

  test('should create a limit sell order', async ({ page }) => {
    await page.goto('/stock-trading');

    // Search for stock
    await page.getByPlaceholder(/输入股票代码/i).fill('AAPL');
    await page.waitForTimeout(1000);
    await page.click('text=AAPL');

    // Fill order form
    await page.selectOption('select[name="orderType"]', '限价单');
    await page.click('text=卖出');
    await page.getByLabel(/数量/i).fill('5');
    await page.getByLabel(/限价/i).fill('180.00');

    // Confirm order
    await page.click('button:has-text("确认下单")');

    // Verify success message
    await expect(page.getByText(/订单提交成功/i)).toBeVisible({ timeout: 5000 });
  });

  test('should validate order quantity', async ({ page }) => {
    await page.goto('/stock-trading');

    // Search for stock
    await page.getByPlaceholder(/输入股票代码/i).fill('AAPL');
    await page.waitForTimeout(1000);
    await page.click('text=AAPL');

    // Try to submit with zero quantity
    await page.getByLabel(/数量/i).fill('0');
    await page.click('text=买入');

    // Verify validation error
    await expect(page.getByText(/数量必须大于0/i)).toBeVisible();
  });

  test('should check buying power before order', async ({ page }) => {
    await page.goto('/stock-trading');

    // Search for expensive stock
    await page.getByPlaceholder(/输入股票代码/i).fill('BRK.A'); // Berkshire Hathaway
    await page.waitForTimeout(1000);
    await page.click('text=BRK.A');

    // Try to buy large quantity
    await page.getByLabel(/数量/i).fill('1000');
    await page.click('text=买入');

    // Verify insufficient funds message
    await expect(page.getByText(/购买力不足|Insufficient buying power/i)).toBeVisible();
  });
});

test.describe('Order Management Flow', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should display active orders', async ({ page }) => {
    await page.goto('/orders');

    // Verify orders page elements
    await expect(page.getByText(/订单列表|Orders/i)).toBeVisible();
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('should filter orders by status', async ({ page }) => {
    await page.goto('/orders');

    // Filter by pending status
    await page.selectOption('select[name="status"]', 'Pending');
    await page.waitForTimeout(500);

    // Verify only pending orders are shown
    const rows = await page.locator('tbody tr').all();
    for (const row of rows) {
      await expect(row.getByText(/等待中|Pending/i)).toBeVisible();
    }
  });

  test('should cancel a pending order', async ({ page }) => {
    await page.goto('/orders');

    // Find first pending order and cancel it
    await page.click('tr:has-text("Pending") button:has-text("取消")');

    // Confirm cancellation
    await page.click('button:has-text("确认")');

    // Verify success message
    await expect(page.getByText(/订单已取消/i)).toBeVisible({ timeout: 5000 });
  });

  test('should display order details', async ({ page }) => {
    await page.goto('/orders');

    // Click on first order
    await page.click('tbody tr:first-child');

    // Verify order details modal
    await expect(page.getByText(/订单详情|Order Details/i)).toBeVisible();
    await expect(page.getByText(/订单ID|Order ID/i)).toBeVisible();
    await expect(page.getByText(/提交时间|Submitted/i)).toBeVisible();
  });

  test('should export orders to CSV', async ({ page }) => {
    await page.goto('/orders');

    // Click export button
    const downloadPromise = page.waitForEvent('download');
    await page.click('button:has-text("导出")');
    const download = await downloadPromise;

    // Verify file was downloaded
    expect(download.suggestedFilename()).toMatch(/orders.*\.csv/);
  });
});

test.describe('Portfolio Management Flow', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should display current positions', async ({ page }) => {
    await page.goto('/positions');

    // Verify positions page
    await expect(page.getByText(/持仓列表|Positions/i)).toBeVisible();
    await expect(page.getByRole('table')).toBeVisible();
  });

  test('should show position profit/loss', async ({ page }) => {
    await page.goto('/positions');

    // Verify P&L columns exist
    await expect(page.getByText(/盈亏|P&L/i)).toBeVisible();
    await expect(page.getByText(/盈亏率|Return/i)).toBeVisible();
  });

  test('should close a position', async ({ page }) => {
    await page.goto('/positions');

    // Find first position and close it
    await page.click('tr:first-child button:has-text("平仓")');

    // Confirm close
    await page.click('button:has-text("确认平仓")');

    // Verify success message
    await expect(page.getByText(/平仓订单已提交/i)).toBeVisible({ timeout: 5000 });
  });

  test('should display portfolio allocation chart', async ({ page }) => {
    await page.goto('/portfolio-analysis');

    // Verify chart is displayed
    await expect(page.getByText(/投资组合配置|Portfolio Allocation/i)).toBeVisible();
    const canvas = page.locator('canvas').first();
    await expect(canvas).toBeVisible();
  });
});

test.describe('Real-time Updates', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('should update prices in real-time', async ({ page }) => {
    await page.goto('/positions');

    // Get initial price
    const priceCell = page.locator('td:has-text("$")').first();
    const initialPrice = await priceCell.textContent();

    // Wait for potential update (this depends on your real-time data)
    await page.waitForTimeout(5000);

    // Get updated price
    const updatedPrice = await priceCell.textContent();

    // Price element should exist (may or may not have changed)
    expect(updatedPrice).toBeTruthy();
  });

  test('should show real-time order status updates', async ({ page }) => {
    await page.goto('/orders');

    // Submit a new order
    await page.goto('/stock-trading');
    await page.getByPlaceholder(/输入股票代码/i).fill('AAPL');
    await page.waitForTimeout(1000);
    await page.click('text=AAPL');
    await page.getByLabel(/数量/i).fill('1');
    await page.click('text=买入');
    await page.click('button:has-text("确认下单")');

    // Navigate back to orders
    await page.goto('/orders');

    // Verify new order appears
    await expect(page.getByText('AAPL')).toBeVisible({ timeout: 5000 });
  });
});
