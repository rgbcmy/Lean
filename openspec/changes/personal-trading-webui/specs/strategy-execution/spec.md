# Spec: Strategy Execution（策略执行）

## ADDED Requirements

### Requirement: Start Lean strategy
系统必须支持启动 Lean 策略引擎。

#### Scenario: Start strategy with config
- **WHEN** 用户选择一个策略并点击"启动"
- **THEN** 系统通过 Process.Start() 启动 Lean 进程并传递策略配置文件路径

#### Scenario: Strategy already running
- **WHEN** 用户尝试启动已在运行的策略
- **THEN** 系统显示错误"策略正在运行中，请先停止"

#### Scenario: Invalid strategy config
- **WHEN** 策略配置文件缺失或格式错误
- **THEN** 系统拒绝启动并显示"配置文件无效：[错误详情]"

### Requirement: Stop running strategy
系统必须支持停止正在运行的策略。

#### Scenario: Graceful shutdown
- **WHEN** 用户点击"停止"按钮
- **THEN** 系统发送停止信号到 Lean 进程，等待其优雅退出（最多 30 秒）

#### Scenario: Force kill if not responding
- **WHEN** Lean 进程 30 秒内未响应停止信号
- **THEN** 系统强制终止进程（Process.Kill()）

#### Scenario: Clean up after stop
- **WHEN** 策略停止后
- **THEN** 系统更新策略状态为"已停止"并记录停止时间

### Requirement: Monitor strategy process health
系统必须监控 Lean 策略进程的健康状态。

#### Scenario: Health check via IPC
- **WHEN** 策略运行时
- **THEN** 系统每 10 秒通过 IPC（Named Pipe 或 TCP）发送心跳请求

#### Scenario: Detect process crash
- **WHEN** Lean 进程意外退出（返回非零退出码）
- **THEN** 系统检测到崩溃，更新策略状态为"已崩溃"并发送告警

#### Scenario: Auto-restart on crash
- **WHEN** 策略启用了自动重启选项且进程崩溃
- **THEN** 系统等待 5 秒后自动重启策略

### Requirement: Stream strategy logs
系统必须实时流式传输策略日志到 WebUI。

#### Scenario: Subscribe to strategy logs
- **WHEN** 用户打开策略详情页
- **THEN** 系统订阅该策略的日志流并通过 SignalR 推送到前端

#### Scenario: Unsubscribe when leaving page
- **WHEN** 用户离开策略详情页
- **THEN** 系统取消日志流订阅

#### Scenario: Display log levels
- **WHEN** 策略输出日志
- **THEN** 系统根据日志级别（DEBUG/INFO/WARN/ERROR）显示不同颜色

#### Scenario: Filter logs by level
- **WHEN** 用户选择仅显示 ERROR 级别日志
- **THEN** 系统过滤并仅显示错误日志

### Requirement: Pass parameters to strategy
系统必须支持向策略传递参数。

#### Scenario: Override default parameters
- **WHEN** 用户在启动策略前修改参数（如止损比例、持仓上限）
- **THEN** 系统将参数写入配置文件或通过命令行参数传递给 Lean

#### Scenario: Validate parameter types
- **WHEN** 用户输入无效的参数值（如字符串传给数字参数）
- **THEN** 系统显示验证错误"参数 [名称] 必须为数字"

#### Scenario: Save parameter preset
- **WHEN** 用户保存当前参数配置为预设
- **THEN** 系统保存参数集到数据库，下次可快速加载

### Requirement: Display strategy execution status
系统必须显示策略的实时执行状态。

#### Scenario: Show running indicator
- **WHEN** 策略正在运行
- **THEN** 系统显示绿色"运行中"指示器和运行时长

#### Scenario: Show strategy statistics
- **WHEN** 用户查看运行中的策略
- **THEN** 系统显示订单数、成交数、持仓数、当前盈亏

#### Scenario: Show CPU and memory usage
- **WHEN** 用户查看策略资源占用
- **THEN** 系统显示 Lean 进程的 CPU 和内存使用率

### Requirement: Handle strategy errors
系统必须处理策略执行过程中的错误。

#### Scenario: Runtime exception in strategy
- **WHEN** 策略代码抛出未捕获异常
- **THEN** 系统捕获异常信息并显示在日志中，策略自动停止

#### Scenario: Broker connection lost
- **WHEN** 策略运行中 IBKR 连接断开
- **THEN** 系统暂停策略执行，等待连接恢复或通知用户

#### Scenario: Insufficient margin
- **WHEN** 策略尝试下单但保证金不足
- **THEN** Lean 拒绝订单，WebUI 显示错误"保证金不足"

### Requirement: Schedule strategy execution
系统必须支持定时启动策略。

#### Scenario: Schedule daily start
- **WHEN** 用户设置策略每天 9:25 自动启动
- **THEN** 系统在指定时间自动启动策略

#### Scenario: Schedule weekly start
- **WHEN** 用户设置策略每周一 9:00 启动
- **THEN** 系统在每周一的指定时间启动策略

#### Scenario: Cancel scheduled start
- **WHEN** 用户取消定时任务
- **THEN** 系统移除调度，不再自动启动

### Requirement: Isolate strategy processes
系统必须确保多个策略进程相互隔离。

#### Scenario: Run multiple strategies
- **WHEN** 用户同时运行多个策略
- **THEN** 每个策略在独立的 Lean 进程中运行，互不影响

#### Scenario: One strategy crash doesn't affect others
- **WHEN** 一个策略进程崩溃
- **THEN** 其他策略继续正常运行

### Requirement: Record strategy execution history
系统必须记录策略的执行历史。

#### Scenario: Save execution record
- **WHEN** 策略停止（正常或异常）
- **THEN** 系统保存执行记录到数据库（开始时间、结束时间、盈亏、订单数）

#### Scenario: View execution history
- **WHEN** 用户查看策略历史
- **THEN** 系统显示该策略的所有历史执行记录

#### Scenario: Compare execution runs
- **WHEN** 用户选择两次执行记录对比
- **THEN** 系统并排显示两次运行的参数、结果、收益曲线
