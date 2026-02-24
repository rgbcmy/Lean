/**
 * Risk Dashboard Page
 * 风险指标展示页面
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Card,
  Row,
  Col,
  Statistic,
  Alert,
  Badge,
  List,
  Button,
  Space,
  message,
  Spin,
  Empty,
  Tag,
} from 'antd';
import {
  WarningOutlined,
  CheckCircleOutlined,
  CloseCircleOutlined,
  ReloadOutlined,
  DownloadOutlined,
} from '@ant-design/icons';
import { getRiskMetrics, getRiskAlerts, acknowledgeRiskAlert, exportRiskReport } from '../api/riskApi';
import type { RiskMetrics, RiskAlert } from '../types/risk';
import './RiskDashboardPage.css';

const { Title, Text } = Typography;

const RiskDashboardPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [metrics, setMetrics] = useState<RiskMetrics | null>(null);
  const [alerts, setAlerts] = useState<RiskAlert[]>([]);
  const [exporting, setExporting] = useState(false);

  // Load risk metrics and alerts
  const loadData = async () => {
    setLoading(true);
    try {
      const [metricsData, alertsData] = await Promise.all([
        getRiskMetrics(),
        getRiskAlerts(false), // Get unacknowledged alerts
      ]);
      setMetrics(metricsData);
      setAlerts(alertsData);
    } catch (error: any) {
      message.error(error.response?.data?.message || '加载风险数据失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  // Acknowledge alert
  const handleAcknowledgeAlert = async (alertId: string) => {
    try {
      await acknowledgeRiskAlert(alertId);
      setAlerts(alerts.filter((a) => a.id !== alertId));
      message.success('告警已确认');
    } catch (error: any) {
      message.error(error.response?.data?.message || '确认告警失败');
    }
  };

  // Export risk report
  const handleExportReport = async () => {
    setExporting(true);
    try {
      const blob = await exportRiskReport();
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `risk-report-${new Date().toISOString().split('T')[0]}.pdf`;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      message.success('风险报告已导出');
    } catch (error: any) {
      message.error(error.response?.data?.message || '导出风险报告失败');
    } finally {
      setExporting(false);
    }
  };

  // Get risk level text
  const getRiskLevelText = (level: string) => {
    switch (level) {
      case 'low':
        return '低风险';
      case 'medium':
        return '中等风险';
      case 'high':
        return '高风险';
      default:
        return '未知';
    }
  };

  // Get alert icon
  const getAlertIcon = (severity: string) => {
    switch (severity) {
      case 'critical':
        return <CloseCircleOutlined style={{ color: '#f5222d' }} />;
      case 'warning':
        return <WarningOutlined style={{ color: '#faad14' }} />;
      case 'info':
        return <CheckCircleOutlined style={{ color: '#1890ff' }} />;
      default:
        return null;
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" tip="加载中..." />
      </div>
    );
  }

  return (
    <div className="risk-dashboard-page">
      <div className="page-header">
        <div>
          <Title level={2}>风险仪表板</Title>
          <Text type="secondary">
            实时监控账户风险指标和告警
          </Text>
        </div>
        <Space>
          <Button icon={<ReloadOutlined />} onClick={loadData}>
            刷新
          </Button>
          <Button
            type="primary"
            icon={<DownloadOutlined />}
            loading={exporting}
            onClick={handleExportReport}
          >
            导出报告
          </Button>
        </Space>
      </div>

      {/* Risk Level Indicator */}
      {metrics && (
        <Alert
          message={`当前风险等级：${getRiskLevelText(metrics.riskLevel)}`}
          type={metrics.riskLevel === 'high' ? 'error' : metrics.riskLevel === 'medium' ? 'warning' : 'success'}
          showIcon
          style={{ marginBottom: 24 }}
        />
      )}

      {/* Active Alerts */}
      {alerts.length > 0 && (
        <Card
          title={
            <Space>
              <Badge count={alerts.length} offset={[10, 0]}>
                <WarningOutlined style={{ fontSize: 20 }} />
              </Badge>
              <span>活跃告警</span>
            </Space>
          }
          style={{ marginBottom: 24 }}
        >
          <List
            dataSource={alerts}
            renderItem={(alert) => (
              <List.Item
                actions={[
                  <Button
                    size="small"
                    onClick={() => handleAcknowledgeAlert(alert.id)}
                  >
                    确认
                  </Button>,
                ]}
              >
                <List.Item.Meta
                  avatar={getAlertIcon(alert.severity)}
                  title={
                    <Space>
                      <Tag color={alert.severity === 'critical' ? 'red' : alert.severity === 'warning' ? 'orange' : 'blue'}>
                        {alert.type.toUpperCase()}
                      </Tag>
                      {alert.symbol && <Tag>{alert.symbol}</Tag>}
                      <span>{alert.message}</span>
                    </Space>
                  }
                  description={new Date(alert.timestamp).toLocaleString('zh-CN')}
                />
              </List.Item>
            )}
          />
        </Card>
      )}

      {/* Risk Metrics */}
      {metrics ? (
        <>
          <Row gutter={[16, 16]} style={{ marginBottom: 16 }}>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="VaR (95% 置信度)"
                  value={metrics.valueAtRisk}
                  precision={2}
                  prefix="$"
                  valueStyle={{ color: '#cf1322' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="投资组合波动率"
                  value={metrics.portfolioVolatility * 100}
                  precision={2}
                  suffix="%"
                  valueStyle={{ color: '#1890ff' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="夏普比率"
                  value={metrics.sharpeRatio}
                  precision={2}
                  valueStyle={{ color: metrics.sharpeRatio >= 1 ? '#3f8600' : '#cf1322' }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} lg={6}>
              <Card>
                <Statistic
                  title="最大回撤"
                  value={metrics.maxDrawdown * 100}
                  precision={2}
                  suffix="%"
                  valueStyle={{ color: '#cf1322' }}
                />
              </Card>
            </Col>
          </Row>

          <Row gutter={[16, 16]}>
            <Col xs={24} md={12}>
              <Card title="持仓集中度风险">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div className="risk-item">
                    <Text type="secondary">最大单只持仓占比</Text>
                    <Text strong style={{ fontSize: 18 }}>
                      {(metrics.concentrationRisk.topPositionPercent * 100).toFixed(2)}%
                    </Text>
                  </div>
                  <div className="risk-item">
                    <Text type="secondary">最大行业集中度</Text>
                    <Space>
                      <Text strong style={{ fontSize: 18 }}>
                        {(metrics.concentrationRisk.topSectorPercent * 100).toFixed(2)}%
                      </Text>
                      <Tag>{metrics.concentrationRisk.topSectorName}</Tag>
                    </Space>
                  </div>
                </Space>
              </Card>
            </Col>

            <Col xs={24} md={12}>
              <Card title="交易状态">
                <Space direction="vertical" style={{ width: '100%' }}>
                  <div className="risk-item">
                    <Text type="secondary">保证金使用率</Text>
                    <Text
                      strong
                      style={{
                        fontSize: 18,
                        color: metrics.marginUsagePercent > 80 ? '#cf1322' : '#3f8600',
                      }}
                    >
                      {metrics.marginUsagePercent.toFixed(2)}%
                    </Text>
                  </div>
                  <div className="risk-item">
                    <Text type="secondary">当前回撤</Text>
                    <Text strong style={{ fontSize: 18, color: '#cf1322' }}>
                      {(metrics.currentDrawdown * 100).toFixed(2)}%
                    </Text>
                  </div>
                  <div className="risk-item">
                    <Text type="secondary">5日内日内交易次数</Text>
                    <Text
                      strong
                      style={{
                        fontSize: 18,
                        color: metrics.dayTradesCount >= 3 ? '#faad14' : '#3f8600',
                      }}
                    >
                      {metrics.dayTradesCount}
                    </Text>
                  </div>
                </Space>
              </Card>
            </Col>
          </Row>
        </>
      ) : (
        <Empty description="暂无风险数据" />
      )}
    </div>
  );
};

export default RiskDashboardPage;
