/**
 * Portfolio Analysis Page
 * 投资组合分析页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Space,
  Button,
  DatePicker,
  Select,
  Spin,
  message,
  Tabs,
  Typography,
  Divider,
} from 'antd';
import {
  ArrowLeftOutlined,
  ReloadOutlined,
  RiseOutlined,
  FallOutlined,
} from '@ant-design/icons';
import type { Dayjs } from 'dayjs';
import dayjs from 'dayjs';
import { useNavigate } from 'react-router-dom';
import {
  getPortfolioSummary,
  getPositionAllocation,
  getSectorAllocation,
  getEquityCurve,
} from '../api/positionsApi';
import PositionAllocationChart from '../components/portfolio/PositionAllocationChart';
import EquityCurveChart from '../components/portfolio/EquityCurveChart';
import type {
  PortfolioSummary,
  AllocationItem,
  EquityCurveData,
} from '../types/position';
import './PortfolioAnalysisPage.css';

const { RangePicker } = DatePicker;
const { Title, Text } = Typography;

/**
 * PortfolioAnalysisPage Component
 * Comprehensive portfolio analysis with charts and statistics
 * 全面的投资组合分析，包含图表和统计数据
 */
const PortfolioAnalysisPage: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [summary, setSummary] = useState<PortfolioSummary | null>(null);
  const [positionAllocation, setPositionAllocation] = useState<AllocationItem[]>([]);
  const [sectorAllocation, setSectorAllocation] = useState<AllocationItem[]>([]);
  const [equityCurve, setEquityCurve] = useState<EquityCurveData[]>([]);

  // Date range for equity curve
  const [dateRange, setDateRange] = useState<[Dayjs | null, Dayjs | null]>([
    dayjs().subtract(3, 'months'),
    dayjs(),
  ]);

  /**
   * Load all data
   * 加载所有数据
   */
  const loadAllData = async () => {
    setLoading(true);
    try {
      // Load summary
      const summaryData = await getPortfolioSummary();
      setSummary(summaryData);

      // Load allocations
      const posAlloc = await getPositionAllocation();
      setPositionAllocation(posAlloc);

      const secAlloc = await getSectorAllocation();
      setSectorAllocation(secAlloc);

      // Load equity curve
      const params = {
        startDate: dateRange[0]?.format('YYYY-MM-DD'),
        endDate: dateRange[1]?.format('YYYY-MM-DD'),
      };
      const equityCurveData = await getEquityCurve(params);
      setEquityCurve(equityCurveData);
    } catch (error) {
      console.error('Failed to load portfolio data:', error);
      message.error('加载投资组合数据失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Load data on mount and when date range changes
   * 组件挂载和日期范围变化时加载数据
   */
  useEffect(() => {
    loadAllData();
  }, [dateRange]);

  /**
   * Calculate performance metrics from available data
   * 从现有数据计算性能指标
   */
  const calculateMetrics = () => {
    if (!summary) return null;

    // Derive max drawdown from equity curve if available
    let maxDrawdown = 0;
    if (equityCurve.length > 1) {
      let peak = equityCurve[0].equity;
      for (const point of equityCurve) {
        if (point.equity > peak) peak = point.equity;
        const drawdown = peak > 0 ? ((point.equity - peak) / peak) * 100 : 0;
        if (drawdown < maxDrawdown) maxDrawdown = drawdown;
      }
    }

    // Total return from summary data
    const totalReturn = summary.totalCostBasis > 0
      ? ((summary.totalMarketValue - summary.totalCostBasis) / summary.totalCostBasis) * 100
      : 0;

    return {
      sharpeRatio: null as number | null, // Requires risk-free rate and historical volatility
      maxDrawdown,
      totalReturn,
    };
  };

  const metrics = calculateMetrics();

  if (loading && !summary) {
    return (
      <div className="portfolio-analysis-page">
        <Spin size="large" tip="加载中..." />
      </div>
    );
  }

  return (
    <div className="portfolio-analysis-page">
      {/* Header */}
      <div className="page-header">
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/positions')}>
            返回
          </Button>
          <Title level={3} style={{ margin: 0 }}>
            投资组合分析
          </Title>
        </Space>
        <Space>
          <RangePicker
            value={dateRange}
            onChange={(dates) => setDateRange(dates as [Dayjs, Dayjs])}
            format="YYYY-MM-DD"
          />
          <Button icon={<ReloadOutlined />} onClick={loadAllData} loading={loading}>
            刷新
          </Button>
        </Space>
      </div>

      {/* Portfolio Summary */}
      {summary && (
        <>
          <Card title="投资组合概览" style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="总市值"
                  value={summary.totalMarketValue}
                  precision={2}
                  prefix="$"
                  valueStyle={{ color: '#1890ff' }}
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="总成本"
                  value={summary.totalCostBasis}
                  precision={2}
                  prefix="$"
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="未实现盈亏"
                  value={Math.abs(summary.totalUnrealizedPnL)}
                  precision={2}
                  prefix={summary.totalUnrealizedPnL >= 0 ? '+$' : '-$'}
                  valueStyle={{
                    color: summary.totalUnrealizedPnL >= 0 ? '#3f8600' : '#cf1322',
                  }}
                  suffix={
                    <span style={{ fontSize: 14 }}>
                      ({summary.totalUnrealizedPnLPercent >= 0 ? '+' : ''}
                      {summary.totalUnrealizedPnLPercent.toFixed(2)}%)
                    </span>
                  }
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="现金余额"
                  value={summary.cashBalance}
                  precision={2}
                  prefix="$"
                />
              </Col>
            </Row>

            <Divider />

            <Row gutter={16}>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="已实现盈亏"
                  value={Math.abs(summary.totalRealizedPnL)}
                  precision={2}
                  prefix={summary.totalRealizedPnL >= 0 ? '+$' : '-$'}
                  valueStyle={{
                    color: summary.totalRealizedPnL >= 0 ? '#3f8600' : '#cf1322',
                  }}
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="持仓收益率"
                  value={Math.abs(metrics?.totalReturn ?? 0)}
                  precision={2}
                  suffix="%"
                  prefix={(metrics?.totalReturn ?? 0) >= 0 ? '+' : '-'}
                  valueStyle={{
                    color: (metrics?.totalReturn ?? 0) >= 0 ? '#3f8600' : '#cf1322',
                  }}
                />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic title="持仓数量" value={summary.positions.length} suffix="个" />
              </Col>
              <Col xs={24} sm={12} md={6}>
                <Statistic
                  title="总资产"
                  value={summary.totalMarketValue + summary.cashBalance}
                  precision={2}
                  prefix="$"
                  valueStyle={{ color: '#722ed1' }}
                />
              </Col>
            </Row>
          </Card>

          {/* Performance Metrics */}
          {metrics && (
            <Card title="性能指标" style={{ marginBottom: 16 }}>
              <Row gutter={16}>
                {metrics.sharpeRatio !== null && (
                  <Col xs={24} sm={8}>
                    <Statistic
                      title="夏普比率"
                      value={metrics.sharpeRatio}
                      precision={2}
                      valueStyle={{ color: metrics.sharpeRatio > 1 ? '#3f8600' : '#faad14' }}
                    />
                  </Col>
                )}
                <Col xs={24} sm={8}>
                  <Statistic
                    title="最大回撤"
                    value={Math.abs(metrics.maxDrawdown)}
                    precision={1}
                    suffix="%"
                    prefix="-"
                    valueStyle={{ color: '#cf1322' }}
                  />
                </Col>
                <Col xs={24} sm={8}>
                  <Statistic
                    title="总收益率"
                    value={Math.abs(metrics.totalReturn)}
                    precision={2}
                    suffix="%"
                    prefix={metrics.totalReturn >= 0 ? '+' : '-'}
                    valueStyle={{ color: metrics.totalReturn >= 0 ? '#3f8600' : '#cf1322' }}
                  />
                </Col>
              </Row>
            </Card>
          )}
        </>
      )}

      {/* Charts */}
      <Card style={{ marginBottom: 16 }}>
        <Tabs
          defaultActiveKey="equity"
          items={[
            {
              key: 'equity',
              label: '收益曲线',
              children: (
                <EquityCurveChart
                  data={equityCurve}
                  title="账户收益曲线"
                  height={500}
                />
              ),
            },
            {
              key: 'position',
              label: '持仓配置',
              children: (
                <Row gutter={16}>
                  <Col xs={24} lg={12}>
                    <PositionAllocationChart
                      data={positionAllocation}
                      title="持仓配置（按股票）"
                      height={400}
                    />
                  </Col>
                  <Col xs={24} lg={12}>
                    <PositionAllocationChart
                      data={sectorAllocation}
                      title="持仓配置（按行业）"
                      height={400}
                    />
                  </Col>
                </Row>
              ),
            },
          ]}
        />
      </Card>

      {/* Risk Analysis */}
      <Card title="风险分析" style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col xs={24} md={12}>
            <Card type="inner" title="持仓集中度">
              {positionAllocation.length > 0 && (
                <>
                  <Text>
                    最大持仓占比: {positionAllocation[0]?.percentage.toFixed(2)}% (
                    {positionAllocation[0]?.name})
                  </Text>
                  <br />
                  <Text>
                    前三大持仓占比:{' '}
                    {positionAllocation
                      .slice(0, 3)
                      .reduce((sum, item) => sum + item.percentage, 0)
                      .toFixed(2)}
                    %
                  </Text>
                </>
              )}
            </Card>
          </Col>
          <Col xs={24} md={12}>
            <Card type="inner" title="行业分散度">
              {sectorAllocation.length > 0 && (
                <>
                  <Text>行业数量: {sectorAllocation.length} 个</Text>
                  <br />
                  <Text>
                    最大行业占比: {sectorAllocation[0]?.percentage.toFixed(2)}% (
                    {sectorAllocation[0]?.name})
                  </Text>
                </>
              )}
            </Card>
          </Col>
        </Row>
      </Card>
    </div>
  );
};

export default PortfolioAnalysisPage;
