/**
 * Positions API
 * 持仓 API 接口
 */

import { apiClient } from './apiClient';
import type {
  Position,
  PositionDetail,
  PortfolioSummary,
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
  const response = await apiClient.get<Position[]>('/api/v1/positions', {
    params,
  });
  return response.data;
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
 * Get portfolio summary
 * 获取投资组合摘要
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
  const response = await apiClient.get<AllocationItem[]>(
    '/api/v1/portfolio/allocation'
  );
  return response.data;
}

/**
 * Get sector allocation
 * 获取行业配置
 */
export async function getSectorAllocation(): Promise<AllocationItem[]> {
  const response = await apiClient.get<AllocationItem[]>(
    '/api/v1/portfolio/allocation/sector'
  );
  return response.data;
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
 * 导出持仓数据
 */
export async function exportPositions(
  request: ExportPositionRequest
): Promise<Blob> {
  const response = await apiClient.post(
    '/api/v1/portfolio/export',
    request,
    {
      responseType: 'blob',
    }
  );
  return response.data;
}

/**
 * Get today's P&L
 * 获取今日盈亏
 */
export async function getTodayPnL(): Promise<number> {
  const response = await apiClient.get<{ todayPnL: number }>(
    '/api/v1/portfolio/today-pnl'
  );
  return response.data.todayPnL;
}

/**
 * Get total return
 * 获取总收益率
 */
export async function getTotalReturn(): Promise<number> {
  const response = await apiClient.get<{ totalReturn: number }>(
    '/api/v1/portfolio/total-return'
  );
  return response.data.totalReturn;
}
