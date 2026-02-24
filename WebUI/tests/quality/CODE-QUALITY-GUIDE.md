# Code Quality Scanning Guide
# 代码质量扫描指南

## Overview
## 概览

Code quality scanning ensures code maintainability, readability, and adherence to best practices.
代码质量扫描确保代码可维护性、可读性和遵循最佳实践。

## Tools
## 工具

### Backend (.NET)
### 后端 (.NET)

1. **Roslyn Analyzers**: Built-in .NET code analyzers
2. **StyleCop**: Code style and consistency
3. **SonarAnalyzer.CSharp**: Comprehensive code analysis
4. **Security Code Scan**: Security-focused analysis

### Frontend (TypeScript/React)
### 前端 (TypeScript/React)

1. **ESLint**: JavaScript/TypeScript linting
2. **TypeScript Compiler**: Type checking
3. **Prettier**: Code formatting
4. **SonarQube**: Comprehensive code quality

## Setup
## 设置

### Backend Code Analyzers
### 后端代码分析器

Add to `WebUI.API.csproj`, `WebUI.Core.csproj`, and `WebUI.Data.csproj`:

```xml
<ItemGroup>
  <!-- Code Analysis -->
  <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="10.0.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  
  <PackageReference Include="SonarAnalyzer.CSharp" Version="9.35.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  
  <PackageReference Include="SecurityCodeScan.VS2019" Version="5.6.7">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
  
  <PackageReference Include="StyleCop.Analyzers" Version="1.2.0-beta.556">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>

<PropertyGroup>
  <!-- Enable all analyzers -->
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
  
  <!-- Treat warnings as errors in Release builds -->
  <TreatWarningsAsErrors Condition="'$(Configuration)' == 'Release'">true</TreatWarningsAsErrors>
  
  <!-- Documentation -->
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>CS1591</NoWarn> <!-- Missing XML comment -->
</PropertyGroup>
```

### .editorconfig
### .editorconfig 配置

Create `.editorconfig` in solution root:

```ini
root = true

[*.cs]
# Code style rules
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

# Naming conventions
dotnet_naming_rule.interfaces_should_be_pascal_case_prefixed_with_i.severity = warning
dotnet_naming_rule.interfaces_should_be_pascal_case_prefixed_with_i.symbols = interface
dotnet_naming_rule.interfaces_should_be_pascal_case_prefixed_with_i.style = begins_with_i

# Code quality rules
dotnet_code_quality_unused_parameters = all:warning
dotnet_code_quality.api_surface = all

# Security rules
dotnet_diagnostic.CA2100.severity = error # SQL injection
dotnet_diagnostic.CA3075.severity = error # Insecure DTD processing
dotnet_diagnostic.CA5350.severity = error # Weak cryptographic algorithms

# Performance rules
dotnet_diagnostic.CA1806.severity = warning # Do not ignore method results
dotnet_diagnostic.CA1822.severity = warning # Mark members as static

[*.{ts,tsx}]
indent_size = 2
```

### Frontend ESLint Configuration
### 前端 ESLint 配置

Already configured in `eslint.config.js`, enhance with:

```javascript
export default [
  // ... existing config
  {
    rules: {
      // Code quality
      'no-console': ['warn', { allow: ['warn', 'error'] }],
      'no-debugger': 'warn',
      'no-unused-vars': 'error',
      
      // React best practices
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'warn',
      'react/prop-types': 'off', // Using TypeScript
      
      // TypeScript
      '@typescript-eslint/no-explicit-any': 'warn',
      '@typescript-eslint/explicit-function-return-type': 'off',
      '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],
      
      // Accessibility
      'jsx-a11y/anchor-is-valid': 'warn',
      'jsx-a11y/click-events-have-key-events': 'warn',
    }
  }
];
```

## Running Code Quality Scans
## 运行代码质量扫描

### Backend Analysis
### 后端分析

```powershell
# Build with analysis
dotnet build /p:Configuration=Release

# Run specific analyzers
dotnet build /p:RunAnalyzers=true /p:AnalysisLevel=latest

# Get analysis report
dotnet build > build-analysis.log 2>&1
```

### Frontend Analysis
### 前端分析

```bash
cd WebUI.Frontend

# Run ESLint
npm run lint

# Fix auto-fixable issues
npm run lint -- --fix

# TypeScript type checking
npm run type-check  # Add to package.json: "type-check": "tsc --noEmit"

# Run all quality checks
npm run quality-check  # Add script combining all checks
```

### SonarQube Analysis
### SonarQube 分析

```powershell
# Install SonarScanner
dotnet tool install --global dotnet-sonarscanner

# Start analysis
dotnet sonarscanner begin `
  /k:"lean-webui" `
  /d:sonar.host.url="http://localhost:9000" `
  /d:sonar.login="YOUR_TOKEN"

# Build
dotnet build --no-incremental

