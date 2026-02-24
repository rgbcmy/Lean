/**
 * Backtest Config Page
 * 回测配置页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Form,
  Input,
  Button,
  Select,
  DatePicker,
  InputNumber,
  Space,
  Typography,
  message,
  Row,
  Col,
  Divider,
  Alert,
  Spin,
} from 'antd';
import {
  PlayCircleOutlined,
  ArrowLeftOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  createBacktest,
  startBacktest,
} from '../api/backtestsApi';
import { getStrategies } from '../api/strategiesApi';
import type { CreateBacktestRequest } from '../types/backtest';
import type { Strategy } from '../types/strategy';
import './BacktestConfigPage.css';

const { Title } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;

/**
 * BacktestConfigPage Component
 * Allows users to configure and start a backtest
 * 允许用户配置并启动回测
 */
const BacktestConfigPage: React.FC = () => {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [loadingStrategies, setLoadingStrategies] = useState(true);
  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [selectedStrategy, setSelectedStrategy] = useState<Strategy | null>(null);

  /**
   * Load strategies list
   * 加载策略列表
   */
  useEffect(() => {
    const loadStrategies = async () => {
      try {
        setLoadingStrategies(true);
        const result = await getStrategies({ status: 'Active' });
        setStrategies(result);
      } catch (error) {
        console.error('Failed to load strategies:', error);
        message.error('加载策略列表失败');
      } finally {
        setLoadingStrategies(false);
      }
    };

    loadStrategies();
  }, []);

  /**
   * Handle strategy selection
   * 处理策略选择
   */
  const handleStrategyChange = (strategyId: string) => {
    const strategy = strategies.find((s) => s.id.toString() === strategyId);
    if (strategy) {
      setSelectedStrategy(strategy);
      // Pre-fill name with strategy name
      if (!form.getFieldValue('name')) {
        form.setFieldsValue({
          name: `${strategy.name} 回测 ${dayjs().format('YYYY-MM-DD HH:mm')}`,
        });
      }
    }
  };

  /**
   * Handle form submission
   * 处理表单提交
   */
  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      // Prepare backtest request
      const request: CreateBacktestRequest = {
        name: values.name,
        strategyId: parseInt(values.strategyId),
        startDate: values.dateRange[0].format('YYYY-MM-DD'),
        endDate: values.dateRange[1].format('YYYY-MM-DD'),
        initialCapital: values.initialCapital,
        benchmark: values.benchmark,
        dataResolution: values.dataResolution || 'Daily',
        parameters: values.parameters ? JSON.parse(values.parameters) : {},
      };

      // Create backtest
      const backtest = await createBacktest(request);
      message.success('回测已创建');

      // Start backtest if autoStart is checked
      if (values.autoStart) {
        await startBacktest(backtest.id);
        message.success('回测已启动');
      }

      // Navigate to backtest result page
      navigate(`/backtests/${backtest.id}`);
    } catch (error: any) {
      console.error('Failed to create backtest:', error);
      const errorMsg = error.response?.data?.message || '创建回测失败';
      message.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Validate date range
   * 验证日期范围
   */
  const validateDateRange = (_: any, value: any) => {
    if (!value || !value[0] || !value[1]) {
      return Promise.reject(new Error('请选择日期范围'));
    }
    
    const start = value[0];
    const end = value[1];
    
    if (end.isBefore(start)) {
      return Promise.reject(new Error('结束日期必须晚于开始日期'));
    }
    
    if (end.isAfter(dayjs())) {
      return Promise.reject(new Error('结束日期不能晚于今天'));
    }
    
    const daysDiff = end.diff(start, 'day');
    if (daysDiff < 1) {
      return Promise.reject(new Error('回测期间至少需要1天'));
    }
    
    if (daysDiff > 3650) {
      return Promise.reject(new Error('回测期间不能超过10年'));
    }
    
    return Promise.resolve();
  };

  return (
    <div className="backtest-config-page">
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
            新建回测
          </Title>
        </Space>
      </Card>

      <Card className="config-form-card">
        <Alert
          message="回测说明"
          description="回测使用历史数据测试策略表现。请选择策略、配置时间范围和初始资金，系统将模拟策略在历史数据上的运行情况。"
          type="info"
          icon={<InfoCircleOutlined />}
          showIcon
          closable
          style={{ marginBottom: 24 }}
        />

        <Spin spinning={loadingStrategies}>
          <Form
            form={form}
            layout="vertical"
            onFinish={handleSubmit}
            initialValues={{
              initialCapital: 100000,
              benchmark: 'SPY',
              dataResolution: 'Daily',
              autoStart: true,
              dateRange: [dayjs().subtract(1, 'year'), dayjs()],
            }}
          >
            <Title level={4}>基本信息</Title>
            <Divider />

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="name"
                  label="回测名称"
                  rules={[
                    { required: true, message: '请输入回测名称' },
                    { max: 100, message: '名称不能超过100个字符' },
                  ]}
                >
                  <Input
                    placeholder="输入回测名称"
                    maxLength={100}
                  />
                </Form.Item>
              </Col>

              <Col span={12}>
                <Form.Item
                  name="strategyId"
                  label="选择策略"
                  rules={[{ required: true, message: '请选择策略' }]}
                >
                  <Select
                    placeholder="选择要回测的策略"
                    onChange={handleStrategyChange}
                    showSearch
                    filterOption={(input, option) =>
                      String(option?.children || '')
                        .toLowerCase()
                        .includes(input.toLowerCase())
                    }
                  >
                    {strategies.map((strategy) => (
                      <Option key={strategy.id} value={strategy.id.toString()}>
                        {strategy.name}
                      </Option>
                    ))}
                  </Select>
                </Form.Item>
              </Col>
            </Row>

            {selectedStrategy && (
              <Alert
                message={`策略: ${selectedStrategy.name}`}
                description={selectedStrategy.description}
                type="success"
                style={{ marginBottom: 16 }}
              />
            )}

            <Title level={4} style={{ marginTop: 24 }}>
              回测参数
            </Title>
            <Divider />

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="dateRange"
                  label="回测日期范围"
                  rules={[
                    { required: true, message: '请选择日期范围' },
                    { validator: validateDateRange },
                  ]}
                >
                  <RangePicker
                    style={{ width: '100%' }}
                    format="YYYY-MM-DD"
                    disabledDate={(current) => current && current.isAfter(dayjs())}
                  />
                </Form.Item>
              </Col>

              <Col span={12}>
                <Form.Item
                  name="initialCapital"
                  label="初始资金 (USD)"
                  rules={[
                    { required: true, message: '请输入初始资金' },
                    {
                      type: 'number',
                      min: 1000,
                      max: 10000000,
                      message: '初始资金必须在 $1,000 到 $10,000,000 之间',
                    },
                  ]}
                >
                  <InputNumber
                    style={{ width: '100%' }}
                    formatter={(value) =>
                      `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
                    }
                    parser={(value) =>
                      value!.replace(/\$\s?|(,*)/g, '') as any
                    }
                    min={1000}
                    max={10000000}
                    step={1000}
                  />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="benchmark"
                  label="基准指数"
                  tooltip="选择一个基准指数用于对比策略表现"
                >
                  <Select placeholder="选择基准指数">
                    <Option value="SPY">SPY (S&P 500)</Option>
                    <Option value="QQQ">QQQ (NASDAQ-100)</Option>
                    <Option value="DIA">DIA (Dow Jones)</Option>
                    <Option value="IWM">IWM (Russell 2000)</Option>
                    <Option value="">无基准</Option>
                  </Select>
                </Form.Item>
              </Col>

              <Col span={12}>
                <Form.Item
                  name="dataResolution"
                  label="数据频率"
                  tooltip="选择回测使用的数据时间粒度"
                >
                  <Select placeholder="选择数据频率">
                    <Option value="Tick">Tick (逐笔)</Option>
                    <Option value="Second">Second (秒)</Option>
                    <Option value="Minute">Minute (分钟)</Option>
                    <Option value="Hour">Hour (小时)</Option>
                    <Option value="Daily">Daily (日线)</Option>
                  </Select>
                </Form.Item>
              </Col>
            </Row>

            {selectedStrategy?.parameters && (
              <>
                <Title level={4} style={{ marginTop: 24 }}>
                  策略参数
                </Title>
                <Divider />

                <Form.Item
                  name="parameters"
                  label="参数配置 (JSON)"
                  tooltip="以 JSON 格式配置策略参数"
                  initialValue={JSON.stringify(selectedStrategy.parameters, null, 2)}
                >
                  <Input.TextArea
                    rows={6}
                    placeholder='{"param1": 10, "param2": 0.5}'
                  />
                </Form.Item>
              </>
            )}

            <Divider />

            <Form.Item>
              <Space>
                <Button
                  type="primary"
                  htmlType="submit"
                  icon={<PlayCircleOutlined />}
                  loading={loading}
                  size="large"
                >
                  创建并启动回测
                </Button>

                <Button
                  onClick={() => navigate('/backtests')}
                  size="large"
                >
                  取消
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Spin>
      </Card>
    </div>
  );
};

export default BacktestConfigPage;
