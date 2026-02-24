/**
 * Logout Utility
 * 登出工具函数
 */

import { message } from 'antd';
import { logout as logoutApi } from '../api/authApi';
import { useAuthStore } from '../stores';
import { useConnectionStore } from '../stores';

/**
 * Perform logout operation
 * 执行登出操作
 * 
 * This function:
 * 1. Calls the logout API to revoke refresh token on server
 * 2. Clears auth state from Zustand store (which clears localStorage)
 * 3. Disconnects SignalR connections
 * 4. Returns true on success, false on failure
 * 
 * @param navigate - React Router navigate function
 * @param showMessage - Whether to show success message (default: true)
 */
export async function performLogout(
  navigate: (path: string, options?: any) => void,
  showMessage: boolean = true
): Promise<boolean> {
  try {
    // Call logout API to revoke refresh token on server
    await logoutApi();
  } catch (error) {
    console.error('Logout API call failed:', error);
    // Continue with client-side cleanup even if API call fails
  }
  
  try {
    // Disconnect SignalR
    const disconnectSignalR = useConnectionStore.getState().disconnect;
    await disconnectSignalR();
  } catch (error) {
    console.error('Failed to disconnect SignalR:', error);
    // Continue with logout even if SignalR disconnect fails
  }
  
  // Clear auth state (this also clears localStorage via persist middleware)
  const clearAuth = useAuthStore.getState().clearAuth;
  clearAuth();
  
  // Show success message if requested
  if (showMessage) {
    message.success('已成功登出');
  }
  
  // Navigate to login page
  navigate('/login', { replace: true });
  
  return true;
}
