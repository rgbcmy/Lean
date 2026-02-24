/**
 * JWT Token Management Utilities
 * JWT Token 管理工具
 */

import { useAuthStore } from '../stores';

/**
 * Decode JWT token payload (without verification)
 * 解码 JWT Token 载荷（不验证签名）
 */
export function decodeJWT(token: string): any {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error('Failed to decode JWT:', error);
    return null;
  }
}

/**
 * Check if token is expired
 * 检查 Token 是否过期
 */
export function isTokenExpired(token: string): boolean {
  const payload = decodeJWT(token);
  if (!payload || !payload.exp) {
    return true;
  }
  
  // exp is in seconds, Date.now() is in milliseconds
  const expirationTime = payload.exp * 1000;
  const currentTime = Date.now();
  
  // Consider token expired 5 minutes before actual expiration for safety
  const bufferTime = 5 * 60 * 1000; // 5 minutes
  
  return currentTime >= (expirationTime - bufferTime);
}

/**
 * Get access token from store
 * 从 store 获取 access token
 */
export function getAccessToken(): string | null {
  return useAuthStore.getState().accessToken;
}

/**
 * Get refresh token from store
 * 从 store 获取 refresh token
 */
export function getRefreshToken(): string | null {
  return useAuthStore.getState().refreshToken;
}

/**
 * Store tokens
 * 存储 tokens
 */
export function storeTokens(accessToken: string, refreshToken: string): void {
  const { setAccessToken, refreshToken: currentRefreshToken } = useAuthStore.getState();
  setAccessToken(accessToken);
  
  // Only update refresh token if it changed
  if (currentRefreshToken !== refreshToken) {
    useAuthStore.setState({ refreshToken });
  }
}

/**
 * Clear all tokens (logout)
 * 清除所有 tokens（登出）
 */
export function clearTokens(): void {
  useAuthStore.getState().clearAuth();
}

/**
 * Check if user is authenticated with valid token
 * 检查用户是否已认证且 token 有效
 */
export function isAuthenticated(): boolean {
  const { isAuthenticated, accessToken } = useAuthStore.getState();
  
  if (!isAuthenticated || !accessToken) {
    return false;
  }
  
  // Check if token is expired
  return !isTokenExpired(accessToken);
}

/**
 * Timer ID for auto token refresh
 */
let refreshTimerId: ReturnType<typeof setInterval> | null = null;

/**
 * Setup automatic token refresh
 * 设置自动 token 刷新
 * 
 * @param refreshCallback - Callback function to refresh token
 */
export function setupAutoTokenRefresh(refreshCallback: () => Promise<void>): void {
  // Clear existing timer
  if (refreshTimerId) {
    clearInterval(refreshTimerId);
  }
  
  // Check token expiration every 60 seconds
  refreshTimerId = setInterval(async () => {
    const accessToken = getAccessToken();
    
    if (accessToken && isTokenExpired(accessToken)) {
      try {
        await refreshCallback();
      } catch (error) {
        console.error('Auto token refresh failed:', error);
        // On refresh failure, clear auth and redirect to login
        clearTokens();
      }
    }
  }, 60 * 1000); // Check every 60 seconds
}

/**
 * Stop automatic token refresh
 * 停止自动 token 刷新
 */
export function stopAutoTokenRefresh(): void {
  if (refreshTimerId) {
    clearInterval(refreshTimerId);
    refreshTimerId = null;
  }
}
