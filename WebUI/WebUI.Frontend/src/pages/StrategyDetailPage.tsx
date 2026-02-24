/**
 * Strategy Detail Page
 * 策略详情页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Space,
  Typography,
  message,
  Spin,
  Tabs,
  Descriptions,
  Tag,
  Badge,
  Statistic,
  Row,
  Col,
  Table,
  Modal,
  List,
  Tooltip,
} from 'antd';
import {
  ArrowLeftOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  EditOutlined,
  CopyOutlined,
  DeleteOutlined,
  ExportOutlined,
  ExclamationCircleOutlined,
  ReloadOutlined,
  HistoryOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate, useParams } from 'react-router-dom';
import {
  getStrategyById,
  getStrategyPerformance,
  getStrategyExecutions,
  startStrategy,
  stopStrategy,
  cloneStrategy,
  deleteStrategy,
  exportStrategy,
} from '../api/strategiesApi';
import type { Strategy, StrategyPerformance, StrategyExecution, StrategyStatus } from '../types/strategy';
import './StrategyDetailPage.css';

const { Title, Text, Paragraph } = Typography;
const { TabPane } = Tabs;
const { confirm } = Modal;

/**
 * StrategyDetailPage Component
 * Displays detailed information about a strategy
 * 显示策略的详细信息
 */
