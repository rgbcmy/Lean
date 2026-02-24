/**
 * Create Strategy Page
 * 新建策略页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Form,
  Input,
  Button,
  Select,
  Space,
  Typography,
  message,
  Steps,
  Upload,
  Row,
  Col,
  Divider,
  Tag,
  Alert,
} from 'antd';
import {
  SaveOutlined,
  ArrowLeftOutlined,
  UploadOutlined,
  InfoCircleOutlined,
} from '@ant-design/icons';
import type { UploadFile } from 'antd/es/upload/interface';
import { useNavigate } from 'react-router-dom';
import {
  createStrategy,
  getStrategyTemplates,
} from '../api/strategiesApi';
import type { CreateStrategyRequest, StrategyTemplate } from '../types/strategy';
import './CreateStrategyPage.css';

const { Title, Text, Paragraph } = Typography;
const { Option } = Select;
const { TextArea } = Input;
const { Step } = Steps;

/**
 * CreateStrategyPage Component
 * Allows users to create a new strategy
 * 允许用户创建新策略
 */
const CreateStrategyPage: React.FC = () => {
  const [form] = Form.useForm();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [templates, setTemplates] = useState<StrategyTemplate[]>([]);
  const [selectedTemplate, setSelectedTemplate] = useState<StrategyTemplate | null>(null);
  const [currentStep, setCurrentStep] = useState(0);
  const [fileList, setFileList] = useState<UploadFile[]>([]);

  /**
   * Load strategy templates
   * 加载策略模板列表
   */
  useEffect(() => {
    const loadTemplates = async () => {
      try {
        const result = await getStrategyTemplates();
        setTemplates(result);
      } catch (error) {
        console.error('Failed to load templates:', error);
        message.error('加载策略模板失败');
      }
    };

    loadTemplates();
  }, []);

  /**
   * Handle template selection
   * 处理模板选择
   */
  const handleTemplateChange = (templateId: string) => {
    if (templateId === 'custom') {
      setSelectedTemplate(null);
      form.setFieldsValue({ parameters: '{}' });
    } else {
      const template = templates.find((t) => t.id === templateId);
      if (template) {
        setSelectedTemplate(template);
        form.setFieldsValue({
          strategyType: template.name,
          description: template.description,
          parameters: JSON.stringify(template.defaultParameters, null, 2),
        });
      }
    }
  };

  /**
   * Handle file upload
   * 处理文件上传
   */
  const handleFileChange = (info: any) => {
    let newFileList = [...info.fileList];
    
    // Limit to 1 file
    newFileList = newFileList.slice(-1);
    
    // Read file content
    if (newFileList.length > 0 && newFileList[0].originFileObj) {
      const file = newFileList[0].originFileObj;
      const reader = new FileReader();
      
      reader.onload = (e) => {
        const content = e.target?.result as string;
        const base64Content = btoa(content);
        form.setFieldsValue({
          codeFileContent: base64Content,
          codeFileName: file.name,
        });
      };
      
      reader.readAsText(file);
    }
    
    setFileList(newFileList);
  };

  /**
   * Handle form submission
   * 处理表单提交
   */
  const handleSubmit = async (values: any) => {
    setLoading(true);
    try {
      // Parse parameters
      let parameters = {};
      if (values.parameters) {
        try {
          parameters = JSON.parse(values.parameters);
        } catch (error) {
          message.error('参数 JSON 格式不正确');
          setLoading(false);
          return;
        }
      }

      // Parse tags
      const tags = values.tags
        ? values.tags.split(',').map((tag: string) => tag.trim()).filter((tag: string) => tag)
        : [];

      const request: CreateStrategyRequest = {
        name: values.name,
        description: values.description,
        strategyType: values.strategyType === 'custom' ? '自定义' : values.strategyType,
        parameters,
        tags,
        codeFileContent: values.codeFileContent,
        codeFileName: values.codeFileName,
      };

      const createdStrategy = await createStrategy(request);
      message.success(`策略 "${createdStrategy.name}" 创建成功`);
      navigate('/strategies');
    } catch (error) {
      console.error('Failed to create strategy:', error);
      message.error(`创建策略失败: ${error}`);
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle cancel
   * 处理取消
   */
  const handleCancel = () => {
    navigate('/strategies');
  };

  /**
   * Next step
   * 下一步
   */
  const nextStep = () => {
    form.validateFields().then(() => {
      setCurrentStep(currentStep + 1);
    });
  };

  /**
   * Previous step
   * 上一步
   */
  const prevStep = () => {
    setCurrentStep(currentStep - 1);
  };

  return (
    <div className="create-strategy-page">
      {/* Header */}
      <div className="page-header">
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={handleCancel}>
            返回
          </Button>
          <Title level={2} style={{ margin: 0 }}>
            创建新策略
          </Title>
        </Space>
      </div>

      {/* Steps */}
      <Card style={{ marginBottom: 24 }}>
        <Steps current={currentStep}>
          <Step title="选择类型" description="选择策略类型或模板" />
          <Step title="基本信息" description="填写策略基本信息" />
          <Step title="参数配置" description="配置策略参数" />
        </Steps>
      </Card>

      {/* Form */}
      <Card>
        <Form
          form={form}
          layout="vertical"
          onFinish={handleSubmit}
          initialValues={{
            strategyType: 'custom',
            parameters: '{}',
          }}
        >
          {/* Step 0: Template Selection */}
          {currentStep === 0 && (
            <>
              <Form.Item
                label="策略类型"
                name="strategyType"
                rules={[{ required: true, message: '请选择策略类型' }]}
              >
                <Select
                  placeholder="选择策略类型或模板"
                  size="large"
                  onChange={handleTemplateChange}
                >
                  <Option value="custom">
                    <Space>
                      <Text strong>自定义策略</Text>
                      <Text type="secondary">（上传自己的代码文件）</Text>
                    </Space>
                  </Option>
                  <Option disabled>─────── 预定义模板 ───────</Option>
                  {templates.map((template) => (
                    <Option key={template.id} value={template.id}>
                      <Space direction="vertical" size="small">
                        <Text strong>{template.name}</Text>
                        <Text type="secondary" style={{ fontSize: '12px' }}>
                          {template.description}
                        </Text>
                        {template.category && (
                          <Tag color="blue" style={{ fontSize: '11px' }}>
                            {template.category}
                          </Tag>
                        )}
                      </Space>
                    </Option>
                  ))}
                </Select>
              </Form.Item>

              {selectedTemplate && (
                <Alert
                  type="info"
                  icon={<InfoCircleOutlined />}
                  message="模板说明"
                  description={
                    <>
                      <Paragraph>{selectedTemplate.description}</Paragraph>
                      <Text strong>默认参数：</Text>
                      <pre style={{ marginTop: 8, padding: 12, background: '#f5f5f5', borderRadius: 4 }}>
                        {JSON.stringify(selectedTemplate.defaultParameters, null, 2)}
                      </pre>
                    </>
                  }
                  style={{ marginBottom: 24 }}
                />
              )}

              {form.getFieldValue('strategyType') === 'custom' && (
                <>
                  <Divider />
                  <Form.Item
                    label="上传策略代码文件"
                    name="codeFile"
                    extra="支持 .py (Python) 或 .cs (C#) 文件"
                  >
                    <Upload
                      accept=".py,.cs"
                      fileList={fileList}
                      onChange={handleFileChange}
                      beforeUpload={() => false}
                      maxCount={1}
                    >
                      <Button icon={<UploadOutlined />}>选择文件</Button>
                    </Upload>
                  </Form.Item>
                  <Form.Item name="codeFileContent" hidden>
                    <Input />
                  </Form.Item>
                  <Form.Item name="codeFileName" hidden>
                    <Input />
                  </Form.Item>
                </>
              )}

              <Form.Item>
                <Space>
                  <Button onClick={handleCancel}>取消</Button>
                  <Button type="primary" onClick={nextStep}>
                    下一步
                  </Button>
                </Space>
              </Form.Item>
            </>
          )}

          {/* Step 1: Basic Information */}
          {currentStep === 1 && (
            <>
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

              <Form.Item>
                <Space>
                  <Button onClick={prevStep}>上一步</Button>
                  <Button onClick={handleCancel}>取消</Button>
                  <Button type="primary" onClick={nextStep}>
                    下一步
                  </Button>
                </Space>
              </Form.Item>
            </>
          )}

          {/* Step 2: Parameters */}
          {currentStep === 2 && (
            <>
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
                description="参数配置将在策略启动时传递给策略代码。您可以在创建后继续调整这些参数。"
                showIcon
                style={{ marginBottom: 24 }}
              />

              <Form.Item>
                <Space>
                  <Button onClick={prevStep}>上一步</Button>
                  <Button onClick={handleCancel}>取消</Button>
                  <Button
                    type="primary"
                    htmlType="submit"
                    icon={<SaveOutlined />}
                    loading={loading}
                  >
                    创建策略
                  </Button>
                </Space>
              </Form.Item>
            </>
          )}
        </Form>
      </Card>
    </div>
  );
};

export default CreateStrategyPage;
