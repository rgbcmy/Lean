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
