/**
 * Ant Design Theme Configuration
 * Ant Design 主题配置
 */

import type { ThemeConfig } from 'antd';

/**
 * Light theme configuration
 * 浅色主题配置
 */
export const lightTheme: ThemeConfig = {
  token: {
    colorPrimary: '#667eea',
    colorSuccess: '#52c41a',
    colorWarning: '#faad14',
    colorError: '#f5222d',
    colorInfo: '#1890ff',
    
    // Font
    fontSize: 14,
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"',
    
    // Border
    borderRadius: 6,
    
    // Layout
    colorBgLayout: '#f0f2f5',
  },
  components: {
    Button: {
      controlHeight: 36,
    },
    Input: {
      controlHeight: 36,
    },
    Select: {
      controlHeight: 36,
    },
  },
};

/**
 * Dark theme configuration
 * 深色主题配置
 */
export const darkTheme: ThemeConfig = {
  token: {
    colorPrimary: '#667eea',
    colorSuccess: '#52c41a',
    colorWarning: '#faad14',
    colorError: '#f5222d',
    colorInfo: '#1890ff',
    
    // Font
    fontSize: 14,
    fontFamily: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif, "Apple Color Emoji", "Segoe UI Emoji", "Segoe UI Symbol"',
    
    // Border
    borderRadius: 6,
    
    // Background colors for dark theme
    colorBgBase: '#141414',
    colorBgContainer: '#1f1f1f',
    colorBgLayout: '#000000',
    colorBgElevated: '#262626',
    
    // Text colors
    colorText: 'rgba(255, 255, 255, 0.85)',
    colorTextSecondary: 'rgba(255, 255, 255, 0.65)',
    colorTextTertiary: 'rgba(255, 255, 255, 0.45)',
    colorTextQuaternary: 'rgba(255, 255, 255, 0.25)',
    
    // Border colors
    colorBorder: '#424242',
    colorBorderSecondary: '#303030',
  },
  components: {
    Button: {
      controlHeight: 36,
    },
    Input: {
      controlHeight: 36,
    },
    Select: {
      controlHeight: 36,
    },
  },
};

/**
 * Get theme configuration based on theme name
 * 根据主题名称获取主题配置
 */
export function getThemeConfig(theme: 'light' | 'dark'): ThemeConfig {
  return theme === 'light' ? lightTheme : darkTheme;
}
