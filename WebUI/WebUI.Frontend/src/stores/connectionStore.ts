/**
 * Connection Store - IBKR and SignalR connection state
 * 连接状态管理
 */

import { create } from 'zustand';

export const ConnectionStatus = {
  Disconnected: 'disconnected',
  Connecting: 'connecting',
  Connected: 'connected',
  Reconnecting: 'reconnecting',
  Failed: 'failed',
} as const;

export type ConnectionStatus = typeof ConnectionStatus[keyof typeof ConnectionStatus];

interface IbkrConnectionState {
  status: ConnectionStatus;
  accountId: string | null;
  isPaperTrading: boolean;
  lastHeartbeat: Date | null;
  errorMessage: string | null;
}

interface SignalRConnectionState {
  status: ConnectionStatus;
  errorMessage: string | null;
}

interface ConnectionState {
  // IBKR connection state
  ibkr: IbkrConnectionState;
  
  // SignalR connection state
  signalR: SignalRConnectionState;
  
  // Update IBKR connection status
  setIbkrStatus: (status: ConnectionStatus, errorMessage?: string | null) => void;
  
  // Set IBKR connection info
  setIbkrConnectionInfo: (accountId: string, isPaperTrading: boolean) => void;
  
  // Update IBKR heartbeat
  updateIbkrHeartbeat: () => void;
  
  // Update SignalR connection status
  setSignalRStatus: (status: ConnectionStatus, errorMessage?: string | null) => void;
  
  // Reset all connections
  resetConnections: () => void;
}

export const useConnectionStore = create<ConnectionState>((set) => ({
  ibkr: {
    status: ConnectionStatus.Disconnected,
    accountId: null,
    isPaperTrading: false,
    lastHeartbeat: null,
    errorMessage: null,
  },
  
  signalR: {
    status: ConnectionStatus.Disconnected,
    errorMessage: null,
  },
  
  setIbkrStatus: (status, errorMessage = null) => set((state) => ({
    ibkr: {
      ...state.ibkr,
      status,
      errorMessage,
    },
  })),
  
  setIbkrConnectionInfo: (accountId, isPaperTrading) => set((state) => ({
    ibkr: {
      ...state.ibkr,
      accountId,
      isPaperTrading,
      status: ConnectionStatus.Connected,
    },
  })),
  
  updateIbkrHeartbeat: () => set((state) => ({
    ibkr: {
      ...state.ibkr,
      lastHeartbeat: new Date(),
    },
  })),
  
  setSignalRStatus: (status, errorMessage = null) => set((state) => ({
    signalR: {
      ...state.signalR,
      status,
      errorMessage,
    },
  })),
  
  resetConnections: () => set({
    ibkr: {
      status: ConnectionStatus.Disconnected,
      accountId: null,
      isPaperTrading: false,
      lastHeartbeat: null,
      errorMessage: null,
    },
    signalR: {
      status: ConnectionStatus.Disconnected,
      errorMessage: null,
    },
  }),
}));
