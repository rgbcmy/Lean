/**
 * Main Layout Component
 * 主布局组件 - 包含侧边栏、头部导航栏和内容区
 */

import React, { useState } from 'react';
import { Layout } from 'antd';
import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';
import './MainLayout.css';

const { Content } = Layout;

/**
 * MainLayout Component
 * Provides the main application layout with sidebar, header, and content area
 * 提供主应用布局，包含侧边栏、头部和内容区
 */
const MainLayout: React.FC = () => {
  const [collapsed, setCollapsed] = useState(false);
  const [mobileMenuVisible, setMobileMenuVisible] = useState(false);

  /**
   * Toggle sidebar collapse state
   * 切换侧边栏折叠状态
   */
  const toggleCollapse = () => {
    setCollapsed(!collapsed);
  };

  /**
   * Toggle mobile menu visibility
   * 切换移动端菜单可见性
   */
  const toggleMobileMenu = () => {
    setMobileMenuVisible(!mobileMenuVisible);
  };

  /**
   * Close mobile menu
   * 关闭移动端菜单
   */
  const closeMobileMenu = () => {
    setMobileMenuVisible(false);
  };

  return (
    <Layout className="main-layout">
      {/* Sidebar Navigation */}
      <Sidebar
        collapsed={collapsed}
        mobileMenuVisible={mobileMenuVisible}
        onCollapse={toggleCollapse}
        onCloseMobileMenu={closeMobileMenu}
      />

      <Layout className="site-layout">
        {/* Header */}
        <Header
          collapsed={collapsed}
          onToggleCollapse={toggleCollapse}
          onToggleMobileMenu={toggleMobileMenu}
        />

        {/* Main Content Area */}
        <Content className="site-content">
          <div className="content-wrapper">
            <Outlet />
          </div>
        </Content>
      </Layout>
    </Layout>
  );
};

export default MainLayout;
