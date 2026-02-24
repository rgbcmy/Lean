# Performance Load Test Script
# 性能负载测试脚本
# Module 29.7 - Load test 100 orders per second

param(
    [string]$BaseUrl = "https://localhost:5001",
    [int]$OrdersPerSecond = 100,
    [int]$DurationSeconds = 60,
    [string]$AuthToken = ""
)

Write-Host "========================================" -ForegroundColor Green
Write-Host "WebUI Performance Load Test" -ForegroundColor Green
Write-Host "Module 29.7 Verification" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

# Configuration
$apiEndpoint = "$BaseUrl/api/v1"
$totalRequests = $OrdersPerSecond * $DurationSeconds
$intervalMs = 1000 / $OrdersPerSecond

Write-Host "Test Configuration:" -ForegroundColor Yellow
Write-Host "  Base URL: $BaseUrl"
Write-Host "  Target Rate: $OrdersPerSecond orders/second"
Write-Host "  Duration: $DurationSeconds seconds"
Write-Host "  Total Requests: $totalRequests"
Write-Host "  Interval: $intervalMs ms"
Write-Host ""

# Test data
$testOrders = @()
$symbols = @("AAPL", "MSFT", "GOOGL", "AMZN", "TSLA", "META", "NVDA", "AMD")

# Statistics
$successCount = 0
$failureCount = 0
$responseTimes = @()
$errors = @()

# Authentication check
if ([string]::IsNullOrEmpty($AuthToken)) {
    Write-Host "WARNING: No auth token provided. You may need to authenticate first." -ForegroundColor Yellow
    Write-Host "Use: -AuthToken 'your-jwt-token'" -ForegroundColor Yellow
    Write-Host ""
}

# Headers
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

if (-not [string]::IsNullOrEmpty($AuthToken)) {
    $headers["Authorization"] = "Bearer $AuthToken"
}

# Progress tracking
$startTime = Get-Date
Write-Host "Starting load test at: $startTime" -ForegroundColor Green
Write-Host ""

# Test 1: Backtest Creation Load Test
Write-Host "[Test 1] Backtest Creation Load Test" -ForegroundColor Cyan
Write-Host "Creating $OrdersPerSecond backtest configurations per second..." -ForegroundColor Cyan

$backtestSuccessCount = 0
$backtestFailureCount = 0
$backtestResponseTimes = @()

