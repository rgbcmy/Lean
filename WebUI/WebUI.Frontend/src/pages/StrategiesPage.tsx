/**
 * Strategies Page
 * 策略列表页面
 */

import React, { useState, useEffect } from 'react';
import {
  Table,
  Card,
  Space,
  Button,
  Input,
  Select,
  Statistic,
  Row,
  Col,
  Modal,
  message,
  Tooltip,
  Typography,
  Tag,
  Badge,
} from 'antd';
import {
  ReloadOutlined,
  PlusOutlined,
  PlayCircleOutlined,
  PauseCircleOutlined,
  DeleteOutlined,
  EditOutlined,
  CopyOutlined,
  ExportOutlined,
  ImportOutlined,
  SearchOutlined,
  FilterOutlined,
  ExclamationCircleOutlined,
  EyeOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from 'react-router-dom';
import {
  getStrategies,
  deleteStrategy,
  startStrategy,
  stopStrategy,
  cloneStrategy,
  archiveStrategy,
} from '../api/strategiesApi';
import type { Strategy, StrategyStatus } from '../types/strategy';
import './StrategiesPage.css';

const { Text, Title } = Typography;
const { Option } = Select;
const { confirm } = Modal;

/**
 * StrategiesPage Component
 * Displays list of strategies with management capabilities
 * 显示策略列表，支持策略管理功能
 */
const StrategiesPage: React.FC = () => {
  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  // Filters
  const [nameFilter, setNameFilter] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<StrategyStatus | 'all'>('all');
  const [tagFilter, setTagFilter] = useState<string>('');



  /**
   * Load strategies from API
   * 从 API 加载策略列表
   */
  const loadStrategies = async () => {
    setLoading(true);
    try {
      const params: any = {};

      if (statusFilter !== 'all') {
        params.status = statusFilter;
      }

      if (tagFilter) {
        params.tags = [tagFilter];
      }

      const result = await getStrategies(params);
      setStrategies(result);
    } catch (error) {
      console.error('Failed to load strategies:', error);
      message.error('加载策略失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Initial load
   * 初始加载
   */
  useEffect(() => {
    loadStrategies();
  }, [statusFilter, tagFilter]);

  /**
   * Get filtered strategies
   * 获取过滤后的策略列表
   */
  const filteredStrategies = strategies.filter((strategy) => {
    if (nameFilter && !strategy.name.toLowerCase().includes(nameFilter.toLowerCase())) {
      return false;
    }
    return true;
  });

  /**
   * Handle start strategy
   * 处理启动策略
   */
  const handleStartStrategy = async (strategyId: string, strategyName: string) => {
    try {
      await startStrategy(strategyId);
      message.success(`策略 "${strategyName}" 已启动`);
      loadStrategies();
    } catch (error) {
      console.error('Failed to start strategy:', error);
      message.error(`启动策略失败: ${error}`);
    }
  };

  /**
   * Handle stop strategy
   * 处理停止策略
   */
  const handleStopStrategy = async (strategyId: string, strategyName: string) => {
    try {
      await stopStrategy(strategyId);
      message.success(`策略 "${strategyName}" 已停止`);
      loadStrategies();
    } catch (error) {
      console.error('Failed to stop strategy:', error);
      message.error(`停止策略失败: ${error}`);
    }
  };

  /**
   * Handle clone strategy
   * 处理克隆策略
   */
  const handleCloneStrategy = async (strategyId: string, strategyName: string) => {
    try {
      const cloneName = `${strategyName} - 副本`;
      await cloneStrategy(strategyId, { name: cloneName });
      message.success(`策略已克隆为 "${cloneName}"`);
      loadStrategies();
    } catch (error) {
      console.error('Failed to clone strategy:', error);
      message.error(`克隆策略失败: ${error}`);
    }
  };

  /**
   * Handle delete strategy
   * 处理删除策略
   */
  const handleDeleteStrategy = (strategyId: string, strategyName: string, status: StrategyStatus) => {
    if (status === 'Running') {
      message.warning('请先停止策略再删除');
      return;
    }

    confirm({
      title: '确认删除',
      icon: <ExclamationCircleOutlined />,
      content: `确认删除策略 "${strategyName}"？此操作不可恢复。`,
      okText: '确认删除',
      okType: 'danger',
      cancelText: '取消',
      onOk: async () => {
        try {
          await deleteStrategy(strategyId);
          message.success(`策略 "${strategyName}" 已删除`);
          loadStrategies();
        } catch (error) {
          console.error('Failed to delete strategy:', error);
          message.error(`删除策略失败: ${error}`);
        }
      },
    });
  };

  /**
   * Handle archive strategy
   * 处理归档策略
   */
  const handleArchiveStrategy = async (strategyId: string, strategyName: string) => {
    try {
      await archiveStrategy(strategyId);
      message.success(`策略 "${strategyName}" 已归档`);
      loadStrategies();
    } catch (error) {
      console.error('Failed to archive strategy:', error);
      message.error(`归档策略失败: ${error}`);
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
   * Format datetime
   * 格式化日期时间
   */
  const formatDateTime = (dateString?: string): string => {
    if (!dateString) return '-';
    const date = new Date(dateString);
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  /**
   * Table columns definition
   * 表格列定义
   */
  const columns: ColumnsType<Strategy> = [
    {
      title: '策略名称',
      dataIndex: 'name',
      key: 'name',
      fixed: 'left',
      width: 200,
      render: (name: string, record: Strategy) => (
        <Space direction="vertical" size="small">
          <Text strong>{name}</Text>
          {record.description && (
            <Text type="secondary" style={{ fontSize: '12px' }}>
              {record.description}
            </Text>
          )}
          {record.tags && record.tags.length > 0 && (
            <Space size="small">
              {record.tags.map((tag) => (
                <Tag key={tag} color="blue" style={{ fontSize: '11px' }}>
                  {tag}
                </Tag>
              ))}
            </Space>
          )}
        </Space>
      ),
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: StrategyStatus) => getStatusBadge(status),
    },
    {
      title: '类型',
      dataIndex: 'strategyType',
      key: 'strategyType',
      width: 120,
      render: (type?: string) => type || '自定义',
    },
    {
      title: '累计收益',
      dataIndex: 'cumulativeReturn',
      key: 'cumulativeReturn',
      width: 120,
      sorter: (a, b) => (a.cumulativeReturn || 0) - (b.cumulativeReturn || 0),
      render: (value?: number) => {
        if (value === undefined || value === null) return '-';
        const color = value >= 0 ? '#3f8600' : '#cf1322';
        return <Text style={{ color, fontWeight: 'bold' }}>{formatPercent(value)}</Text>;
      },
    },
    {
      title: '夏普比率',
      dataIndex: 'sharpeRatio',
      key: 'sharpeRatio',
      width: 100,
      sorter: (a, b) => (a.sharpeRatio || 0) - (b.sharpeRatio || 0),
      render: (value?: number) => (value !== undefined ? value.toFixed(2) : '-'),
    },
    {
      title: '最大回撤',
      dataIndex: 'maxDrawdown',
      key: 'maxDrawdown',
      width: 100,
      sorter: (a, b) => (a.maxDrawdown || 0) - (b.maxDrawdown || 0),
      render: (value?: number) => (value !== undefined ? formatPercent(value) : '-'),
    },
    {
      title: '胜率',
      dataIndex: 'winRate',
      key: 'winRate',
      width: 100,
      sorter: (a, b) => (a.winRate || 0) - (b.winRate || 0),
      render: (value?: number) => (value !== undefined ? formatPercent(value) : '-'),
    },
    {
      title: '交易次数',
      dataIndex: 'totalTrades',
      key: 'totalTrades',
      width: 100,
      sorter: (a, b) => (a.totalTrades || 0) - (b.totalTrades || 0),
      render: (value?: number) => value || 0,
    },
    {
      title: '最后运行',
      dataIndex: 'lastRunAt',
      key: 'lastRunAt',
      width: 150,
      render: (dateString?: string) => formatDateTime(dateString),
    },
    {
      title: '操作',
      key: 'actions',
      fixed: 'right',
      width: 280,
      render: (_: any, record: Strategy) => (
        <Space size="small">
          {record.status === 'Stopped' || record.status === 'Error' ? (
            <Tooltip title="启动策略">
              <Button
                type="primary"
                size="small"
                icon={<PlayCircleOutlined />}
                onClick={() => handleStartStrategy(record.id, record.name)}
              >
                启动
              </Button>
            </Tooltip>
          ) : (
            <Tooltip title="停止策略">
              <Button
                danger
                size="small"
                icon={<PauseCircleOutlined />}
                onClick={() => handleStopStrategy(record.id, record.name)}
              >
                停止
              </Button>
            </Tooltip>
          )}
          <Tooltip title="查看详情">
            <Button
              size="small"
              icon={<EyeOutlined />}
              onClick={() => navigate(`/strategies/${record.id}`)}
            >
              详情
            </Button>
          </Tooltip>
          <Tooltip title="编辑">
            <Button
              size="small"
              icon={<EditOutlined />}
              disabled={record.status === 'Running'}
              onClick={() => navigate(`/strategies/${record.id}/edit`)}
            />
          </Tooltip>
          <Tooltip title="克隆">
            <Button
              size="small"
              icon={<CopyOutlined />}
              onClick={() => handleCloneStrategy(record.id, record.name)}
            />
          </Tooltip>
          <Tooltip title="删除">
            <Button
              size="small"
              danger
              icon={<DeleteOutlined />}
              disabled={record.status === 'Running'}
              onClick={() => handleDeleteStrategy(record.id, record.name, record.status)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  /**
   * Calculate statistics
   * 计算统计数据
   */
  const stats = {
    total: strategies.length,
    running: strategies.filter((s) => s.status === 'Running').length,
    stopped: strategies.filter((s) => s.status === 'Stopped').length,
    error: strategies.filter((s) => s.status === 'Error').length,
  };

  return (
    <div className="strategies-page">
      {/* Header */}
      <div className="page-header">
        <Title level={2}>策略管理</Title>
        <Space>
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => navigate('/strategies/create')}
          >
            创建策略
          </Button>
          <Button icon={<ImportOutlined />} onClick={() => message.info('导入功能开发中')}>
            导入策略
          </Button>
          <Button icon={<ReloadOutlined />} onClick={loadStrategies} loading={loading}>
            刷新
          </Button>
        </Space>
      </div>

      {/* Statistics */}
      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={6}>
          <Card>
            <Statistic title="策略总数" value={stats.total} />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="运行中"
              value={stats.running}
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="已停止"
              value={stats.stopped}
              valueStyle={{ color: '#999' }}
            />
          </Card>
        </Col>
        <Col span={6}>
          <Card>
            <Statistic
              title="错误"
              value={stats.error}
              valueStyle={{ color: '#cf1322' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Space size="middle" wrap>
          <Input
            placeholder="搜索策略名称"
            prefix={<SearchOutlined />}
            value={nameFilter}
            onChange={(e) => setNameFilter(e.target.value)}
            style={{ width: 200 }}
            allowClear
          />
          <Select
            value={statusFilter}
            onChange={setStatusFilter}
            style={{ width: 150 }}
            placeholder="筛选状态"
          >
            <Option value="all">全部状态</Option>
            <Option value="Running">运行中</Option>
            <Option value="Stopped">已停止</Option>
            <Option value="Paused">已暂停</Option>
            <Option value="Error">错误</Option>
          </Select>
          <Input
            placeholder="筛选标签"
            prefix={<FilterOutlined />}
            value={tagFilter}
            onChange={(e) => setTagFilter(e.target.value)}
            style={{ width: 150 }}
            allowClear
          />
        </Space>
      </Card>

      {/* Strategies List */}
      <Card
        title={`策略列表 (${filteredStrategies.length})`}
        extra={
          <Text type="secondary">
            显示 {filteredStrategies.length} / {strategies.length} 个策略
          </Text>
        }
      >
        {filteredStrategies.length === 0 ? (
          <div style={{ textAlign: 'center', padding: '60px 0' }}>
            <Text type="secondary" style={{ fontSize: 16 }}>
              {strategies.length === 0
                ? '暂无策略，点击"创建策略"开始'
                : '没有符合筛选条件的策略'}
            </Text>
          </div>
        ) : (
          <Table
            columns={columns}
            dataSource={filteredStrategies}
            rowKey="id"
            loading={loading}
            pagination={{
              pageSize: 10,
              showSizeChanger: true,
              showTotal: (total) => `共 ${total} 个策略`,
            }}
            scroll={{ x: 1500 }}
          />
        )}
      </Card>
    </div>
  );
};

export default StrategiesPage;
