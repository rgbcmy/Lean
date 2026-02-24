/**
 * Backtests API
 * 回测 API 接口
 */

import { apiClient } from './apiClient';
import type {
  BacktestSummary,
  BacktestDetail,
  CreateBacktestRequest,
  UpdateBacktestRequest,
  BacktestStatusResponse,
  CompareBacktestsRequest,
  CompareBacktestsResponse,
  OptimizeParametersRequest,
  OptimizeParametersResponse,
  BacktestCharts,
  BacktestListResponse,
} from '../types/backtest';

/**
 * Get all backtests
 * 获取所有回测列表
 */
export async function getBacktests(params?: {
  strategyId?: number;
  status?: string;
}): Promise<BacktestListResponse> {
  const response = await apiClient.get<BacktestListResponse>('/api/v1/backtests', {
    params,
  });
  return response.data;
}

/**
 * Get backtest by ID
 * 根据ID获取回测详情
 */
export async function getBacktestById(backtestId: number): Promise<BacktestDetail> {
  const response = await apiClient.get<BacktestDetail>(
    `/api/v1/backtests/${backtestId}`
  );
  return response.data;
}

/**
 * Create a new backtest
 * 创建新回测
 */
export async function createBacktest(
  request: CreateBacktestRequest
): Promise<BacktestDetail> {
  const response = await apiClient.post<BacktestDetail>(
    '/api/v1/backtests',
    request
  );
  return response.data;
}

/**
 * Update backtest
 * 更新回测
 */
export async function updateBacktest(
  backtestId: number,
  request: UpdateBacktestRequest
): Promise<BacktestDetail> {
  const response = await apiClient.put<BacktestDetail>(
    `/api/v1/backtests/${backtestId}`,
    request
  );
  return response.data;
}

/**
 * Delete backtest
 * 删除回测
 */
export async function deleteBacktest(backtestId: number): Promise<void> {
  await apiClient.delete(`/api/v1/backtests/${backtestId}`);
}

/**
 * Start backtest execution
 * 开始回测执行
 */
export async function startBacktest(backtestId: number): Promise<{ message: string }> {
  const response = await apiClient.post<{ message: string }>(
    `/api/v1/backtests/${backtestId}/start`
  );
  return response.data;
}

/**
 * Stop backtest execution
 * 停止回测执行
 */
export async function stopBacktest(backtestId: number): Promise<{ message: string }> {
  const response = await apiClient.post<{ message: string }>(
    `/api/v1/backtests/${backtestId}/stop`
  );
  return response.data;
}

/**
 * Get backtest execution status
 * 获取回测执行状态
 */
export async function getBacktestStatus(backtestId: number): Promise<BacktestStatusResponse> {
  const response = await apiClient.get<BacktestStatusResponse>(
    `/api/v1/backtests/${backtestId}/status`
  );
  return response.data;
}

/**
 * Get backtest charts data
 * 获取回测图表数据
 */
export async function getBacktestCharts(backtestId: number): Promise<BacktestCharts> {
  const response = await apiClient.get<BacktestCharts>(
    `/api/v1/backtests/${backtestId}/charts`
  );
  return response.data;
}

/**
 * Compare multiple backtests
 * 对比多个回测
 */
export async function compareBacktests(
  request: CompareBacktestsRequest
): Promise<CompareBacktestsResponse> {
  const response = await apiClient.post<CompareBacktestsResponse>(
    '/api/v1/backtests/compare',
    request
  );
  return response.data;
}

/**
 * Start parameter optimization
 * 开始参数优化
 */
export async function optimizeParameters(
  request: OptimizeParametersRequest
): Promise<OptimizeParametersResponse> {
  const response = await apiClient.post<OptimizeParametersResponse>(
    '/api/v1/backtests/optimize',
    request
  );
  return response.data;
}

/**
 * Get optimization results
 * 获取优化结果
 */
export async function getOptimization(optimizationId: string): Promise<OptimizeParametersResponse> {
  const response = await apiClient.get<OptimizeParametersResponse>(
    `/api/v1/backtests/optimizations/${optimizationId}`
  );
  return response.data;
}

/**
 * Export backtest report
 * 导出回测报告
 */
export async function exportBacktest(
  backtestId: number,
  format: 'PDF' | 'CSV' | 'JSON'
): Promise<Blob> {
  const response = await apiClient.get(
    `/api/v1/backtests/${backtestId}/export`,
    {
      params: { format },
      responseType: 'blob',
    }
  );
  return response.data;
}
