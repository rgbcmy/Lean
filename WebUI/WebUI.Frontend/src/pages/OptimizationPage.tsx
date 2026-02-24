/**
 * Optimization Page
 * 参数优化页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Button,
  Form,
  Input,
  Select,
  DatePicker,
  InputNumber,
  Space,
  Typography,
  message,
  Row,
  Col,
  Divider,
  Table,
  Tag,
  Progress,
  Alert,
  Spin,
  Statistic,
} from 'antd';
import {
  PlayCircleOutlined,
  ArrowLeftOutlined,
  DeleteOutlined,
  PlusOutlined,
} from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import dayjs from 'dayjs';
import {
  optimizeParameters,
  getOptimization,
} from '../api/backtestsApi';
import { getStrategies } from '../api/strategiesApi';
import type { OptimizeParametersRequest, OptimizeParametersResponse, OptimizationResult, ParameterRange } from '../types/backtest';
import type { Strategy } from '../types/strategy';
import './OptimizationPage.css';

const { Title, Text } = Typography;
const { Option } = Select;
const { RangePicker } = DatePicker;
const { Column } = Table;

/**
 * OptimizationPage Component
 * Allows users to run parameter optimization
 * 允许用户运行参数优化
 */
const OptimizationPage: React.FC = () => {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [loadingStrategies, setLoadingStrategies] = useState(true);
  const [strategies, setStrategies] = useState<Strategy[]>([]);
  const [optimizationResults, setOptimizationResults] = useState<OptimizeParametersResponse | null>(null);
  const pollingIntervalRef = React.useRef<number | null>(null);

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
      // Pre-fill name
      if (!form.getFieldValue('name')) {
        form.setFieldsValue({
          name: `${strategy.name} 优化 ${dayjs().format('YYYY-MM-DD HH:mm')}`,
        });
      }
    }
  };

  /**
   * Poll optimization status
   * 轮询优化状态
   */
  const startPolling = (optId: string) => {
    if (pollingIntervalRef.current) return;

    pollingIntervalRef.current = setInterval(async () => {
      try {
        const result = await getOptimization(optId);
        setOptimizationResults(result);

        // Stop polling if completed or failed
        if (result.status === 'Completed' || result.status === 'Failed') {
          stopPolling();
        }
      } catch (error) {
        console.error('Failed to fetch optimization status:', error);
      }
    }, 5000); // Poll every 5 seconds
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
   * Handle form submission
   * 处理表单提交
   */
  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      // Prepare optimization request
      const parameters: ParameterRange[] = values.parameters.map((p: any) => ({
        name: p.name,
        min: p.min,
        max: p.max,
        step: p.step,
      }));

      const request: OptimizeParametersRequest = {
        name: values.name,
        strategyId: parseInt(values.strategyId),
        startDate: values.dateRange[0].format('YYYY-MM-DD'),
        endDate: values.dateRange[1].format('YYYY-MM-DD'),
        initialCapital: values.initialCapital,
        parameters,
        optimizationTarget: values.optimizationTarget || 'SharpeRatio',
      };

      // Start optimization
      const result = await optimizeParameters(request);
      setOptimizationResults(result);
      message.success('参数优化已启动');

      // Start polling
      startPolling(result.optimizationId);
    } catch (error: any) {
      console.error('Failed to start optimization:', error);
      const errorMsg = error.response?.data?.message || '启动优化失败';
      message.error(errorMsg);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Clean up polling on unmount
   * 组件卸载时清理轮询
   */
  useEffect(() => {
    return () => {
      stopPolling();
    };
  }, []);

  /**
   * Get status tag
   * 获取状态标签
   */
  const getStatusTag = (status: string) => {
    const statusMap: Record<string, { color: string; text: string }> = {
      Running: { color: 'processing', text: '运行中' },
      Completed: { color: 'success', text: '已完成' },
      Failed: { color: 'error', text: '失败' },
    };

    const config = statusMap[status] || { color: 'default', text: status };
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  return (
    <div className="optimization-page">
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
            参数优化
          </Title>
        </Space>
      </Card>

      <Card className="optimization-form-card">
        <Alert
          message="参数优化说明"
          description="参数优化通过网格搜索测试参数的不同组合，找出最佳参数配置。请设置参数范围和步长，系统将自动运行多次回测。"
          type="info"
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
              optimizationTarget: 'SharpeRatio',
              dateRange: [dayjs().subtract(1, 'year'), dayjs()],
              parameters: [{ name: '', min: 0, max: 100, step: 10 }],
            }}
          >
            <Title level={4}>基本信息</Title>
            <Divider />

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="name"
                  label="优化名称"
                  rules={[
                    { required: true, message: '请输入优化名称' },
                    { max: 100, message: '名称不能超过100个字符' },
                  ]}
                >
                  <Input placeholder="输入优化名称" maxLength={100} />
                </Form.Item>
              </Col>

              <Col span={12}>
                <Form.Item
                  name="strategyId"
                  label="选择策略"
                  rules={[{ required: true, message: '请选择策略' }]}
                >
                  <Select
                    placeholder="选择要优化的策略"
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

            <Title level={4} style={{ marginTop: 24 }}>
              优化配置
            </Title>
            <Divider />

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="dateRange"
                  label="回测日期范围"
                  rules={[{ required: true, message: '请选择日期范围' }]}
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
                  rules={[{ required: true, message: '请输入初始资金' }]}
                >
                  <InputNumber
                    style={{ width: '100%' }}
                    formatter={(value) =>
                      `$ ${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
                    }
                    parser={(value) => value!.replace(/\$\s?|(,*)/g, '') as any}
                    min={1000}
                    max={10000000}
                    step={1000}
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              name="optimizationTarget"
              label="优化目标"
              rules={[{ required: true, message: '请选择优化目标' }]}
            >
              <Select placeholder="选择优化目标指标">
                <Option value="TotalReturn">总收益率</Option>
                <Option value="SharpeRatio">夏普比率</Option>
                <Option value="MaxDrawdown">最小回撤</Option>
              </Select>
            </Form.Item>

            <Title level={4} style={{ marginTop: 24 }}>
              参数范围
            </Title>
            <Divider />

            <Form.List name="parameters">
              {(fields, { add, remove }) => (
                <>
                  {fields.map((field, index) => (
                    <Card
                      key={field.key}
                      size="small"
                      title={`参数 ${index + 1}`}
                      extra={
                        fields.length > 1 && (
                          <Button
                            type="text"
                            danger
                            size="small"
                            icon={<DeleteOutlined />}
                            onClick={() => remove(field.name)}
                          >
                            删除
                          </Button>
                        )
                      }
                      style={{ marginBottom: 16 }}
                    >
                      <Row gutter={16}>
                        <Col span={6}>
                          <Form.Item
                            {...field}
                            name={[field.name, 'name']}
                            label="参数名称"
                            rules={[{ required: true, message: '请输入参数名称' }]}
                          >
                            <Input placeholder="例如: stopLoss" />
                          </Form.Item>
                        </Col>
                        <Col span={6}>
                          <Form.Item
                            {...field}
                            name={[field.name, 'min']}
                            label="最小值"
                            rules={[{ required: true, message: '请输入最小值' }]}
                          >
                            <InputNumber style={{ width: '100%' }} />
                          </Form.Item>
                        </Col>
                        <Col span={6}>
                          <Form.Item
                            {...field}
                            name={[field.name, 'max']}
                            label="最大值"
                            rules={[{ required: true, message: '请输入最大值' }]}
                          >
                            <InputNumber style={{ width: '100%' }} />
                          </Form.Item>
                        </Col>
                        <Col span={6}>
                          <Form.Item
                            {...field}
                            name={[field.name, 'step']}
                            label="步长"
                            rules={[{ required: true, message: '请输入步长' }]}
                          >
                            <InputNumber style={{ width: '100%' }} min={0.01} />
                          </Form.Item>
                        </Col>
                      </Row>
                    </Card>
                  ))}
                  <Button
                    type="dashed"
                    onClick={() => add()}
                    icon={<PlusOutlined />}
                    block
                    style={{ marginBottom: 16 }}
                  >
                    添加参数
                  </Button>
                </>
              )}
            </Form.List>

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
                  开始优化
                </Button>

                <Button onClick={() => navigate('/backtests')} size="large">
                  取消
                </Button>
              </Space>
            </Form.Item>
          </Form>
        </Spin>
      </Card>

      {/* Optimization Results */}
      {optimizationResults && (
        <Card title="优化结果" style={{ marginTop: 24 }}>
          <Space direction="vertical" style={{ width: '100%' }}>
            <Row justify="space-between">
              <Col>
                <Text strong>状态: </Text>
                {getStatusTag(optimizationResults.status)}
              </Col>
              <Col>
                <Text type="secondary">
                  已完成: {optimizationResults.completedCombinations} / {optimizationResults.totalCombinations}
                </Text>
              </Col>
            </Row>

            {optimizationResults.status === 'Running' && (
              <Progress
                percent={
                  (optimizationResults.completedCombinations /
                    optimizationResults.totalCombinations) *
                  100
                }
                status="active"
              />
            )}

            {optimizationResults.bestResult && (
              <Card title="最佳参数组合" size="small" style={{ marginTop: 16 }}>
                <Row gutter={16}>
                  <Col span={8}>
                    <Statistic
                      title="总收益率"
                      value={optimizationResults.bestResult.totalReturn}
                      precision={2}
                      suffix="%"
                      valueStyle={{ color: '#3f8600' }}
                    />
                  </Col>
                  <Col span={8}>
                    <Statistic
                      title="夏普比率"
                      value={optimizationResults.bestResult.sharpeRatio}
                      precision={2}
                    />
                  </Col>
                  <Col span={8}>
                    <Statistic
                      title="最大回撤"
                      value={optimizationResults.bestResult.maxDrawdown}
                      precision={2}
                      suffix="%"
                      valueStyle={{ color: '#cf1322' }}
                    />
                  </Col>
                </Row>
                <Divider />
                <Text strong>参数值:</Text>
                <div style={{ marginTop: 8 }}>
                  {Object.entries(optimizationResults.bestResult.parameters).map(
                    ([key, value]) => (
                      <Tag key={key} color="blue" style={{ marginTop: 4 }}>
                        {key}: {value}
                      </Tag>
                    )
                  )}
                </div>
              </Card>
            )}

            {optimizationResults.results && optimizationResults.results.length > 0 && (
              <Table
                dataSource={optimizationResults.results}
                rowKey={(_, index) => index!}
                pagination={{ pageSize: 10 }}
                scroll={{ x: true }}
                style={{ marginTop: 16 }}
              >
                <Column
                  title="参数组合"
                  key="parameters"
                  render={(_, record: OptimizationResult) => (
                    <Space wrap>
                      {Object.entries(record.parameters).map(([key, value]) => (
                        <Tag key={key}>
                          {key}: {value}
                        </Tag>
                      ))}
                    </Space>
                  )}
                />
                <Column
                  title="总收益率"
                  dataIndex="totalReturn"
                  key="totalReturn"
                  align="right"
                  render={(value) => `${value.toFixed(2)}%`}
                  sorter={(a: OptimizationResult, b: OptimizationResult) =>
                    a.totalReturn - b.totalReturn
                  }
                />
                <Column
                  title="夏普比率"
                  dataIndex="sharpeRatio"
                  key="sharpeRatio"
                  align="right"
                  render={(value) => value.toFixed(2)}
                  sorter={(a: OptimizationResult, b: OptimizationResult) =>
                    a.sharpeRatio - b.sharpeRatio
                  }
                />
                <Column
                  title="最大回撤"
                  dataIndex="maxDrawdown"
                  key="maxDrawdown"
                  align="right"
                  render={(value) => `${value.toFixed(2)}%`}
                  sorter={(a: OptimizationResult, b: OptimizationResult) =>
                    b.maxDrawdown - a.maxDrawdown
                  }
                />
                <Column
                  title="胜率"
                  dataIndex="winRate"
                  key="winRate"
                  align="right"
                  render={(value) => `${value.toFixed(2)}%`}
                />
                <Column
                  title="得分"
                  dataIndex="score"
                  key="score"
                  align="right"
                  render={(value) => value.toFixed(2)}
                  sorter={(a: OptimizationResult, b: OptimizationResult) =>
                    b.score - a.score
                  }
                />
              </Table>
            )}
          </Space>
        </Card>
      )}
    </div>
  );
};

export default OptimizationPage;
