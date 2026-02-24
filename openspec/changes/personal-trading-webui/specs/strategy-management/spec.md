# Spec: Strategy Management（策略管理）

## ADDED Requirements

### Requirement: List all strategies
系统必须显示用户的所有策略列表。

#### Scenario: Display strategy list
- **WHEN** 用户打开策略管理页面
- **THEN** 系统显示所有策略（名称、状态、最后运行时间、累计收益）

#### Scenario: Empty strategy list
- **WHEN** 用户尚未创建任何策略
- **THEN** 系统显示"暂无策略，点击创建策略"提示

#### Scenario: Filter strategies by status
- **WHEN** 用户选择"仅显示运行中"
- **THEN** 系统仅显示状态为"运行中"的策略

### Requirement: Create new strategy
系统必须支持创建新策略。

#### Scenario: Create strategy from template
- **WHEN** 用户选择模板（如"均线策略"）并填写名称
- **THEN** 系统创建策略配置文件并保存到数据库

#### Scenario: Create strategy from scratch
- **WHEN** 用户选择"自定义策略"并上传 Python/C# 代码文件
- **THEN** 系统保存代码文件到 `Strategies/` 目录并创建策略记录

#### Scenario: Duplicate name validation
- **WHEN** 用户输入已存在的策略名称
- **THEN** 系统显示错误"策略名称已存在，请使用其他名称"

### Requirement: Edit strategy configuration
系统必须支持编辑策略的配置参数。

#### Scenario: Edit strategy parameters
- **WHEN** 用户打开策略编辑页面并修改参数
- **THEN** 系统保存参数到配置文件

#### Scenario: Prevent editing running strategy
- **WHEN** 用户尝试编辑正在运行的策略
- **THEN** 系统显示警告"请先停止策略再编辑"

#### Scenario: Validate parameter values
- **WHEN** 用户输入超出范围的参数（如止损比例 > 100%）
- **THEN** 系统显示验证错误"参数值超出允许范围"

### Requirement: Delete strategy
系统必须支持删除策略。

#### Scenario: Delete stopped strategy
- **WHEN** 用户选择已停止的策略并点击"删除"
- **THEN** 系统弹出确认框"确认删除策略 [名称]？此操作不可恢复"

#### Scenario: Confirm deletion
- **WHEN** 用户确认删除
- **THEN** 系统删除策略配置、代码文件和数据库记录

#### Scenario: Prevent deleting running strategy
- **WHEN** 用户尝试删除正在运行的策略
- **THEN** 系统拒绝并显示"请先停止策略再删除"

#### Scenario: Archive instead of delete
- **WHEN** 用户选择"归档"而非"删除"
- **THEN** 系统将策略标记为已归档（不显示在列表中但保留数据）

### Requirement: Clone strategy
系统必须支持复制策略。

#### Scenario: Clone existing strategy
- **WHEN** 用户选择一个策略并点击"克隆"
- **THEN** 系统创建副本（名称为"原名称 - 副本"），复制所有配置和代码

#### Scenario: Edit cloned strategy independently
- **WHEN** 用户修改克隆的策略
- **THEN** 原策略不受影响

### Requirement: Display strategy performance summary
系统必须显示策略的性能摘要。

#### Scenario: View cumulative return
- **WHEN** 用户查看策略摘要
- **THEN** 系统显示策略的累计收益率

#### Scenario: View win rate
- **WHEN** 用户查看策略统计
- **THEN** 系统显示胜率（盈利交易数 / 总交易数）

#### Scenario: View Sharpe ratio
- **WHEN** 用户查看策略风险指标
- **THEN** 系统显示夏普比率（风险调整后收益）

#### Scenario: View max drawdown
- **WHEN** 用户查看策略风险
- **THEN** 系统显示最大回撤（历史最大下跌幅度）

### Requirement: Manage strategy versions
系统必须支持策略的版本控制。

#### Scenario: Save strategy version
- **WHEN** 用户保存策略代码修改
- **THEN** 系统创建新版本记录（版本号、时间、变更说明）

#### Scenario: View version history
- **WHEN** 用户查看策略版本历史
- **THEN** 系统显示所有版本列表（版本号、日期、描述）

#### Scenario: Rollback to previous version
- **WHEN** 用户选择历史版本并点击"恢复"
- **THEN** 系统将策略代码回滚到该版本

#### Scenario: Compare versions
- **WHEN** 用户选择两个版本进行对比
- **THEN** 系统显示代码差异（diff）

### Requirement: Tag strategies
系统必须支持为策略添加标签。

#### Scenario: Add tags to strategy
- **WHEN** 用户添加标签（如"趋势跟踪"、"高频"）
- **THEN** 系统保存标签到策略记录

#### Scenario: Filter by tags
- **WHEN** 用户点击某个标签
- **THEN** 系统显示所有包含该标签的策略

#### Scenario: Suggest popular tags
- **WHEN** 用户输入标签
- **THEN** 系统显示已存在的常用标签供快速选择

### Requirement: Export and import strategies
系统必须支持策略的导出和导入。

#### Scenario: Export strategy package
- **WHEN** 用户点击"导出策略"
- **THEN** 系统生成 ZIP 文件（包含代码、配置、说明文档）

#### Scenario: Import strategy package
- **WHEN** 用户上传策略 ZIP 文件
- **THEN** 系统解压并创建新策略记录

#### Scenario: Validate imported package
- **WHEN** 上传的文件格式不正确
- **THEN** 系统拒绝导入并显示"文件格式无效"

### Requirement: Manage strategy permissions
系统必须支持策略的权限控制（为未来多用户扩展预留）。

#### Scenario: Set strategy visibility
- **WHEN** 用户设置策略为"私有"
- **THEN** 系统标记策略仅对创建者可见

#### Scenario: Share strategy (future)
- **WHEN** 用户设置策略为"公开"（预留功能）
- **THEN** 系统允许其他用户查看但不能修改

### Requirement: Display strategy dependencies
系统必须显示策略的依赖项。

#### Scenario: View required indicators
- **WHEN** 用户查看策略依赖
- **THEN** 系统显示策略使用的技术指标（如 MACD、RSI）

#### Scenario: View required data
- **WHEN** 用户查看策略数据需求
- **THEN** 系统显示所需的历史数据范围和频率（如"1 年日线数据"）

#### Scenario: Check missing dependencies
- **WHEN** 策略缺少必需的数据或库
- **THEN** 系统显示警告"缺少依赖：[依赖名称]"
