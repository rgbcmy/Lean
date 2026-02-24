/**
 * Backtest Result Page
 * 回测结果页面
 */

import React, { useState, useEffect, useRef } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  message,
  Row,
  Col,
  Statistic,
  Tag,
  Tabs,
  Table,
  Spin,
  Progress,
  Descriptions,
  Dropdown,
  Menu,
} from 'antd';
import {
  ArrowLeftOutlined,
  PlayCircleOutlined,
  StopOutlined,
  DownloadOutlined,
  ReloadOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import * as echarts from 'echarts';
import type { EChartsOption } from 'echarts';
import {
  getBacktestById,
  getBacktestStatus,
  getBacktestCharts,
  startBacktest,
  stopBacktest,
  exportBacktest,
} from '../api/backtestsApi';
import type { BacktestDetail, BacktestStatusResponse, BacktestCharts } from '../types/backtest';
import './BacktestResultPage.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;
const { Column } = Table;

/**
 * BacktestResultPage Component
 * Displays backtest results, charts, and trade history
 * 显示回测结果、图表和交易历史
 */
const BacktestResultPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [backtest, setBacktest] = useState<BacktestDetail | null>(null);
  const [status, setStatus] = useState<BacktestStatusResponse | null>(null);
  const [charts, setCharts] = useState<BacktestCharts | null>(null);
  const [actionLoading, setActionLoading] = useState(false);
  const [exporting, setExporting] = useState(false);

  // Chart refs
  const equityChartRef = useRef<HTMLDivElement>(null);
  const drawdownChartRef = useRef<HTMLDivElement>(null);
  const equityChartInstance = useRef<echarts.ECharts | null>(null);
  const drawdownChartInstance = useRef<echarts.ECharts | null>(null);

  // Polling interval for running backtests
  const pollingIntervalRef = useRef<number | null>(null);

  /**
   * Load backtest details
   * 加载回测详情
   */
  const loadBacktest = async () => {
    try {
      setLoading(true);
      const backtestId = parseInt(id!);
      const data = await getBacktestById(backtestId);
      setBacktest(data);

      // Load charts if backtest is completed
      if (data.status === 'Completed') {
        const chartsData = await getBacktestCharts(backtestId);
        setCharts(chartsData);
      }

      // Load status if running
      if (data.status === 'Running') {
        const statusData = await getBacktestStatus(backtestId);
        setStatus(statusData);
        startPolling();
      }
    } catch (error: any) {
      console.error('Failed to load backtest:', error);
      message.error('加载回测详情失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Start polling for backtest status
   * 开始轮询回测状态
   */
  const startPolling = () => {
    if (pollingIntervalRef.current) return;

    pollingIntervalRef.current = setInterval(async () => {
      try {
        const backtestId = parseInt(id!);
        const statusData = await getBacktestStatus(backtestId);
        setStatus(statusData);

        // Stop polling if backtest is no longer running
        if (statusData.status !== 'Running') {
          stopPolling();
          loadBacktest();
        }
      } catch (error) {
        console.error('Failed to fetch status:', error);
      }
    }, 3000); // Poll every 3 seconds
  };

  /**
   * Stop polling
   * 停止轮询
   */
  const stopPolling = () => {
    if (pollingIntervalRef.current) {
      clearInterval(pollingIntervalRef.current);
      pollingIntervalRef.current = null;
    }
  };

  /**
   * Initialize equity curve chart
   * 初始化收益曲线图
   */
  const initializeEquityChart = () => {
    if (!equityChartRef.current || !charts?.equityCurve) return;

    if (equityChartInstance.current) {
      equityChartInstance.current.dispose();
    }

    const chart = echarts.init(equityChartRef.current);
    equityChartInstance.current = chart;

    const times = charts.equityCurve.map((p) => p.time);
    const equityData = charts.equityCurve.map((p) => p.equity);
    const benchmarkData = charts.equityCurve.map((p) => p.benchmarkEquity || null);

    const series: any[] = [
      {
        name: '策略净值',
        type: 'line',
        data: equityData,
        smooth: true,
        lineStyle: { width: 2, color: '#1890ff' },
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(24, 144, 255, 0.3)' },
            { offset: 1, color: 'rgba(24, 144, 255, 0.05)' },
          ]),
        },
      },
    ];

    if (benchmarkData.some((v) => v !== null)) {
      series.push({
        name: '基准净值',
        type: 'line',
        data: benchmarkData,
        smooth: true,
        lineStyle: { width: 2, color: '#52c41a', type: 'dashed' },
      });
    }

    const option: EChartsOption = {
      title: {
        text: '收益曲线',
        left: 'center',
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'cross' },
      },
      legend: {
        data: series.map((s) => s.name),
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
        data: times,
        boundaryGap: false,
      },
      yAxis: {
        type: 'value',
        axisLabel: {
          formatter: (value: number) => `$${value.toLocaleString()}`,
        },
      },
      series,
    };

    chart.setOption(option);

    // Resize handler
    window.addEventListener('resize', () => chart.resize());
  };

  /**
   * Initialize drawdown chart
   * 初始化回撤图
   */
  const initializeDrawdownChart = () => {
    if (!drawdownChartRef.current || !charts?.drawdown) return;

    if (drawdownChartInstance.current) {
      drawdownChartInstance.current.dispose();
    }

    const chart = echarts.init(drawdownChartRef.current);
    drawdownChartInstance.current = chart;

    const times = charts.drawdown.map((p) => p.time);
    const drawdownData = charts.drawdown.map((p) => -p.drawdownPercent);

    const option: EChartsOption = {
      title: {
        text: '回撤图',
        left: 'center',
      },
      tooltip: {
        trigger: 'axis',
        axisPointer: { type: 'cross' },
        formatter: (params: any) => {
          const point = params[0];
          return `${point.name}<br/>${point.seriesName}: ${Math.abs(point.value).toFixed(2)}%`;
        },
      },
      grid: {
        left: '3%',
        right: '4%',
        bottom: '10%',
        containLabel: true,
      },
      xAxis: {
        type: 'category',
        data: times,
        boundaryGap: false,
      },
      yAxis: {
        type: 'value',
        axisLabel: {
          formatter: (value: number) => `${Math.abs(value).toFixed(1)}%`,
        },
        inverse: false,
      },
      series: [
        {
          name: '回撤比例',
          type: 'line',
          data: drawdownData,
          smooth: true,
          lineStyle: { width: 2, color: '#ff4d4f' },
          areaStyle: {
            color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
              { offset: 0, color: 'rgba(255, 77, 79, 0.3)' },
              { offset: 1, color: 'rgba(255, 77, 79, 0.05)' },
            ]),
          },
        },
      ],
    };

    chart.setOption(option);

    // Resize handler
    window.addEventListener('resize', () => chart.resize());
  };

  /**
   * Initialize charts when charts data is loaded
   * 图表数据加载后初始化图表
   */
  useEffect(() => {
    if (charts) {
      setTimeout(() => {
        initializeEquityChart();
        initializeDrawdownChart();
      }, 100);
    }

    return () => {
      equityChartInstance.current?.dispose();
      drawdownChartInstance.current?.dispose();
    };
  }, [charts]);

  /**
   * Load data on mount
   * 组件挂载时加载数据
   */
  useEffect(() => {
    loadBacktest();

    return () => {
      stopPolling();
    };
  }, [id]);

  /**
   * Handle start backtest
   * 处理启动回测
   */
  const handleStart = async () => {
    try {
      setActionLoading(true);
      await startBacktest(parseInt(id!));
      message.success('回测已启动');
      startPolling();
      await loadBacktest();
    } catch (error: any) {
      console.error('Failed to start backtest:', error);
      message.error('启动回测失败');
    } finally {
      setActionLoading(false);
    }
  };

  /**
   * Handle stop backtest
   * 处理停止回测
   */
  const handleStop = async () => {
    try {
      setActionLoading(true);
      await stopBacktest(parseInt(id!));
      message.success('回测已停止');
      stopPolling();
      await loadBacktest();
    } catch (error: any) {
      console.error('Failed to stop backtest:', error);
      message.error('停止回测失败');
    } finally {
      setActionLoading(false);
    }
  };

  /**
   * Handle export backtest
   * 处理导出回测
   */
  const handleExport = async (format: 'PDF' | 'CSV' | 'JSON') => {
    try {
      setExporting(true);
      const blob = await exportBacktest(parseInt(id!), format);
      
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `backtest_${id}_${new Date().getTime()}.${format.toLowerCase()}`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
      
      message.success(`导出${format}成功`);
    } catch (error) {
      console.error('Failed to export backtest:', error);
      message.error('导出失败');
    } finally {
      setExporting(false);
    }
  };

  /**
   * Get status tag
   * 获取状态标签
   */
  const getStatusTag = (status: string) => {
    const statusMap: Record<string, { color: string; text: string }> = {
      Pending: { color: 'default', text: '待执行' },
      Running: { color: 'processing', text: '运行中' },
      Completed: { color: 'success', text: '已完成' },
      Failed: { color: 'error', text: '失败' },
      Cancelled: { color: 'warning', text: '已取消' },
    };

    const config = statusMap[status] || { color: 'default', text: status };
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  /**
   * Render export menu
   */
  const exportMenu = (
    <Menu>
      <Menu.Item key="pdf" onClick={() => handleExport('PDF')}>
        导出 PDF 报告
      </Menu.Item>
      <Menu.Item key="csv" onClick={() => handleExport('CSV')}>
        导出 CSV 交易记录
      </Menu.Item>
      <Menu.Item key="json" onClick={() => handleExport('JSON')}>
        导出 JSON 原始数据
      </Menu.Item>
    </Menu>
  );

  if (loading) {
    return (
      <div className="backtest-result-page">
        <Card>
          <Spin size="large" tip="加载中..." />
        </Card>
      </div>
    );
  }

  if (!backtest) {
    return (
      <div className="backtest-result-page">
        <Card>
          <Text>回测不存在</Text>
        </Card>
      </div>
    );
  }

  return (
    <div className="backtest-result-page">
      {/* Header */}
      <Card className="page-header-card">
        <Row justify="space-between" align="middle">
          <Col>
            <Space>
              <Button
                type="text"
                icon={<ArrowLeftOutlined />}
                onClick={() => navigate('/backtests')}
              >
                返回
              </Button>
              <Title level={2} style={{ margin: 0 }}>
                {backtest.name}
              </Title>
              {getStatusTag(backtest.status)}
            </Space>
          </Col>
          <Col>
            <Space>
              {backtest.status === 'Pending' && (
                <Button
                  type="primary"
                  icon={<PlayCircleOutlined />}
                  onClick={handleStart}
                  loading={actionLoading}
                >
                  启动
                </Button>
              )}
              {backtest.status === 'Running' && (
                <Button
                  danger
                  icon={<StopOutlined />}
                  onClick={handleStop}
                  loading={actionLoading}
                >
                  停止
                </Button>
              )}
              {backtest.status === 'Completed' && (
                <Dropdown menu={{ items: exportMenu.props.children.map((item: any) => ({ key: item.key, label: item.props.children, onClick: item.props.onClick })) }} placement="bottomRight">
                  <Button
                    icon={<DownloadOutlined />}
                    loading={exporting}
                  >
                    导出
                  </Button>
                </Dropdown>
              )}
              <Button
                icon={<ReloadOutlined />}
                onClick={loadBacktest}
              >
                刷新
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Running Status */}
      {backtest.status === 'Running' && status && (
        <Card style={{ marginBottom: 16 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Text strong>回测进度</Text>
            <Progress
              percent={status.progress || 0}
              status="active"
              format={(percent) => `${percent?.toFixed(1)}%`}
            />
            {status.currentDate && (
              <Text type="secondary">当前回测日期: {status.currentDate}</Text>
            )}
            {status.message && <Text>{status.message}</Text>}
          </Space>
        </Card>
      )}

      {/* Basic Info */}
      <Card title="基本信息" style={{ marginBottom: 16 }}>
        <Descriptions column={3} bordered>
          <Descriptions.Item label="策略名称">{backtest.strategyName}</Descriptions.Item>
          <Descriptions.Item label="回测日期">
            {backtest.startDate} ~ {backtest.endDate}
          </Descriptions.Item>
          <Descriptions.Item label="初始资金">
            ${backtest.initialCapital.toLocaleString()}
          </Descriptions.Item>
          {backtest.benchmark && (
            <Descriptions.Item label="基准指数">{backtest.benchmark}</Descriptions.Item>
          )}
          {backtest.dataResolution && (
            <Descriptions.Item label="数据频率">{backtest.dataResolution}</Descriptions.Item>
          )}
          {backtest.executionTime && (
            <Descriptions.Item label="执行时间">
              {backtest.executionTime.toFixed(2)} 秒
            </Descriptions.Item>
          )}
        </Descriptions>
      </Card>

      {/* Performance Metrics */}
      {backtest.status === 'Completed' && (
        <>
          <Card title="性能指标" style={{ marginBottom: 16 }}>
            <Row gutter={16}>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="总收益率"
                  value={backtest.totalReturn || 0}
                  precision={2}
                  suffix="%"
                  valueStyle={{
                    color: (backtest.totalReturn || 0) >= 0 ? '#3f8600' : '#cf1322',
                  }}
                />
              </Col>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="年化收益率"
                  value={backtest.annualizedReturn || 0}
                  precision={2}
                  suffix="%"
                />
              </Col>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="夏普比率"
                  value={backtest.sharpeRatio || 0}
                  precision={2}
                />
              </Col>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="最大回撤"
                  value={backtest.maxDrawdownPercent || 0}
                  precision={2}
                  suffix="%"
                  valueStyle={{ color: '#cf1322' }}
                />
              </Col>
            </Row>
            <Row gutter={16} style={{ marginTop: 16 }}>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="胜率"
                  value={backtest.winRate || 0}
                  precision={2}
                  suffix="%"
                />
              </Col>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="盈利因子"
                  value={backtest.profitFactor || 0}
                  precision={2}
                />
              </Col>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="总交易次数"
                  value={backtest.totalTrades || 0}
                />
              </Col>
              <Col xs={24} sm={12} md={8} lg={6}>
                <Statistic
                  title="平均持仓天数"
                  value={backtest.averageHoldingPeriod || 0}
                  precision={1}
                />
              </Col>
            </Row>
          </Card>

          {/* Charts and Trades */}
          <Card>
            <Tabs defaultActiveKey="equity">
              <TabPane tab="收益曲线" key="equity">
                <div ref={equityChartRef} style={{ width: '100%', height: 400 }} />
              </TabPane>
              <TabPane tab="回撤图" key="drawdown">
                <div ref={drawdownChartRef} style={{ width: '100%', height: 400 }} />
              </TabPane>
              <TabPane tab="交易记录" key="trades">
                <Table
                  dataSource={charts?.trades || []}
                  rowKey="id"
                  pagination={{ pageSize: 20 }}
                  scroll={{ x: true }}
                >
                  <Column title="时间" dataIndex="time" key="time" />
                  <Column title="股票" dataIndex="symbol" key="symbol" />
                  <Column
                    title="方向"
                    dataIndex="direction"
                    key="direction"
                    render={(direction: string) => (
                      <Tag color={direction === 'Buy' ? 'green' : 'red'}>
                        {direction === 'Buy' ? '买入' : '卖出'}
                      </Tag>
                    )}
                  />
                  <Column
                    title="数量"
                    dataIndex="quantity"
                    key="quantity"
                    align="right"
                  />
                  <Column
                    title="价格"
                    dataIndex="price"
                    key="price"
                    align="right"
                    render={(price: number) => `$${price.toFixed(2)}`}
                  />
                  <Column
                    title="交易额"
                    dataIndex="value"
                    key="value"
                    align="right"
                    render={(value: number) => `$${value.toLocaleString()}`}
                  />
                  <Column
                    title="手续费"
                    dataIndex="commission"
                    key="commission"
                    align="right"
                    render={(commission: number) => `$${commission.toFixed(2)}`}
                  />
                  <Column
                    title="盈亏"
                    dataIndex="profitLoss"
                    key="profitLoss"
                    align="right"
                    render={(pl: number | undefined) =>
                      pl !== undefined ? (
                        <Text
                          style={{ color: pl >= 0 ? '#3f8600' : '#cf1322' }}
                        >
                          ${pl.toFixed(2)}
                        </Text>
                      ) : (
                        '-'
                      )
                    }
                  />
                </Table>
              </TabPane>
            </Tabs>
          </Card>
        </>
      )}

      {/* Error Info */}
      {backtest.status === 'Failed' && backtest.errorMessage && (
        <Card title="错误信息" style={{ marginTop: 16 }}>
          <Text type="danger">{backtest.errorMessage}</Text>
        </Card>
      )}
    </div>
  );
};

export default BacktestResultPage;
