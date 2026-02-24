/**
 * Positions API
 * 持仓 API 接口
 */

import { apiClient } from './apiClient';
import type {
  Position,
  PositionDetail,
  PortfolioSummary,
  PortfolioAllocationResponse,
  AllocationItem,
  EquityCurveData,
  ExportPositionRequest,
} from '../types/position';

/**
 * Get all positions
 * 获取所有持仓
 */
export async function getPositions(params?: {
  symbol?: string;
  sortBy?: 'pnl' | 'value' | 'symbol';
  sortOrder?: 'asc' | 'desc';
  onlyProfitable?: boolean;
}): Promise<Position[]> {
  // Backend returns PortfolioPositionsResponse {positions: [...], totalMarketValue, ...}
  const response = await apiClient.get<PortfolioSummary>('/api/v1/positions', {
    params,
  });
  return response.data.positions ?? [];
}

/**
 * Get position detail by symbol
 * 根据股票代码获取持仓详情
 */
export async function getPositionDetail(symbol: string): Promise<PositionDetail> {
  const response = await apiClient.get<PositionDetail>(
    `/api/v1/positions/${symbol}`
  );
  return response.data;
}

/**
 * Close position (sell all)
 * 平仓（卖出全部）
 */
export async function closePosition(
  symbol: string,
  quantity?: number
): Promise<{ orderId: string }> {
  const response = await apiClient.post<{ orderId: string }>(
    `/api/v1/positions/${symbol}/close`,
    { quantity }
  );
  return response.data;
}

/**
 * Get portfolio summary (positions + aggregate totals)
 * 获取投资组合摘要（持仓+汇总数据）
 */
export async function getPortfolioSummary(): Promise<PortfolioSummary> {
  const response = await apiClient.get<PortfolioSummary>(
    '/api/v1/portfolio/summary'
  );
  return response.data;
}

/**
 * Get position allocation (pie chart data)
 * 获取持仓配置（饼图数据）
 */
export async function getPositionAllocation(): Promise<AllocationItem[]> {
  const response = await apiClient.get<PortfolioAllocationResponse>(
    '/api/v1/portfolio/allocation'
  );
  // Include cash in the allocation list
  const { positions, cash } = response.data;
  return cash.value > 0 ? [...positions, cash] : positions;
}

/**
 * Get sector allocation — derived from position allocation since backend does
 * not expose a dedicated sector endpoint.
 * 获取行业配置 — 从持仓配置推导（后端无独立行业端点）
 */
export async function getSectorAllocation(): Promise<AllocationItem[]> {
  // Fallback: return an empty array; real sector data requires IBKR integration
  return [];
}

/**
 * Get equity curve data
 * 获取账户收益曲线数据
 */
export async function getEquityCurve(params?: {
  startDate?: string;
  endDate?: string;
}): Promise<EquityCurveData[]> {
  const response = await apiClient.get<EquityCurveData[]>(
    '/api/v1/portfolio/equity-curve',
    { params }
  );
  return response.data;
}

/**
 * Export positions to CSV/Excel
 * 导出持仓数据 — uses the positions export endpoint
 */
export async function exportPositions(
  request: ExportPositionRequest
): Promise<Blob> {
  const response = await apiClient.get(
    '/api/v1/positions/export',
    {
      params: { format: request.format },
      responseType: 'blob',
    }
  );
  return response.data;
}

/**
 * Get today's P&L — derived from portfolio summary (totalRealizedPnL)
 * 获取今日盈亏 — 从投资组合摘要推导
 * @deprecated use getPortfolioSummary().totalRealizedPnL instead
 */
export async function getTodayPnL(): Promise<number> {
  const summary = await getPortfolioSummary();
  return summary.totalRealizedPnL;
}

/**
 * Get total return percentage — derived from portfolio summary
 * 获取总收益率
 * @deprecated use getPortfolioSummary().totalUnrealizedPnLPercent instead
 */
export async function getTotalReturn(): Promise<number> {
  const summary = await getPortfolioSummary();
  return summary.totalUnrealizedPnLPercent;
}
