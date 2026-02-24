/**
 * Environment configuration
 * 环境变量配置
 */

export const ENV = {
  // API Base URL
  API_BASE_URL: import.meta.env.VITE_API_BASE_URL || 'https://localhost:5001',
  
  // SignalR Hub URL
  SIGNALR_HUB_URL: import.meta.env.VITE_SIGNALR_HUB_URL || 'https://localhost:5001/hubs',
  
  // Development mode
  IS_DEV: import.meta.env.DEV,
  
  // Production mode
  IS_PROD: import.meta.env.PROD,
} as const;
