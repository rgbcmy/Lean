# Lean WebUI 项目文档

## 快速启动

### 1. 启动后端 API
```powershell
cd d:\source\repos\Lean\WebUI\WebUI.API
dotnet run
```

后端将运行在：`http://localhost:5041`

### 2. 启动前端
```powershell
cd d:\source\repos\Lean\WebUI\WebUI.Frontend
npm run dev
```

前端将运行在：`http://localhost:5174`

## 默认登录信息

### 管理员账户
- **用户名**: `admin`
- **密码**: `admin123`
- **邮箱**: `admin@leanwebui.local`
- **角色**: Administrator

## 访问地址

- **前端登录页面**: http://localhost:5174/
- **后端API**: http://localhost:5041
- **API文档 (Swagger)**: http://localhost:5041/swagger

## 技术栈

### 后端
- **.NET**: ASP.NET Core 10.0
- **数据库**: SQLite (开发环境), PostgreSQL (生产环境)
- **认证**: JWT Token with BCrypt password hashing
- **ORM**: Entity Framework Core 10.0.3
- **实时通信**: SignalR

### 前端
- **框架**: React 18
- **构建工具**: Vite
- **UI库**: Ant Design
- **状态管理**: React Query + Context API
- **图表**: ECharts

## 项目结构

```
WebUI/
├── WebUI.API/          # ASP.NET Core Web API
├── WebUI.Core/         # 核心业务逻辑和服务接口
├── WebUI.Data/         # 数据访问层 (EF Core, Repositories)
├── WebUI.Frontend/     # React 前端应用
└── WebUI.Tests/        # 单元测试项目
```

## 数据库配置

### 开发环境 (SQLite)
默认使用 SQLite 数据库，数据库文件位于：
```
d:\source\repos\Lean\WebUI\WebUI.API\webui.db
```

### 连接字符串配置
配置文件：`WebUI.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=webui.db"
  },
  "WebUI": {
    "DatabaseProvider": "SQLite"
  }
}
```

## API 认证

### 登录
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**响应**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "YiiTZLwzZs7qsXmvx9dLkQBtIBTzWEu9...",
  "expiresAt": "2026-02-20T15:33:53.7020023Z",
  "userId": 1,
  "username": "admin"
}
```

### 使用 Token
在后续 API 请求中添加 Authorization header：
```http
Authorization: Bearer {accessToken}
```

## CORS 配置

允许的源：
- `http://localhost:3000`
- `http://localhost:5173`
- `http://localhost:5174`

## 常见问题

### 1. 密码哈希不匹配
如果登录失败返回 401，可能是数据库中的密码哈希算法不匹配。

**解决方案**：删除数据库文件并重新启动后端
```powershell
Remove-Item d:\source\repos\Lean\WebUI\WebUI.API\webui.db -Force
cd d:\source\repos\Lean\WebUI\WebUI.API
dotnet run
```

### 2. 端口已被占用
如果端口 5041 或 5174 已被占用：

**查看占用端口的进程**:
```powershell
Get-NetTCPConnection -LocalPort 5041 | Select-Object OwningProcess
Get-Process -Id <ProcessId>
```

**停止进程**:
```powershell
Stop-Process -Id <ProcessId> -Force
```

### 3. CORS 错误
确保：
1. 前端 `.env.development` 文件配置正确
2. 后端 `appsettings.json` 中包含前端 URL
3. 后端 `Program.cs` 中 CORS 中间件在认证之前

## 开发注意事项

### 密码哈希
系统使用 **PBKDF2** 算法进行密码哈希：
- 迭代次数：10,000
- 哈希长度：256 bits
- Salt 长度：128 bits
- 格式：`base64(salt).base64(hash)`

### 依赖关系
```
WebUI.API → WebUI.Data → WebUI.Core
```

**注意**：不要在 Core 层引用 Data 层，避免循环依赖。

### SignalR Hubs
实时数据推送端点：
- `/hubs/marketdata` - 市场数据
- `/hubs/orders` - 订单更新
- `/hubs/strategies` - 策略执行日志

## 测试账户

目前系统只有一个默认管理员账户。要创建新用户，可以：

1. 通过 API 注册（如果启用）
2. 直接在数据库中添加
3. 通过管理员界面创建（待实现）

## 重要提醒

⚠️ **生产环境部署前必须修改**：
- 修改默认管理员密码
- 更新 JWT Secret Key
- 配置生产级数据库 (PostgreSQL)
- 启用 HTTPS
- 配置适当的 CORS 策略

## 更新日志

### 2026-02-20
- ✅ 修复循环依赖问题 (WebUI.Core ↔ WebUI.Data)
- ✅ 修复密码哈希不匹配 (BCrypt → PBKDF2)
- ✅ 添加 CORS 支持 localhost:5174
- ✅ 实现数据库自动种子数据
- ✅ 验证登录功能正常工作
