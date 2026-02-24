/**
 * Edit Strategy Page
 * 编辑策略页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Form,
  Input,
  Button,
  Space,
  Typography,
  message,
  Spin,
  Alert,
  Divider,
} from 'antd';
import {
  SaveOutlined,
  ArrowLeftOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons';
import { useNavigate, useParams } from 'react-router-dom';
import {
  getStrategyById,
  updateStrategy,
} from '../api/strategiesApi';
import type { Strategy, UpdateStrategyRequest } from '../types/strategy';
import './EditStrategyPage.css';

const { Title, Text } = Typography;
const { TextArea } = Input;

/**
 * EditStrategyPage Component
 * Allows users to edit an existing strategy
 * 允许用户编辑现有策略
 */
const EditStrategyPage: React.FC = () => {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [strategy, setStrategy] = useState<Strategy | null>(null);

  /**
   * Load strategy data
   * 加载策略数据
   */
  useEffect(() => {
    if (!id) {
      message.error('策略 ID 无效');
      navigate('/strategies');
      return;
    }

    const loadStrategy = async () => {
      setLoading(true);
      try {
        const result = await getStrategyById(id);
        setStrategy(result);

        // Parse parameters
        let parameters = '{}';
        if (result.parameters) {
          try {
            parameters = JSON.stringify(JSON.parse(result.parameters), null, 2);
          } catch (error) {
            parameters = result.parameters;
          }
        }

        // Parse tags
        const tags = result.tags ? result.tags.join(', ') : '';

        // Set form values
        form.setFieldsValue({
          name: result.name,
          description: result.description || '',
          parameters,
          tags,
        });
      } catch (error) {
        console.error('Failed to load strategy:', error);
        message.error('加载策略失败');
        navigate('/strategies');
      } finally {
        setLoading(false);
      }
    };

    loadStrategy();
  }, [id, form, navigate]);

  /**
   * Handle form submission
   * 处理表单提交
   */
  const handleSubmit = async (values: any) => {
    if (!id || !strategy) return;

    setSaving(true);
    try {
      // Parse parameters
      let parameters = {};
      if (values.parameters) {
        try {
          parameters = JSON.parse(values.parameters);
        } catch (error) {
          message.error('参数 JSON 格式不正确');
          setSaving(false);
          return;
        }
      }

      // Parse tags
      const tags = values.tags
        ? values.tags.split(',').map((tag: string) => tag.trim()).filter((tag: string) => tag)
        : [];

      const request: UpdateStrategyRequest = {
        name: values.name,
        description: values.description,
        parameters,
        tags,
      };

      await updateStrategy(id, request);
      message.success(`策略 "${values.name}" 已更新`);
      navigate(`/strategies/${id}`);
    } catch (error) {
      console.error('Failed to update strategy:', error);
      message.error(`更新策略失败: ${error}`);
    } finally {
      setSaving(false);
    }
  };

  /**
   * Handle cancel
   * 处理取消
   */
  const handleCancel = () => {
    navigate(`/strategies/${id}`);
  };

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

  return (
    <div className="edit-strategy-page">
      {/* Header */}
      <div className="page-header">
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={handleCancel}>
            返回
          </Button>
          <Title level={2} style={{ margin: 0 }}>
            编辑策略: {strategy.name}
          </Title>
        </Space>
      </div>

      {/* Warning for running strategy */}
      {strategy.status === 'Running' && (
        <Alert
          type="warning"
          message="策略正在运行"
          description="正在运行的策略无法编辑。请先停止策略，然后再进行修改。"
          showIcon
          style={{ marginBottom: 24 }}
          action={
            <Button size="small" onClick={() => navigate(`/strategies/${id}`)}>
              返回详情
            </Button>
          }
        />
      )}

      {/* Info Alert */}
      {strategy.status !== 'Running' && (
        <Alert
          type="info"
          icon={<InfoCircleOutlined />}
          message="编辑说明"
          description={
            <>
              <Text>
                编辑策略将创建一个新版本。您可以随时查看历史版本或恢复到之前的版本。
              </Text>
              <br />
              <Text type="secondary">
                策略类型: <strong>{strategy.strategyType || '自定义'}</strong>
              </Text>
            </>
          }
          style={{ marginBottom: 24 }}
        />
      )}

      {/* Form */}
      <Card>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          disabled={strategy.status === 'Running'}
        >
          <Form.Item
            label="策略名称"
            name="name"
            rules={[
              { required: true, message: '请输入策略名称' },
              { max: 100, message: '策略名称不能超过100个字符' },
            ]}
          >
            <Input placeholder="例如：双均线策略" size="large" />
          </Form.Item>

          <Form.Item
            label="策略描述"
            name="description"
            rules={[{ max: 500, message: '描述不能超过500个字符' }]}
          >
            <TextArea
              rows={4}
              placeholder="简要描述策略的交易逻辑和目标"
            />
          </Form.Item>

          <Form.Item
            label="标签"
            name="tags"
            extra="多个标签用逗号分隔，例如：趋势跟踪,高频,股票"
          >
            <Input placeholder="趋势跟踪,股票" />
          </Form.Item>

          <Divider />

          <Form.Item
            label="策略参数 (JSON 格式)"
            name="parameters"
            rules={[
              {
                validator: (_, value) => {
                  if (!value) return Promise.resolve();
                  try {
                    JSON.parse(value);
                    return Promise.resolve();
                  } catch (error) {
                    return Promise.reject(new Error('参数格式必须是有效的 JSON'));
                  }
                },
              },
            ]}
            extra="配置策略运行所需的参数，格式为 JSON 对象"
          >
            <TextArea
              rows={12}
              placeholder={`{
  "fast_period": 10,
  "slow_period": 30,
  "stop_loss": 0.05,
  "take_profit": 0.1
}`}
              style={{ fontFamily: 'monospace' }}
            />
          </Form.Item>

          <Alert
            type="info"
            message="提示"
            description="参数修改将在下次启动策略时生效。正在运行的策略不会受到影响。"
            showIcon
            style={{ marginBottom: 24 }}
          />

          <Form.Item>
            <Space size="middle">
              <Button onClick={handleCancel}>取消</Button>
              <Button
                type="primary"
                htmlType="submit"
                icon={<SaveOutlined />}
                loading={saving}
                disabled={strategy.status === 'Running'}
              >
                保存修改
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>

      {/* Strategy Info */}
      <Card title="策略信息" style={{ marginTop: 24 }}>
        <Space direction="vertical" size="small" style={{ width: '100%' }}>
          <div>
            <Text type="secondary">策略 ID: </Text>
            <Text code>{strategy.id}</Text>
          </div>
          <div>
            <Text type="secondary">创建时间: </Text>
            <Text>{new Date(strategy.createdAt).toLocaleString('zh-CN')}</Text>
          </div>
          <div>
            <Text type="secondary">最后更新: </Text>
            <Text>{new Date(strategy.updatedAt).toLocaleString('zh-CN')}</Text>
          </div>
          {strategy.version && (
            <div>
              <Text type="secondary">当前版本: </Text>
              <Text>v{strategy.version}</Text>
            </div>
          )}
          {strategy.codeFilePath && (
            <div>
              <Text type="secondary">代码文件: </Text>
              <Text code>{strategy.codeFilePath}</Text>
            </div>
          )}
        </Space>
      </Card>
    </div>
  );
};

export default EditStrategyPage;
