/**
 * Backtests Page
 * 回测列表页面
 */

import React, { useState, useEffect } from 'react';
import {
  Table,
  Card,
  Space,
  Button,
  Input,
  Select,
  Modal,
  message,
  Typography,
  Tag,
  Statistic,
  Row,
  Col,
} from 'antd';
import {
  ReloadOutlined,
  PlusOutlined,
  DeleteOutlined,
  PlayCircleOutlined,
  EyeOutlined,
  ExperimentOutlined,
  SearchOutlined,
  ExclamationCircleOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from 'react-router-dom';
import {
  getBacktests,
  deleteBacktest,
  startBacktest,
} from '../api/backtestsApi';
import type { BacktestSummary, BacktestStatus } from '../types/backtest';
import './BacktestsPage.css';

const { Text, Title } = Typography;
const { Option } = Select;
const { confirm } = Modal;

/**
 * BacktestsPage Component
 * Displays list of backtests with management capabilities
 * 显示回测列表，支持回测管理功能
 */
const BacktestsPage: React.FC = () => {
  const [backtests, setBacktests] = useState<BacktestSummary[]>([]);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  // Filters
  const [nameFilter, setNameFilter] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<BacktestStatus | 'all'>('all');

  // Statistics
  const [statistics, setStatistics] = useState({
    total: 0,
    completed: 0,
    running: 0,
    failed: 0,
  });

  /**
   * Load backtests from API
   * 从 API 加载回测列表
   */
  const loadBacktests = async () => {
    setLoading(true);
    try {
      const params: any = {};

      if (statusFilter !== 'all') {
        params.status = statusFilter;
      }

      const result = await getBacktests(params);
      setBacktests(result.backtests);

      // Calculate statistics
      const stats = {
        total: result.backtests.length,
        completed: result.backtests.filter((b) => b.status === 'Completed').length,
        running: result.backtests.filter((b) => b.status === 'Running').length,
        failed: result.backtests.filter((b) => b.status === 'Failed').length,
      };
      setStatistics(stats);
    } catch (error) {
      console.error('Failed to load backtests:', error);
      message.error('加载回测列表失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Initial load
   * 初始加载
   */
  useEffect(() => {
    loadBacktests();
  }, [statusFilter]);

  /**
   * Get filtered backtests
   * 获取过滤后的回测列表
   */
  const filteredBacktests = backtests.filter((backtest) => {
    if (nameFilter && !backtest.name.toLowerCase().includes(nameFilter.toLowerCase())) {
      return false;
    }
    return true;
  });

  /**
   * Handle view backtest
   * 处理查看回测
   */
  const handleViewBacktest = (backtestId: number) => {
    navigate(`/backtests/${backtestId}`);
  };

  /**
   * Handle start backtest
   * 处理启动回测
   */
  const handleStartBacktest = async (backtestId: number, backtestName: string) => {
    try {
      await startBacktest(backtestId);
      message.success(`回测 "${backtestName}" 已启动`);
      loadBacktests();
    } catch (error) {
      console.error('Failed to start backtest:', error);
      message.error(`启动回测失败`);
    }
  };

  /**
   * Handle delete backtest
   * 处理删除回测
   */
  const handleDeleteBacktest = (backtestId: number, backtestName: string) => {
    confirm({
      title: '确认删除',
      icon: <ExclamationCircleOutlined />,
      content: `确定要删除回测 "${backtestName}" 吗？此操作不可恢复。`,
      okText: '删除',
      okType: 'danger',
      cancelText: '取消',
      onOk: async () => {
        try {
          await deleteBacktest(backtestId);
          message.success(`回测 "${backtestName}" 已删除`);
          loadBacktests();
        } catch (error) {
          console.error('Failed to delete backtest:', error);
          message.error(`删除回测失败`);
        }
      },
    });
  };

  /**
   * Get status tag
   * 获取状态标签
   */
  const getStatusTag = (status: BacktestStatus) => {
    const statusMap: Record<BacktestStatus, { color: string; text: string }> = {
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
   * Table columns
   * 表格列定义
   */
  const columns: ColumnsType<BacktestSummary> = [
    {
      title: '回测名称',
      dataIndex: 'name',
      key: 'name',
      fixed: 'left',
      width: 250,
      render: (name: string, record: BacktestSummary) => (
        <Button
          type="link"
          onClick={() => handleViewBacktest(record.id)}
          style={{ padding: 0 }}
        >
          {name}
        </Button>
      ),
    },
    {
      title: '策略',
      dataIndex: 'strategyName',
      key: 'strategyName',
      width: 200,
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 100,
      render: (status: BacktestStatus) => getStatusTag(status),
    },
    {
      title: '回测期间',
      key: 'dateRange',
      width: 200,
      render: (_, record: BacktestSummary) => (
        <Text type="secondary">
          {record.startDate} ~ {record.endDate}
        </Text>
      ),
    },
    {
      title: '初始资金',
      dataIndex: 'initialCapital',
      key: 'initialCapital',
      width: 120,
      align: 'right',
      render: (value: number) => `$${value.toLocaleString()}`,
    },
    {
      title: '总收益率',
      dataIndex: 'totalReturn',
      key: 'totalReturn',
      width: 120,
      align: 'right',
      render: (value: number | undefined) =>
        value !== undefined ? (
          <Text style={{ color: value >= 0 ? '#3f8600' : '#cf1322' }}>
            {value.toFixed(2)}%
          </Text>
        ) : (
          '-'
        ),
    },
    {
      title: '夏普比率',
      dataIndex: 'sharpeRatio',
      key: 'sharpeRatio',
      width: 100,
      align: 'right',
      render: (value: number | undefined) =>
        value !== undefined ? value.toFixed(2) : '-',
    },
    {
      title: '最大回撤',
      dataIndex: 'maxDrawdown',
      key: 'maxDrawdown',
      width: 100,
      align: 'right',
      render: (value: number | undefined) =>
        value !== undefined ? (
          <Text style={{ color: '#cf1322' }}>{value.toFixed(2)}%</Text>
        ) : (
          '-'
        ),
    },
    {
      title: '创建时间',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 180,
      render: (value: string) => new Date(value).toLocaleString('zh-CN'),
    },
    {
      title: '操作',
      key: 'actions',
      fixed: 'right',
      width: 150,
      render: (_: any, record: BacktestSummary) => (
        <Space>
          <Button
            type="link"
            size="small"
            icon={<EyeOutlined />}
            onClick={() => handleViewBacktest(record.id)}
          >
            查看
          </Button>
          {record.status === 'Pending' && (
            <Button
              type="link"
              size="small"
              icon={<PlayCircleOutlined />}
              onClick={() => handleStartBacktest(record.id, record.name)}
            >
              启动
            </Button>
          )}
          <Button
            type="link"
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDeleteBacktest(record.id, record.name)}
          >
            删除
          </Button>
        </Space>
      ),
    },
  ];

  return (
    <div className="backtests-page">
      {/* Header */}
      <Card className="page-header-card">
        <Row justify="space-between" align="middle">
          <Col>
            <Title level={2} style={{ margin: 0 }}>
              回测管理
            </Title>
          </Col>
          <Col>
            <Space>
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={() => navigate('/backtests/new')}
              >
                新建回测
              </Button>
              <Button
                icon={<ExperimentOutlined />}
                onClick={() => navigate('/backtests/compare')}
              >
                对比回测
              </Button>
              <Button
                icon={<ExperimentOutlined />}
                onClick={() => navigate('/backtests/optimize')}
              >
                参数优化
              </Button>
              <Button icon={<ReloadOutlined />} onClick={loadBacktests}>
                刷新
              </Button>
            </Space>
          </Col>
        </Row>
      </Card>

      {/* Statistics */}
      <Card style={{ marginBottom: 16 }}>
        <Row gutter={16}>
          <Col xs={24} sm={12} md={6}>
            <Statistic title="总回测数" value={statistics.total} />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="已完成"
              value={statistics.completed}
              valueStyle={{ color: '#3f8600' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="运行中"
              value={statistics.running}
              valueStyle={{ color: '#1890ff' }}
            />
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Statistic
              title="失败"
              value={statistics.failed}
              valueStyle={{ color: '#cf1322' }}
            />
          </Col>
        </Row>
      </Card>

      {/* Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Space wrap>
          <Input
            placeholder="搜索回测名称"
            prefix={<SearchOutlined />}
            value={nameFilter}
            onChange={(e) => setNameFilter(e.target.value)}
            style={{ width: 200 }}
            allowClear
          />
          <Select
            placeholder="状态"
            value={statusFilter}
            onChange={setStatusFilter}
            style={{ width: 120 }}
          >
            <Option value="all">全部状态</Option>
            <Option value="Pending">待执行</Option>
            <Option value="Running">运行中</Option>
            <Option value="Completed">已完成</Option>
            <Option value="Failed">失败</Option>
            <Option value="Cancelled">已取消</Option>
          </Select>
        </Space>
      </Card>

      {/* Backtests Table */}
      <Card>
        <Table
          columns={columns}
          dataSource={filteredBacktests}
          rowKey="id"
          loading={loading}
          pagination={{
            pageSize: 10,
            showSizeChanger: true,
            showTotal: (total) => `共 ${total} 条记录`,
          }}
          scroll={{ x: 1400 }}
        />
      </Card>
    </div>
  );
};

export default BacktestsPage;
