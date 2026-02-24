/**
 * Recurring Investment Plans Page
 * 定投计划页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Table,
  Modal,
  Form,
  InputNumber,
  Select,
  Space,
  Tag,
  message,
  Typography,
  Statistic,
  Row,
  Col,
} from 'antd';
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  PauseCircleOutlined,
  PlayCircleOutlined,
  DollarOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import StockSearch from '../components/trading/StockSearch';
import './RecurringInvestmentPage.css';

const { Title, Text } = Typography;
const { Option } = Select;

interface RecurringPlan {
  id: string;
  symbol: string;
  companyName: string;
  frequency: 'daily' | 'weekly' | 'monthly';
  amount: number;
  isActive: boolean;
  nextExecutionDate: Date;
  createdAt: Date;
  totalInvested: number;
  executionCount: number;
}

/**
 * RecurringInvestmentPage Component
 * Manage recurring investment plans (DCA - Dollar Cost Averaging)
 * 管理定投计划（定期定额投资）
 */
const RecurringInvestmentPage: React.FC = () => {
  const [plans, setPlans] = useState<RecurringPlan[]>([]);
  const [loading, setLoading] = useState(false);
  const [modalVisible, setModalVisible] = useState(false);
  const [editingPlan, setEditingPlan] = useState<RecurringPlan | null>(null);
  const [form] = Form.useForm();

  /**
   * Load recurring plans
   * 加载定投计划
   */
  const loadPlans = async () => {
    setLoading(true);
    try {
      // TODO: Replace with actual API call
      // const result = await getRecurringPlans();
      // setPlans(result);
      
      // Mock data for demonstration
      const mockPlans: RecurringPlan[] = [
        {
          id: '1',
          symbol: 'SPY',
          companyName: 'SPDR S&P 500 ETF',
          frequency: 'monthly',
          amount: 500,
          isActive: true,
          nextExecutionDate: new Date('2024-02-01'),
          createdAt: new Date('2023-06-01'),
          totalInvested: 4000,
          executionCount: 8,
        },
        {
          id: '2',
          symbol: 'QQQ',
          companyName: 'Invesco QQQ Trust',
          frequency: 'weekly',
          amount: 100,
          isActive: true,
          nextExecutionDate: new Date('2024-01-22'),
          createdAt: new Date('2023-11-01'),
          totalInvested: 1200,
          executionCount: 12,
        },
      ];
      setPlans(mockPlans);
    } catch (error) {
      console.error('Failed to load plans:', error);
      message.error('加载定投计划失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle create/update plan
   * 处理创建/更新计划
   */
  const handleSavePlan = async (_values: any) => {
    try {
      if (editingPlan) {
        // TODO: Update plan API call
        message.success('定投计划已更新');
      } else {
        // TODO: Create plan API call
        message.success('定投计划已创建');
      }
      
      setModalVisible(false);
      form.resetFields();
      setEditingPlan(null);
      loadPlans();
    } catch (error: any) {
      console.error('Failed to save plan:', error);
      message.error(error.response?.data?.message || '保存定投计划失败');
    }
  };

  /**
   * Handle toggle plan status
   * 处理启用/暂停计划
   */
  const handleTogglePlan = async (_planId: string, isActive: boolean) => {
    try {
      // TODO: Toggle plan API call
      message.success(isActive ? '定投计划已启用' : '定投计划已暂停');
      loadPlans();
    } catch (error: any) {
      console.error('Failed to toggle plan:', error);
      message.error(error.response?.data?.message || '操作失败');
    }
  };

  /**
   * Handle delete plan
   * 处理删除计划
   */
  const handleDeletePlan = (_planId: string) => {
    Modal.confirm({
      title: '确认删除',
      content: '您确定要删除此定投计划吗？此操作不可撤销。',
      okText: '确认',
      cancelText: '取消',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          // TODO: Delete plan API call
          message.success('定投计划已删除');
          loadPlans();
        } catch (error: any) {
          console.error('Failed to delete plan:', error);
          message.error(error.response?.data?.message || '删除失败');
        }
      },
    });
  };

  /**
   * Open edit modal
   * 打开编辑弹窗
   */
  const handleEdit = (plan: RecurringPlan) => {
    setEditingPlan(plan);
    form.setFieldsValue({
      symbol: plan.symbol,
      frequency: plan.frequency,
      amount: plan.amount,
    });
    setModalVisible(true);
  };

  /**
   * Get frequency tag
   * 获取频率标签
   */
  const getFrequencyTag = (frequency: string) => {
    const config: Record<string, { color: string; text: string }> = {
      daily: { color: 'blue', text: '每日' },
      weekly: { color: 'green', text: '每周' },
      monthly: { color: 'purple', text: '每月' },
    };
    const item = config[frequency];
    return <Tag color={item.color}>{item.text}</Tag>;
  };

  /**
   * Table columns definition
   * 表格列定义
   */
  const columns: ColumnsType<RecurringPlan> = [
    {
      title: '股票/ETF',
      key: 'stock',
      render: (_, record) => (
        <div>
          <Text strong>{record.symbol}</Text>
          <br />
          <Text type="secondary" style={{ fontSize: 12 }}>
            {record.companyName}
          </Text>
        </div>
      ),
    },
    {
      title: '投资频率',
      dataIndex: 'frequency',
      key: 'frequency',
      render: (frequency: string) => getFrequencyTag(frequency),
    },
    {
      title: '投资金额',
      dataIndex: 'amount',
      key: 'amount',
      render: (amount: number) => `$${amount.toFixed(2)}`,
    },
    {
      title: '累计投资',
      key: 'totalInvested',
      render: (_, record) => (
        <div>
          <div>${record.totalInvested.toFixed(2)}</div>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {record.executionCount} 次
          </Text>
        </div>
      ),
    },
    {
      title: '下次执行',
      dataIndex: 'nextExecutionDate',
      key: 'nextExecutionDate',
      render: (date: Date) => dayjs(date).format('YYYY-MM-DD'),
    },
    {
      title: '状态',
      dataIndex: 'isActive',
      key: 'isActive',
      render: (isActive: boolean) => (
        <Tag color={isActive ? 'green' : 'default'}>
          {isActive ? '运行中' : '已暂停'}
        </Tag>
      ),
    },
    {
      title: '操作',
      key: 'actions',
      render: (_, record) => (
        <Space>
          <Button
            type="link"
            size="small"
            icon={record.isActive ? <PauseCircleOutlined /> : <PlayCircleOutlined />}
            onClick={() => handleTogglePlan(record.id, !record.isActive)}
          >
            {record.isActive ? '暂停' : '启用'}
          </Button>
          <Button
            type="link"
            size="small"
            icon={<EditOutlined />}
            onClick={() => handleEdit(record)}
          >
            编辑
          </Button>
          <Button
            type="link"
            size="small"
            danger
            icon={<DeleteOutlined />}
            onClick={() => handleDeletePlan(record.id)}
          >
            删除
          </Button>
        </Space>
      ),
    },
  ];

  /**
   * Calculate statistics
   * 计算统计数据
   */
  const stats = {
    activePlans: plans.filter((p) => p.isActive).length,
    totalMonthlyInvestment: plans
      .filter((p) => p.isActive)
      .reduce((sum, p) => {
        const multiplier = p.frequency === 'daily' ? 30 : p.frequency === 'weekly' ? 4 : 1;
        return sum + p.amount * multiplier;
      }, 0),
    totalInvested: plans.reduce((sum, p) => sum + p.totalInvested, 0),
  };

  useEffect(() => {
    loadPlans();
  }, []);

  return (
    <div className="recurring-investment-page">
      <Title level={3}>定投计划</Title>
      <Text type="secondary">
        通过定期定额投资（Dollar Cost Averaging），降低市场波动风险，实现长期财富增长。
      </Text>

      {/* Statistics */}
      <Row gutter={[16, 16]} style={{ marginTop: 24 }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="活跃计划"
              value={stats.activePlans}
              suffix="个"
              prefix={<PlayCircleOutlined />}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="预计月投资额"
              value={stats.totalMonthlyInvestment}
              precision={2}
              prefix="$"
              valueStyle={{ color: '#3f8600' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="累计投资总额"
              value={stats.totalInvested}
              precision={2}
              prefix={<DollarOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
      </Row>

      {/* Plans Table */}
      <Card
        title="我的定投计划"
        style={{ marginTop: 24 }}
        extra={
          <Button
            type="primary"
            icon={<PlusOutlined />}
            onClick={() => {
              setEditingPlan(null);
              form.resetFields();
              setModalVisible(true);
            }}
          >
            新建计划
          </Button>
        }
      >
        <Table
          columns={columns}
          dataSource={plans}
          rowKey="id"
          loading={loading}
          pagination={{
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total) => `共 ${total} 个计划`,
          }}
        />
      </Card>

      {/* Create/Edit Modal */}
      <Modal
        title={editingPlan ? '编辑定投计划' : '新建定投计划'}
        open={modalVisible}
        onCancel={() => {
          setModalVisible(false);
          form.resetFields();
          setEditingPlan(null);
        }}
        onOk={() => form.submit()}
        width={600}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSavePlan}
          initialValues={{
            frequency: 'monthly',
            amount: 100,
          }}
        >
          <Form.Item
            name="symbol"
            label="股票/ETF 代码"
            rules={[{ required: true, message: '请选择股票或 ETF' }]}
          >
            <StockSearch
              onSelect={(symbol) => form.setFieldValue('symbol', symbol)}
              placeholder="搜索股票或 ETF"
            />
          </Form.Item>

          <Form.Item
            name="frequency"
            label="投资频率"
            rules={[{ required: true, message: '请选择投资频率' }]}
          >
            <Select>
              <Option value="daily">每日</Option>
              <Option value="weekly">每周</Option>
              <Option value="monthly">每月</Option>
            </Select>
          </Form.Item>

          <Form.Item
            name="amount"
            label="每次投资金额（美元）"
            rules={[
              { required: true, message: '请输入投资金额' },
              { type: 'number', min: 1, message: '投资金额必须大于 0' },
            ]}
          >
            <InputNumber
              min={1}
              step={10}
              precision={2}
              style={{ width: '100%' }}
              prefix="$"
            />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  );
};

export default RecurringInvestmentPage;
