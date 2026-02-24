/**
 * Strategies API
 * 策略 API 接口
 */

import { apiClient } from './apiClient';
import type {
  Strategy,
  CreateStrategyRequest,
  UpdateStrategyRequest,
  StrategyExecutionRequest,
  StrategyTemplate,
  StrategyVersion,
  StrategyExecution,
  StrategyPerformance,
  CloneStrategyRequest,
  ExportStrategyResponse,
  ImportStrategyRequest,
} from '../types/strategy';

/**
 * Get all strategies
 * 获取所有策略列表
 */
export async function getStrategies(params?: {
  status?: string;
  tags?: string[];
  includeArchived?: boolean;
}): Promise<Strategy[]> {
  const response = await apiClient.get<Strategy[]>('/api/v1/strategies', {
    params,
  });
  return response.data;
}

/**
 * Get strategy by ID
 * 根据ID获取策略详情
 */
export async function getStrategyById(strategyId: string): Promise<Strategy> {
  const response = await apiClient.get<Strategy>(
    `/api/v1/strategies/${strategyId}`
  );
  return response.data;
}

/**
 * Create a new strategy
 * 创建新策略
 */
export async function createStrategy(
  request: CreateStrategyRequest
): Promise<Strategy> {
  const response = await apiClient.post<Strategy>(
    '/api/v1/strategies',
    request
  );
  return response.data;
}

/**
 * Update strategy
 * 更新策略
 */
export async function updateStrategy(
  strategyId: string,
  request: UpdateStrategyRequest
): Promise<Strategy> {
  const response = await apiClient.put<Strategy>(
    `/api/v1/strategies/${strategyId}`,
    request
  );
  return response.data;
}

/**
 * Delete strategy
 * 删除策略
 */
export async function deleteStrategy(strategyId: string): Promise<void> {
  await apiClient.delete(`/api/v1/strategies/${strategyId}`);
}

/**
 * Start strategy execution
 * 启动策略执行
 */
export async function startStrategy(
  strategyId: string,
  request?: StrategyExecutionRequest
): Promise<{ executionId: string }> {
  const response = await apiClient.post<{ executionId: string }>(
    `/api/v1/strategies/${strategyId}/start`,
    request || {}
  );
  return response.data;
}

/**
 * Stop strategy execution
 * 停止策略执行
 */
export async function stopStrategy(strategyId: string): Promise<void> {
  await apiClient.post(`/api/v1/strategies/${strategyId}/stop`);
}

/**
 * Clone strategy
 * 克隆策略
 */
export async function cloneStrategy(
  strategyId: string,
  request: CloneStrategyRequest
): Promise<Strategy> {
  const response = await apiClient.post<Strategy>(
    `/api/v1/strategies/${strategyId}/clone`,
    request
  );
  return response.data;
}

/**
 * Get strategy versions
 * 获取策略版本列表
 */
export async function getStrategyVersions(
  strategyId: string
): Promise<StrategyVersion[]> {
  const response = await apiClient.get<StrategyVersion[]>(
    `/api/v1/strategies/${strategyId}/versions`
  );
  return response.data;
}

/**
 * Restore strategy to a specific version
 * 恢复策略到指定版本
 */
export async function restoreStrategyVersion(
  strategyId: string,
  versionId: string
): Promise<Strategy> {
  const response = await apiClient.post<Strategy>(
    `/api/v1/strategies/${strategyId}/versions/${versionId}/restore`
  );
  return response.data;
}

/**
 * Get strategy executions history
 * 获取策略执行历史
 */
export async function getStrategyExecutions(
  strategyId: string
): Promise<StrategyExecution[]> {
  const response = await apiClient.get<StrategyExecution[]>(
    `/api/v1/strategies/${strategyId}/executions`
  );
  return response.data;
}

/**
 * Get strategy performance summary
 * 获取策略性能摘要
 */
export async function getStrategyPerformance(
  strategyId: string
): Promise<StrategyPerformance> {
  const response = await apiClient.get<StrategyPerformance>(
    `/api/v1/strategies/${strategyId}/performance`
  );
  return response.data;
}

/**
 * Get available strategy templates
 * 获取可用的策略模板列表
 */
export async function getStrategyTemplates(): Promise<StrategyTemplate[]> {
  const response = await apiClient.get<StrategyTemplate[]>(
    '/api/v1/strategies/templates'
  );
  return response.data;
}

/**
 * Export strategy
 * 导出策略
 */
export async function exportStrategy(
  strategyId: string
): Promise<ExportStrategyResponse> {
  const response = await apiClient.get<ExportStrategyResponse>(
    `/api/v1/strategies/${strategyId}/export`
  );
  return response.data;
}

/**
 * Import strategy
 * 导入策略
 */
export async function importStrategy(
  request: ImportStrategyRequest
): Promise<Strategy> {
  const response = await apiClient.post<Strategy>(
    '/api/v1/strategies/import',
    request
  );
  return response.data;
}

/**
 * Archive strategy
 * 归档策略
 */
export async function archiveStrategy(strategyId: string): Promise<void> {
  await apiClient.post(`/api/v1/strategies/${strategyId}/archive`);
}

/**
 * Unarchive strategy
 * 取消归档策略
 */
export async function unarchiveStrategy(strategyId: string): Promise<void> {
  await apiClient.post(`/api/v1/strategies/${strategyId}/unarchive`);
}
