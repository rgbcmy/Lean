/**
 * Risk Control API Client
 * 风险控制 API 客户端
 */

import { apiClient } from './apiClient';
import type { RiskConfig, RiskMetrics, RiskAlert, RiskReport } from '../types/risk';

/**
 * Get current risk configuration
 * 获取当前风险配置
 */
export async function getRiskConfig(): Promise<RiskConfig> {
  const response = await apiClient.get<RiskConfig>('/api/v1/risk/config');
  return response.data;
}

/**
 * Update risk configuration
 * 更新风险配置
 */
export async function updateRiskConfig(config: Partial<RiskConfig>): Promise<RiskConfig> {
  const response = await apiClient.put<RiskConfig>('/api/v1/risk/config', config);
  return response.data;
}

/**
 * Get current risk metrics
 * 获取当前风险指标
 */
export async function getRiskMetrics(): Promise<RiskMetrics> {
  const response = await apiClient.get<RiskMetrics>('/api/v1/risk/metrics');
  return response.data;
}

/**
 * Get risk alerts
 * 获取风险告警
 */
export async function getRiskAlerts(
  acknowledged?: boolean,
  limit?: number
): Promise<RiskAlert[]> {
  const params = new URLSearchParams();
  if (acknowledged !== undefined) {
    params.append('acknowledged', acknowledged.toString());
  }
  if (limit !== undefined) {
    params.append('limit', limit.toString());
  }
  
  const response = await apiClient.get<RiskAlert[]>(
    `/api/v1/risk/alerts?${params.toString()}`
  );
  return response.data;
}

/**
 * Acknowledge a risk alert
 * 确认风险告警
 */
export async function acknowledgeRiskAlert(alertId: string): Promise<void> {
  await apiClient.post(`/api/v1/risk/alerts/${alertId}/acknowledge`);
}

/**
 * Get risk report
 * 获取风险报告
 */
export async function getRiskReport(date?: string): Promise<RiskReport> {
  const url = date 
    ? `/api/v1/risk/report?date=${date}`
    : '/api/v1/risk/report';
  const response = await apiClient.get<RiskReport>(url);
  return response.data;
}

/**
 * Export risk report as PDF
 * 导出风险报告为 PDF
 */
export async function exportRiskReport(date?: string): Promise<Blob> {
  const url = date 
    ? `/api/v1/risk/report/export?date=${date}`
    : '/api/v1/risk/report/export';
  const response = await apiClient.get(url, {
    responseType: 'blob',
  });
  return response.data;
}
