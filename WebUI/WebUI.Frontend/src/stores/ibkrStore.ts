/**
 * IBKR Connection Store
 * IBKR 连接状态管理
 */

import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export type ConnectionStatus = 'connected' | 'connecting' | 'disconnected';

interface IBKRState {
  connectionStatus: ConnectionStatus;
  accountId: string | null;
  lastConnected: Date | null;
  errorMessage: string | null;
  
  // Actions
  setConnectionStatus: (status: ConnectionStatus) => void;
  setAccountId: (accountId: string | null) => void;
  setConnected: (accountId: string) => void;
  setDisconnected: (errorMessage?: string) => void;
  setConnecting: () => void;
  clearError: () => void;
}

/**
 * IBKR Connection Store
 * 管理 IBKR 连接状态
 */
export const useIBKRStore = create<IBKRState>()(
  persist(
    (set) => ({
      connectionStatus: 'disconnected',
      accountId: null,
      lastConnected: null,
      errorMessage: null,

      setConnectionStatus: (status) => set({ connectionStatus: status }),

      setAccountId: (accountId) => set({ accountId }),

      setConnected: (accountId) =>
        set({
          connectionStatus: 'connected',
          accountId,
          lastConnected: new Date(),
          errorMessage: null,
        }),

      setDisconnected: (errorMessage) =>
        set({
          connectionStatus: 'disconnected',
          accountId: null,
          errorMessage: errorMessage || null,
        }),

      setConnecting: () =>
        set({
          connectionStatus: 'connecting',
          errorMessage: null,
        }),

      clearError: () => set({ errorMessage: null }),
    }),
    {
      name: 'ibkr-connection-storage',
      // Only persist certain fields
      partialize: (state) => ({
        accountId: state.accountId,
        lastConnected: state.lastConnected,
      }),
    }
  )
);
