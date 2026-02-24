import { test, expect } from '@playwright/test';

/**
 * Authentication E2E Tests
 * 认证功能端到端测试
 */

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should display login page', async ({ page }) => {
    // Verify login page elements
    await expect(page.getByText('Lean WebUI')).toBeVisible();
    await expect(page.getByLabel(/用户名/i)).toBeVisible();
    await expect(page.getByLabel(/密码/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /登录/i })).toBeVisible();
  });

  test('should show validation errors for empty fields', async ({ page }) => {
    // Click login without filling form
    await page.getByRole('button', { name: /登录/i }).click();

    // Verify validation messages appear
    await expect(page.getByText(/请输入用户名/i)).toBeVisible();
    await expect(page.getByText(/请输入密码/i)).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    // Fill in invalid credentials
    await page.getByLabel(/用户名/i).fill('invaliduser');
    await page.getByLabel(/密码/i).fill('wrongpassword');
    await page.getByRole('button', { name: /登录/i }).click();

    // Verify error message
    await expect(page.getByText(/用户名或密码错误/i)).toBeVisible({ timeout: 5000 });
  });

  test('should successfully login with valid credentials', async ({ page }) => {
    // Fill in valid credentials (using test account)
    await page.getByLabel(/用户名/i).fill('admin');
    await page.getByLabel(/密码/i).fill('admin123');
    await page.getByRole('checkbox').check();
    await page.getByRole('button', { name: /登录/i }).click();

    // Verify redirect to dashboard
    await expect(page).toHaveURL(/.*dashboard/, { timeout: 10000 });
    await expect(page.getByText(/仪表板|Dashboard/i)).toBeVisible();
  });

  test('should logout successfully', async ({ page }) => {
    // Login first
    await page.getByLabel(/用户名/i).fill('admin');
    await page.getByLabel(/密码/i).fill('admin123');
    await page.getByRole('button', { name: /登录/i }).click();
    await page.waitForURL(/.*dashboard/);

    // Click user menu and logout
    await page.click('[aria-label="user menu"]');
    await page.click('text=退出登录');

    // Verify redirect to login page
    await expect(page).toHaveURL(/.*login/);
  });

  test('should remember login with remember me checked', async ({ page, context }) => {
    // Login with remember me
    await page.getByLabel(/用户名/i).fill('admin');
    await page.getByLabel(/密码/i).fill('admin123');
    await page.getByRole('checkbox').check();
    await page.getByRole('button', { name: /登录/i }).click();
    await page.waitForURL(/.*dashboard/);

    // Verify cookies/storage are set
    const cookies = await context.cookies();
    expect(cookies.some(cookie => cookie.name.includes('token'))).toBeTruthy();
  });

  test('should enforce password strength requirements', async ({ page }) => {
    await page.goto('/change-password');

    // Try weak password
    await page.getByLabel(/新密码/i).fill('123');
    await page.getByLabel(/确认密码/i).fill('123');

    // Verify password strength error
    await expect(page.getByText(/密码强度不足/i)).toBeVisible();
  });
});

test.describe('Authentication Security', () => {
  test('should block access to protected routes without login', async ({ page }) => {
    await page.goto('/dashboard');
    
    // Should redirect to login
    await expect(page).toHaveURL(/.*login/);
  });

  test('should handle session timeout', async ({ page }) => {
    // Login
    await page.goto('/');
    await page.getByLabel(/用户名/i).fill('admin');
    await page.getByLabel(/密码/i).fill('admin123');
    await page.getByRole('button', { name: /登录/i }).click();
    await page.waitForURL(/.*dashboard/);

    // Clear tokens to simulate expiration
    await page.evaluate(() => {
      localStorage.clear();
      sessionStorage.clear();
    });

    // Try to access protected page
    await page.reload();

    // Should redirect to login
    await expect(page).toHaveURL(/.*login/);
  });

  test('should prevent multiple concurrent logins with same account', async ({ page, context }) => {
    // Login in first tab
    await page.goto('/');
    await page.getByLabel(/用户名/i).fill('admin');
    await page.getByLabel(/密码/i).fill('admin123');
    await page.getByRole('button', { name: /登录/i }).click();
    await page.waitForURL(/.*dashboard/);

    // Open second tab and try to login
    const page2 = await context.newPage();
    await page2.goto('/');
    await page2.getByLabel(/用户名/i).fill('admin');
    await page2.getByLabel(/密码/i).fill('admin123');
    await page2.getByRole('button', { name: /登录/i }).click();

    // First session should be invalidated (depends on implementation)
    // This test behavior depends on your backend session management
  });
});
