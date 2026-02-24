/**
 * Authentication API Calls
 * 认证相关 API 调用
 */

import { apiClient } from './apiClient';

/**
 * Login request payload
 * 登录请求载荷
 */
export interface LoginRequest {
  username: string;
  password: string;
  rememberMe?: boolean;
}

/**
 * Login response
 * 登录响应
 */
export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  user: {
    id: number;
    username: string;
    email?: string;
    role: string;
  };
}

/**
 * Change password request payload
 * 修改密码请求载荷
 */
export interface ChangePasswordRequest {
  oldPassword: string;
  newPassword: string;
}

/**
 * Refresh token request payload
 * 刷新令牌请求载荷
 */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/**
 * Refresh token response
 * 刷新令牌响应
 */
export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
}

/**
 * API: User login
 * 用户登录
 * 
 * @param credentials - Login credentials (username, password)
 * @returns Login response with tokens and user info
 */
export async function login(credentials: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>(
    '/api/v1/auth/login',
    credentials
  );
  return response.data;
}

/**
 * API: User logout
 * 用户登出
 * 
 * Revokes refresh token on server side
 */
export async function logout(): Promise<void> {
  await apiClient.post('/api/v1/auth/logout');
}

/**
 * API: Refresh access token
 * 刷新访问令牌
 * 
 * @param refreshToken - Current refresh token
 * @returns New access token and refresh token
 */
export async function refreshToken(refreshToken: string): Promise<RefreshTokenResponse> {
  const response = await apiClient.post<RefreshTokenResponse>(
    '/api/v1/auth/refresh',
    { refreshToken }
  );
  return response.data;
}

/**
 * API: Change password
 * 修改密码
 * 
 * @param request - Change password request (old and new password)
 */
export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await apiClient.post('/api/v1/auth/change-password', request);
}