# End analysis
dotnet sonarscanner end /d:sonar.login="YOUR_TOKEN"
```

## Quality Metrics
## 质量指标

### Target Metrics
### 目标指标

| Metric | Target | Critical Threshold |
|--------|--------|--------------------|
| Code Coverage | > 70% | > 60% |
| Technical Debt Ratio | < 5% | < 10% |
| Duplicated Lines | < 3% | < 5% |
| Code Smells | < 50 | < 100 |
| Security Hotspots | 0 | 0 |
| Bugs | 0 | 0 |
| Vulnerabilities | 0 | 0 |
| Maintainability Rating | A | B |
| Reliability Rating | A | B |
| Security Rating | A | B |

### Code Complexity
### 代码复杂度

- **Cyclomatic Complexity**: < 10 per method
- **Cognitive Complexity**: < 15 per method
- **Lines of Code per Class**: < 500
- **Method Length**: < 50 lines

## Common Issues and Fixes
## 常见问题和修复

### CS8618: Non-nullable field must contain a non-null value
### CS8618: 不可为空的字段必须包含非空值

```csharp
// Before
public class User
{
    public string Username { get; set; }  // Warning
}

// After
public class User
{
    public string Username { get; set; } = string.Empty;
    // Or
    public required string Username { get; set; }
}
```

### CA2007: Do not directly await a Task
### CA2007: 不要直接等待 Task

```csharp
// Before
await DoSomethingAsync();

// After (in library code)
await DoSomethingAsync().ConfigureAwait(false);
```

### CA1062: Validate arguments of public methods
### CA1062: 验证公共方法的参数

```csharp
// Before
public void Process(Order order)
{
    order.Status = "Processed";  // Warning
}

// After
public void Process(Order order)
{
    ArgumentNullException.ThrowIfNull(order);
    order.Status = "Processed";
}
```

### IDE0005: Remove unnecessary using directives
### IDE0005: 删除不必要的 using 指令

```powershell
# Auto-fix in Visual Studio
# Or use dotnet format
dotnet format --no-restore
```

## CI/CD Integration
## CI/CD 集成

### GitHub Actions

```yaml
name: Code Quality

on: [push, pull_request]

jobs:
  code-quality:
    runs-on: ubuntu-latest
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Restore dependencies
        run: dotnet restore
      
      - name: Build with analyzers
        run: dotnet build --configuration Release /warnaserror
      
      - name: Run tests with coverage
        run: dotnet test --configuration Release --collect:"XPlat Code Coverage"
      
      - name: Setup Node.js
        uses: actions/setup-node@v3
        with:
          node-version: '20'
      
      - name: Install frontend dependencies
        run: |
          cd WebUI.Frontend
          npm ci
      
      - name: Run ESLint
        run: |
          cd WebUI.Frontend
          npm run lint
      
      - name: TypeScript check
        run: |
          cd WebUI.Frontend
          npm run type-check
      
      - name: SonarCloud Scan
        uses: SonarSource/sonarcloud-github-action@master
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          SONAR_TOKEN: ${{ secrets.SONAR_TOKEN }}
```

## Suppressing Warnings
## 抑制警告

Only suppress warnings when absolutely necessary:
仅在绝对必要时抑制警告:

```csharp
// Single suppression
#pragma warning disable CA1062 // Validate arguments of public methods
public void Process(Order order)
{
    // Justification: order is validated by framework model binding
    order.Status = "Processed";
}
#pragma warning restore CA1062

// Global suppression in GlobalSuppressions.cs
[assembly: SuppressMessage("Category", "Rule", Justification = "Reason")]
```

## Reports
## 报告

Generate and save reports:
生成并保存报告:

```powershell
# Generate markdown report
dotnet build 2>&1 | Out-File -FilePath code-quality-report.txt

# SonarQube report
# Available at http://localhost:9000/dashboard?id=lean-webui
```

## Best Practices
## 最佳实践

1. **Fix issues early**: Address warnings during development
   **早期修复问题**: 在开发期间解决警告

2. **Zero tolerance**: No warnings in Release builds
   **零容忍**: 发布版本中无警告

3. **Code reviews**: Review analyzer findings in PRs
   **代码审查**: 在 PR 中审查分析器发现

4. **Continuous monitoring**: Track metrics over time
   **持续监控**: 跟踪随时间变化的指标

5. **Team standards**: Agree on coding standards
   **团队标准**: 就编码标准达成一致

## Tools to Install
## 要安装的工具

```powershell
# Global tools
dotnet tool install --global dotnet-format
dotnet tool install --global dotnet-sonarscanner
dotnet tool install --global dotnet-reportgenerator-globaltool

# VS Code extensions
code --install-extension ms-dotnettools.csharp
code --install-extension SonarSource.sonarlint-vscode
code --install-extension dbaeumer.vscode-eslint
```

## References
## 参考

- [.NET Code Analysis](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview)
- [SonarQube Documentation](https://docs.sonarqube.org/)
- [ESLint Rules](https://eslint.org/docs/rules/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/)
