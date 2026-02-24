/**
 * Trading Frequency Limit Configuration Form
 * 交易频率限制配置表单
 */

import React from 'react';
import { Form, InputNumber, Switch, Space, Typography } from 'antd';
import type { FormInstance } from 'antd';

const { Text } = Typography;

interface TradingFrequencyFormProps {
  form: FormInstance;
}

const TradingFrequencyForm: React.FC<TradingFrequencyFormProps> = ({ form }) => {
  const enabled = Form.useWatch(['tradingFrequency', 'enabled'], form);

  return (
    <div>
      <Form.Item
        label="启用交易频率限制"
        name={['tradingFrequency', 'enabled']}
        valuePropName="checked"
        tooltip="开启后将限制日内交易频率"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="每日最大交易笔数"
        name={['tradingFrequency', 'maxTradesPerDay']}
        tooltip="单日允许的最大交易笔数（买入+卖出）"
        rules={[
          { required: enabled, message: '请输入每日最大交易笔数' },
          { type: 'number', min: 1, message: '至少允许1笔交易' },
        ]}
      >
        <Space>
          <InputNumber
            min={1}
            style={{ width: 120 }}
            placeholder="例如：20"
            disabled={!enabled}
          />
          <Text>笔</Text>
          <Text type="secondary">（推荐：10-50笔）</Text>
        </Space>
      </Form.Item>

      <Form.Item
        label="每日最大交易金额"
        name={['tradingFrequency', 'maxOrderValuePerDay']}
        tooltip="单日允许的最大交易金额（累计）"
        rules={[
          { required: enabled, message: '请输入每日最大交易金额' },
          { type: 'number', min: 0, message: '金额必须大于0' },
        ]}
      >
        <Space>
          <Text>$</Text>
          <InputNumber
            min={0}
            style={{ width: 150 }}
            placeholder="例如：50000"
            disabled={!enabled}
          />
          <Text type="secondary">（0表示不限制）</Text>
        </Space>
      </Form.Item>

      <Form.Item
        label="同一股票交易冷却时间"
        name={['tradingFrequency', 'cooldownPeriodMinutes']}
        tooltip="对同一股票进行两次交易之间的最小时间间隔"
        rules={[
          { required: enabled, message: '请输入冷却时间' },
          { type: 'number', min: 0, message: '冷却时间不能为负' },
        ]}
      >
        <Space>
          <InputNumber
            min={0}
            style={{ width: 120 }}
            placeholder="例如：10"
            disabled={!enabled}
          />
          <Text>分钟</Text>
          <Text type="secondary">（0表示不限制）</Text>
        </Space>
      </Form.Item>
    </div>
  );
};

export default TradingFrequencyForm;
