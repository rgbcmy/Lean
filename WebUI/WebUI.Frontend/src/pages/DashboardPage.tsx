/**
 * Dashboard Page Component
 * 仪表板页面组件
 */

import React, { useEffect, useState } from 'react';
import { Typography, Card, Row, Col, Statistic, Button, Space, Progress, Spin } from 'antd';
import {
  DollarOutlined,
  RiseOutlined,
  FallOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores';
import QuickActions from '../components/dashboard/QuickActions';
import { getPortfolioSummary } from '../api/positionsApi';
import { getStrategies } from '../api/strategiesApi';
import { getOrders } from '../api/ordersApi';
import type { PortfolioSummary } from '../types/position';
import type { Strategy } from '../types/strategy';
import { StrategyStatus } from '../types/strategy';
import './DashboardPage.css';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);

  const [loading, setLoading] = useState(true);
  const [summary, setSummary] = useState<PortfolioSummary | null>(null);
  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [orderStats, setOrderStats] = useState({ pending: 0, filled: 0, cancelled: 0 });

  useEffect(() => {
    const loadDashboard = async () => {
      setLoading(true);
      try {
        const [summaryData, strategyData, orderData] = await Promise.allSettled([
          getPortfolioSummary(),
          getStrategies(),
          getOrders({ pageSize: 200 }),
        ]);

        if (summaryData.status === 'fulfilled') setSummary(summaryData.value);
        if (strategyData.status === 'fulfilled') setStrategies(strategyData.value);
        if (orderData.status === 'fulfilled') {
          const orders = orderData.value.orders;
          setOrderStats({
            pending: orders.filter(o => o.status === 'submitted').length,
            filled: orders.filter(o => o.status === 'filled').length,
            cancelled: orders.filter(o => o.status === 'cancelled').length,
          });
        }
      } finally {
        setLoading(false);
      }
    };

    loadDashboard();
  }, []);

  const runningStrategies = strategies.filter(s => s.status === StrategyStatus.Running).length;
  const pausedStrategies = strategies.filter(s => s.status === StrategyStatus.Paused).length;
  const positionCount = summary?.positions.length ?? 0;
  const profitPositions = summary?.positions.filter(p => p.unrealizedPnL > 0).length ?? 0;
  const lossPositions = summary?.positions.filter(p => p.unrealizedPnL < 0).length ?? 0;
  const profitPercent = positionCount > 0 ? (profitPositions / positionCount) * 100 : 0;

  const totalPortfolioValue = summary?.totalPortfolioValue ?? 0;
  const cashBalance = summary?.cashBalance ?? 0;
  const totalMarketValue = summary?.totalMarketValue ?? 0;
  const totalUnrealizedPnL = summary?.totalUnrealizedPnL ?? 0;
  const totalUnrealizedPnLPercent = summary?.totalUnrealizedPnLPercent ?? 0;

  if (loading) {
    return (
      <div className="dashboard-page" style={{ textAlign: 'center', paddingTop: 80 }}>
        <Spin size="large" tip="加载中..." />
      </div>
    );
  }

  return (
    <div className="dashboard-page">
      {/* Welcome Section */}
      <div className="dashboard-header">
        <div>
          <Title level={2} style={{ marginBottom: 8 }}>仪表板</Title>
          <Text type="secondary">
            欢迎回来，{user?.username || '用户'}！
          </Text>
        </div>
      </div>

      {/* Quick Actions */}
      <QuickActions />

      {/* Account Overview */}
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col xs={24} sm={12} xl={6}>
          <Card className="stat-card">
            <Statistic
              title="账户总值"
              value={totalPortfolioValue}
              precision={2}
              prefix={<DollarOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} xl={6}>
          <Card className="stat-card">
            <Statistic
              title="可用现金"
              value={cashBalance}
              precision={2}
              prefix={<DollarOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} xl={6}>
          <Card className="stat-card">
            <Statistic
              title="持仓市值"
              value={totalMarketValue}
              precision={2}
              prefix={<DollarOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} xl={6}>
          <Card className="stat-card">
            <Statistic
              title="未实现盈亏"
              value={totalUnrealizedPnL}
              precision={2}
              prefix={totalUnrealizedPnL >= 0 ? <RiseOutlined /> : <FallOutlined />}
              valueStyle={{ color: totalUnrealizedPnL >= 0 ? '#cf1322' : '#3f8600' }}
              suffix={`(${totalUnrealizedPnLPercent >= 0 ? '+' : ''}${totalUnrealizedPnLPercent.toFixed(2)}%)`}
            />
          </Card>
        </Col>
      </Row>

      {/* Quick Stats Cards */}
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} md={12} lg={8}>
          <Card title="持仓概览" bordered={false} className="info-card">
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <div className="stat-item">
                <Text type="secondary">持仓股票数</Text>
                <Text strong style={{ fontSize: 20 }}>{positionCount}</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">今日盈利股票</Text>
                <Space>
                  <Text strong style={{ color: '#cf1322' }}>{profitPositions}</Text>
                  <Text type="secondary">/</Text>
                  <Text strong style={{ color: '#3f8600' }}>{lossPositions}</Text>
                </Space>
              </div>
              {positionCount > 0 && (
                <Progress percent={profitPercent} strokeColor="#cf1322" size="small" showInfo={false} />
              )}
              <Button type="link" onClick={() => navigate('/portfolio/positions')} style={{ padding: 0 }}>
                查看详情 →
              </Button>
            </Space>
          </Card>
        </Col>

        <Col xs={24} md={12} lg={8}>
          <Card title="订单" bordered={false} className="info-card">
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <div className="stat-item">
                <Text type="secondary">待成交订单</Text>
                <Text strong style={{ fontSize: 20 }}>{orderStats.pending}</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">今日已成交</Text>
                <Text strong style={{ fontSize: 20 }}>{orderStats.filled}</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">今日已取消</Text>
                <Text strong style={{ fontSize: 20 }}>{orderStats.cancelled}</Text>
              </div>
              <Button type="link" onClick={() => navigate('/orders/active')} style={{ padding: 0 }}>
                查看详情 →
              </Button>
            </Space>
          </Card>
        </Col>

        <Col xs={24} md={12} lg={8}>
          <Card title="策略运行" bordered={false} className="info-card">
            <Space direction="vertical" style={{ width: '100%' }} size="middle">
              <div className="stat-item">
                <Text type="secondary">运行中策略</Text>
                <Text strong style={{ fontSize: 20, color: '#52c41a' }}>{runningStrategies}</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">已暂停策略</Text>
                <Text strong style={{ fontSize: 20 }}>{pausedStrategies}</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">总策略数</Text>
                <Text strong style={{ fontSize: 20 }}>{strategies.length}</Text>
              </div>
              <Button type="link" onClick={() => navigate('/strategies/list')} style={{ padding: 0 }}>
                查看详情 →
              </Button>
            </Space>
          </Card>
        </Col>
      </Row>
    </div>
  );
};

export default DashboardPage;

