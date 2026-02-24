/**
 * Axios HTTP Client Configuration
 * Axios HTTP 客户端配置
 */

import axios, { AxiosError } from 'axios';
import type { AxiosResponse, InternalAxiosRequestConfig } from 'axios';
import { message } from 'antd';
import { ENV } from '../config/env';
import { 
  getAccessToken, 
  getRefreshToken, 
  isTokenExpired, 
  storeTokens, 
  clearTokens 
} from '../utils/tokenManager';

/**
 * Create Axios instance with base configuration
 * 创建配置了基础设置的 Axios 实例
 */
export const apiClient = axios.create({
  baseURL: ENV.API_BASE_URL,
  timeout: 30000, // 30 seconds
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Flag to prevent multiple simultaneous refresh attempts
 */
let isRefreshing = false;

/**
 * Queue of failed requests waiting for token refresh
 */
let failedRequestsQueue: Array<{
  resolve: (token: string) => void;
  reject: (error: any) => void;
}> = [];

/**
 * Process queued requests after token refresh
 */
function processQueue(error: any = null, token: string | null = null) {
  failedRequestsQueue.forEach((promise) => {
    if (error) {
      promise.reject(error);
    } else {
      promise.resolve(token!);
    }
  });
  
  failedRequestsQueue = [];
}

/**
 * Refresh access token using refresh token
 * 使用 refresh token 刷新 access token
 */
async function refreshAccessToken(): Promise<string> {
  const refreshToken = getRefreshToken();
  
  if (!refreshToken) {
    throw new Error('No refresh token available');
  }
  
  try {
    // Call refresh token API
    const response = await axios.post(
      `${ENV.API_BASE_URL}/api/v1/auth/refresh`,
      { refreshToken },
      { headers: { 'Content-Type': 'application/json' } }
    );
    
    const { accessToken, refreshToken: newRefreshToken } = response.data;
    
    // Store new tokens
    storeTokens(accessToken, newRefreshToken);
    
    return accessToken;
  } catch (error) {
    // Refresh failed, clear auth and redirect to login
    clearTokens();
    window.location.href = '/login';
    throw error;
  }
}

/**
 * Request Interceptor
 * 请求拦截器 - 添加 Authorization Header
 */
apiClient.interceptors.request.use(
  async (config: InternalAxiosRequestConfig) => {
    // Get access token
    let accessToken = getAccessToken();
    
    // Check if token needs refresh (and config doesn't already have auth)
    if (accessToken && isTokenExpired(accessToken) && !config.headers.Authorization) {
      // Token is expired, try to refresh
      if (!isRefreshing) {
        isRefreshing = true;
        
        try {
          accessToken = await refreshAccessToken();
          isRefreshing = false;
          processQueue(null, accessToken);
        } catch (error) {
          isRefreshing = false;
          processQueue(error, null);
          return Promise.reject(error);
        }
      } else {
        // Wait for ongoing refresh
        return new Promise((resolve, reject) => {
          failedRequestsQueue.push({
            resolve: (token: string) => {
              config.headers.Authorization = `Bearer ${token}`;
              resolve(config);
            },
            reject: (error: any) => {
              reject(error);
            },
          });
        });
      }
    }
    
    // Add Authorization header if token exists
    if (accessToken && !config.headers.Authorization) {
      config.headers.Authorization = `Bearer ${accessToken}`;
    }
    
    return config;
  },
  (error: AxiosError) => {
    return Promise.reject(error);
  }
);

/**
 * Response Interceptor
 * 响应拦截器 - 错误处理、Token 刷新
 */
apiClient.interceptors.response.use(
  (response: AxiosResponse) => {
    // Successful response, return data directly
    return response;
  },
  async (error: AxiosError<any>) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // Handle 401 Unauthorized (token expired or invalid)
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      if (!isRefreshing) {
        isRefreshing = true;
        
        try {
          const newAccessToken = await refreshAccessToken();
          isRefreshing = false;
          processQueue(null, newAccessToken);
          
          // Retry original request with new token
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
          return apiClient(originalRequest);
        } catch (refreshError) {
          isRefreshing = false;
          processQueue(refreshError, null);
          return Promise.reject(refreshError);
        }
      } else {
        // Wait for ongoing refresh
        return new Promise((resolve, reject) => {
          failedRequestsQueue.push({
            resolve: (token: string) => {
              originalRequest.headers.Authorization = `Bearer ${token}`;
              resolve(apiClient(originalRequest));
            },
            reject: (err: any) => {
              reject(err);
            },
          });
        });
      }
    }
    
    // Handle other errors
    const errorMessage = error.response?.data?.message 
      || error.response?.data?.error
      || error.message 
      || '请求失败';
    
    // Display error message based on status code
    switch (error.response?.status) {
      case 400:
        message.error(`请求错误: ${errorMessage}`);
        break;
      case 403:
        message.error('没有权限访问该资源');
        break;
      case 404:
        message.error('请求的资源不存在');
        break;
      case 500:
        message.error('服务器错误，请稍后重试');
        break;
      case 502:
        message.error('网关错误，请检查服务是否正常');
        break;
      case 503:
        message.error('服务暂时不可用，请稍后重试');
        break;
      default:
        if (error.code === 'ECONNABORTED') {
          message.error('请求超时，请检查网络连接');
        } else if (error.code === 'ERR_NETWORK') {
          message.error('网络错误，请检查网络连接');
        } else {
          message.error(errorMessage);
        }
    }
    
    return Promise.reject(error);
  }
);

export default apiClient;
