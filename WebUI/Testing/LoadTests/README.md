# Performance Load Testing

This directory contains performance and load testing scripts for the Personal Trading WebUI.

## Overview

The load testing suite validates that the WebUI can handle the required throughput and maintains acceptable response times under high load conditions.

## Test Scripts

### PerformanceLoadTest.ps1

Main performance load test script that simulates high-volume API requests.

**Features**:
- Configurable target load (requests per second)
- Multiple test scenarios (backtest creation, queries, mixed operations)
- Real-time progress tracking
- Performance threshold validation
- JSON result export
- Detailed statistics reporting

## Prerequisites

- PowerShell 5.1 or later (Windows) or PowerShell Core 7+ (cross-platform)
- WebUI API running and accessible
- Valid authentication token (for authenticated endpoints)

## Quick Start

### Basic Test Run

```powershell
# Run with default settings (100 req/s for 60 seconds)
.\PerformanceLoadTest.ps1 -BaseUrl "https://localhost:5001"
```

### Custom Configuration

```powershell
# Custom load and duration
.\PerformanceLoadTest.ps1 `
    -BaseUrl "https://localhost:5001" `
    -OrdersPerSecond 150 `
    -DurationSeconds 120
```

### Authenticated Tests

```powershell
# With authentication token
$token = "your-jwt-token-here"

.\PerformanceLoadTest.ps1 `
    -BaseUrl "https://localhost:5001" `
    -OrdersPerSecond 100 `
    -DurationSeconds 60 `
    -AuthToken $token
```

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `BaseUrl` | string | `https://localhost:5001` | Base URL of the WebUI API |
| `OrdersPerSecond` | int | `100` | Target number of requests per second |
| `DurationSeconds` | int | `60` | Duration of each test scenario in seconds |
| `AuthToken` | string | *(empty)* | JWT authentication token |

## Test Scenarios

The load test runs three scenarios sequentially:

### 1. Backtest Creation Test
- Creates new backtest configurations
- Tests POST /api/v1/backtests endpoint
- Validates database write performance

### 2. Order Query Test
- Queries order history
- Tests GET /api/v1/orders endpoint
- Validates database read performance

### 3. Mixed Operations Test
- Alternates between different API endpoints:
  - GET /positions
  - GET /strategies
  - GET /backtests
  - GET /risk/config
  - GET /market/quote/:symbol
- Tests realistic usage patterns

## Performance Thresholds

The test validates against these thresholds:

| Metric | Threshold | Pass Criteria |
|--------|-----------|---------------|
| Average Response Time | 500ms | Must be ≤ 500ms |
| Maximum Response Time | 2000ms | Must be ≤ 2000ms |
| Success Rate | 95% | Must be ≥ 95% |
| Throughput | 90 req/s | Must be ≥ 90% of target (100 req/s) |

## Output

### Console Output

The script provides real-time feedback:

```
========================================
WebUI Performance Load Test
Module 29.7 Verification
========================================

Test Configuration:
  Base URL: https://localhost:5001
  Target Rate: 100 orders/second
  Duration: 60 seconds
  Total Requests: 6000
  Interval: 10 ms

[Test 1] Backtest Creation Load Test
Creating 100 backtest configurations per second...
  Progress: 100/6000 requests sent (100 successful)
  Progress: 200/6000 requests sent (200 successful)
  ...

Backtest Creation Test Complete
  Success: 5980
  Failures: 20
  Avg Response Time: 342.15 ms
  Min Response Time: 125.30 ms
  Max Response Time: 1854.20 ms

[Test 2] Order Query Load Test
...

========================================
Load Test Summary
========================================

Overall Statistics:
  Total Requests: 18000
  Successful: 17820
  Failed: 180
  Success Rate: 99.00%

Response Time Statistics:
  Average: 385.42 ms
  Minimum: 98.50 ms
  Maximum: 1923.10 ms

Performance Thresholds:
  ✓ Average response time <= 500ms: PASS
  ✓ Maximum response time <= 2000ms: PASS
  ✓ Success rate >= 95%: PASS

Actual Throughput: 98.33 requests/second
✓ Throughput target achieved (>= 90% of target)

Test completed successfully!
```

### JSON Output

Results are automatically exported to a JSON file:

**Filename**: `LoadTest_YYYYMMDD_HHMMSS.json`

**Example**:
```json
{
  "TestDate": "2026-02-19T10:30:00.000Z",
  "Configuration": {
    "BaseUrl": "https://localhost:5001",
    "OrdersPerSecond": 100,
    "DurationSeconds": 60,
    "TotalRequests": 18000
  },
  "BacktestCreation": {
    "SuccessCount": 5980,
    "FailureCount": 20,
    "AvgResponseTime": 342.15
  },
  "OrderQuery": {
    "SuccessCount": 5950,
    "FailureCount": 50,
    "AvgResponseTime": 298.30
  },
  "MixedOperations": {
    "SuccessCount": 5890,
    "FailureCount": 110,
    "AvgResponseTime": 515.81
  },
  "Overall": {
    "TotalSuccess": 17820,
    "TotalFailure": 180,
    "SuccessRate": 99.00,
    "AvgResponseTime": 385.42,
    "ActualThroughput": 98.33
  }
}
```

