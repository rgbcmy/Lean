/**
 * Header Component
 * 头部导航栏组件 - 包含用户菜单、通知和 IBKR 状态
 */

import React, { useState } from 'react';
import { Layout, Button, Dropdown, Badge, Space, Avatar, Typography } from 'antd';
import type { MenuProps } from 'antd';
import {
  MenuFoldOutlined,
  MenuUnfoldOutlined,
  BellOutlined,
  UserOutlined,
  SettingOutlined,
  LogoutOutlined,
  KeyOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  SyncOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import ThemeToggle from '../ThemeToggle';
import Breadcrumb from './Breadcrumb';
import { useAuthStore } from '../../stores/authStore';
import { useIBKRStore } from '../../stores/ibkrStore';
import { useNotificationStore } from '../../stores/notificationStore';
import { performLogout } from '../../utils/logout';
import './Header.css';

const { Header: AntHeader } = Layout;
const { Text } = Typography;

interface HeaderProps {
  collapsed: boolean;
  onToggleCollapse: () => void;
  onToggleMobileMenu: () => void;
}

/**
 * Header Component
 * Provides top navigation bar with user menu, notifications, and IBKR status
 * 提供顶部导航栏，包含用户菜单、通知和 IBKR 状态
 */
const Header: React.FC<HeaderProps> = ({
  collapsed,
  onToggleCollapse,
  onToggleMobileMenu,
}) => {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const { connectionStatus } = useIBKRStore();
  const { unreadCount } = useNotificationStore();
  
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);

  React.useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth < 768);
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  /**
   * Handle logout
   * 处理登出
   */
  const handleLogout = async () => {
    await performLogout(navigate);
  };

  /**
   * Handle notification click
   * 处理通知点击
   */
  const handleNotificationClick = () => {
    navigate('/notifications');
  };

  /**
   * User dropdown menu items
   * 用户下拉菜单项
   */
  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <UserOutlined />,
      label: '个人资料',
      onClick: () => navigate('/settings/account'),
    },
    {
      key: 'change-password',
      icon: <KeyOutlined />,
      label: '修改密码',
      onClick: () => navigate('/change-password'),
    },
    {
      key: 'settings',
      icon: <SettingOutlined />,
      label: '系统设置',
      onClick: () => navigate('/settings/system'),
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: '退出登录',
      onClick: handleLogout,
      danger: true,
    },
  ];

  /**
   * Get IBKR status icon and color
   * 获取 IBKR 状态图标和颜色
   */
  const getIBKRStatusIcon = () => {
    switch (connectionStatus) {
      case 'connected':
        return {
          icon: <CheckCircleOutlined />,
          color: '#52c41a',
          text: '已连接',
        };
      case 'connecting':
        return {
          icon: <SyncOutlined spin />,
          color: '#1890ff',
          text: '连接中',
        };
      case 'disconnected':
      default:
        return {
          icon: <CloseCircleOutlined />,
          color: '#ff4d4f',
          text: '未连接',
        };
    }
  };

  const ibkrStatus = getIBKRStatusIcon();

  /**
   * Handle IBKR status click
   * 处理 IBKR 状态点击
   */
  const handleIBKRClick = () => {
    navigate('/settings/ibkr');
  };

  return (
    <AntHeader className="site-header">
      <div className="header-left">
        {/* Desktop: Collapse toggle */}
        {!isMobile && (
          <Button
            type="text"
            icon={collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
            onClick={onToggleCollapse}
            className="trigger"
          />
        )}
        
        {/* Mobile: Hamburger menu */}
        {isMobile && (
          <Button
            type="text"
            icon={<MenuUnfoldOutlined />}
            onClick={onToggleMobileMenu}
            className="trigger"
          />
        )}

        {/* Breadcrumb */}
        {!isMobile && <Breadcrumb />}
      </div>

      <div className="header-right">
        <Space size="middle">
          {/* IBKR Connection Status */}
          <Button
            type="text"
            className="ibkr-status-btn"
            onClick={handleIBKRClick}
          >
            <Space>
              <span style={{ color: ibkrStatus.color }}>
                {ibkrStatus.icon}
              </span>
              {!isMobile && (
                <Text style={{ color: ibkrStatus.color }}>
                  IBKR: {ibkrStatus.text}
                </Text>
              )}
            </Space>
          </Button>

          {/* Theme Toggle */}
          <ThemeToggle />

          {/* Notifications */}
          <Badge count={unreadCount} overflowCount={99}>
            <Button
              type="text"
              icon={<BellOutlined style={{ fontSize: 18 }} />}
              onClick={handleNotificationClick}
              className="notification-btn"
            />
          </Badge>

          {/* User Menu */}
          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight">
            <Space className="user-menu">
              <Avatar icon={<UserOutlined />} />
              {!isMobile && (
                <span className="username">{user?.username || '用户'}</span>
              )}
            </Space>
          </Dropdown>
        </Space>
      </div>
    </AntHeader>
  );
};

export default Header;
