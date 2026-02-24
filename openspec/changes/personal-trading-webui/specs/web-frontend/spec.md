# Spec: Web Frontend（Web 前端应用）

## ADDED Requirements

### Requirement: Responsive layout
系统必须提供响应式布局适配不同屏幕尺寸。

#### Scenario: Desktop layout
- **WHEN** 用户在桌面浏览器（宽度 > 1280px）访问
- **THEN** 系统显示完整布局（侧边栏 + 主内容区）

#### Scenario: Tablet layout
- **WHEN** 用户在平板设备（768px < 宽度 < 1280px）访问
- **THEN** 系统显示可折叠侧边栏

#### Scenario: Mobile layout
- **WHEN** 用户在手机（宽度 < 768px）访问
- **THEN** 系统显示汉堡菜单和单列布局

#### Scenario: Orientation change
- **WHEN** 用户旋转设备（横屏/竖屏切换）
- **THEN** 系统自动调整布局

### Requirement: Navigation structure
系统必须提供清晰的导航结构。

#### Scenario: Main navigation menu
- **WHEN** 用户登录后
- **THEN** 显示主菜单（首页、交易、持仓、订单、策略、回测、设置）

#### Scenario: Breadcrumb navigation
- **WHEN** 用户浏览多层页面（如 策略 > 详情 > 编辑）
- **THEN** 显示面包屑导航

#### Scenario: Quick actions
- **WHEN** 用户查看首页
- **THEN** 显示快速操作按钮（新建策略、快速下单、查看持仓）

#### Scenario: User menu
- **WHEN** 用户点击头像
- **THEN** 显示下拉菜单（个人设置、修改密码、登出）

### Requirement: Theme and styling
系统必须提供一致的视觉风格。

#### Scenario: Ant Design components
- **WHEN** 构建 UI 组件
- **THEN** 使用 Ant Design 组件库（Button, Table, Form, Modal 等）

#### Scenario: Light theme (default)
- **WHEN** 用户首次访问
- **THEN** 应用浅色主题（白色背景、灰色文字）

#### Scenario: Dark theme
- **WHEN** 用户切换到深色模式
- **THEN** 应用深色主题（深灰背景、浅色文字）

#### Scenario: Save theme preference
- **WHEN** 用户切换主题
- **THEN** 系统保存偏好到 localStorage，下次访问自动应用

### Requirement: Chinese localization
系统必须提供完整的中文界面。

#### Scenario: All UI text in Chinese
- **WHEN** 用户使用界面
- **THEN** 所有按钮、标签、提示文字显示中文

#### Scenario: Chinese date/time format
- **WHEN** 显示日期时间
- **THEN** 使用中文格式（如"2026年2月15日 14:30"）

#### Scenario: Currency format
- **WHEN** 显示金额
- **THEN** 使用逗号分隔千位（如"¥12,345.67"或"$12,345.67"）

#### Scenario: Number localization
- **WHEN** 显示数字
- **THEN** 使用中文习惯（涨跌颜色红涨绿跌，可配置）

### Requirement: State management
系统必须使用 Zustand 管理全局状态。

#### Scenario: User state
- **WHEN** 用户登录
- **THEN** 系统将用户信息存储到 Zustand store（用户名、角色等）

#### Scenario: Connection status
- **WHEN** IBKR 连接状态变化
- **THEN** 更新全局状态并触发 UI 更新

#### Scenario: Persist state
- **WHEN** 用户刷新页面
- **THEN** 从 localStorage 恢复部分状态（如主题偏好）

#### Scenario: Clear state on logout
- **WHEN** 用户登出
- **THEN** 清除所有敏感状态（持仓、订单等）

### Requirement: Form validation
系统必须在客户端验证表单输入。

#### Scenario: Required field validation
- **WHEN** 用户提交表单但缺少必填项
- **THEN** 显示错误提示"请填写 [字段名]"

#### Scenario: Real-time validation
- **WHEN** 用户输入数据
- **THEN** 实时显示验证状态（绿色勾号/红色错误）

#### Scenario: Custom validation rules
- **WHEN** 用户输入股票数量
- **THEN** 验证必须为正整数

#### Scenario: Display validation summary
- **WHEN** 表单存在多个错误
- **THEN** 在顶部显示错误摘要列表

### Requirement: Loading states
系统必须提供清晰的加载状态反馈。

#### Scenario: Spinner for async operations
- **WHEN** 发起 API 请求
- **THEN** 显示加载动画（Spin 组件）

#### Scenario: Skeleton screens
- **WHEN** 加载列表数据（如订单列表）
- **THEN** 显示骨架屏占位符

#### Scenario: Progress bar for long operations
- **WHEN** 执行耗时操作（如回测）
- **THEN** 显示进度条和预估时间

#### Scenario: Disable buttons during loading
- **WHEN** 请求进行中
- **THEN** 禁用提交按钮防止重复提交

### Requirement: Error handling
系统必须友好地显示错误信息。

#### Scenario: API error notification
- **WHEN** API 请求失败
- **THEN** 显示 Toast 通知（Ant Design Message 组件）

#### Scenario: Validation error inline display
- **WHEN** 表单验证失败
- **THEN** 在对应输入框下方显示红色错误文字

#### Scenario: Network error retry
- **WHEN** 网络请求失败
- **THEN** 显示"网络错误，点击重试"按钮

#### Scenario: Fallback UI
- **WHEN** 页面渲染崩溃
- **THEN** 显示 Error Boundary 页面"出错了，请刷新页面"

