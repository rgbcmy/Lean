/**
 * Sidebar Component
 * 侧边栏组件 - 主导航菜单
 */

import React, { useEffect } from 'react';
import { Layout, Menu, Drawer } from 'antd';
import {
  DashboardOutlined,
  StockOutlined,
  WalletOutlined,
  FileTextOutlined,
  RocketOutlined,
  BarChartOutlined,
  SettingOutlined,
  SafetyOutlined,
  QuestionCircleOutlined,
} from '@ant-design/icons';
import { useNavigate, useLocation } from 'react-router-dom';
import type { MenuProps } from 'antd';
import './Sidebar.css';

const { Sider } = Layout;

interface SidebarProps {
  collapsed: boolean;
  mobileMenuVisible: boolean;
  onCollapse: () => void;
  onCloseMobileMenu: () => void;
}

type MenuItem = Required<MenuProps>['items'][number];

/**
 * Create menu item
 * 创建菜单项
 */
function getItem(
  label: React.ReactNode,
  key: string,
  icon?: React.ReactNode,
  children?: MenuItem[],
): MenuItem {
  return {
    key,
    icon,
    children,
    label,
  } as MenuItem;
}

/**
 * Sidebar Component
 * Provides navigation menu with icons and labels
 * 提供带图标和标签的导航菜单
 */
const Sidebar: React.FC<SidebarProps> = ({
  collapsed,
  mobileMenuVisible,
  onCollapse,
  onCloseMobileMenu,
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  
  // Get current selected key from route
  const selectedKey = location.pathname === '/' ? '/dashboard' : location.pathname;

  /**
   * Menu items configuration
   * 菜单项配置
   */
  const menuItems: MenuItem[] = [
    getItem('首页', '/dashboard', <DashboardOutlined />),
    getItem('交易', '/trading', <StockOutlined />, [
      getItem('股票交易', '/trading/stocks'),
      getItem('ETF 交易', '/trading/etfs'),
      getItem('定投计划', '/trading/recurring'),
    ]),
    getItem('持仓', '/portfolio', <WalletOutlined />, [
      getItem('持仓列表', '/portfolio/positions'),
      getItem('收益分析', '/portfolio/analysis'),
    ]),
    getItem('订单', '/orders', <FileTextOutlined />, [
      getItem('当前订单', '/orders/active'),
      getItem('历史订单', '/orders/history'),
    ]),
    getItem('策略', '/strategies', <RocketOutlined />, [
      getItem('策略列表', '/strategies/list'),
      getItem('新建策略', '/strategies/create'),
    ]),
    getItem('回测', '/backtests', <BarChartOutlined />, [
      getItem('回测列表', '/backtests/list'),
      getItem('新建回测', '/backtests/new'),
      getItem('对比回测', '/backtests/compare'),
    ]),
    getItem('风控', '/risk', <SafetyOutlined />, [
      getItem('风险指标', '/risk/dashboard'),
      getItem('风控配置', '/risk/config'),
    ]),
    getItem('设置', '/settings', <SettingOutlined />),
  ];

  // Help menu item - shown at the bottom
  const helpMenuItem: MenuItem = getItem('帮助中心', '/help', <QuestionCircleOutlined />);

  /**
   * Handle menu item click
   * 处理菜单项点击
   */
  const handleMenuClick: MenuProps['onClick'] = (e) => {
    navigate(e.key);
    // Close mobile menu when item is clicked
    if (mobileMenuVisible) {
      onCloseMobileMenu();
    }
  };

  /**
   * Get default open keys based on current route
   * 根据当前路由获取默认展开的菜单键
   */
  const getDefaultOpenKeys = () => {
    const path = location.pathname;
    const openKeys: string[] = [];
    
    if (path.startsWith('/trading')) openKeys.push('/trading');
    if (path.startsWith('/portfolio')) openKeys.push('/portfolio');
    if (path.startsWith('/orders')) openKeys.push('/orders');
    if (path.startsWith('/strategies')) openKeys.push('/strategies');
    if (path.startsWith('/backtests')) openKeys.push('/backtests');
    if (path.startsWith('/risk')) openKeys.push('/risk');
    
    return openKeys;
  };

  /**
   * Sidebar logo/title
   * 侧边栏 Logo/标题
   */
  const sidebarHeader = (
    <div className="sidebar-header">
      <div className="logo">
        <StockOutlined className="logo-icon" />
        {!collapsed && <span className="logo-text">Lean Trading</span>}
      </div>
    </div>
  );

  /**
   * Sidebar menu content
   * 侧边栏菜单内容
   */
  const sidebarContent = (
    <>
      {sidebarHeader}
      <div className="sidebar-menu-wrapper">
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          defaultOpenKeys={getDefaultOpenKeys()}
          items={menuItems}
          onClick={handleMenuClick}
          className="sidebar-menu"
        />
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[selectedKey]}
          items={[helpMenuItem]}
          onClick={handleMenuClick}
          className="sidebar-help-menu"
        />
      </div>
    </>
  );

  // Check if mobile view
  const [isMobile, setIsMobile] = React.useState(window.innerWidth < 768);

  useEffect(() => {
    const handleResize = () => {
      setIsMobile(window.innerWidth < 768);
    };

    window.addEventListener('resize', handleResize);
    return () => window.removeEventListener('resize', handleResize);
  }, []);

  // Mobile view: Use Drawer
  if (isMobile) {
    return (
      <Drawer
        placement="left"
        open={mobileMenuVisible}
        onClose={onCloseMobileMenu}
        closable={false}
        width={256}
        className="mobile-sidebar-drawer"
        styles={{
          body: { padding: 0, background: '#001529' },
        }}
      >
        {sidebarContent}
      </Drawer>
    );
  }

  // Desktop view: Use Sider
  return (
    <Sider
      collapsible
      collapsed={collapsed}
      onCollapse={onCollapse}
      breakpoint="lg"
      theme="dark"
      width={256}
      className="sidebar"
    >
      {sidebarContent}
    </Sider>
  );
};

export default Sidebar;
