# Lean Web UI 快速启动

> 为 Lean 引擎添加 Web 管理界面

---

## 📋 项目结构

```
Lean/
├── WebApi/                          # ASP.NET Core Web API（新增）
│   ├── Controllers/
│   │   ├── StrategyController.cs   # 策略管理
│   │   ├── BacktestController.cs   # 回测控制
│   │   ├── ConfigController.cs     # 配置管理
│   │   └── LiveController.cs       # 实盘管理
│   ├── Services/
│   │   ├── ILeanEngineService.cs   # 引擎服务接口
│   │   └── LeanEngineService.cs    # 引擎服务实现
│   ├── Hubs/
│   │   └── BacktestHub.cs          # SignalR 实时通信
│   ├── Models/
│   │   ├── BacktestRequest.cs
│   │   └── BacktestResult.cs
│   ├── Data/
│   │   └── LeanDbContext.cs        # EF Core 数据库
│   ├── Program.cs
│   └── appsettings.json
│
├── lean-ui/                         # React 前端（新增）
│   ├── public/
│   ├── src/
│   │   ├── components/
│   │   │   ├── Dashboard/          # 仪表盘
│   │   │   ├── Strategy/           # 策略管理
│   │   │   ├── Backtest/           # 回测管理
│   │   │   └── Config/             # 配置管理
│   │   ├── services/
│   │   │   ├── api.ts              # API 客户端
│   │   │   └── signalr.ts          # SignalR 客户端
│   │   ├── pages/
│   │   ├── App.tsx
│   │   └── index.tsx
│   ├── package.json
│   └── tsconfig.json
│
└── [现有 Lean 项目文件...]
```

---

## 🚀 快速创建

### 步骤 1：创建 Web API 项目

```powershell
# 在 Lean 根目录执行
cd d:\source\repos\Lean

# 创建 Web API 项目
dotnet new webapi -n QuantConnect.Lean.WebApi -o WebApi

# 添加到解决方案
dotnet sln add WebApi/QuantConnect.Lean.WebApi.csproj

# 添加项目引用
cd WebApi
dotnet add reference ../Engine/QuantConnect.Lean.Engine.csproj
dotnet add reference ../Common/QuantConnect.csproj
dotnet add reference ../Algorithm/QuantConnect.Algorithm.csproj
dotnet add reference ../Configuration/QuantConnect.Configuration.csproj

# 安装必要的包
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Swashbuckle.AspNetCore
dotnet add package Newtonsoft.Json

cd ..
```

### 步骤 2：创建核心服务

创建 `WebApi/Services/ILeanEngineService.cs`：

```csharp
using QuantConnect.Lean.WebApi.Models;

namespace QuantConnect.Lean.WebApi.Services
{
    public interface ILeanEngineService
    {
        Task<string> StartBacktestAsync(BacktestRequest request);
        Task StopBacktest(string backtestId);
        BacktestStatus GetBacktestStatus(string backtestId);
        BacktestResult GetBacktestResults(string backtestId);
        List<BacktestInfo> GetBacktestHistory();
    }
}
```

创建 `WebApi/Services/LeanEngineService.cs`：

