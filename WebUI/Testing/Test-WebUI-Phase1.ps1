# WebUI Phase 1 MVP Automated Test Script
# Personal Trading WebUI - 第一阶段自动化测试脚本
# This script tests core API endpoints and basic functionality

param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$Username = "admin",
    [string]$Password = "admin123",
    [switch]$Verbose
)

# Color output functions
function Write-Success { Write-Host "✅ $args" -ForegroundColor Green }
function Write-Failure { Write-Host "❌ $args" -ForegroundColor Red }
function Write-Info { Write-Host "ℹ️  $args" -ForegroundColor Cyan }
function Write-Test { Write-Host "🧪 $args" -ForegroundColor Yellow }

# Test results tracking
$script:TestResults = @{
    Passed = 0
    Failed = 0
    Skipped = 0
    Tests = @()
}

function Add-TestResult {
    param(
        [string]$Name,
        [bool]$Passed,
        [string]$Message = "",
        [object]$Data = $null
    )
    
    $script:TestResults.Tests += @{
        Name = $Name
        Passed = $Passed
        Message = $Message
        Timestamp = Get-Date
        Data = $Data
    }
    
    if ($Passed) {
        $script:TestResults.Passed++
        Write-Success "$Name - PASSED"
        if ($Message) { Write-Info "  $Message" }
    } else {
        $script:TestResults.Failed++
        Write-Failure "$Name - FAILED"
        if ($Message) { Write-Info "  $Message" }
    }
}

# Global variables
$script:AuthToken = $null
$script:RefreshToken = $null

Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║   Personal Trading WebUI - Phase 1 MVP Test Suite       ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝`n" -ForegroundColor Magenta

Write-Info "Base URL: $BaseUrl"
Write-Info "Username: $Username"
Write-Info "Test started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host ""

# ============================================================================
# Test 1: Health Check
# ============================================================================
Write-Test "Test 1: Health Check Endpoint"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/health" -Method Get -TimeoutSec 10
    
    if ($response) {
        Add-TestResult -Name "Health Check" -Passed $true -Message "Service is healthy"
    } else {
        Add-TestResult -Name "Health Check" -Passed $false -Message "No response from health endpoint"
    }
} catch {
    Add-TestResult -Name "Health Check" -Passed $false -Message "Failed to connect: $($_.Exception.Message)"
    Write-Failure "Cannot connect to WebUI service. Please ensure it's running on $BaseUrl"
    exit 1
}

# ============================================================================
# Test 2: Authentication - Login
# ============================================================================
Write-Test "`nTest 2: User Authentication (Login)"
try {
    $loginBody = @{
        username = $Username
        password = $Password
    } | ConvertTo-Json

    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/login" -Method Post `
        -Body $loginBody -ContentType "application/json" -TimeoutSec 10

    if ($response.token) {
        $script:AuthToken = $response.token
        $script:RefreshToken = $response.refreshToken
        Add-TestResult -Name "Login" -Passed $true -Message "JWT token received"
        
        if ($Verbose) {
            Write-Info "  Token: $($script:AuthToken.Substring(0, 20))..."
        }
    } else {
        Add-TestResult -Name "Login" -Passed $false -Message "No token in response"
    }
} catch {
    Add-TestResult -Name "Login" -Passed $false -Message $_.Exception.Message
    Write-Failure "Login failed. Cannot continue with authenticated tests."
    exit 1
}

# Create auth header for subsequent requests
$headers = @{
    "Authorization" = "Bearer $script:AuthToken"
    "Content-Type" = "application/json"
}

