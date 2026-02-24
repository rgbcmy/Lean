/**
 * Strategy Types
 * 策略类型定义
 */

/**
 * Strategy status
 * 策略状态
 */
export enum StrategyStatus {
  /** 已停止 */
  Stopped = 'Stopped',
  /** 运行中 */
  Running = 'Running',
  /** 错误 */
  Error = 'Error',
  /** 已暂停 */
  Paused = 'Paused',
}

/**
 * Strategy interface
 * 策略接口
 */
export interface Strategy {
  /** 策略ID */
  id: string;
  /** 策略名称 */
  name: string;
  /** 策略描述 */
  description?: string;
  /** 策略状态 */
  status: StrategyStatus;
  /** 策略类型（模板名称或"自定义"） */
  strategyType?: string;
  /** 创建时间 */
  createdAt: string;
  /** 更新时间 */
  updatedAt: string;
  /** 最后运行时间 */
  lastRunAt?: string;
  /** 累计收益率 */
  cumulativeReturn?: number;
  /** 夏普比率 */
  sharpeRatio?: number;
  /** 最大回撤 */
  maxDrawdown?: number;
  /** 胜率 */
  winRate?: number;
  /** 总交易次数 */
  totalTrades?: number;
  /** 标签 */
  tags?: string[];
  /** 版本号 */
  version?: number;
  /** 是否归档 */
  isArchived?: boolean;
  /** 配置参数（JSON字符串） */
  parameters?: string;
  /** 代码文件路径 */
  codeFilePath?: string;
  /** 配置文件路径 */
  configFilePath?: string;
}

/**
 * Create strategy request
 * 创建策略请求
 */
export interface CreateStrategyRequest {
  /** 策略名称 */
  name: string;
  /** 策略描述 */
  description?: string;
  /** 策略类型（模板名称或"自定义"） */
  strategyType?: string;
  /** 配置参数（JSON对象） */
  parameters?: Record<string, any>;
  /** 标签 */
  tags?: string[];
  /** 代码文件内容（base64编码） */
  codeFileContent?: string;
  /** 代码文件名 */
  codeFileName?: string;
}

/**
 * Update strategy request
 * 更新策略请求
 */
export interface UpdateStrategyRequest {
  /** 策略名称 */
  name?: string;
  /** 策略描述 */
  description?: string;
  /** 配置参数（JSON对象） */
  parameters?: Record<string, any>;
  /** 标签 */
  tags?: string[];
}

/**
 * Strategy execution request
 * 策略执行请求
 */
export interface StrategyExecutionRequest {
  /** 策略ID */
  strategyId: string;
  /** 执行参数（运行时覆盖） */
  overrideParameters?: Record<string, any>;
}

/**
 * Strategy template
 * 策略模板
 */
export interface StrategyTemplate {
  /** 模板ID */
  id: string;
  /** 模板名称 */
  name: string;
  /** 模板描述 */
  description: string;
  /** 模板分类 */
  category?: string;
  /** 默认参数 */
  defaultParameters: Record<string, any>;
  /** 参数模式定义 */
  parameterSchema?: Record<string, any>;
}

/**
 * Strategy version
 * 策略版本
 */
export interface StrategyVersion {
  /** 版本ID */
  id: string;
  /** 策略ID */
  strategyId: string;
  /** 版本号 */
  versionNumber: number;
  /** 变更说明 */
  changeDescription?: string;
  /** 创建时间 */
  createdAt: string;
  /** 代码文件路径 */
  codeFilePath?: string;
  /** 配置参数（JSON字符串） */
  parameters?: string;
}

/**
 * Strategy execution history
 * 策略执行历史
 */
export interface StrategyExecution {
  /** 执行ID */
  id: string;
  /** 策略ID */
  strategyId: string;
  /** 开始时间 */
  startTime: string;
  /** 结束时间 */
  endTime?: string;
  /** 执行状态 */
  status: 'Running' | 'Completed' | 'Failed' | 'Stopped';
  /** 收益率 */
  returnRate?: number;
  /** 交易次数 */
  tradeCount?: number;
  /** 错误信息 */
  errorMessage?: string;
}

/**
 * Strategy log entry
 * 策略日志条目
 */
export interface StrategyLogEntry {
  /** 日志ID */
  id: string;
  /** 策略ID */
  strategyId: string;
  /** 时间戳 */
  timestamp: string;
  /** 日志级别 */
  level: 'Trace' | 'Debug' | 'Info' | 'Warning' | 'Error';
  /** 日志消息 */
  message: string;
}

/**
 * Strategy performance summary
 * 策略性能摘要
 */
export interface StrategyPerformance {
  /** 策略ID */
  strategyId: string;
  /** 累计收益率 */
  cumulativeReturn: number;
  /** 年化收益率 */
  annualizedReturn: number;
  /** 夏普比率 */
  sharpeRatio: number;
  /** 索提诺比率 */
  sortinoRatio?: number;
  /** 最大回撤 */
  maxDrawdown: number;
  /** 胜率 */
  winRate: number;
  /** 总交易次数 */
  totalTrades: number;
  /** 盈利交易次数 */
  profitTrades: number;
  /** 亏损交易次数 */
  lossTrades: number;
  /** 平均盈利 */
  averageWin?: number;
  /** 平均亏损 */
  averageLoss?: number;
}

/**
 * Clone strategy request
 * 克隆策略请求
 */
export interface CloneStrategyRequest {
  /** 新策略名称 */
  name: string;
  /** 新策略描述 */
  description?: string;
}

/**
 * Export strategy response
 * 导出策略响应
 */
export interface ExportStrategyResponse {
  /** 文件名 */
  fileName: string;
  /** 文件内容（base64编码） */
  fileContent: string;
  /** 文件大小（字节） */
  fileSize: number;
}

/**
 * Import strategy request
 * 导入策略请求
 */
export interface ImportStrategyRequest {
  /** 文件内容（base64编码） */
  fileContent: string;
  /** 文件名 */
  fileName: string;
}
