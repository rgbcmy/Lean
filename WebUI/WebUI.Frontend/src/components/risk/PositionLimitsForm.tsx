/**
 * Position Limits Configuration Form
 * 仓位限制配置表单
 */

import React from 'react';
import { Form, InputNumber, Space, Typography } from 'antd';
import type { FormInstance } from 'antd';

const { Text } = Typography;

interface PositionLimitsFormProps {
  form: FormInstance;
}

const PositionLimitsForm: React.FC<PositionLimitsFormProps> = ({ form: _form }) => {
  return (
    <div>
      <Form.Item
        label="单个持仓最大占比"
        name={['positionLimits', 'maxPositionSizePercent']}
        tooltip="单个股票持仓价值不得超过账户总值的该百分比"
        rules={[
          { required: true, message: '请输入最大持仓占比' },
          { type: 'number', min: 0, max: 100, message: '请输入0-100之间的数字' },
        ]}
      >
        <Space>
          <InputNumber
            min={0}
            max={100}
            style={{ width: 120 }}
            placeholder="例如：20"
          />
          <Text>%</Text>
          <Text type="secondary">（推荐：10-20%）</Text>
        </Space>
      </Form.Item>

      <Form.Item
        label="最大持仓数量"
        name={['positionLimits', 'maxNumberOfPositions']}
        tooltip="同时持有的股票数量上限"
        rules={[
          { required: true, message: '请输入最大持仓数量' },
          { type: 'number', min: 1, message: '至少允许1个持仓' },
        ]}
      >
        <Space>
          <InputNumber
            min={1}
            style={{ width: 120 }}
            placeholder="例如：10"
          />
          <Text>只</Text>
          <Text type="secondary">（推荐：5-15只）</Text>
        </Space>
      </Form.Item>

      <Form.Item
        label="最低现金储备"
        name={['positionLimits', 'minCashReservePercent']}
        tooltip="账户必须保留的最低现金比例"
        rules={[
          { required: true, message: '请输入最低现金储备' },
          { type: 'number', min: 0, max: 100, message: '请输入0-100之间的数字' },
        ]}
      >
        <Space>
          <InputNumber
            min={0}
            max={100}
            style={{ width: 120 }}
            placeholder="例如：10"
          />
          <Text>%</Text>
          <Text type="secondary">（推荐：5-15%）</Text>
        </Space>
      </Form.Item>

      <Form.Item
        label="单个行业最大集中度"
        name={['positionLimits', 'maxSectorConcentrationPercent']}
        tooltip="单个行业持仓总值不得超过账户总值的该百分比"
        rules={[
          { required: true, message: '请输入最大行业集中度' },
          { type: 'number', min: 0, max: 100, message: '请输入0-100之间的数字' },
        ]}
      >
        <Space>
          <InputNumber
            min={0}
            max={100}
            style={{ width: 120 }}
            placeholder="例如：40"
          />
          <Text>%</Text>
          <Text type="secondary">（推荐：30-50%）</Text>
        </Space>
      </Form.Item>
    </div>
  );
};

export default PositionLimitsForm;
