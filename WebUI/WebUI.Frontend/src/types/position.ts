/**
 * Position Types
 * 持仓类型定义
 */

export interface Position {
  symbol: string;
  quantity: number;
  averageCost: number;
  currentPrice: number;
  marketValue: number;
  unrealizedPnL: number;
  unrealizedPnLPercent: number;
  sector?: string;
  firstPurchaseDate: Date;
  holdingDays: number;
}

export interface PositionDetail extends Position {
  buyHistory: Transaction[];
  sellHistory: Transaction[];
  realizedPnL: number;
}

export interface Transaction {
  transactionId: string;
  date: Date;
  quantity: number;
  price: number;
  commission: number;
  pnL?: number; // Only for sell transactions
}

/**
 * Portfolio summary — matches backend PortfolioPositionsResponse
 * 投资组合摘要 — 与后端 PortfolioPositionsResponse 完全匹配
 */
export interface PortfolioSummary {
  /** Positions list / 持仓列表 */
  positions: Position[];
  /** Total market value / 总市值 */
  totalMarketValue: number;
  /** Total cost basis / 总成本 */
  totalCostBasis: number;
  /** Total unrealized P&L / 总未实现盈亏 */
  totalUnrealizedPnL: number;
  /** Total unrealized P&L percent / 总未实现盈亏百分比 */
  totalUnrealizedPnLPercent: number;
  /** Total realized P&L / 总已实现盈亏 */
  totalRealizedPnL: number;
  /** Cash balance / 现金余额 */
  cashBalance: number;
  /** Total portfolio value (positions + cash) / 总资产（持仓+现金） */
  totalPortfolioValue: number;
}

/**
 * Allocation item — matches backend AllocationItem
 * 配置项 — 与后端 AllocationItem 完全匹配
 */
export interface AllocationItem {
  /** Symbol or category name / 股票代码或类别名称 */
  name: string;
  /** Market value / 市值 */
  value: number;
  /** Percentage of total portfolio / 占总投资组合的百分比 */
  percentage: number;
  /** Unrealized P&L / 未实现盈亏 */
  unrealizedPnL: number;
}

/**
 * Portfolio allocation response — matches backend PortfolioAllocationResponse
 * 投资组合配置响应 — 与后端完全匹配
 */
export interface PortfolioAllocationResponse {
  positions: AllocationItem[];
  cash: AllocationItem;
  totalValue: number;
}

export interface EquityCurveData {
  date: string;
  equity: number;
  deposits: number;
  withdrawals: number;
}

export interface ExportPositionRequest {
  format: 'csv' | 'excel';
  includeHistory?: boolean;
}

