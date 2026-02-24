/**
 * Compare Backtests Page
 * 回测对比页面
 */

import React, { useState, useEffect, useRef } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  message,
  Table,
  Select,
  Statistic,
  Row,
  Col,
  Alert,
} from 'antd';
import {
  ArrowLeftOutlined,
  StarFilled,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import * as echarts from 'echarts';
import type { EChartsOption } from 'echarts';
import {
  getBacktests,
  compareBacktests,
} from '../api/backtestsApi';
import type { BacktestSummary, CompareBacktestsResponse, BacktestComparison } from '../types/backtest';
import './CompareBacktestsPage.css';

const { Title, Text } = Typography;
const { Option } = Select;
const { Column } = Table;

/**
 * CompareBacktestsPage Component
 * Allows users to compare multiple backtest results
 * 允许用户对比多个回测结果
 */
const CompareBacktestsPage: React.FC = () => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [comparing, setComparing] = useState(false);
  const [backtests, setBacktests] = useState<BacktestSummary[]>([]);
  const [selectedIds, setSelectedIds] = useState<number[]>([]);
  const [comparisonData, setComparisonData] = useState<CompareBacktestsResponse | null>(null);

  // Chart refs
  const comparisonCharRef = useRef<HTMLDivElement>(null);
  const comparisonChartInstance = useRef<echarts.ECharts | null>(null);

  /**
   * Load backtests list
   * 加载回测列表
   */
  useEffect(() => {
    const loadBacktests = async () => {
      try {
        setLoading(true);
        const result = await getBacktests({ status: 'Completed' });
        setBacktests(result.backtests);
      } catch (error) {
        console.error('Failed to load backtests:', error);
        message.error('加载回测列表失败');
      } finally {
        setLoading(false);
      }
    };

    loadBacktests();
  }, []);

  /**
   * Handle compare
   * 处理对比操作
   */
  const handleCompare = async () => {
    if (selectedIds.length < 2) {
      message.warning('请至少选择2个回测');
      return;
    }

    if (selectedIds.length > 4) {
      message.warning('最多只能对比4个回测');
      return;
    }

    try {
      setComparing(true);
      const result = await compareBacktests({ backtestIds: selectedIds });
      setComparisonData(result);
      
      // Initialize comparison chart
      setTimeout(() => {
        initializeComparisonChart(result.comparisons);
      }, 100);
    } catch (error) {
      console.error('Failed to compare backtests:', error);
      message.error('对比回测失败');
    } finally {
      setComparing(false);
    }
  };

  /**
   * Initialize comparison chart
   * 初始化对比图表
   */
  const initializeComparisonChart = (comparisons: BacktestComparison[]) => {
    if (!comparisonCharRef.current) return;

    if (comparisonChartInstance.current) {
      comparisonChartInstance.current.dispose();
    }

    const chart = echarts.init(comparisonCharRef.current);
    comparisonChartInstance.current = chart;

    const names = comparisons.map((c) => c.backtest.name);
    const metrics = [
      { name: '总收益率 (%)', key: 'totalReturn' },
      { name: '夏普比率', key: 'sharpeRatio' },
      { name: '胜率 (%)', key: 'winRate' },
    ];

    const series = metrics.map((metric) => ({
      name: metric.name,
      type: 'bar',
      data: comparisons.map((c) => (c as any)[metric.key]),
      label: {
        show: true,
        position: 'top',
        formatter: (params: any) => params.value.toFixed(2),
      },
    }));

    const option: EChartsOption = {
      title: {
        text: '回测性能对比',
        left: 'center',
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'shadow' },
      },
      legend: {
        data: metrics.map((m) => m.name),
        bottom: 10,
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '15%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        data: names,
        axisLabel: {
          interval: 0,
          rotate: 30,
        },
      },
      yAxis: {
        type: 'value',
      },
      series: series as any,
    };

    chart.setOption(option);

    // Resize handler
    window.addEventListener('resize', () => chart.resize());
  };

  /**
   * Get best indicator
   * 获取最佳指标标识
   */
  const isBest = (backtestId: number, metric: keyof CompareBacktestsResponse['bestByMetric']): boolean => {
    return comparisonData?.bestByMetric[metric] === backtestId;
  };

  /**
   * Render value with best indicator
   * 渲染带最佳标识的值
   */
  const renderValueWithBest = (value: number, backtestId: number, metric: keyof CompareBacktestsResponse['bestByMetric']) => {
    const best = isBest(backtestId, metric);
    return (
      <Space>
        <Text strong={best} style={{ color: best ? '#52c41a' : undefined }}>
          {value.toFixed(2)}
          {metric === 'totalReturn' && '%'}
        </Text>
        {best && <StarFilled style={{ color: '#faad14' }} />}
      </Space>
    );
  };

  return (
    <div className="compare-backtests-page">
      {/* Header */}
      <Card className="page-header-card">
        <Space>
          <Button
            type="text"
            icon={<ArrowLeftOutlined />}
            onClick={() => navigate('/backtests')}
          >
            返回
          </Button>
          <Title level={2} style={{ margin: 0 }}>
            回测对比
          </Title>
        </Space>
      </Card>

      <Card>
        <Alert
          message="选择要对比的回测"
          description="最少选择2个，最多选择4个已完成的回测进行对比分析。"
          type="info"
          showIcon
          closable
          style={{ marginBottom: 24 }}
        />

        <Space style={{ marginBottom: 16 }}>
          <Text strong>选择回测:</Text>
          <Select
            mode="multiple"
            style={{ minWidth: 400 }}
            placeholder="选择要对比的回测"
            value={selectedIds}
            onChange={setSelectedIds}
            loading={loading}
            maxTagCount={4}
          >
            {backtests.map((backtest) => (
              <Option key={backtest.id} value={backtest.id}>
                {backtest.name} ({backtest.startDate} ~ {backtest.endDate})
              </Option>
            ))}
          </Select>
          <Button
            type="primary"
            onClick={handleCompare}
            loading={comparing}
            disabled={selectedIds.length < 2 || selectedIds.length > 4}
          >
            开始对比
          </Button>
        </Space>

        {comparisonData && (
          <>
            {/* Comparison Chart */}
            <Card title="性能指标对比" style={{ marginTop: 24 }}>
              <div ref={comparisonCharRef} style={{ width: '100%', height: 400 }} />
            </Card>

            {/* Comparison Table */}
            <Card title="详细对比" style={{ marginTop: 24 }}>
              <Table
                dataSource={comparisonData.comparisons}
                rowKey={(record) => record.backtest.id}
                pagination={false}
                scroll={{ x: true }}
              >
                <Column
                  title="回测名称"
                  dataIndex={['backtest', 'name']}
                  key="name"
                  fixed="left"
                  width={200}
                />
                <Column
                  title="策略"
                  dataIndex={['backtest', 'strategyName']}
                  key="strategy"
                  width={150}
                />
                <Column
                  title="总收益率"
                  dataIndex="totalReturn"
                  key="totalReturn"
                  align="right"
                  render={(value, record: any) =>
                    renderValueWithBest(value, record.backtest.id, 'totalReturn')
                  }
                />
                <Column
                  title="年化收益率"
                  dataIndex="annualizedReturn"
                  key="annualizedReturn"
                  align="right"
                  render={(value) => `${value.toFixed(2)}%`}
                />
                <Column
                  title="夏普比率"
                  dataIndex="sharpeRatio"
                  key="sharpeRatio"
                  align="right"
                  render={(value, record: any) =>
                    renderValueWithBest(value, record.backtest.id, 'sharpeRatio')
                  }
                />
                <Column
                  title="最大回撤"
                  dataIndex="maxDrawdown"
                  key="maxDrawdown"
                  align="right"
                  render={(value, record: any) => (
                    <Space>
                      <Text
                        strong={isBest(record.backtest.id, 'maxDrawdown')}
                        style={{ color: '#cf1322' }}
                      >
                        {value.toFixed(2)}%
                      </Text>
                      {isBest(record.backtest.id, 'maxDrawdown') && (
                        <StarFilled style={{ color: '#faad14' }} />
                      )}
                    </Space>
                  )}
                />
                <Column
                  title="胜率"
                  dataIndex="winRate"
                  key="winRate"
                  align="right"
                  render={(value) => `${value.toFixed(2)}%`}
                />
                <Column
                  title="盈利因子"
                  dataIndex="profitFactor"
                  key="profitFactor"
                  align="right"
                  render={(value) => value.toFixed(2)}
                />
                <Column
                  title="总交易次数"
                  dataIndex="totalTrades"
                  key="totalTrades"
                  align="right"
                />
              </Table>
            </Card>

            {/* Summary */}
            <Card title="对比总结" style={{ marginTop: 24 }}>
              <Row gutter={16}>
                <Col span={8}>
                  <Card>
                    <Statistic
                      title="最高总收益率"
                      value={
                        comparisonData.comparisons.find(
                          (c) => c.backtest.id === comparisonData.bestByMetric.totalReturn
                        )?.totalReturn || 0
                      }
                      precision={2}
                      suffix="%"
                      valueStyle={{ color: '#3f8600' }}
                    />
                    <Text type="secondary" style={{ marginTop: 8, display: 'block' }}>
                      {comparisonData.comparisons.find(
                        (c) => c.backtest.id === comparisonData.bestByMetric.totalReturn
                      )?.backtest.name}
                    </Text>
                  </Card>
                </Col>
                <Col span={8}>
                  <Card>
                    <Statistic
                      title="最高夏普比率"
                      value={
                        comparisonData.comparisons.find(
                          (c) => c.backtest.id === comparisonData.bestByMetric.sharpeRatio
                        )?.sharpeRatio || 0
                      }
                      precision={2}
                      valueStyle={{ color: '#1890ff' }}
                    />
                    <Text type="secondary" style={{ marginTop: 8, display: 'block' }}>
                      {comparisonData.comparisons.find(
                        (c) => c.backtest.id === comparisonData.bestByMetric.sharpeRatio
                      )?.backtest.name}
                    </Text>
                  </Card>
                </Col>
                <Col span={8}>
                  <Card>
                    <Statistic
                      title="最小回撤"
                      value={
                        comparisonData.comparisons.find(
                          (c) => c.backtest.id === comparisonData.bestByMetric.maxDrawdown
                        )?.maxDrawdown || 0
                      }
                      precision={2}
                      suffix="%"
                      valueStyle={{ color: '#52c41a' }}
                    />
                    <Text type="secondary" style={{ marginTop: 8, display: 'block' }}>
                      {comparisonData.comparisons.find(
                        (c) => c.backtest.id === comparisonData.bestByMetric.maxDrawdown
                      )?.backtest.name}
                    </Text>
                  </Card>
                </Col>
              </Row>
            </Card>
          </>
        )}
      </Card>
    </div>
  );
};

export default CompareBacktestsPage;
