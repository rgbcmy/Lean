/**
 * Dashboard Page Component
 * 仪表板页面组件
 */

import React from 'react';
import { Typography, Card, Row, Col, Statistic, Button, Space, Progress } from 'antd';
import {
  DollarOutlined,
  RiseOutlined,
  FallOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores';
import QuickActions from '../components/dashboard/QuickActions';
import './DashboardPage.css';

const { Title, Text } = Typography;

const DashboardPage: React.FC = () => {
  const navigate = useNavigate();
  const user = useAuthStore((state) => state.user);

  // Mock data - will be replaced with real data from API
  const accountData = {
    totalValue: 125680.50,
    cash: 45230.20,
    equity: 80450.30,
    profitLoss: 5680.50,
    profitLossPercent: 4.73,
  };

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
              value={accountData.totalValue}
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
              value={accountData.cash}
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
              value={accountData.equity}
              precision={2}
              prefix={<DollarOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} xl={6}>
          <Card className="stat-card">
            <Statistic
              title="盈亏"
              value={accountData.profitLoss}
              precision={2}
              prefix={accountData.profitLoss >= 0 ? <RiseOutlined /> : <FallOutlined />}
              valueStyle={{ color: accountData.profitLoss >= 0 ? '#cf1322' : '#3f8600' }}
              suffix={`(${accountData.profitLossPercent >= 0 ? '+' : ''}${accountData.profitLossPercent}%)`}
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
                <Text strong style={{ fontSize: 20 }}>8</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">今日盈利股票</Text>
                <Space>
                  <Text strong style={{ color: '#cf1322' }}>5</Text>
                  <Text type="secondary">/</Text>
                  <Text strong style={{ color: '#3f8600' }}>3</Text>
                </Space>
              </div>
              <Progress percent={62.5} strokeColor="#cf1322" size="small" showInfo={false} />
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
                <Text strong style={{ fontSize: 20 }}>3</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">今日已成交</Text>
                <Text strong style={{ fontSize: 20 }}>7</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">今日已取消</Text>
                <Text strong style={{ fontSize: 20 }}>1</Text>
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
                <Text strong style={{ fontSize: 20, color: '#52c41a' }}>2</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">已暂停策略</Text>
                <Text strong style={{ fontSize: 20 }}>1</Text>
              </div>
              <div className="stat-item">
                <Text type="secondary">总策略数</Text>
                <Text strong style={{ fontSize: 20 }}>5</Text>
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