# ============================================================================
# Test 3: Invalid Login Attempt
# ============================================================================
Write-Test "`nTest 3: Invalid Login Attempt (Security)"
try {
    $invalidBody = @{
        username = "invalid"
        password = "wrongpassword"
    } | ConvertTo-Json

    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/login" -Method Post `
            -Body $invalidBody -ContentType "application/json" -ErrorAction Stop
        
        Add-TestResult -Name "Invalid Login Rejection" -Passed $false -Message "Invalid login was accepted!"
    } catch {
        if ($_.Exception.Response.StatusCode -eq 401 -or $_.Exception.Response.StatusCode -eq 400) {
            Add-TestResult -Name "Invalid Login Rejection" -Passed $true -Message "Correctly rejected invalid credentials"
        } else {
            Add-TestResult -Name "Invalid Login Rejection" -Passed $false -Message "Unexpected error: $($_.Exception.Message)"
        }
    }
} catch {
    Add-TestResult -Name "Invalid Login Rejection" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 4: IBKR Connection Status
# ============================================================================
Write-Test "`nTest 4: IBKR Connection Status"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/ibkr/status" -Method Get `
        -Headers $headers -TimeoutSec 10

    if ($response) {
        $isConnected = $response.isConnected -or $response.connected -or $response.status -eq "Connected"
        
        Add-TestResult -Name "IBKR Connection Status" -Passed $true `
            -Message "Status: $(if ($isConnected) { 'Connected' } else { 'Disconnected' })" `
            -Data $response
        
        if ($Verbose -and $isConnected) {
            Write-Info "  Account ID: $($response.accountId)"
            Write-Info "  Account Type: $($response.accountType)"
        }
    } else {
        Add-TestResult -Name "IBKR Connection Status" -Passed $false -Message "No response from API"
    }
} catch {
    Add-TestResult -Name "IBKR Connection Status" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 5: Account Balance Query
# ============================================================================
Write-Test "`nTest 5: Account Balance Query"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/ibkr/account" -Method Get `
        -Headers $headers -TimeoutSec 10

    if ($response) {
        Add-TestResult -Name "Account Balance Query" -Passed $true `
            -Message "Account data retrieved" -Data $response
        
        if ($Verbose) {
            Write-Info "  Net Liquidation: $($response.netLiquidation)"
            Write-Info "  Cash Available: $($response.cashAvailable)"
            Write-Info "  Buying Power: $($response.buyingPower)"
        }
    } else {
        Add-TestResult -Name "Account Balance Query" -Passed $false -Message "No account data returned"
    }
} catch {
    Add-TestResult -Name "Account Balance Query" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 6: Market Quote Query
# ============================================================================
Write-Test "`nTest 6: Market Quote Query (AAPL)"
try {
    $symbol = "AAPL"
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/market/quote/$symbol" -Method Get `
        -Headers $headers -TimeoutSec 10

    if ($response -and ($response.symbol -eq $symbol -or $response.ticker -eq $symbol)) {
        Add-TestResult -Name "Market Quote Query" -Passed $true `
            -Message "Quote for $symbol retrieved" -Data $response
        
        if ($Verbose) {
            Write-Info "  Symbol: $($response.symbol)"
            Write-Info "  Last Price: $($response.lastPrice -or $response.price)"
            Write-Info "  Bid: $($response.bid)"
            Write-Info "  Ask: $($response.ask)"
        }
    } else {
        Add-TestResult -Name "Market Quote Query" -Passed $false -Message "Invalid quote data for $symbol"
    }
} catch {
    Add-TestResult -Name "Market Quote Query" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 7: Positions Query
# ============================================================================
Write-Test "`nTest 7: Positions Query"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/portfolio/positions" -Method Get `
        -Headers $headers -TimeoutSec 10

    if ($response -ne $null) {
        $posCount = if ($response.positions) { $response.positions.Count } elseif ($response.Count) { $response.Count } else { 0 }
        
        Add-TestResult -Name "Positions Query" -Passed $true `
            -Message "Retrieved $posCount positions" -Data $response
        
        if ($Verbose -and $posCount -gt 0) {
            $positions = if ($response.positions) { $response.positions } else { $response }
            $positions | ForEach-Object {
                Write-Info "  $($_.symbol): $($_.quantity) @ $($_.averageCost)"
            }
        }
    } else {
        Add-TestResult -Name "Positions Query" -Passed $false -Message "No response from positions endpoint"
    }
} catch {
    Add-TestResult -Name "Positions Query" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 8: Orders History Query
# ============================================================================
Write-Test "`nTest 8: Orders History Query"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/orders" -Method Get `
        -Headers $headers -TimeoutSec 10

    if ($response -ne $null) {
        $orderCount = if ($response.orders) { $response.orders.Count } elseif ($response.Count) { $response.Count } else { 0 }
        
        Add-TestResult -Name "Orders History Query" -Passed $true `
            -Message "Retrieved $orderCount orders" -Data $response
        
        if ($Verbose -and $orderCount -gt 0) {
            $orders = if ($response.orders) { $response.orders } else { $response }
            $orders | Select-Object -First 5 | ForEach-Object {
                Write-Info "  $($_.symbol) $($_.action) $($_.quantity) @ $($_.orderType) - Status: $($_.status)"
            }
        }
    } else {
        Add-TestResult -Name "Orders History Query" -Passed $false -Message "No response from orders endpoint"
    }
} catch {
    Add-TestResult -Name "Orders History Query" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 9: Order Validation (without actual submission)
# ============================================================================
Write-Test "`nTest 9: Order Validation"
try {
    $orderBody = @{
        symbol = "SPY"
        action = "BUY"
        orderType = "MARKET"
        quantity = 1
    } | ConvertTo-Json

    # Try to validate order (use validate endpoint or get error from submit)
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/orders/validate" -Method Post `
            -Headers $headers -Body $orderBody -TimeoutSec 10
        
        Add-TestResult -Name "Order Validation" -Passed $true -Message "Order validation passed"
    } catch {
        # If no validate endpoint, check if it's a validation error or other error
        if ($_.Exception.Response.StatusCode -eq 400) {
            $errorContent = $_.ErrorDetails.Message | ConvertFrom-Json
            if ($errorContent.message -match "validate") {
                Add-TestResult -Name "Order Validation" -Passed $true -Message "Validation endpoint working"
            } else {
                Add-TestResult -Name "Order Validation" -Passed $false -Message "Validation failed: $($errorContent.message)"
            }
        } elseif ($_.Exception.Response.StatusCode -eq 404) {
            Add-TestResult -Name "Order Validation" -Passed $false -Message "Validation endpoint not found (skipped test)" -Data "SKIPPED"
            $script:TestResults.Skipped++
            $script:TestResults.Failed--
        } else {
            throw
        }
    }
} catch {
    Add-TestResult -Name "Order Validation" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 10: Token Refresh
# ============================================================================
Write-Test "`nTest 10: Token Refresh"
try {
    if ($script:RefreshToken) {
        $refreshBody = @{
            refreshToken = $script:RefreshToken
        } | ConvertTo-Json

        $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/refresh" -Method Post `
            -Body $refreshBody -ContentType "application/json" -TimeoutSec 10

        if ($response.token) {
            Add-TestResult -Name "Token Refresh" -Passed $true -Message "New token received"
        } else {
            Add-TestResult -Name "Token Refresh" -Passed $false -Message "No token in refresh response"
        }
    } else {
        Add-TestResult -Name "Token Refresh" -Passed $false -Message "No refresh token available"
    }
} catch {
    Add-TestResult -Name "Token Refresh" -Passed $false -Message $_.Exception.Message
}

# ============================================================================
# Test 11: Logout
# ============================================================================
Write-Test "`nTest 11: Logout"
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/v1/auth/logout" -Method Post `
        -Headers $headers -TimeoutSec 10

    Add-TestResult -Name "Logout" -Passed $true -Message "Successfully logged out"
} catch {
    # Some implementations may return 204 No Content
    if ($_.Exception.Response.StatusCode -eq 204) {
        Add-TestResult -Name "Logout" -Passed $true -Message "Successfully logged out (204)"
    } else {
        Add-TestResult -Name "Logout" -Passed $false -Message $_.Exception.Message
    }
}

# ============================================================================
# Test Summary
# ============================================================================
Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                    TEST SUMMARY                          ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝`n" -ForegroundColor Magenta

Write-Host "Total Tests:    $($script:TestResults.Passed + $script:TestResults.Failed + $script:TestResults.Skipped)" -ForegroundColor White
Write-Success "Passed:         $($script:TestResults.Passed)"
Write-Failure "Failed:         $($script:TestResults.Failed)"

if ($script:TestResults.Skipped -gt 0) {
    Write-Host "⊘  Skipped:        $($script:TestResults.Skipped)" -ForegroundColor Gray
}

$passRate = [math]::Round(($script:TestResults.Passed / ($script:TestResults.Passed + $script:TestResults.Failed)) * 100, 2)
Write-Host "Pass Rate:      $passRate%" -ForegroundColor $(if ($passRate -ge 90) { 'Green' } elseif ($passRate -ge 70) { 'Yellow' } else { 'Red' })

Write-Host "`nTest completed: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')`n"

# Export results to JSON
$reportPath = ".\TestResults-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$script:TestResults | ConvertTo-Json -Depth 10 | Out-File $reportPath
Write-Info "Test results exported to: $reportPath"

# Exit code
if ($script:TestResults.Failed -gt 0) {
    exit 1
} else {
    exit 0
}
