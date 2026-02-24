# Performance Testing Guide
# 性能测试指南

## Overview
## 概览

Performance tests ensure the WebUI API meets response time and throughput requirements under various load conditions.
性能测试确保 WebUI API 在各种负载条件下满足响应时间和吞吐量要求。

## Tools
## 工具

- **k6**: Load testing tool for API performance
- **Lighthouse**: Frontend performance auditing
- **dotnet-counters**: .NET performance monitoring

## Prerequisites
## 前提条件

### Install k6
### 安装 k6

**Windows (Chocolatey)**:
```powershell
choco install k6
```

**macOS (Homebrew)**:
```bash
brew install k6
```

**Linux**:
```bash
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys C5AD17C747E3415A3642D57D77C6C491D6AC1D69
echo "deb https://dl.k6.io/deb stable main" | sudo tee /etc/apt/sources.list.d/k6.list
sudo apt-get update
sudo apt-get install k6
```

### Install Lighthouse
### 安装 Lighthouse

```bash
npm install -g lighthouse
```

## Running Performance Tests
## 运行性能测试

### 1. API Load Testing (k6)
### 1. API 负载测试 (k6)

Run basic load test:
运行基本负载测试:

```bash
k6 run api-load-test.js
```

Run with custom configuration:
使用自定义配置运行:

```bash
k6 run --vus 100 --duration 30s api-load-test.js
```

Run with environment variables:
使用环境变量运行:

```bash
k6 run -e BASE_URL=http://localhost:5000 api-load-test.js
```

Generate HTML report:
生成 HTML 报告:

```bash
k6 run --out html=results.html api-load-test.js
```

### 2. Frontend Performance (Lighthouse)
### 2. 前端性能 (Lighthouse)

Run Lighthouse audit:
运行 Lighthouse 审计:

```bash
lighthouse http://localhost:5173 --output html --output-path ./lighthouse-report.html
```

Run with specific categories:
运行特定类别:

```bash
lighthouse http://localhost:5173 --only-categories=performance,accessibility --output json --output-path ./results.json
```

### 3. .NET Performance Monitoring
### 3. .NET 性能监控

Monitor running WebUI.API:
监控运行的 WebUI.API:

```powershell
dotnet-counters monitor -n WebUI.API --counters System.Runtime,Microsoft.AspNetCore.Hosting
```

## Performance Targets
## 性能目标

### API Response Times
### API 响应时间

| Endpoint | P50 | P95 | P99 |
|----------|-----|-----|-----|
| GET `/api/v1/market/summary` | < 100ms | < 300ms | < 500ms |
| GET `/api/v1/orders` | < 150ms | < 400ms | < 600ms |
| GET `/api/v1/positions` | < 150ms | < 400ms | < 600ms |
| POST `/api/v1/orders` | < 300ms | < 800ms | < 1000ms |
| GET `/api/v1/strategies` | < 200ms | < 500ms | < 800ms |

### Throughput
### 吞吐量

- **Target**: Handle 100 concurrent orders per second
- **目标**: 处理每秒 100 个并发订单

### Error Rate
### 错误率

- **Target**: < 1% error rate under normal load
- **目标**: 正常负载下错误率 < 1%

### Frontend Performance
### 前端性能

- **First Contentful Paint (FCP)**: < 1.5s
- **Largest Contentful Paint (LCP)**: < 2.5s
- **Time to Interactive (TTI)**: < 3.5s
- **Cumulative Layout Shift (CLS)**: < 0.1

## Load Testing Scenarios
## 负载测试场景

### Scenario 1: Normal Load
### 场景 1: 正常负载

Simulates typical usage with 10-50 concurrent users.
模拟 10-50 个并发用户的典型使用。

```bash
k6 run --stage 2m:10,5m:10,2m:50,5m:50,5m:0 api-load-test.js
```

### Scenario 2: Peak Load
### 场景 2: 峰值负载

Simulates peak trading hours with 100-200 concurrent users.
模拟 100-200 个并发用户的交易高峰时段。

```bash
k6 run --stage 5m:100,10m:100,5m:200,10m:200,5m:0 api-load-test.js
```

### Scenario 3: Stress Test
### 场景 3: 压力测试

Tests system limits by gradually increasing load.
通过逐渐增加负载测试系统极限。

```bash
k6 run --stage 2m:50,5m:100,5m:200,5m:300,5m:0 api-load-test.js
```

### Scenario 4: Spike Test
### 场景 4: 峰值测试

Tests system response to sudden load increase.
测试系统对突然负载增加的响应。

```bash
k6 run --stage 1m:10,30s:500,1m:10,5m:0 api-load-test.js
```

## Analyzing Results
## 分析结果

### Key Metrics to Monitor
### 监控的关键指标

1. **Response Time**: P50, P95, P99 percentiles
   **响应时间**: P50, P95, P99 百分位

2. **Throughput**: Requests per second
   **吞吐量**: 每秒请求数

3. **Error Rate**: Failed requests / Total requests
   **错误率**: 失败请求 / 总请求

4. **CPU Usage**: Should stay < 80% under peak load
   **CPU 使用率**: 峰值负载下应保持 < 80%

5. **Memory Usage**: Should be stable, no leaks
   **内存使用**: 应保持稳定，无泄漏

6. **Database Connections**: Monitor connection pool usage
   **数据库连接**: 监控连接池使用

## Performance Optimization Tips
## 性能优化建议

1. **Enable Response Caching**: Cache frequently accessed data
   **启用响应缓存**: 缓存频繁访问的数据

2. **Database Indexing**: Add indexes on frequently queried columns
   **数据库索引**: 在频繁查询的列上添加索引

3. **Connection Pooling**: Optimize database connection pool settings
   **连接池**: 优化数据库连接池设置

4. **API Response Compression**: Enable Gzip compression
   **API 响应压缩**: 启用 Gzip 压缩

5. **Static Asset Caching**: Configure browser caching for static files
   **静态资源缓存**: 为静态文件配置浏览器缓存

6. **SignalR Scaling**: Use Redis backplane for multiple servers
   **SignalR 扩展**: 为多服务器使用 Redis 后台板

## Continuous Performance Monitoring
## 持续性能监控

Integrate performance tests into CI/CD:
将性能测试集成到 CI/CD:

```yaml
- name: Run Performance Tests
  run: |
    k6 run --summary-export=summary.json api-load-test.js
    # Fail if P95 > 500ms or error rate > 1%
```

## Troubleshooting
## 故障排查

### High Response Times
### 高响应时间

1. Check database query performance
2. Review API endpoint implementation
3. Check network latency
4. Verify server resources (CPU, Memory)

### High Error Rates
### 高错误率

1. Check server logs for exceptions
2. Verify database connection availability
3. Check rate limiting configuration
4. Review concurrent request handling

### Memory Leaks
### 内存泄漏

1. Use dotnet-counters to monitor GC
2. Check for unmanaged resource disposal
3. Review event handler subscriptions
4. Check SignalR connection cleanup

## Reports
## 报告

Performance test results should be archived for comparison:
性能测试结果应归档以便比较:

```
tests/performance/results/
  ├── 2024-01-15-baseline.json
  ├── 2024-02-01-after-optimization.json
  └── lighthouse-reports/
```

## References
## 参考

- [k6 Documentation](https://k6.io/docs/)
- [Lighthouse Documentation](https://developers.google.com/web/tools/lighthouse)
- [ASP.NET Core Performance Best Practices](https://docs.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)
