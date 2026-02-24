/**
 * Backtest-related type definitions
 * 回测相关类型定义
 */

/**
 * Backtest status enum
 * 回测状态枚举
 */
export type BacktestStatus = 'Pending' | 'Running' | 'Completed' | 'Failed' | 'Cancelled';

/**
 * Backtest summary DTO
 * 回测摘要数据传输对象
 */
export interface BacktestSummary {
  id: number;
  name: string;
  strategyId: number;
  strategyName: string;
  status: BacktestStatus;
  startDate: string;
  endDate: string;
  initialCapital: number;
  totalReturn?: number;
  sharpeRatio?: number;
  maxDrawdown?: number;
  createdAt: string;
  completedAt?: string;
}

/**
 * Backtest detail DTO
 * 回测详情数据传输对象
 */
export interface BacktestDetail {
  id: number;
  name: string;
  strategyId: number;
  strategyName: string;
  status: BacktestStatus;
  startDate: string;
  endDate: string;
  initialCapital: number;
  benchmark?: string;
  dataResolution?: string;
  parameters?: Record<string, any>;
  
  // Performance metrics
  totalReturn?: number;
  annualizedReturn?: number;
  sharpeRatio?: number;
  sortinoRatio?: number;
  maxDrawdown?: number;
  maxDrawdownPercent?: number;
  winRate?: number;
  profitFactor?: number;
  totalTrades?: number;
  averageTradeReturn?: number;
  averageHoldingPeriod?: number;
  
  // Benchmark comparison
  alpha?: number;
  beta?: number;
  benchmarkReturn?: number;
  
  // Timing
  createdAt: string;
  startedAt?: string;
  completedAt?: string;
  executionTime?: number;
  
  // Error info
  errorMessage?: string;
}

/**
 * Backtest equity curve point
 * 回测收益曲线数据点
 */
export interface EquityCurvePoint {
  time: string;
  equity: number;
  benchmarkEquity?: number;
}

/**
 * Backtest drawdown point
 * 回测回撤数据点
 */
export interface DrawdownPoint {
  time: string;
  drawdown: number;
  drawdownPercent: number;
}

/**
 * Backtest trade
 * 回测交易记录
 */
export interface BacktestTrade {
  id: string;
  time: string;
  symbol: string;
  direction: 'Buy' | 'Sell';
  quantity: number;
  price: number;
  value: number;
  commission: number;
  profitLoss?: number;
  profitLossPercent?: number;
}

/**
 * Backtest charts data
 * 回测图表数据
 */
export interface BacktestCharts {
  equityCurve: EquityCurvePoint[];
  drawdown: DrawdownPoint[];
  trades: BacktestTrade[];
}

/**
 * Create backtest request
 * 创建回测请求
 */
export interface CreateBacktestRequest {
  name: string;
  strategyId: number;
  startDate: string;
  endDate: string;
  initialCapital: number;
  benchmark?: string;
  dataResolution?: string;
  parameters?: Record<string, any>;
}

/**
 * Update backtest request
 * 更新回测请求
 */
export interface UpdateBacktestRequest {
  name?: string;
  parameters?: Record<string, any>;
}

/**
 * Backtest status response
 * 回测状态响应
 */
export interface BacktestStatusResponse {
  backtestId: number;
  status: BacktestStatus;
  progress?: number;
  currentDate?: string;
  message?: string;
  error?: string;
}

/**
 * Compare backtests request
 * 对比回测请求
 */
export interface CompareBacktestsRequest {
  backtestIds: number[];
}

/**
 * Backtest comparison item
 * 回测对比项
 */
export interface BacktestComparison {
  backtest: BacktestSummary;
  totalReturn: number;
  annualizedReturn: number;
  sharpeRatio: number;
  maxDrawdown: number;
  winRate: number;
  profitFactor: number;
  totalTrades: number;
}

/**
 * Compare backtests response
 * 对比回测响应
 */
export interface CompareBacktestsResponse {
  comparisons: BacktestComparison[];
  bestByMetric: {
    totalReturn: number;
    sharpeRatio: number;
    maxDrawdown: number;
  };
}

/**
 * Parameter range for optimization
 * 参数优化范围
 */
export interface ParameterRange {
  name: string;
  min: number;
  max: number;
  step: number;
}

/**
 * Optimize parameters request
 * 参数优化请求
 */
export interface OptimizeParametersRequest {
  name: string;
  strategyId: number;
  startDate: string;
  endDate: string;
  initialCapital: number;
  parameters: ParameterRange[];
  optimizationTarget?: 'TotalReturn' | 'SharpeRatio' | 'MaxDrawdown';
}

/**
 * Optimization result item
 * 优化结果项
 */
export interface OptimizationResult {
  parameters: Record<string, number>;
  totalReturn: number;
  sharpeRatio: number;
  maxDrawdown: number;
  winRate: number;
  score: number;
}

/**
 * Optimize parameters response
 * 参数优化响应
 */
export interface OptimizeParametersResponse {
  optimizationId: string;
  status: 'Running' | 'Completed' | 'Failed';
  totalCombinations: number;
  completedCombinations: number;
  results?: OptimizationResult[];
  bestResult?: OptimizationResult;
}

/**
 * Export backtest request
 * 导出回测请求
 */
export interface ExportBacktestRequest {
  backtestId: number;
  format: 'PDF' | 'CSV' | 'JSON';
  includeCharts?: boolean;
  includeTrades?: boolean;
}

/**
 * Backtest list response
 * 回测列表响应
 */
export interface BacktestListResponse {
  backtests: BacktestSummary[];
  total: number;
}
