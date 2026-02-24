/**
 * Settings Types
 * 系统设置类型定义
 */

/**
 * IBKR Connection Configuration
 * IBKR 连接配置
 */
export interface IBKRConfig {
  host: string; // TWS/Gateway host (e.g., "127.0.0.1")
  port: number; // TWS port (7496 for live, 7497 for paper) or Gateway port (4001/4002)
  clientId: number; // Client ID (0-32)
  accountId: string; // IBKR account ID
  usePaperTrading: boolean; // Paper trading mode
  autoReconnect: boolean;
  reconnectIntervalSeconds: number;
  heartbeatIntervalSeconds: number;
}

/**
 * Database Configuration
 * 数据库配置
 */
export interface DatabaseConfig {
  provider: 'postgresql' | 'sqlite'; // Database provider
  connectionString: string; // Connection string (hidden for security)
  autoMigrate: boolean; // Automatically apply migrations
  backupEnabled: boolean;
  backupIntervalHours: number;
}

/**
 * Theme Configuration
 * 主题配置
 */
export interface ThemeConfig {
  mode: 'light' | 'dark'; // Theme mode
  primaryColor: string; // Primary color (hex)
  accentColor: string; // Accent color (hex)
  chartColorScheme: 'default' | 'colorblind' | 'highContrast';
  priceColorMode: 'western' | 'chinese'; // Western: green=up, red=down; Chinese: red=up, green=down
}

/**
 * Language Configuration
 * 语言配置
 */
export interface LanguageConfig {
  locale: 'zh-CN' | 'en-US'; // Language locale
  dateFormat: string; // Date format (e.g., "YYYY-MM-DD")
  timeFormat: '12h' | '24h'; // Time format
  currencyFormat: string; // Currency symbol (e.g., "$", "¥")
  numberFormat: {
    decimalSeparator: string;
    thousandsSeparator: string;
  };
}

/**
 * Notification Preferences
 * 通知偏好
 */
export interface NotificationPreferences {
  email: {
    enabled: boolean;
    address: string;
    events: {
      orderFilled: boolean;
      orderCanceled: boolean;
      stopLossTriggered: boolean;
      takeProfitTriggered: boolean;
      marginCall: boolean;
      strategyError: boolean;
      dailyReport: boolean;
    };
  };
  push: {
    enabled: boolean;
    events: {
      orderFilled: boolean;
      orderCanceled: boolean;
      stopLossTriggered: boolean;
      marginCall: boolean;
      riskAlert: boolean;
    };
  };
  inApp: {
    enabled: boolean;
    sound: boolean;
    events: {
      all: boolean; // Enable all in-app notifications
    };
  };
}

/**
 * System Settings
 * 系统设置
 */
export interface SystemSettings {
  ibkr: IBKRConfig;
  database: DatabaseConfig;
  theme: ThemeConfig;
  language: LanguageConfig;
  notifications: NotificationPreferences;
  updatedAt?: string;
}

/**
 * Settings Update Request
 * 设置更新请求
 */
export interface SettingsUpdateRequest {
  ibkr?: Partial<IBKRConfig>;
  database?: Partial<DatabaseConfig>;
  theme?: Partial<ThemeConfig>;
  language?: Partial<LanguageConfig>;
  notifications?: Partial<NotificationPreferences>;
}
