# Spec: Backtesting System（回测系统）

## ADDED Requirements

### Requirement: Configure backtest parameters
系统必须提供回测参数配置界面。

#### Scenario: Set date range
- **WHEN** 用户设置回测开始和结束日期
- **THEN** 系统验证日期范围有效（结束日期 > 开始日期）

#### Scenario: Set initial capital
- **WHEN** 用户输入初始资金（如 $100,000）
- **THEN** 系统保存初始资金到回测配置

#### Scenario: Select benchmark
- **WHEN** 用户选择基准指数（如 SPY）
- **THEN** 系统在回测结果中包含基准对比

#### Scenario: Configure data resolution
- **WHEN** 用户选择数据频率（日线、小时线、分钟线）
- **THEN** 系统使用对应频率的历史数据进行回测

### Requirement: Run backtest
系统必须支持执行策略回测。

#### Scenario: Start backtest
- **WHEN** 用户选择策略和参数并点击"开始回测"
- **THEN** 系统启动 Lean 回测进程并显示"回测进行中..."

#### Scenario: Display backtest progress
- **WHEN** 回测运行时
- **THEN** 系统显示进度条（已处理天数 / 总天数）

#### Scenario: Backtest completed
- **WHEN** 回测成功完成
- **THEN** 系统解析结果并显示摘要（总收益、夏普比率、最大回撤）

#### Scenario: Backtest failed
- **WHEN** 回测过程中出错（如数据缺失、代码错误）
- **THEN** 系统显示错误信息并提供日志下载

### Requirement: Display backtest results
系统必须显示完整的回测结果。

#### Scenario: View performance metrics
- **WHEN** 用户查看回测结果
- **THEN** 系统显示关键指标：总收益率、年化收益率、夏普比率、最大回撤、胜率

#### Scenario: View equity curve
- **WHEN** 用户查看收益曲线
- **THEN** 系统显示账户净值随时间的变化图表

#### Scenario: View drawdown chart
- **WHEN** 用户查看回撤图
- **THEN** 系统显示历史回撤幅度和持续时间

#### Scenario: View trade list
- **WHEN** 用户查看交易记录
- **THEN** 系统显示回测期间所有交易（日期、股票、方向、价格、盈亏）

### Requirement: Compare backtest runs
系统必须支持多个回测结果的对比。

#### Scenario: Select runs to compare
- **WHEN** 用户选择 2-4 个回测结果进行对比
- **THEN** 系统并排显示各指标（收益率、夏普比率、最大回撤）

#### Scenario: Compare equity curves
- **WHEN** 用户查看对比图表
- **THEN** 系统在同一图表中显示多条收益曲线

#### Scenario: Highlight best run
- **WHEN** 对比多个回测结果
- **THEN** 系统标记最优结果（如最高夏普比率）为绿色

### Requirement: Optimize strategy parameters
系统必须支持参数优化（网格搜索）。

#### Scenario: Define parameter ranges
- **WHEN** 用户设置参数优化范围（如止损比例 5%-20%，步长 5%）
- **THEN** 系统生成参数组合列表

#### Scenario: Run optimization
- **WHEN** 用户启动参数优化
- **THEN** 系统并行运行多个回测（每个参数组合一次）

#### Scenario: Display optimization results
- **WHEN** 优化完成
- **THEN** 系统显示所有参数组合的结果表格（按收益率排序）

#### Scenario: Visualize parameter space
- **WHEN** 用户查看优化结果
- **THEN** 系统显示热力图（参数 vs 收益率）

### Requirement: Save backtest results
系统必须保存回测结果到数据库。

#### Scenario: Auto-save on completion
- **WHEN** 回测完成
- **THEN** 系统自动保存结果到数据库（策略ID、参数、日期范围、指标）

#### Scenario: View historical backtests
- **WHEN** 用户查看策略的回测历史
- **THEN** 系统显示该策略的所有历史回测记录

#### Scenario: Delete old backtest results
- **WHEN** 用户删除历史回测记录
- **THEN** 系统从数据库中删除该记录

### Requirement: Export backtest report
系统必须支持导出回测报告。

#### Scenario: Export PDF report
- **WHEN** 用户点击"导出 PDF"
- **THEN** 系统生成格式化的 PDF 报告（包含图表、指标、交易列表）

#### Scenario: Export CSV trades
- **WHEN** 用户导出交易记录
- **THEN** 系统生成 CSV 文件（包含所有交易详情）

#### Scenario: Export JSON results
- **WHEN** 用户导出原始数据
- **THEN** 系统生成 JSON 文件（包含完整回测数据）

### Requirement: Display benchmark comparison
系统必须支持与基准的对比分析。

#### Scenario: Compare to SPY
- **WHEN** 用户选择 SPY 作为基准
- **THEN** 系统显示策略与 SPY 的收益率对比

#### Scenario: Display alpha and beta
- **WHEN** 用户查看基准对比
- **THEN** 系统计算并显示 Alpha（超额收益）和 Beta（系统风险）

#### Scenario: Show relative performance
- **WHEN** 用户查看相对表现
- **THEN** 系统显示策略跑赢/跑输基准的百分比

### Requirement: Analyze trade statistics
系统必须提供详细的交易统计分析。

#### Scenario: View average trade P&L
- **WHEN** 用户查看交易统计
- **THEN** 系统显示平均每笔交易盈亏

#### Scenario: View holding period
- **WHEN** 用户查看持仓分析
- **THEN** 系统显示平均持仓天数

#### Scenario: View win/loss ratio
- **WHEN** 用户查看胜率分析
- **THEN** 系统显示盈利交易数、亏损交易数、胜率

#### Scenario: View profit factor
- **WHEN** 用户查看盈利因子
- **THEN** 系统显示总盈利 / 总亏损的比值

### Requirement: Detect overfitting
系统必须提供过拟合检测工具。

#### Scenario: Walk-forward analysis
- **WHEN** 用户启用滚动窗口测试
- **THEN** 系统将数据分段（如每 6 个月为一段）并测试策略稳定性

#### Scenario: Out-of-sample testing
- **WHEN** 用户划分样本内/样本外数据
- **THEN** 系统分别测试并对比样本内和样本外表现

#### Scenario: Warn about overfitting
- **WHEN** 样本外收益显著低于样本内
- **THEN** 系统显示警告"策略可能存在过拟合风险"
