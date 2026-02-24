/**
 * Theme Provider Component
 * 主题提供者组件
 */

import React, { useEffect } from 'react';
import { ConfigProvider, App as AntApp } from 'antd';
import zhCN from 'antd/locale/zh_CN';
import { useThemeStore } from '../stores';
import { getThemeConfig } from '../config/theme';
import 'dayjs/locale/zh-cn';

interface ThemeProviderProps {
  children: React.ReactNode;
}

/**
 * ThemeProvider wraps the app with Ant Design ConfigProvider
 * and applies the selected theme
 * 主题提供者包装应用并应用选中的主题
 */
const ThemeProvider: React.FC<ThemeProviderProps> = ({ children }) => {
  const { theme } = useThemeStore();
  const themeConfig = getThemeConfig(theme);
  
  // Apply theme class to body for custom CSS
  useEffect(() => {
    document.body.className = theme === 'dark' ? 'dark-theme' : 'light-theme';
  }, [theme]);
  
  return (
    <ConfigProvider 
      theme={themeConfig}
      locale={zhCN}
    >
      <AntApp>
        {children}
      </AntApp>
    </ConfigProvider>
  );
};

export default ThemeProvider;