const StrategyDetailPage: React.FC = () => {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(true);
  const [strategy, setStrategy] = useState<Strategy | null>(null);
  const [performance, setPerformance] = useState<StrategyPerformance | null>(null);
  const [executions, setExecutions] = useState<StrategyExecution[]>([]);
  const [activeTab, setActiveTab] = useState('overview');

  /**
   * Load strategy data
   * 加载策略数据
   */
  const loadStrategy = async () => {
    if (!id) {
      message.error('策略 ID 无效');
      navigate('/strategies');
      return;
    }

    setLoading(true);
    try {
      const strategyData = await getStrategyById(id);
      setStrategy(strategyData);

      // Load performance if strategy has been run
      if (strategyData.totalTrades && strategyData.totalTrades > 0) {
        try {
          const performanceData = await getStrategyPerformance(id);
          setPerformance(performanceData);
        } catch (error) {
          console.error('Failed to load performance:', error);
        }
      }

      // Load executions
      try {
        const executionsData = await getStrategyExecutions(id);
        setExecutions(executionsData);
      } catch (error) {
        console.error('Failed to load executions:', error);
      }
    } catch (error) {
      console.error('Failed to load strategy:', error);
      message.error('加载策略失败');
      navigate('/strategies');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadStrategy();
  }, [id]);

  /**
   * Handle start strategy
   * 处理启动策略
   */
  const handleStart = async () => {
    if (!strategy) return;
    try {
      await startStrategy(strategy.id);
      message.success(`策略 "${strategy.name}" 已启动`);
      loadStrategy();
    } catch (error) {
      console.error('Failed to start strategy:', error);
      message.error(`启动策略失败: ${error}`);
    }
  };

  /**
   * Handle stop strategy
   * 处理停止策略
   */
  const handleStop = async () => {
    if (!strategy) return;
    try {
      await stopStrategy(strategy.id);
      message.success(`策略 "${strategy.name}" 已停止`);
      loadStrategy();
    } catch (error) {
      console.error('Failed to stop strategy:', error);
      message.error(`停止策略失败: ${error}`);
    }
  };

  /**
   * Handle clone strategy
   * 处理克隆策略
   */
  const handleClone = async () => {
    if (!strategy) return;
    try {
      const cloneName = `${strategy.name} - 副本`;
      const cloned = await cloneStrategy(strategy.id, { name: cloneName });
      message.success(`策略已克隆为 "${cloneName}"`);
      navigate(`/strategies/${cloned.id}`);
    } catch (error) {
      console.error('Failed to clone strategy:', error);
      message.error(`克隆策略失败: ${error}`);
    }
  };

  /**
   * Handle delete strategy
   * 处理删除策略
   */
  const handleDelete = () => {
    if (!strategy) return;

    if (strategy.status === 'Running') {
      message.warning('请先停止策略再删除');
      return;
    }

    confirm({
      title: '确认删除',
      icon: <ExclamationCircleOutlined />,
      content: `确认删除策略 "${strategy.name}"？此操作不可恢复。`,
      okText: '确认删除',
      okType: 'danger',
      cancelText: '取消',
      onOk: async () => {
        try {
          await deleteStrategy(strategy.id);
          message.success(`策略 "${strategy.name}" 已删除`);
          navigate('/strategies');
        } catch (error) {
          console.error('Failed to delete strategy:', error);
          message.error(`删除策略失败: ${error}`);
        }
      },
    });
  };

  /**
   * Handle export strategy
   * 处理导出策略
   */
  const handleExport = async () => {
    if (!strategy) return;
    try {
      const result = await exportStrategy(strategy.id);
      
      // Create download link
      const blob = new Blob([atob(result.fileContent)], { type: 'application/zip' });
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = result.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      URL.revokeObjectURL(url);
      
      message.success('策略已导出');
    } catch (error) {
      console.error('Failed to export strategy:', error);
      message.error(`导出策略失败: ${error}`);
    }
  };

  /**
   * Get status badge
   * 获取状态徽章
   */
  const getStatusBadge = (status: StrategyStatus) => {
    switch (status) {
      case 'Running':
        return <Badge status="processing" text="运行中" />;
      case 'Stopped':
        return <Badge status="default" text="已停止" />;
      case 'Paused':
        return <Badge status="warning" text="已暂停" />;
      case 'Error':
        return <Badge status="error" text="错误" />;
      default:
        return <Badge status="default" text={status} />;
    }
  };

  /**
   * Format percentage
   * 格式化百分比
   */
  const formatPercent = (value?: number): string => {
    if (value === undefined || value === null) return '-';
    const sign = value >= 0 ? '+' : '';
    return `${sign}${(value * 100).toFixed(2)}%`;
  };

  /**
   * Format number
   * 格式化数字
   */
  const formatNumber = (value?: number, decimals: number = 2): string => {
    if (value === undefined || value === null) return '-';
    return value.toFixed(decimals);
  };

  /**
   * Executions table columns
   * 执行历史表格列
   */
  const executionColumns: ColumnsType<StrategyExecution> = [
    {
      title: '执行 ID',
      dataIndex: 'id',
      key: 'id',
      width: 120,
      render: (id: string) => <Text code>{id.substring(0, 8)}</Text>,
    },
    {
      title: '开始时间',
      dataIndex: 'startTime',
      key: 'startTime',
      width: 180,
      render: (dateString: string) => new Date(dateString).toLocaleString('zh-CN'),
    },
    {
      title: '结束时间',
      dataIndex: 'endTime',
      key: 'endTime',
      width: 180,
      render: (dateString?: string) => 
        dateString ? new Date(dateString).toLocaleString('zh-CN') : '运行中',
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: string) => {
        const colorMap: Record<string, string> = {
          Running: 'processing',
          Completed: 'success',
          Failed: 'error',
          Stopped: 'default',
        };
        return <Tag color={colorMap[status] || 'default'}>{status}</Tag>;
      },
    },
    {
      title: '收益率',
      dataIndex: 'returnRate',
      key: 'returnRate',
      width: 120,
      render: (value?: number) => {
        if (value === undefined || value === null) return '-';
        const color = value >= 0 ? '#3f8600' : '#cf1322';
        return <Text style={{ color, fontWeight: 'bold' }}>{formatPercent(value)}</Text>;
      },
    },
    {
      title: '交易次数',
      dataIndex: 'tradeCount',
      key: 'tradeCount',
      width: 100,
      render: (value?: number) => value || 0,
    },
    {
      title: '错误信息',
      dataIndex: 'errorMessage',
      key: 'errorMessage',
      ellipsis: true,
      render: (message?: string) => message || '-',
    },
  ];

  if (loading) {
    return (
      <div style={{ 
        display: 'flex', 
        justifyContent: 'center', 
        alignItems: 'center', 
        height: '100vh' 
      }}>
        <Spin size="large" tip="加载中..." />
      </div>
    );
  }

  if (!strategy) {
    return null;
  }

  // Parse parameters for display
  let parametersDisplay = '';
  if (strategy.parameters) {
    try {
      parametersDisplay = JSON.stringify(JSON.parse(strategy.parameters), null, 2);
    } catch (error) {
      parametersDisplay = strategy.parameters;
    }
  }

  return (
    <div className="strategy-detail-page">
      {/* Header */}
      <div className="page-header">
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/strategies')}>
            返回列表
          </Button>
          <Title level={2} style={{ margin: 0 }}>
            {strategy.name}
          </Title>
          {getStatusBadge(strategy.status)}
        </Space>
        <Space>
          {strategy.status === 'Stopped' || strategy.status === 'Error' ? (
            <Tooltip title="启动策略">
              <Button
                type="primary"
                icon={<PlayCircleOutlined />}
                onClick={handleStart}
              >
                启动
              </Button>
            </Tooltip>
          ) : (
            <Tooltip title="停止策略">
              <Button
                danger
                icon={<PauseCircleOutlined />}
                onClick={handleStop}
              >
                停止
              </Button>
            </Tooltip>
          )}
          <Button
            icon={<EditOutlined />}
            disabled={strategy.status === 'Running'}
            onClick={() => navigate(`/strategies/${strategy.id}/edit`)}
          >
            编辑
          </Button>
          <Button icon={<CopyOutlined />} onClick={handleClone}>
            克隆
          </Button>
          <Button icon={<ExportOutlined />} onClick={handleExport}>
            导出
          </Button>
          <Button
            danger
            icon={<DeleteOutlined />}
            disabled={strategy.status === 'Running'}
            onClick={handleDelete}
          >
            删除
          </Button>
          <Button icon={<ReloadOutlined />} onClick={loadStrategy}>
            刷新
          </Button>
        </Space>
      </div>

      {/* Performance Summary */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic
              title="累计收益率"
              value={strategy.cumulativeReturn !== undefined ? 
                (strategy.cumulativeReturn * 100).toFixed(2) : '-'}
              suffix="%"
              valueStyle={{
                color: (strategy.cumulativeReturn || 0) >= 0 ? '#3f8600' : '#cf1322',
              }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="夏普比率"
              value={formatNumber(strategy.sharpeRatio)}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="最大回撤"
              value={strategy.maxDrawdown !== undefined ? 
                (strategy.maxDrawdown * 100).toFixed(2) : '-'}
              suffix="%"
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="胜率"
              value={strategy.winRate !== undefined ? 
                (strategy.winRate * 100).toFixed(2) : '-'}
              suffix="%"
            />
          </Card>
        </Col>
      </Row>

      {/* Tabs */}
      <Card>
        <Tabs activeKey={activeTab} onChange={setActiveTab}>
          {/* Overview Tab */}
          <TabPane tab="概览" key="overview">
            <Descriptions bordered column={2}>
              <Descriptions.Item label="策略 ID">
                <Text code>{strategy.id}</Text>
              </Descriptions.Item>
              <Descriptions.Item label="策略类型">
                {strategy.strategyType || '自定义'}
              </Descriptions.Item>
              <Descriptions.Item label="状态">
                {getStatusBadge(strategy.status)}
              </Descriptions.Item>
              <Descriptions.Item label="版本">
                v{strategy.version || 1}
              </Descriptions.Item>
              <Descriptions.Item label="创建时间">
                {new Date(strategy.createdAt).toLocaleString('zh-CN')}
              </Descriptions.Item>
              <Descriptions.Item label="最后更新">
                {new Date(strategy.updatedAt).toLocaleString('zh-CN')}
              </Descriptions.Item>
              <Descriptions.Item label="最后运行">
                {strategy.lastRunAt
                  ? new Date(strategy.lastRunAt).toLocaleString('zh-CN')
                  : '从未运行'}
              </Descriptions.Item>
              <Descriptions.Item label="总交易次数">
                {strategy.totalTrades || 0}
              </Descriptions.Item>
              <Descriptions.Item label="描述" span={2}>
                {strategy.description || '暂无描述'}
              </Descriptions.Item>
              {strategy.tags && strategy.tags.length > 0 && (
                <Descriptions.Item label="标签" span={2}>
                  <Space>
                    {strategy.tags.map((tag) => (
                      <Tag key={tag} color="blue">
                        {tag}
                      </Tag>
                    ))}
                  </Space>
                </Descriptions.Item>
              )}
              {strategy.codeFilePath && (
                <Descriptions.Item label="代码文件" span={2}>
                  <Text code>{strategy.codeFilePath}</Text>
                </Descriptions.Item>
              )}
            </Descriptions>
          </TabPane>

          {/* Parameters Tab */}
          <TabPane tab="参数配置" key="parameters">
            <Card title="策略参数">
              <pre style={{ 
                background: '#f5f5f5', 
                padding: 16, 
                borderRadius: 4,
                fontFamily: 'monospace',
                fontSize: 13,
                lineHeight: 1.6,
              }}>
                {parametersDisplay || '{}'}
              </pre>
            </Card>
          </TabPane>

          {/* Performance Tab */}
          <TabPane tab="性能指标" key="performance">
            {performance ? (
              <Descriptions bordered column={2}>
                <Descriptions.Item label="累计收益率">
                  <Text style={{ 
                    color: performance.cumulativeReturn >= 0 ? '#3f8600' : '#cf1322',
                    fontWeight: 'bold',
                  }}>
                    {formatPercent(performance.cumulativeReturn)}
                  </Text>
                </Descriptions.Item>
                <Descriptions.Item label="年化收益率">
                  {formatPercent(performance.annualizedReturn)}
                </Descriptions.Item>
                <Descriptions.Item label="夏普比率">
                  {formatNumber(performance.sharpeRatio)}
                </Descriptions.Item>
                <Descriptions.Item label="索提诺比率">
                  {formatNumber(performance.sortinoRatio)}
                </Descriptions.Item>
                <Descriptions.Item label="最大回撤">
                  <Text style={{ color: '#cf1322' }}>
                    {formatPercent(performance.maxDrawdown)}
                  </Text>
                </Descriptions.Item>
                <Descriptions.Item label="胜率">
                  {formatPercent(performance.winRate)}
                </Descriptions.Item>
                <Descriptions.Item label="总交易次数">
                  {performance.totalTrades}
                </Descriptions.Item>
                <Descriptions.Item label="盈利交易">
                  <Text style={{ color: '#3f8600' }}>
                    {performance.profitTrades}
                  </Text>
                </Descriptions.Item>
                <Descriptions.Item label="亏损交易">
                  <Text style={{ color: '#cf1322' }}>
                    {performance.lossTrades}
                  </Text>
                </Descriptions.Item>
                <Descriptions.Item label="平均盈利">
                  {performance.averageWin ? `$${formatNumber(performance.averageWin)}` : '-'}
                </Descriptions.Item>
                <Descriptions.Item label="平均亏损">
                  {performance.averageLoss ? `$${formatNumber(performance.averageLoss)}` : '-'}
                </Descriptions.Item>
              </Descriptions>
            ) : (
              <div style={{ textAlign: 'center', padding: '60px 0' }}>
                <Text type="secondary">暂无性能数据，策略尚未运行或执行</Text>
              </div>
            )}
          </TabPane>

          {/* Executions Tab */}
          <TabPane tab="执行历史" key="executions">
            {executions.length > 0 ? (
              <Table
                columns={executionColumns}
                dataSource={executions}
                rowKey="id"
                pagination={{ pageSize: 10 }}
              />
            ) : (
              <div style={{ textAlign: 'center', padding: '60px 0' }}>
                <Text type="secondary">暂无执行历史</Text>
              </div>
            )}
          </TabPane>

          {/* Logs Tab */}
          <TabPane tab="日志" key="logs">
            <div style={{ textAlign: 'center', padding: '60px 0' }}>
              <Text type="secondary">日志功能将在任务 20.9 中实现</Text>
            </div>
          </TabPane>

          {/* Versions Tab */}
          <TabPane tab="版本历史" key="versions">
            <div style={{ textAlign: 'center', padding: '60px 0' }}>
              <Text type="secondary">版本历史功能将在任务 20.11 中实现</Text>
              <br />
              <Button 
                type="link" 
                icon={<HistoryOutlined />}
                onClick={() => message.info('版本历史功能开发中')}
              >
                查看版本历史
              </Button>
            </div>
          </TabPane>
        </Tabs>
      </Card>
    </div>
  );
};

export default StrategyDetailPage;