```csharp
using QuantConnect.Configuration;
using QuantConnect.Lean.Engine;
using QuantConnect.Packets;
using System.Collections.Concurrent;

namespace QuantConnect.Lean.WebApi.Services
{
    public class LeanEngineService : ILeanEngineService
    {
        private readonly ConcurrentDictionary<string, Task> _runningBacktests = new();
        private readonly ILogger<LeanEngineService> _logger;

        public LeanEngineService(ILogger<LeanEngineService> logger)
        {
            _logger = logger;
        }

        public async Task<string> StartBacktestAsync(BacktestRequest request)
        {
            var backtestId = Guid.NewGuid().ToString();
            
            // 配置回测参数
            Config.Set("algorithm-type-name", request.StrategyName);
            Config.Set("algorithm-language", request.Language ?? "CSharp");
            
            // 异步启动回测任务
            var task = Task.Run(() => RunBacktest(backtestId, request));
            _runningBacktests.TryAdd(backtestId, task);
            
            return backtestId;
        }

        private void RunBacktest(string backtestId, BacktestRequest request)
        {
            try
            {
                _logger.LogInformation($"开始回测: {backtestId}");
                
                // 创建 Lean 引擎实例
                var systemHandlers = LeanEngineSystemHandlers.FromConfiguration(Composer.Instance);
                var algorithmHandlers = LeanEngineAlgorithmHandlers.FromConfiguration(Composer.Instance);
                
                var engine = new Engine.Engine(systemHandlers, algorithmHandlers, false);
                
                var job = new BacktestNodePacket
                {
                    BacktestId = backtestId,
                    Name = request.StrategyName,
                    Language = Language.CSharp
                };
                
                // 运行引擎
                engine.Run(job, new AlgorithmManager(false), "sync");
                
                _logger.LogInformation($"回测完成: {backtestId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"回测失败: {backtestId}");
            }
        }

        public async Task StopBacktest(string backtestId)
        {
            if (_runningBacktests.TryRemove(backtestId, out var task))
            {
                // 实现停止逻辑
                _logger.LogInformation($"停止回测: {backtestId}");
            }
        }

        public BacktestStatus GetBacktestStatus(string backtestId)
        {
            var isRunning = _runningBacktests.ContainsKey(backtestId);
            return new BacktestStatus
            {
                BacktestId = backtestId,
                Status = isRunning ? "Running" : "Completed",
                Progress = 75 // 实际应从引擎获取
            };
        }

        public BacktestResult GetBacktestResults(string backtestId)
        {
            // 读取结果文件
            var resultPath = $"./results/{backtestId}-result.json";
            if (File.Exists(resultPath))
            {
                var json = File.ReadAllText(resultPath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<BacktestResult>(json);
            }
            
            return new BacktestResult { BacktestId = backtestId };
        }

        public List<BacktestInfo> GetBacktestHistory()
        {
            // 从数据库或文件系统读取历史记录
            return new List<BacktestInfo>();
        }
    }
}
```

### 步骤 3：创建控制器

创建 `WebApi/Controllers/BacktestController.cs`：

```csharp
using Microsoft.AspNetCore.Mvc;
using QuantConnect.Lean.WebApi.Models;
using QuantConnect.Lean.WebApi.Services;

namespace QuantConnect.Lean.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BacktestController : ControllerBase
    {
        private readonly ILeanEngineService _engineService;
        private readonly ILogger<BacktestController> _logger;

        public BacktestController(
            ILeanEngineService engineService,
            ILogger<BacktestController> logger)
        {
            _engineService = engineService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> StartBacktest([FromBody] BacktestRequest request)
        {
            try
            {
                var backtestId = await _engineService.StartBacktestAsync(request);
                return Ok(new { BacktestId = backtestId, Status = "Started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动回测失败");
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("{backtestId}/status")]
        public IActionResult GetStatus(string backtestId)
        {
            var status = _engineService.GetBacktestStatus(backtestId);
            return Ok(status);
        }

        [HttpGet("{backtestId}/results")]
        public IActionResult GetResults(string backtestId)
        {
            var results = _engineService.GetBacktestResults(backtestId);
            return Ok(results);
        }

        [HttpPost("{backtestId}/stop")]
        public async Task<IActionResult> StopBacktest(string backtestId)
        {
            await _engineService.StopBacktest(backtestId);
            return Ok(new { Message = "已停止" });
        }

        [HttpGet("history")]
        public IActionResult GetHistory()
        {
            var history = _engineService.GetBacktestHistory();
            return Ok(history);
        }
    }
}
```

### 步骤 4：配置 Program.cs

```csharp
using QuantConnect.Lean.WebApi.Services;
using QuantConnect.Lean.WebApi.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 添加 SignalR
builder.Services.AddSignalR();

// 注册 Lean 引擎服务
builder.Services.AddSingleton<ILeanEngineService, LeanEngineService>();

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.MapHub<BacktestHub>("/hubs/backtest");

app.Run();
```

### 步骤 5：创建 React 前端

```bash
# 在 Lean 根目录
npx create-react-app lean-ui --template typescript

cd lean-ui

# 安装依赖
npm install axios @microsoft/signalr
npm install antd @ant-design/icons
npm install react-router-dom
npm install echarts echarts-for-react
npm install dayjs

# 开发环境代理配置
# 创建 .env 文件
echo "REACT_APP_API_URL=http://localhost:5000/api" > .env
echo "REACT_APP_HUB_URL=http://localhost:5000/hubs" >> .env
```

创建 `lean-ui/src/services/api.ts`：

