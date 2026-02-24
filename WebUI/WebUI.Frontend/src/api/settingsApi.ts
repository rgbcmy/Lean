/**
 * Settings API Client
 * 系统设置 API 客户端
 */

import { apiClient } from './apiClient';
import type { SystemSettings, SettingsUpdateRequest } from '../types/settings';

/**
 * Get current system settings
 * 获取当前系统设置 — maps simplified backend response to frontend SystemSettings type
 */
export async function getSettings(): Promise<SystemSettings> {
  const response = await apiClient.get<any>('/api/v1/settings');
  const data = response.data;

  // Map backend flat IBKR/theme/language shape to SystemSettings
  return {
    ibkr: {
      host: data.ibkr?.host ?? '127.0.0.1',
      port: data.ibkr?.port ?? 4002,
      clientId: data.ibkr?.clientId ?? 1,
      accountId: data.ibkr?.accountId ?? '',
      usePaperTrading: data.ibkr?.usePaperTrading ?? true,
      autoReconnect: data.ibkr?.autoReconnect ?? true,
      reconnectIntervalSeconds: data.ibkr?.reconnectIntervalSeconds ?? 30,
      heartbeatIntervalSeconds: data.ibkr?.heartbeatIntervalSeconds ?? 60,
    },
    database: {
      provider: 'sqlite',
      connectionString: '',
      autoMigrate: true,
      backupEnabled: false,
      backupIntervalHours: 24,
    },
    theme: {
      mode: data.theme?.mode ?? 'light',
      primaryColor: data.theme?.primaryColor ?? '#1890ff',
      accentColor: data.theme?.accentColor ?? '#52c41a',
      chartColorScheme: data.theme?.chartColorScheme ?? 'default',
      priceColorMode: data.theme?.priceColorMode ?? 'chinese',
    },
    language: {
      locale: data.language?.locale ?? 'zh-CN',
      dateFormat: data.language?.dateFormat ?? 'YYYY-MM-DD',
      timeFormat: data.language?.timeFormat ?? '24h',
      currencyFormat: data.language?.currencyFormat ?? '$',
      numberFormat: {
        decimalSeparator: '.',
        thousandsSeparator: ',',
      },
    },
    notifications: {
      email: {
        enabled: data.notifications?.email?.enabled ?? false,
        address: data.notifications?.email?.address ?? '',
        events: {
          orderFilled: data.notifications?.email?.events?.orderFilled ?? true,
          orderCanceled: data.notifications?.email?.events?.orderCanceled ?? true,
          stopLossTriggered: data.notifications?.email?.events?.stopLossTriggered ?? true,
          takeProfitTriggered: data.notifications?.email?.events?.takeProfitTriggered ?? true,
          marginCall: data.notifications?.email?.events?.marginCall ?? true,
          strategyError: data.notifications?.email?.events?.strategyError ?? true,
          dailyReport: data.notifications?.email?.events?.dailyReport ?? false,
        },
      },
      push: {
        enabled: data.notifications?.push?.enabled ?? false,
        events: {
          orderFilled: data.notifications?.push?.events?.orderFilled ?? true,
          orderCanceled: data.notifications?.push?.events?.orderCanceled ?? false,
          stopLossTriggered: data.notifications?.push?.events?.stopLossTriggered ?? true,
          marginCall: data.notifications?.push?.events?.marginCall ?? true,
          riskAlert: data.notifications?.push?.events?.riskAlert ?? true,
        },
      },
      inApp: {
        enabled: data.notifications?.inApp?.enabled ?? true,
        sound: data.notifications?.inApp?.sound ?? true,
        events: { all: data.notifications?.inApp?.events?.all ?? true },
      },
    },
    updatedAt: data.updatedAt,
  };
}

/**
 * Update system settings
 * 更新系统设置
 */
export async function updateSettings(settings: SettingsUpdateRequest): Promise<SystemSettings> {
  await apiClient.put('/api/v1/settings', settings);
  return getSettings();
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
 * Test database connection — not yet available on backend
 * 测试数据库连接（后端暂不支持）
 */
export async function testDatabaseConnection(_config: {
  provider: 'postgresql' | 'sqlite';
  connectionString: string;
}): Promise<{ success: boolean; message: string }> {
  return { success: true, message: '数据库连接测试暂不支持。' };
}

/**
 * Get database backup status — not yet available
 */
export async function getDatabaseBackupStatus(): Promise<{
  lastBackup: string | null;
  nextBackup: string | null;
  backupCount: number;
}> {
  return { lastBackup: null, nextBackup: null, backupCount: 0 };
}

/**
 * Trigger manual database backup — not yet available
 */
export async function triggerDatabaseBackup(): Promise<{ success: boolean; message: string }> {
  return { success: false, message: '数据库备份暂不支持。' };
}

