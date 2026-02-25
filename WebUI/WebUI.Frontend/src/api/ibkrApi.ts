/**
 * IBKR Connection API Client
 * IBKR 连接 API 客户端
 */

import { apiClient } from './apiClient';

export interface IbkrConnectionStatus {
  status: 'Disconnected' | 'Connecting' | 'Connected' | 'Reconnecting' | 'Error';
  accountId: string;
  accountType: 'Paper' | 'Live';
  lastError?: string;
  connectedAt?: string;
  lastHeartbeatAt?: string;
}

/**
 * GET /api/v1/ibkr/status — returns the real connection state from the backend service.
 * 获取后端 IBKR 连接的实际状态
 */
export async function getIbkrStatus(): Promise<IbkrConnectionStatus> {
  const response = await apiClient.get<IbkrConnectionStatus>('/api/v1/ibkr/status');
  return response.data;
}

export interface IbkrConnectRequest {
  host: string;
  port: number;
  clientId: number;
  accountId: string;
  accountType: 'Paper' | 'Live';
  enableAutoReconnect?: boolean;
}

/**
 * POST /api/v1/ibkr/connect — establish the actual IBKR connection.
 * 建立 IBKR 实际连接
 */
export async function connectIbkr(config: IbkrConnectRequest): Promise<IbkrConnectionStatus> {
  const response = await apiClient.post<IbkrConnectionStatus>('/api/v1/ibkr/connect', config);
  return response.data;
}

/**
 * POST /api/v1/ibkr/disconnect — disconnect from IBKR.
 * 断开 IBKR 连接
 */
export async function disconnectIbkr(): Promise<IbkrConnectionStatus> {
  const response = await apiClient.post<IbkrConnectionStatus>('/api/v1/ibkr/disconnect');
  return response.data;
}

export interface IbkrSyncResult {
  success: boolean;
  positionsSynced: number;
  positionsAdded: number;
  positionsUpdated: number;
  positionsRemoved: number;
  cashBalance: number;
  netLiquidation: number;
  syncedAt: string;
  message: string;
}

/**
 * POST /api/v1/ibkr/sync-positions
 * Fetch all positions from IBKR TWS and store them in the local database.
 * 从 IBKR TWS 获取所有持仓并保存到本地数据库
 */
export async function syncIbkrPositions(): Promise<IbkrSyncResult> {
  const response = await apiClient.post<IbkrSyncResult>('/api/v1/ibkr/sync-positions');
  return response.data;
}
