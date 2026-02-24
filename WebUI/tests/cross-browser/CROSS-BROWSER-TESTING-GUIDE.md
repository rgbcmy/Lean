# Cross-Browser Testing Guide
# 跨浏览器测试指南

## Overview
## 概览

Cross-browser testing ensures the WebUI works correctly across different browsers and browser versions.
跨浏览器测试确保 WebUI 在不同浏览器和浏览器版本上正常工作。

## Supported Browsers
## 支持的浏览器

### Primary Support (Full Testing)
### 主要支持 (完整测试)

| Browser | Minimum Version | Notes |
|---------|----------------|-------|
| **Chrome** | Latest - 2 | Primary development browser |
| **Firefox** | Latest - 2 | Full feature support |
| **Edge** | Latest - 2 | Chromium-based |
| **Safari** | 15+ | macOS and iOS |

### Secondary Support (Basic Testing)
### 次要支持 (基础测试)

| Browser | Minimum Version | Notes |
|---------|----------------|-------|
| **Opera** | Latest | Chromium-based |
| **Brave** | Latest | Chromium-based |
| **Samsung Internet** | Latest | Mobile only |

### Not Supported
### 不支持

- Internet Explorer (all versions) - Deprecated
- Opera Mini - Limited JavaScript support
- UC Browser - Security and compatibility concerns

## Testing Tools
## 测试工具

### Automated Testing
### 自动化测试

1. **Playwright** (Primary)
   - Already configured in E2E tests
   - Tests Chrome, Firefox, Safari (WebKit)
   - Includes mobile browsers

2. **BrowserStack** (Optional)
   - Real device testing
   - Legacy browser testing
   - Parallel execution

### Manual Testing
### 手动测试

1. **Browser DevTools**
   - Chrome DevTools
   - Firefox Developer Tools
   - Safari Web Inspector

2. **Browser Extensions**
   - React Developer Tools
   - Redux DevTools
   - Accessibility Inspector

## Running Cross-Browser Tests
## 运行跨浏览器测试

### Using Playwright
### 使用 Playwright

```bash
cd WebUI/tests/e2e

# Run on all browsers
npm run test:e2e

# Run on specific browser
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit

# Run on all mobile browsers
npx playwright test --project="Mobile Chrome"
npx playwright test --project="Mobile Safari"

# Generate HTML report
npx playwright show-report
```

### Manual Testing Checklist
### 手动测试清单

For each supported browser:
对于每个支持的浏览器:

#### 1. Initial Load
#### 1. 初始加载

- [ ] Application loads without errors
- [ ] Correct page title displayed
- [ ] Favicon loads correctly
- [ ] No console errors
- [ ] CSS styles applied correctly

#### 2. Authentication
#### 2. 身份验证

- [ ] Login page displays correctly
- [ ] Login form validation works
- [ ] Successful login redirects to dashboard
- [ ] Invalid credentials show error message
- [ ] Logout works correctly

#### 3. Navigation
#### 3. 导航

- [ ] All menu items clickable
- [ ] URL routing works correctly
- [ ] Back/forward browser buttons work
- [ ] Breadcrumb navigation works
- [ ] Mobile hamburger menu works (mobile browsers)

#### 4. Trading Features
#### 4. 交易功能

- [ ] Stock search works
- [ ] Order form displays correctly
- [ ] Order submission works
- [ ] Real-time price updates work
- [ ] Order list displays correctly

#### 5. Portfolio Features
#### 5. 投资组合功能

- [ ] Positions table displays correctly
- [ ] Profit/loss calculations correct
- [ ] Charts render correctly
- [ ] Export functionality works

#### 6. Real-time Features
#### 6. 实时功能

- [ ] SignalR connection establishes
- [ ] Real-time updates received
- [ ] WebSocket connection stable
- [ ] No memory leaks after prolonged use

#### 7. Responsive Design
#### 7. 响应式设计

- [ ] Desktop layout (1920x1080)
- [ ] Laptop layout (1366x768)
- [ ] Tablet layout (768x1024)
- [ ] Mobile layout (375x667)

#### 8. Forms and Input
#### 8. 表单和输入

- [ ] All form fields accept input
- [ ] Input validation works
- [ ] Dropdowns work correctly
- [ ] Checkboxes/radio buttons work
- [ ] Date pickers work

#### 9. Accessibility
#### 9. 无障碍性

- [ ] Keyboard navigation works
- [ ] Tab order is logical
- [ ] Focus indicators visible
- [ ] Screen reader compatible
- [ ] ARIA labels present

#### 10. Performance
#### 10. 性能

- [ ] Page load < 3 seconds
- [ ] Smooth scrolling
- [ ] No layout shift
- [ ] Animations smooth
- [ ] No lag in interactions

## Browser-Specific Issues
## 浏览器特定问题

### Chrome/Edge (Chromium)
### Chrome/Edge (Chromium)

**Known Issues:**
- Generally most stable
- Best DevTools support

**Testing Focus:**
- Service Worker caching
- PWA functionality
- Chrome-specific APIs

### Firefox
### Firefox

**Known Issues:**
- Slightly different flexbox behavior
- Date input styling differences
- WebSocket reconnection timing

**Testing Focus:**
- CSS Grid layout
- Flexbox edge cases
- Custom scrollbars

### Safari
### Safari

**Known Issues:**
- Different date/time input rendering
- Stricter CORS policies
- iOS Safari viewport height quirks
- No support for some newer CSS features

