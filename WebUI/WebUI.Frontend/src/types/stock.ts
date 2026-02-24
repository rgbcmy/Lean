/**
 * Stock Types
 * 股票类型定义
 */

export interface Stock {
  symbol: string;
  name: string;
  exchange: string;
  lastPrice?: number;
  change?: number;
  changePercent?: number;
  volume?: number;
  marketStatus?: 'open' | 'closed' | 'pre-market' | 'after-hours';
}

export interface StockQuote extends Stock {
  companyName?: string; // Alias for name
  lastPrice: number;
  change?: number;
  changePercent?: number;
  volume?: number;
  bid?: number;
  bidPrice?: number;
  ask?: number;
  askPrice?: number;
  bidSize?: number;
  askSize?: number;
  high?: number;
  low?: number;
  open?: number;
  previousClose?: number;
  lastUpdate?: Date;
  delayed?: boolean;
  // ETF-specific properties
  nav?: number; // Net Asset Value
  navChange?: number; // NAV change percent
}
