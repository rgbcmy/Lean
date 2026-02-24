/**
 * Settings API Client
 * 系统设置 API 客户端
 */

import { apiClient } from './apiClient';
import type { SystemSettings, SettingsUpdateRequest } from '../types/settings';

/**
 * Get current system settings
 * 获取当前系统设置
 */
export async function getSettings(): Promise<SystemSettings> {
  const response = await apiClient.get<SystemSettings>('/api/v1/settings');
  return response.data;
}

/**
 * Update system settings
 * 更新系统设置
 */
export async function updateSettings(settings: SettingsUpdateRequest): Promise<SystemSettings> {
  const response = await apiClient.put<SystemSettings>('/api/v1/settings', settings);
  return response.data;
}

/**
 * Test IBKR connection with provided configuration
 * 测试 IBKR 连接
 */
export async function testIBKRConnection(config: {
  host: string;
  port: number;
  clientId: number;
}): Promise<{ success: boolean; message: string }> {
  const response = await apiClient.post<{ success: boolean; message: string }>(
    '/api/v1/settings/ibkr/test',
    config
  );
  return response.data;
}

/**
 * Test database connection
 * 测试数据库连接
 */
export async function testDatabaseConnection(config: {
  provider: 'postgresql' | 'sqlite';
  connectionString: string;
}): Promise<{ success: boolean; message: string }> {
  const response = await apiClient.post<{ success: boolean; message: string }>(
    '/api/v1/settings/database/test',
    config
  );
  return response.data;
}

/**
 * Get database backup status
 * 获取数据库备份状态
 */
export async function getDatabaseBackupStatus(): Promise<{
  lastBackup: string | null;
  nextBackup: string | null;
  backupCount: number;
}> {
  const response = await apiClient.get('/api/v1/settings/database/backup-status');
  return response.data;
}

/**
 * Trigger manual database backup
 * 触发手动数据库备份
 */
export async function triggerDatabaseBackup(): Promise<{ success: boolean; message: string }> {
  const response = await apiClient.post<{ success: boolean; message: string }>(
    '/api/v1/settings/database/backup'
  );
  return response.data;
}