**Testing Focus:**
- Date/time pickers
- iOS Safari navigation bar
- Touch interactions
- localStorage/IndexedDB

**Common Fixes:**
```css
/* Safari-specific fix for viewport height */
.full-height {
  height: 100vh;
  height: -webkit-fill-available;
}

/* Safari date input styling */
input[type="date"]::-webkit-calendar-picker-indicator {
  opacity: 1;
}
```

### Mobile Browsers
### 移动浏览器

**Known Issues:**
- Touch event handling
- Virtual keyboard issues
- Fixed positioning quirks
- Different scrolling behavior

**Testing Focus:**
- Touch gestures
- Viewport resizing
- Orientation changes
- Pull-to-refresh conflicts

## Browser Feature Detection
## 浏览器功能检测

Use feature detection instead of browser detection:
使用功能检测而不是浏览器检测:

```typescript
// Good: Feature detection
if ('serviceWorker' in navigator) {
  // Use service worker
}

if ('WebSocket' in window) {
  // Use WebSocket
}

// Bad: Browser detection
if (navigator.userAgent.includes('Chrome')) {
  // Don't do this
}
```

## Polyfills and Fallbacks
## Polyfill 和回退

Configure Vite to include necessary polyfills:
配置 Vite 包含必要的 polyfill:

```javascript
// vite.config.ts
export default defineConfig({
  build: {
    target: ['es2015', 'edge88', 'firefox78', 'chrome87', 'safari14'],
    polyfillDynamicImport: true,
  },
});
```

## Testing Matrix
## 测试矩阵

| Feature | Chrome | Firefox | Edge | Safari Desktop | Safari iOS | Priority |
|---------|--------|---------|------|----------------|------------|----------|
| Login/Logout | ✅ | ✅ | ✅ | ✅ | ✅ | High |
| Stock Trading | ✅ | ✅ | ✅ | ✅ | ✅ | High |
| Real-time Updates | ✅ | ✅ | ✅ | ✅ | ✅ | High |
| Portfolio View | ✅ | ✅ | ✅ | ✅ | ✅ | High |
| Charts | ✅ | ✅ | ✅ | ⚠️ | ⚠️ | Medium |
| Export CSV | ✅ | ✅ | ✅ | ✅ | ⚠️ | Low |

✅ = Fully supported | ⚠️ = Partial support | ❌ = Not supported

## Automated Visual Regression Testing
## 自动化视觉回归测试

Using Playwright for visual testing:
使用 Playwright 进行视觉测试:

```typescript
// visual.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Visual regression tests', () => {
  test('login page looks correct', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveScreenshot('login-page.png');
  });

  test('dashboard looks correct', async ({ page, context }) => {
    // Login first
    await page.goto('/');
    await page.fill('[name="username"]', 'admin');
    await page.fill('[name="password"]', 'admin123');
    await page.click('button:has-text("登录")');
    await page.waitForURL('**/dashboard');
    
    // Take screenshot
    await expect(page).toHaveScreenshot('dashboard.png');
  });
});
```

## Browser-Specific Testing Scripts
## 浏览器特定测试脚本

```json
// package.json
{
  "scripts": {
    "test:chrome": "playwright test --project=chromium",
    "test:firefox": "playwright test --project=firefox",
    "test:safari": "playwright test --project=webkit",
    "test:mobile": "playwright test --project='Mobile Chrome' --project='Mobile Safari'",
    "test:all-browsers": "playwright test"
  }
}
```

## Debugging Browser-Specific Issues
## 调试浏览器特定问题

### Chrome DevTools
### Chrome 开发工具

```bash
# Open with DevTools
playwright test --project=chromium --debug
```

### Firefox DevTools
### Firefox 开发工具

```bash
# Open with Firefox DevTools
playwright test --project=firefox --debug
```

### Safari Web Inspector
### Safari Web 检查器

```bash
# Open with Safari
playwright test --project=webkit --debug
```

## CI/CD Integration
## CI/CD 集成

```yaml
# .github/workflows/cross-browser-tests.yml
name: Cross Browser Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        browser: [chromium, firefox, webkit]
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install dependencies
        run: |
          cd WebUI/tests/e2e
          npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install --with-deps ${{ matrix.browser }}
      
      - name: Run tests
        run: |
          cd WebUI/tests/e2e
          npx playwright test --project=${{ matrix.browser }}
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@v3
        with:
          name: playwright-results-${{ matrix.browser }}
          path: WebUI/tests/e2e/test-results/
```

## Browser Support Policy
## 浏览器支持策略

- **Current version + 2 previous versions**: Full support
- **当前版本 + 前2个版本**: 完全支持

- **Older versions**: Best effort, no guarantees
- **更旧版本**: 尽力而为，不保证

- **Update cycle**: Review and update quarterly
- **更新周期**: 每季度审查和更新

## Reporting Browser Issues
## 报告浏览器问题

When reporting browser-specific issues, include:
报告浏览器特定问题时，包括:

1. Browser name and version
2. Operating system
3. Steps to reproduce
4. Screenshots/videos
5. Console errors
6. Network logs (if relevant)

## References
## 参考

- [Can I Use](https://caniuse.com/) - Browser feature support
- [MDN Web Docs](https://developer.mozilla.org/) - Web standards documentation
- [Playwright Documentation](https://playwright.dev/)
- [BrowserStack](https://www.browserstack.com/) - Real device testing

---

Last Updated: 2024-02-16
最后更新: 2024-02-16