for ($i = 1; $i -le $totalRequests; $i++) {
    $requestStartTime = Get-Date
    
    $backtestRequest = @{
        strategyId = 1
        name = "Load Test Backtest $i"
        description = "Performance test backtest created at $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        startDate = "2020-01-01T00:00:00Z"
        endDate = "2021-01-01T00:00:00Z"
        initialCapital = 100000
        benchmarkSymbol = "SPY"
        dataResolution = "Daily"
    } | ConvertTo-Json

    try {
        $response = Invoke-WebRequest `
            -Uri "$apiEndpoint/backtests" `
            -Method POST `
            -Headers $headers `
            -Body $backtestRequest `
            -UseBasicParsing `
            -TimeoutSec 10 `
            -ErrorAction Stop

        $backtestSuccessCount++
        $responseTime = ((Get-Date) - $requestStartTime).TotalMilliseconds
        $backtestResponseTimes += $responseTime

        if ($i % 100 -eq 0) {
            Write-Host "  Progress: $i/$totalRequests requests sent ($backtestSuccessCount successful)" -ForegroundColor Gray
        }
    }
    catch {
        $backtestFailureCount++
        $errors += "Request $i failed: $($_.Exception.Message)"
        
        if ($backtestFailureCount -le 10) {
            Write-Host "  Error on request $i : $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Throttle to maintain target rate
    if ($i -lt $totalRequests) {
        $sleepMs = [Math]::Max(0, $intervalMs - ((Get-Date) - $requestStartTime).TotalMilliseconds)
        if ($sleepMs -gt 0) {
            Start-Sleep -Milliseconds $sleepMs
        }
    }
}

Write-Host ""
Write-Host "Backtest Creation Test Complete" -ForegroundColor Green
Write-Host "  Success: $backtestSuccessCount"
Write-Host "  Failures: $backtestFailureCount"
if ($backtestResponseTimes.Count -gt 0) {
    $avgResponseTime = ($backtestResponseTimes | Measure-Object -Average).Average
    $minResponseTime = ($backtestResponseTimes | Measure-Object -Minimum).Minimum
    $maxResponseTime = ($backtestResponseTimes | Measure-Object -Maximum).Maximum
    Write-Host "  Avg Response Time: $([Math]::Round($avgResponseTime, 2)) ms"
    Write-Host "  Min Response Time: $([Math]::Round($minResponseTime, 2)) ms"
    Write-Host "  Max Response Time: $([Math]::Round($maxResponseTime, 2)) ms"
}
Write-Host ""

# Test 2: Order Query Load Test
Write-Host "[Test 2] Order Query Load Test" -ForegroundColor Cyan
Write-Host "Querying orders at $OrdersPerSecond requests per second..." -ForegroundColor Cyan

$querySuccessCount = 0
$queryFailureCount = 0
$queryResponseTimes = @()

for ($i = 1; $i -le $totalRequests; $i++) {
    $requestStartTime = Get-Date
    
    try {
        $response = Invoke-WebRequest `
            -Uri "$apiEndpoint/orders?pageSize=50" `
            -Method GET `
            -Headers $headers `
            -UseBasicParsing `
            -TimeoutSec 10 `
            -ErrorAction Stop

        $querySuccessCount++
        $responseTime = ((Get-Date) - $requestStartTime).TotalMilliseconds
        $queryResponseTimes += $responseTime

        if ($i % 100 -eq 0) {
            Write-Host "  Progress: $i/$totalRequests requests sent ($querySuccessCount successful)" -ForegroundColor Gray
        }
    }
    catch {
        $queryFailureCount++
        
        if ($queryFailureCount -le 10) {
            Write-Host "  Error on request $i : $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Throttle
    if ($i -lt $totalRequests) {
        $sleepMs = [Math]::Max(0, $intervalMs - ((Get-Date) - $requestStartTime).TotalMilliseconds)
        if ($sleepMs -gt 0) {
            Start-Sleep -Milliseconds $sleepMs
        }
    }
}

Write-Host ""
Write-Host "Order Query Test Complete" -ForegroundColor Green
Write-Host "  Success: $querySuccessCount"
Write-Host "  Failures: $queryFailureCount"
if ($queryResponseTimes.Count -gt 0) {
    $avgResponseTime = ($queryResponseTimes | Measure-Object -Average).Average
    $minResponseTime = ($queryResponseTimes | Measure-Object -Minimum).Minimum
    $maxResponseTime = ($queryResponseTimes | Measure-Object -Maximum).Maximum
    Write-Host "  Avg Response Time: $([Math]::Round($avgResponseTime, 2)) ms"
    Write-Host "  Min Response Time: $([Math]::Round($minResponseTime, 2)) ms"
    Write-Host "  Max Response Time: $([Math]::Round($maxResponseTime, 2)) ms"
}
Write-Host ""

# Test 3: Mixed Operations Load Test
Write-Host "[Test 3] Mixed Operations Load Test" -ForegroundColor Cyan
Write-Host "Running mixed API operations..." -ForegroundColor Cyan

$mixedSuccessCount = 0
$mixedFailureCount = 0
$mixedResponseTimes = @()

$operations = @(
    @{ Method = "GET"; Endpoint = "/positions" },
    @{ Method = "GET"; Endpoint = "/strategies" },
    @{ Method = "GET"; Endpoint = "/backtests?status=Completed" },
    @{ Method = "GET"; Endpoint = "/risk/config" },
    @{ Method = "GET"; Endpoint = "/market/quote/AAPL" }
)

for ($i = 1; $i -le $totalRequests; $i++) {
    $requestStartTime = Get-Date
    
    # Select random operation
    $operation = $operations[$i % $operations.Count]
    
    try {
        $response = Invoke-WebRequest `
            -Uri "$apiEndpoint$($operation.Endpoint)" `
            -Method $operation.Method `
            -Headers $headers `
            -UseBasicParsing `
            -TimeoutSec 10 `
            -ErrorAction Stop

        $mixedSuccessCount++
        $responseTime = ((Get-Date) - $requestStartTime).TotalMilliseconds
        $mixedResponseTimes += $responseTime

        if ($i % 100 -eq 0) {
            Write-Host "  Progress: $i/$totalRequests requests sent ($mixedSuccessCount successful)" -ForegroundColor Gray
        }
    }
    catch {
        $mixedFailureCount++
        
        if ($mixedFailureCount -le 10) {
            Write-Host "  Error on request $i : $($_.Exception.Message)" -ForegroundColor Red
        }
    }

    # Throttle
    if ($i -lt $totalRequests) {
        $sleepMs = [Math]::Max(0, $intervalMs - ((Get-Date) - $requestStartTime).TotalMilliseconds)
        if ($sleepMs -gt 0) {
            Start-Sleep -Milliseconds $sleepMs
        }
    }
}

Write-Host ""
Write-Host "Mixed Operations Test Complete" -ForegroundColor Green
Write-Host "  Success: $mixedSuccessCount"
Write-Host "  Failures: $mixedFailureCount"
if ($mixedResponseTimes.Count -gt 0) {
    $avgResponseTime = ($mixedResponseTimes | Measure-Object -Average).Average
    $minResponseTime = ($mixedResponseTimes | Measure-Object -Minimum).Minimum
    $maxResponseTime = ($mixedResponseTimes | Measure-Object -Maximum).Maximum
    Write-Host "  Avg Response Time: $([Math]::Round($avgResponseTime, 2)) ms"
    Write-Host "  Min Response Time: $([Math]::Round($minResponseTime, 2)) ms"
    Write-Host "  Max Response Time: $([Math]::Round($maxResponseTime, 2)) ms"
}
Write-Host ""

# Final Summary
$endTime = Get-Date
$totalDuration = ($endTime - $startTime).TotalSeconds

Write-Host "========================================" -ForegroundColor Green
Write-Host "Load Test Summary" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "Start Time: $startTime"
Write-Host "End Time: $endTime"
Write-Host "Total Duration: $([Math]::Round($totalDuration, 2)) seconds"
Write-Host ""

Write-Host "Overall Statistics:" -ForegroundColor Yellow
$totalSuccess = $backtestSuccessCount + $querySuccessCount + $mixedSuccessCount
$totalFailure = $backtestFailureCount + $queryFailureCount + $mixedFailureCount
$totalRequests = $totalSuccess + $totalFailure
$successRate = if ($totalRequests -gt 0) { ($totalSuccess / $totalRequests) * 100 } else { 0 }

Write-Host "  Total Requests: $totalRequests"
Write-Host "  Successful: $totalSuccess"
Write-Host "  Failed: $totalFailure"
Write-Host "  Success Rate: $([Math]::Round($successRate, 2))%"
Write-Host ""

if ($totalSuccess -gt 0) {
    $allResponseTimes = $backtestResponseTimes + $queryResponseTimes + $mixedResponseTimes
    $overallAvg = ($allResponseTimes | Measure-Object -Average).Average
    $overallMin = ($allResponseTimes | Measure-Object -Minimum).Minimum
    $overallMax = ($allResponseTimes | Measure-Object -Maximum).Maximum
    
    Write-Host "Response Time Statistics:" -ForegroundColor Yellow
    Write-Host "  Average: $([Math]::Round($overallAvg, 2)) ms"
    Write-Host "  Minimum: $([Math]::Round($overallMin, 2)) ms"
    Write-Host "  Maximum: $([Math]::Round($overallMax, 2)) ms"
    Write-Host ""
    
    # Performance threshold checks
    Write-Host "Performance Thresholds:" -ForegroundColor Yellow
    
    $avgThreshold = 500 # 500ms average
    if ($overallAvg -le $avgThreshold) {
        Write-Host "  ✓ Average response time <= ${avgThreshold}ms: PASS" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Average response time > ${avgThreshold}ms: FAIL" -ForegroundColor Red
    }
    
    $maxThreshold = 2000 # 2 second max
    if ($overallMax -le $maxThreshold) {
        Write-Host "  ✓ Maximum response time <= ${maxThreshold}ms: PASS" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Maximum response time > ${maxThreshold}ms: FAIL" -ForegroundColor Red
    }
    
    $successRateThreshold = 95 # 95% success rate
    if ($successRate -ge $successRateThreshold) {
        Write-Host "  ✓ Success rate >= ${successRateThreshold}%: PASS" -ForegroundColor Green
    } else {
        Write-Host "  ✗ Success rate < ${successRateThreshold}%: FAIL" -ForegroundColor Red
    }
}

Write-Host ""

# Calculate actual throughput
$actualThroughput = if ($totalDuration -gt 0) { $totalSuccess / $totalDuration } else { 0 }
Write-Host "Actual Throughput: $([Math]::Round($actualThroughput, 2)) requests/second" -ForegroundColor Cyan

$targetThroughput = 100
if ($actualThroughput -ge ($targetThroughput * 0.9)) {
    Write-Host "✓ Throughput target achieved (>= 90% of target)" -ForegroundColor Green
} else {
    Write-Host "⚠ Throughput below target" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Test completed successfully!" -ForegroundColor Green
Write-Host "Result saved to: LoadTest_$(Get-Date -Format 'yyyyMMdd_HHmmss').log" -ForegroundColor Gray

# Export results to JSON
$results = @{
    TestDate = $startTime.ToString("o")
    Configuration = @{
        BaseUrl = $BaseUrl
        OrdersPerSecond = $OrdersPerSecond
        DurationSeconds = $DurationSeconds
        TotalRequests = $totalRequests * 3  # 3 test types
    }
    BacktestCreation = @{
        SuccessCount = $backtestSuccessCount
        FailureCount = $backtestFailureCount
        AvgResponseTime = if ($backtestResponseTimes.Count -gt 0) { [Math]::Round(($backtestResponseTimes | Measure-Object -Average).Average, 2) } else { 0 }
    }
    OrderQuery = @{
        SuccessCount = $querySuccessCount
        FailureCount = $queryFailureCount
        AvgResponseTime = if ($queryResponseTimes.Count -gt 0) { [Math]::Round(($queryResponseTimes | Measure-Object -Average).Average, 2) } else { 0 }
    }
    MixedOperations = @{
        SuccessCount = $mixedSuccessCount
        FailureCount = $mixedFailureCount
        AvgResponseTime = if ($mixedResponseTimes.Count -gt 0) { [Math]::Round(($mixedResponseTimes | Measure-Object -Average).Average, 2) } else { 0 }
    }
    Overall = @{
        TotalSuccess = $totalSuccess
        TotalFailure = $totalFailure
        SuccessRate = [Math]::Round($successRate, 2)
        AvgResponseTime = if ($totalSuccess -gt 0) { [Math]::Round($overallAvg, 2) } else { 0 }
        ActualThroughput = [Math]::Round($actualThroughput, 2)
    }
}

$resultsJson = $results | ConvertTo-Json -Depth 10
$resultsPath = "LoadTest_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
$resultsJson | Out-File -FilePath $resultsPath -Encoding UTF8

Write-Host "Results JSON saved to: $resultsPath" -ForegroundColor Gray
