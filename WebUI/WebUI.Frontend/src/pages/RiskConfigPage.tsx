/**
 * Risk Configuration Page
 * 风险控制配置页面
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Card,
  Form,
  Button,
  Space,
  message,
  Spin,
  Tabs,
  Divider,
  InputNumber,
  Switch,
} from 'antd';
import { SaveOutlined, ReloadOutlined } from '@ant-design/icons';
import { getRiskConfig, updateRiskConfig } from '../api/riskApi';
import type { RiskConfig } from '../types/risk';
import StopLossTakeProfitForm from '../components/risk/StopLossTakeProfitForm';
import PositionLimitsForm from '../components/risk/PositionLimitsForm';
import TradingFrequencyForm from '../components/risk/TradingFrequencyForm';
import './RiskConfigPage.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

const RiskConfigPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [config, setConfig] = useState<RiskConfig | null>(null);

  // Load risk configuration
  const loadConfig = async () => {
    setLoading(true);
    try {
      const data = await getRiskConfig();
      setConfig(data);
      form.setFieldsValue(data);
    } catch (error: any) {
      message.error(error.response?.data?.message || '加载风险配置失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadConfig();
  }, []);

  // Save risk configuration
  const handleSave = async () => {
    try {
      const values = await form.validateFields();
      setSaving(true);
      
      const updatedConfig = await updateRiskConfig(values);
      setConfig(updatedConfig);
      message.success('风险配置已保存');
    } catch (error: any) {
      if (error.errorFields) {
        message.error('请检查表单中的错误');
      } else {
        message.error(error.response?.data?.message || '保存风险配置失败');
      }
    } finally {
      setSaving(false);
    }
  };

  // Reset form
  const handleReset = () => {
    if (config) {
      form.setFieldsValue(config);
      message.info('表单已重置');
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '50px' }}>
        <Spin size="large" tip="加载中..." />
      </div>
    );
  }

  return (
    <div className="risk-config-page">
      <div className="page-header">
        <div>
          <Title level={2}>风险控制配置</Title>
          <Text type="secondary">
            配置止损止盈规则、仓位限制和交易频率控制
          </Text>
        </div>
        <Space>
          <Button icon={<ReloadOutlined />} onClick={handleReset}>
            重置
          </Button>
          <Button
            type="primary"
            icon={<SaveOutlined />}
            loading={saving}
            onClick={handleSave}
          >
            保存配置
          </Button>
        </Space>
      </div>

      <Form
        form={form}
        layout="vertical"
        initialValues={config || undefined}
      >
        <Tabs defaultActiveKey="stopLossTakeProfit">
          <TabPane tab="止损止盈" key="stopLossTakeProfit">
            <Card>
              <StopLossTakeProfitForm form={form} />
            </Card>
          </TabPane>

          <TabPane tab="仓位限制" key="positionLimits">
            <Card>
              <PositionLimitsForm form={form} />
            </Card>
          </TabPane>

          <TabPane tab="交易频率" key="tradingFrequency">
            <Card>
              <TradingFrequencyForm form={form} />
            </Card>
          </TabPane>

          <TabPane tab="高级设置" key="advanced">
            <Card title="保证金告警">
              <Form.Item
                label="保证金使用率警告阈值"
                name="marginWarningThreshold"
                tooltip="当保证金使用率超过此阈值时发送警告"
                rules={[{ required: true, message: '请输入警告阈值' }]}
              >
                <InputNumber min={0} max={100} addonAfter="%" style={{ width: 150 }} />
              </Form.Item>

              <Form.Item
                label="保证金使用率阻止阈值"
                name="marginBlockThreshold"
                tooltip="当保证金使用率超过此阈值时阻止新开仓"
                rules={[{ required: true, message: '请输入阻止阈值' }]}
              >
                <InputNumber min={0} max={100} addonAfter="%" style={{ width: 150 }} />
              </Form.Item>
            </Card>

            <Divider />

            <Card title="熔断机制">
              <Form.Item
                label="启用熔断机制"
                name="circuitBreakerEnabled"
                valuePropName="checked"
              >
                <Switch />
              </Form.Item>

              <Form.Item
                label="价格异常波动阈值"
                name="priceMovementThreshold"
                tooltip="1分钟内价格变动超过此百分比时触发熔断"
              >
                <InputNumber min={0} max={100} addonAfter="%" style={{ width: 150 }} />
              </Form.Item>

              <Form.Item
                label="大额订单确认阈值"
                name="largeOrderThreshold"
                tooltip="订单金额超过此值时需要二次确认"
              >
                <InputNumber min={0} addonBefore="$" style={{ width: 180 }} />
              </Form.Item>
            </Card>

            <Divider />

            <Card title="日内交易规则">
              <Form.Item
                label="启用日内交易规则（PDT）"
                name="dayTradingRulesEnabled"
                valuePropName="checked"
                tooltip="遵守美国Pattern Day Trader规则"
              >
                <Switch />
              </Form.Item>
            </Card>
          </TabPane>
        </Tabs>
      </Form>
    </div>
  );
};

export default RiskConfigPage;
