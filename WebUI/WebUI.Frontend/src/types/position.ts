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

export interface PortfolioSummary {
  totalMarketValue: number;
  totalCost: number;
  totalUnrealizedPnL: number;
  totalUnrealizedPnLPercent: number;
  cashBalance: number;
  todayPnL: number;
  totalReturn: number;
  positionCount: number;
}

export interface AllocationItem {
  symbol: string;
  value: number;
  percentage: number;
  sector?: string;
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
