/**
 * Breadcrumb Component
 * 面包屑导航组件
 */

import React from 'react';
import { Breadcrumb as AntBreadcrumb } from 'antd';
import { Link, useLocation } from 'react-router-dom';
import { HomeOutlined } from '@ant-design/icons';
import './Breadcrumb.css';

/**
 * Route name mapping
 * 路由名称映射
 */
const routeNameMap: Record<string, string> = {
  dashboard: '首页',
  trading: '交易',
  stocks: '股票交易',
  etfs: 'ETF 交易',
  portfolio: '持仓',
  positions: '持仓列表',
  analysis: '收益分析',
  orders: '订单',
  active: '当前订单',
  history: '历史订单',
  strategies: '策略',
  list: '策略列表',
  create: '新建策略',
  edit: '编辑策略',
  backtesting: '回测',
  risk: '风控',
  metrics: '风险指标',
  config: '风控配置',
  settings: '设置',
  account: '账户设置',
  ibkr: 'IBKR 配置',
  system: '系统配置',
  notifications: '通知中心',
  'change-password': '修改密码',
};

/**
 * Breadcrumb Component
 * Generates breadcrumb navigation based on current route
 * 根据当前路由生成面包屑导航
 */
const Breadcrumb: React.FC = () => {
  const location = useLocation();
  const pathSnippets = location.pathname.split('/').filter((i) => i);

  /**
   * Generate breadcrumb items
   * 生成面包屑项
   */
  const breadcrumbItems = [
    {
      title: (
        <Link to="/dashboard">
          <HomeOutlined />
        </Link>
      ),
    },
    ...pathSnippets.map((snippet, index) => {
      const url = `/${pathSnippets.slice(0, index + 1).join('/')}`;
      const isLast = index === pathSnippets.length - 1;
      const name = routeNameMap[snippet] || snippet;

      // If it's the last item, don't make it a link
      if (isLast) {
        return {
          title: name,
        };
      }

      return {
        title: <Link to={url}>{name}</Link>,
      };
    }),
  ];

  // Don't show breadcrumb on login or 404 pages
  if (location.pathname === '/login' || location.pathname === '/404') {
    return null;
  }

  // Don't show breadcrumb if only on homepage
  if (pathSnippets.length === 0 || (pathSnippets.length === 1 && pathSnippets[0] === 'dashboard')) {
    return null;
  }

  return (
    <div className="breadcrumb-container">
      <AntBreadcrumb items={breadcrumbItems} />
    </div>
  );
};

export default Breadcrumb;