```typescript
import axios from 'axios';

const API_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000/api';

export const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const backtestApi = {
  start: (data: any) => api.post('/backtest', data),
  getStatus: (id: string) => api.get(`/backtest/${id}/status`),
  getResults: (id: string) => api.get(`/backtest/${id}/results`),
  stop: (id: string) => api.post(`/backtest/${id}/stop`),
  getHistory: () => api.get('/backtest/history'),
};
```

创建 `lean-ui/src/App.tsx`：

```typescript
import React from 'react';
import { Layout, Menu } from 'antd';
import { DashboardOutlined, RocketOutlined, SettingOutlined } from '@ant-design/icons';
import { BrowserRouter, Routes, Route, Link } from 'react-router-dom';
import Dashboard from './pages/Dashboard';
import Backtest from './pages/Backtest';
import Config from './pages/Config';

const { Header, Content, Sider } = Layout;

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <Layout style={{ minHeight: '100vh' }}>
        <Header style={{ color: 'white', fontSize: '20px' }}>
          Lean 量化交易平台
        </Header>
        <Layout>
          <Sider width={200}>
            <Menu mode="inline" defaultSelectedKeys={['1']} style={{ height: '100%' }}>
              <Menu.Item key="1" icon={<DashboardOutlined />}>
                <Link to="/">仪表盘</Link>
              </Menu.Item>
              <Menu.Item key="2" icon={<RocketOutlined />}>
                <Link to="/backtest">回测管理</Link>
              </Menu.Item>
              <Menu.Item key="3" icon={<SettingOutlined />}>
                <Link to="/config">系统配置</Link>
              </Menu.Item>
            </Menu>
          </Sider>
          <Content style={{ padding: '24px', minHeight: 280 }}>
            <Routes>
              <Route path="/" element={<Dashboard />} />
              <Route path="/backtest" element={<Backtest />} />
              <Route path="/config" element={<Config />} />
            </Routes>
          </Content>
        </Layout>
      </Layout>
    </BrowserRouter>
  );
};

export default App;
```

---

## 🎯 启动应用

### 启动后端 API

```powershell
# Terminal 1: 启动 Web API
cd d:\source\repos\Lean\WebApi
dotnet run

# API 将运行在 http://localhost:5000
# Swagger 文档: http://localhost:5000/swagger
```

### 启动前端

```bash
# Terminal 2: 启动 React 应用
cd d:\source\repos\Lean\lean-ui
npm start

# 应用将运行在 http://localhost:3000
```

---

## 📸 预期效果

访问 `http://localhost:3000`，你将看到：

1. **仪表盘页面**：展示系统概览、最近回测、性能指标
2. **回测管理页面**：启动回测、查看进度、分析结果
3. **系统配置页面**：修改 config.json、切换环境

---

## 🔧 下一步开发

### 完善功能
- [ ] 策略编辑器（Monaco Editor / CodeMirror）
- [ ] 图表可视化（持仓、收益曲线、回撤）
- [ ] 实时日志查看
- [ ] 参数优化界面
- [ ] 告警和通知
- [ ] 用户认证和权限管理

### 性能优化
- [ ] 添加 Redis 缓存
- [ ] 实现任务队列（Hangfire）
- [ ] 数据库查询优化
- [ ] 前端代码分割和懒加载

### 部署
- [ ] Docker 容器化
- [ ] Nginx 反向代理
- [ ] HTTPS 配置
- [ ] 自动化部署（CI/CD）

---

## 💡 开发技巧

### 调试 API

```bash
# 使用 curl 测试
curl -X POST http://localhost:5000/api/backtest \
  -H "Content-Type: application/json" \
  -d '{"strategyName":"BasicTemplateAlgorithm","startDate":"2020-01-01","endDate":"2020-12-31","initialCash":100000}'

# 使用 Postman 或 Thunder Client 扩展
```

### 前端调试

```typescript
// 在组件中添加日志
console.log('API Response:', response.data);

// 使用 React DevTools
// Chrome 扩展：React Developer Tools
```

### SignalR 调试

```typescript
connection.on('ProgressUpdate', data => {
  console.log('Progress:', data);
});
```

---

## 📚 参考资源

- **ASP.NET Core 文档**：https://docs.microsoft.com/aspnet/core
- **React 文档**：https://react.dev
- **Ant Design 组件库**：https://ant.design
- **SignalR 文档**：https://docs.microsoft.com/aspnet/core/signalr

---

**开始构建你的量化交易平台吧！** 🚀