## Interpreting Results

### Success Criteria

✅ **PASS** - All thresholds met, system performs as expected

Example:
- Average response time: 385ms (< 500ms ✓)
- Max response time: 1923ms (< 2000ms ✓)
- Success rate: 99% (> 95% ✓)
- Throughput: 98.33 req/s (> 90 req/s ✓)

⚠️ **WARNING** - Some metrics slightly below threshold

Example:
- Success rate: 94% (just below 95% threshold)
- Action: Investigate failures, may need optimization

❌ **FAIL** - Critical thresholds not met

Example:
- Average response time: 847ms (exceeds 500ms)
- Action: Performance optimization required before production

### Common Issues and Solutions

#### High Response Times

**Symptoms**:
- Average response time > 500ms
- Maximum response time > 2000ms

**Possible Causes**:
- Database query not optimized
- Missing database indexes
- Network latency
- CPU/memory constraints

**Solutions**:
1. Review slow query logs
2. Add database indexes
3. Enable query caching
4. Scale up server resources
5. Enable response compression

#### Low Success Rate

**Symptoms**:
- Success rate < 95%
- Many timeout errors

**Possible Causes**:
- Rate limiting triggered
- Database connection pool exhausted
- Server overload
- Network issues

**Solutions**:
1. Increase rate limit thresholds
2. Increase database connection pool size
3. Scale horizontally (add more servers)
4. Optimize resource allocation
5. Review error logs for specific failures

#### Low Throughput

**Symptoms**:
- Actual throughput < 90% of target

**Possible Causes**:
- Request throttling
- Network bandwidth limitations
- Test script performance issues

**Solutions**:
1. Verify network connectivity
2. Run test from machine closer to server
3. Increase test script concurrency
4. Check server resource utilization

## Best Practices

### Before Running Tests

1. **Ensure clean test environment**
   ```powershell
   # Clear test data
   Remove-Item -Path "LoadTest_*.json"
   ```

2. **Warm up the application**
   ```powershell
   # Make a few manual requests first
   Invoke-WebRequest -Uri "https://localhost:5001/health"
   ```

3. **Monitor server resources**
   - Check CPU, memory, disk usage
   - Ensure sufficient capacity

### During Tests

1. **Don't run other intensive processes**
2. **Monitor application logs** for errors
3. **Watch database performance metrics**
4. **Ensure stable network connection**

### After Tests

1. **Review results JSON file**
2. **Compare against baseline metrics**
3. **Investigate any failures**
4. **Document performance characteristics**
5. **Archive results for trend analysis**

## Advanced Usage

### Custom Test Scenarios

You can modify the script to add custom test scenarios:

```powershell
# Add a new test scenario
$customOperations = @(
    @{ Method = "GET"; Endpoint = "/custom/endpoint" },
    @{ Method = "POST"; Endpoint = "/another/endpoint" }
)
```

### Continuous Performance Testing

Integrate into CI/CD pipeline:

```yaml
# GitHub Actions example
- name: Run Performance Tests
  run: |
    $results = .\WebUI\Testing\LoadTests\PerformanceLoadTest.ps1 `
      -BaseUrl "https://staging.example.com" `
      -OrdersPerSecond 100 `
      -DurationSeconds 60
    
    # Parse results and fail if thresholds not met
    if ($results.SuccessRate -lt 95) {
      exit 1
    }
```

### Stress Testing

Test system limits:

```powershell
# Gradually increase load to find breaking point
foreach ($rate in 50, 100, 150, 200, 250) {
    Write-Host "Testing at $rate req/s"
    .\PerformanceLoadTest.ps1 -OrdersPerSecond $rate -DurationSeconds 30
    Start-Sleep -Seconds 60  # Cool down between tests
}
```

## Troubleshooting

### SSL Certificate Errors

If using self-signed certificates:

```powershell
# Temporarily accept all certificates (development only!)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}
```

### Authentication Issues

1. Obtain a valid JWT token:
   ```powershell
   $loginBody = @{
       username = "testuser"
       password = "testpassword"
   } | ConvertTo-Json
   
   $response = Invoke-WebRequest `
       -Uri "https://localhost:5001/api/v1/auth/login" `
       -Method POST `
       -Body $loginBody `
       -ContentType "application/json"
   
   $token = ($response.Content | ConvertFrom-Json).token
   ```

2. Use the token in load tests:
   ```powershell
   .\PerformanceLoadTest.ps1 -AuthToken $token
   ```

### Timeout Errors

Increase timeout if needed:

```powershell
# Edit the script to increase timeout
-TimeoutSec 30  # Instead of 10
```

## Support

For issues or questions:
1. Check module logs in WebUI logs directory
2. Review [WebUI Developer Guide](../../docs/WebUI开发者完整指南.md)
3. See [Module 29 Summary](../MODULE_29_SUMMARY.md) for verification details
4. File an issue in the project repository

## References

- [Module 29 Verification Tasks](../../openspec/changes/personal-trading-webui/tasks.md)
- [Performance Requirements](../../openspec/changes/personal-trading-webui/design.md)
- [API Documentation](../WebUI.API/README.md)

---

**Version**: 1.0  
**Last Updated**: 2026-02-19  
**Module**: 29.7 - Performance Load Testing