### Requirement: Real-time data updates
系统必须通过 SignalR 接收实时数据。

#### Scenario: Establish SignalR connection
- **WHEN** 用户登录后
- **THEN** 前端建立 SignalR WebSocket 连接

#### Scenario: Subscribe to market data
- **WHEN** 用户打开股票详情页
- **THEN** 前端订阅该股票的实时行情推送

#### Scenario: Update UI on new data
- **WHEN** 收到 SignalR 推送
- **THEN** 前端更新 UI（如实时价格、订单状态）

#### Scenario: Reconnect on disconnect
- **WHEN** SignalR 连接断开
- **THEN** 前端自动尝试重连并恢复订阅

### Requirement: Performance optimization
系统必须优化前端性能。

#### Scenario: Code splitting
- **WHEN** 用户访问不同页面
- **THEN** 只加载当前页面所需的 JS 代码（懒加载）

#### Scenario: Virtualized lists
- **WHEN** 显示大量数据（如 1000+ 条订单）
- **THEN** 使用虚拟滚动只渲染可见行

#### Scenario: Memoize components
- **WHEN** 渲染复杂组件
- **THEN** 使用 React.memo 避免不必要的重新渲染

#### Scenario: Debounce search input
- **WHEN** 用户输入搜索关键词
- **THEN** 延迟 300ms 后再发起搜索请求

### Requirement: Accessibility
系统必须支持基本的无障碍访问。

#### Scenario: Keyboard navigation
- **WHEN** 用户使用键盘（Tab 键）
- **THEN** 可以导航到所有交互元素

#### Scenario: Screen reader support
- **WHEN** 使用屏幕阅读器
- **THEN** 所有按钮和输入框有 aria-label

#### Scenario: Focus indicators
- **WHEN** 元素获得焦点
- **THEN** 显示明显的焦点边框

#### Scenario: Color contrast
- **WHEN** 设计 UI
- **THEN** 确保文字和背景对比度符合 WCAG AA 标准

### Requirement: Data tables
系统必须提供功能丰富的数据表格。

#### Scenario: Sortable columns
- **WHEN** 用户点击表头
- **THEN** 表格按该列升序/降序排序

#### Scenario: Filterable columns
- **WHEN** 用户点击筛选图标
- **THEN** 显示筛选条件输入框

#### Scenario: Resizable columns
- **WHEN** 用户拖动列边界
- **THEN** 调整列宽

#### Scenario: Export to CSV
- **WHEN** 用户点击"导出"
- **THEN** 下载当前表格数据的 CSV 文件

### Requirement: Notifications
系统必须提供多种通知方式。

#### Scenario: Toast notifications
- **WHEN** 发生事件（如订单成交）
- **THEN** 右上角显示 Toast 通知（3 秒后自动消失）

#### Scenario: Badge notifications
- **WHEN** 有未读消息
- **THEN** 菜单图标显示红色数字徽章

#### Scenario: Notification center
- **WHEN** 用户点击通知图标
- **THEN** 显示最近 20 条通知列表

#### Scenario: Mark as read
- **WHEN** 用户点击某条通知
- **THEN** 标记为已读并跳转到相关页面

### Requirement: Confirm dialogs
系统必须在危险操作前请求确认。

#### Scenario: Confirm before closing position
- **WHEN** 用户点击"平仓"
- **THEN** 显示确认对话框"确认以市价卖出全部持仓？"

#### Scenario: Confirm before deleting strategy
- **WHEN** 用户删除策略
- **THEN** 显示确认对话框"确认删除？此操作不可恢复"

#### Scenario: Confirm on unsaved changes
- **WHEN** 用户修改表单后尝试离开页面
- **THEN** 显示"有未保存的修改，确认离开？"

### Requirement: Offline detection
系统必须检测网络状态。

#### Scenario: Show offline banner
- **WHEN** 用户网络离线
- **THEN** 页面顶部显示黄色横幅"网络已断开"

#### Scenario: Hide offline banner on reconnect
- **WHEN** 网络恢复
- **THEN** 隐藏横幅并显示"网络已恢复"提示

#### Scenario: Disable actions when offline
- **WHEN** 用户离线时
- **THEN** 禁用所有交易操作按钮

### Requirement: User preferences
系统必须保存用户偏好设置。

#### Scenario: Save table column visibility
- **WHEN** 用户隐藏某列
- **THEN** 保存设置到 localStorage，下次访问保持

#### Scenario: Save default timeframe
- **WHEN** 用户在 K线图选择日线/周线
- **THEN** 保存为默认，下次打开自动应用

#### Scenario: Save notification preferences
- **WHEN** 用户关闭某类通知
- **THEN** 以后不再显示该类型通知

### Requirement: Help and documentation
系统必须提供帮助文档。

#### Scenario: Help icon on pages
- **WHEN** 用户点击页面右上角"?"图标
- **THEN** 显示该页面的使用说明

#### Scenario: Inline tooltips
- **WHEN** 用户鼠标悬停在某个字段
- **THEN** 显示 Tooltip 解释该字段含义

#### Scenario: Link to full documentation
- **WHEN** 用户点击"查看完整文档"
- **THEN** 打开在线帮助文档（中英文）

#### Scenario: Keyboard shortcuts guide
- **WHEN** 用户按 ? 键
- **THEN** 显示快捷键列表（如 Ctrl+K 快速搜索）
