# E2E Testing Setup and Execution Guide
# E2E 测试设置和执行指南

## Installation
## 安装

```bash
cd WebUI/tests/e2e
npm install
npx playwright install
```

## Environment Setup
## 环境设置

Create a `.env` file in the `e2e` directory:
在 `e2e` 目录创建 `.env` 文件:

```env
BASE_URL=http://localhost:5173
API_URL=http://localhost:5000
TEST_USERNAME=admin
TEST_PASSWORD=admin123
```

## Running Tests
## 运行测试

### Run all tests
### 运行所有测试
```bash
npm run test:e2e
```

### Run tests in headed mode (see browser)
### 在有头模式运行（查看浏览器）
```bash
npm run test:e2e:headed
```

### Run tests with UI mode
### 使用 UI 模式运行
```bash
npm run test:e2e:ui
```

### Run specific test file
### 运行特定测试文件
```bash
npx playwright test tests/auth.spec.ts
```

### Run tests on specific browser
### 在特定浏览器运行
```bash
npx playwright test --project=chromium
npx playwright test --project=firefox
npx playwright test --project=webkit
```

### Debug mode
### 调试模式
```bash
npm run test:e2e:debug
```

## Test Reports
## 测试报告

View HTML report:
查看 HTML 报告:

```bash
npm run test:e2e:report
```

Reports are generated in `playwright-report/` directory.
报告生成在 `playwright-report/` 目录。

## Cross-Browser Testing
## 跨浏览器测试

Tests automatically run on:
测试自动运行在:

- Chromium (Chrome/Edge)
- Firefox
- WebKit (Safari)
- Mobile Chrome (Pixel 5)
- Mobile Safari (iPhone 12)
- iPad Pro

## Best Practices
## 最佳实践

1. **Test Independence**: Each test should be independent and not rely on other tests
   **测试独立性**: 每个测试应该独立，不依赖其他测试

2. **Clean State**: Use `beforeEach` to ensure clean state
   **清洁状态**: 使用 `beforeEach` 确保干净的状态

3. **Explicit Waits**: Use `waitFor` methods instead of fixed timeouts
   **显式等待**: 使用 `waitFor` 方法而不是固定超时

4. **Selectors**: Prefer accessible selectors (role, label) over CSS
   **选择器**: 优先使用可访问性选择器（role, label）而不是 CSS

5. **Assertions**: Use meaningful assertion messages
   **断言**: 使用有意义的断言消息

## CI/CD Integration
## CI/CD 集成

Add to your CI pipeline:
添加到 CI 流水线:

```yaml
- name: Run E2E Tests
  run: |
    cd WebUI/tests/e2e
    npm install
    npx playwright install --with-deps
    npm run test:e2e
```

## Troubleshooting
## 故障排查

### Tests fail to start
### 测试启动失败

- Ensure WebUI.API is running on correct port
- Check `.env` file configuration
- Verify database is accessible

### Flaky tests
### 不稳定的测试

- Increase timeout values in `playwright.config.ts`
- Add explicit waits for dynamic content
- Check network conditions

### Browser not found
### 浏览器未找到

```bash
npx playwright install
```

## Performance Testing
## 性能测试

E2E tests also measure page load times and interaction responsiveness.
E2E 测试也测量页面加载时间和交互响应性。

Check performance metrics in test reports.
在测试报告中检查性能指标。
