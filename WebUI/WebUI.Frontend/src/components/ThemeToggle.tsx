/**
 * Theme Toggle Button Component
 * 主题切换按钮组件
 */

import React from 'react';
import { Button } from 'antd';
import { BulbOutlined, BulbFilled } from '@ant-design/icons';
import { useThemeStore } from '../stores';

interface ThemeToggleProps {
  style?: React.CSSProperties;
}

/**
 * ThemeToggle button to switch between light and dark themes
 * 主题切换按钮，用于在浅色和深色主题之间切换
 */
const ThemeToggle: React.FC<ThemeToggleProps> = ({ style }) => {
  const { theme, toggleTheme } = useThemeStore();
  
  return (
    <Button
      type="text"
      icon={theme === 'light' ? <BulbOutlined /> : <BulbFilled />}
      onClick={toggleTheme}
      style={style}
      title={theme === 'light' ? '切换到深色模式' : '切换到浅色模式'}
    >
      {theme === 'light' ? '深色' : '浅色'}
    </Button>
  );
};

export default ThemeToggle;
