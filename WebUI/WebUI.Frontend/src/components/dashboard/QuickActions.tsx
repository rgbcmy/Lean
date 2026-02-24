/**
 * Quick Actions Component
 * 快速操作组件 - 提供常用操作的快捷入口
 */

import React from 'react';
import { Card, Row, Col, Button } from 'antd';
import {
  StockOutlined,
  PlusOutlined,
  EyeOutlined,
  ThunderboltOutlined,
  FileTextOutlined,
  BarChartOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import './QuickActions.css';

/**
 * QuickActions Component
 * Provides quick access to common actions
 * 提供常用操作的快速访问
 */
const QuickActions: React.FC = () => {
  const navigate = useNavigate();

  const actions = [
    {
      key: 'quick-trade',
      icon: <ThunderboltOutlined />,
      title: '快速下单',
      description: '快速买入或卖出股票',
      onClick: () => navigate('/trading/stocks'),
      type: 'primary' as const,
    },
    {
      key: 'view-positions',
      icon: <EyeOutlined />,
      title: '查看持仓',
      description: '查看当前持仓和盈亏',
      onClick: () => navigate('/portfolio/positions'),
    },
    {
      key: 'view-orders',
      icon: <FileTextOutlined />,
      title: '查看订单',
      description: '查看当前和历史订单',
      onClick: () => navigate('/orders/active'),
    },
    {
      key: 'create-strategy',
      icon: <PlusOutlined />,
      title: '新建策略',
      description: '创建新的交易策略',
      onClick: () => navigate('/strategies/create'),
    },
    {
      key: 'run-backtest',
      icon: <BarChartOutlined />,
      title: '运行回测',
      description: '回测策略性能',
      onClick: () => navigate('/backtests/new'),
    },
    {
      key: 'trade-etf',
      icon: <StockOutlined />,
      title: 'ETF 交易',
      description: '交易 ETF 产品',
      onClick: () => navigate('/trading/etfs'),
    },
  ];

  return (
    <Card title="快速操作" bordered={false} className="quick-actions-card">
      <Row gutter={[16, 16]}>
        {actions.map((action) => (
          <Col xs={24} sm={12} lg={8} xl={4} key={action.key}>
            <Button
              type={action.type || 'default'}
              size="large"
              icon={action.icon}
              onClick={action.onClick}
              className="quick-action-btn"
              block
            >
              <div className="quick-action-content">
                <div className="quick-action-title">{action.title}</div>
                <div className="quick-action-description">{action.description}</div>
              </div>
            </Button>
          </Col>
        ))}
      </Row>
    </Card>
  );
};

export default QuickActions;
