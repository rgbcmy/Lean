/**
 * User Store - Global user state management
 * 用户状态管理
 */

import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface User {
  id: number;
  username: string;
  email?: string;
  role: string;
}

interface AuthState {
  // Access token
  accessToken: string | null;
  
  // Refresh token
  refreshToken: string | null;
  
  // User info
  user: User | null;
  
  // Is authenticated
  isAuthenticated: boolean;
  
  // Set auth data (after login)
  setAuth: (accessToken: string, refreshToken: string, user: User) => void;
  
  // Clear auth data (logout)
  clearAuth: () => void;
  
  // Update access token
  setAccessToken: (token: string) => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      isAuthenticated: false,
      
      setAuth: (accessToken, refreshToken, user) => set({
        accessToken,
        refreshToken,
        user,
        isAuthenticated: true,
      }),
      
      clearAuth: () => set({
        accessToken: null,
        refreshToken: null,
        user: null,
        isAuthenticated: false,
      }),
      
      setAccessToken: (token) => set({ accessToken: token }),
    }),
    {
      name: 'auth-storage', // localStorage key
      // Only persist refresh token and user, not access token (for security)
      partialize: (state) => ({
        refreshToken: state.refreshToken,
        user: state.user,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);
