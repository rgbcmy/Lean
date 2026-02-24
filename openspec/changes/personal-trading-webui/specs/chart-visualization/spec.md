# Spec: Chart Visualization（图表可视化）

## ADDED Requirements

### Requirement: Display candlestick charts
系统必须显示K线图（蜡烛图）。

#### Scenario: Render daily candlesticks
- **WHEN** 用户查看股票日K线
- **THEN** 系统使用 ECharts 显示 OHLC 数据（红涨绿跌）

#### Scenario: Zoom and pan chart
- **WHEN** 用户在图表上滚动鼠标滚轮或拖拽
- **THEN** 系统支持缩放时间范围和平移查看不同日期

#### Scenario: Switch timeframes
- **WHEN** 用户选择时间周期（日线、周线、月线）
- **THEN** 系统重新加载对应周期的K线数据

#### Scenario: Display volume bars
- **WHEN** K线图显示时
- **THEN** 系统在下方显示成交量柱状图（同步时间轴）

### Requirement: Add technical indicators
系统必须支持在K线图上叠加技术指标。

#### Scenario: Add moving average
- **WHEN** 用户添加移动平均线（如 MA20、MA50）
- **THEN** 系统在K线图上绘制对应的均线

#### Scenario: Add MACD indicator
- **WHEN** 用户添加 MACD 指标
- **THEN** 系统在下方新增面板显示 MACD 线、信号线、柱状图

#### Scenario: Add RSI indicator
- **WHEN** 用户添加 RSI 指标
- **THEN** 系统显示 RSI 曲线和超买/超卖线（70/30）

#### Scenario: Customize indicator parameters
- **WHEN** 用户修改指标参数（如 MA 周期改为 30）
- **THEN** 系统重新计算并更新图表

#### Scenario: Remove indicator
- **WHEN** 用户删除某个指标
- **THEN** 系统从图表中移除该指标

### Requirement: Display portfolio performance chart
系统必须显示投资组合的收益曲线。

#### Scenario: Equity curve
- **WHEN** 用户查看账户收益曲线
- **THEN** 系统显示账户净值随时间变化的折线图

#### Scenario: Benchmark overlay
- **WHEN** 用户启用基准对比
- **THEN** 系统在同一图表中叠加基准指数曲线（如 SPY）

#### Scenario: Drawdown shading
- **WHEN** 用户查看回撤
- **THEN** 系统用红色阴影标记回撤区域

#### Scenario: Highlight trades
- **WHEN** 用户启用交易标记
- **THEN** 系统在曲线上标注买入（绿色箭头）和卖出（红色箭头）点

### Requirement: Display position allocation charts
系统必须显示持仓分配图表。

#### Scenario: Portfolio pie chart
- **WHEN** 用户查看持仓分布
- **THEN** 系统显示饼图（每个股票占总市值百分比）

#### Scenario: Sector allocation chart
- **WHEN** 用户查看行业分布
- **THEN** 系统显示按行业分类的柱状图或饼图

#### Scenario: Asset class breakdown
- **WHEN** 用户查看资产类别
- **THEN** 系统显示股票、ETF、现金的占比

#### Scenario: Interactive chart
- **WHEN** 用户点击图表某个部分
- **THEN** 系统导航到该股票的详情页

### Requirement: Display real-time price charts
系统必须显示实时价格变化图表。

#### Scenario: Live price line
- **WHEN** 用户查看实时行情
- **THEN** 系统使用 Lightweight Charts 显示分时图（价格随时间变化）

#### Scenario: Update chart on new tick
- **WHEN** 收到新的行情数据
- **THEN** 系统实时追加新数据点到图表

#### Scenario: Display bid/ask spread
- **WHEN** 用户查看实时买卖价
- **THEN** 系统在图表上显示买卖价差区域

### Requirement: Display P&L charts
系统必须显示盈亏分析图表。

#### Scenario: Daily P&L bar chart
- **WHEN** 用户查看每日盈亏
- **THEN** 系统显示柱状图（每天的盈亏金额，盈利为绿色、亏损为红色）

#### Scenario: Cumulative P&L line chart
- **WHEN** 用户查看累计盈亏
- **THEN** 系统显示累计盈亏曲线

#### Scenario: P&L by symbol
- **WHEN** 用户查看各股票盈亏贡献
- **THEN** 系统显示水平柱状图（每只股票的盈亏对比）

### Requirement: Display order execution charts
系统必须显示订单执行分析图表。

#### Scenario: Price vs execution time
- **WHEN** 用户查看订单执行质量
- **THEN** 系统显示散点图（提交时间 vs 成交价）

#### Scenario: Slippage distribution
- **WHEN** 用户查看滑点分析
- **THEN** 系统显示直方图（滑点金额的分布）

#### Scenario: Fill rate over time
- **WHEN** 用户查看成交率
- **THEN** 系统显示折线图（每日订单成交率）

### Requirement: Export charts
系统必须支持导出图表。

#### Scenario: Export chart as PNG
- **WHEN** 用户点击"保存图片"
- **THEN** 系统将当前图表导出为 PNG 文件

#### Scenario: Export chart as SVG
- **WHEN** 用户选择导出矢量图
- **THEN** 系统导出 SVG 格式（适合打印）

#### Scenario: Copy chart to clipboard
- **WHEN** 用户点击"复制图表"
- **THEN** 系统将图表复制到剪贴板（可粘贴到文档）

### Requirement: Responsive chart design
系统必须确保图表在不同设备上正常显示。

#### Scenario: Desktop view
- **WHEN** 用户在桌面浏览器查看图表
- **THEN** 系统显示完整图表（包含所有工具栏）

#### Scenario: Tablet view
- **WHEN** 用户在平板上查看图表
- **THEN** 系统自动调整图表尺寸和布局

#### Scenario: Mobile view
- **WHEN** 用户在手机浏览器查看图表
- **THEN** 系统显示简化版图表（触摸友好的交互）

### Requirement: Customizable chart appearance
系统必须支持图表外观定制。

#### Scenario: Switch color scheme
- **WHEN** 用户选择深色/浅色主题
- **THEN** 系统切换图表配色方案

#### Scenario: Adjust chart colors
- **WHEN** 用户修改涨跌颜色（如改为绿涨红跌）
- **THEN** 系统应用新配色到所有图表

#### Scenario: Save chart layout
- **WHEN** 用户调整图表布局（如指标位置）
- **THEN** 系统保存布局偏好到用户配置

### Requirement: Performance optimization
系统必须优化大数据量图表的渲染性能。

#### Scenario: Large dataset rendering
- **WHEN** 显示超过 10,000 个数据点
- **THEN** 系统采用降采样或虚拟化技术，保持流畅渲染

#### Scenario: Lazy load historical data
- **WHEN** 用户滚动查看更早的历史数据
- **THEN** 系统按需加载历史数据（不一次性加载全部）

#### Scenario: Debounce real-time updates
- **WHEN** 实时数据快速变化
- **THEN** 系统限制图表更新频率（如最多 10 FPS）
