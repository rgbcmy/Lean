/**
 * Stop Loss and Take Profit Configuration Form
 * 止损止盈配置表单
 */

import React from 'react';
import {
  Form,
  Input,
  Select,
  Switch,
  InputNumber,
  Button,
  Space,
  Card,
} from 'antd';
import type { FormInstance } from 'antd';
import { PlusOutlined, DeleteOutlined } from '@ant-design/icons';

const { Option } = Select;

interface StopLossTakeProfitFormProps {
  form: FormInstance;
}

const StopLossTakeProfitForm: React.FC<StopLossTakeProfitFormProps> = ({ form: _form }) => {
  return (
    <div>
      <Form.Item label="止损规则">
        <Form.List name={['stopLossRules']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Card
                  key={field.key}
                  size="small"
                  style={{ marginBottom: 16 }}
                  extra={
                    <Button
                      type="link"
                      danger
                      icon={<DeleteOutlined />}
                      onClick={() => remove(field.name)}
                    >
                      删除
                    </Button>
                  }
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Form.Item
                      {...field}
                      name={[field.name, 'enabled']}
                      valuePropName="checked"
                      label="启用"
                    >
                      <Switch />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'type']}
                      label="类型"
                      rules={[{ required: true, message: '请选择止损类型' }]}
                    >
                      <Select placeholder="选择止损类型">
                        <Option value="percentage">百分比止损</Option>
                        <Option value="dollar">金额止损</Option>
                        <Option value="trailing">移动止损</Option>
                      </Select>
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'value']}
                      label="止损值"
                      rules={[{ required: true, message: '请输入止损值' }]}
                    >
                      <InputNumber
                        min={0}
                        style={{ width: '100%' }}
                        placeholder="例如：5（表示5%或$5）"
                      />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'symbol']}
                      label="适用股票（可选）"
                    >
                      <Input placeholder="留空表示适用于所有持仓" />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'description']}
                      label="描述"
                    >
                      <Input.TextArea rows={2} placeholder="规则描述" />
                    </Form.Item>
                  </Space>
                </Card>
              ))}
              <Button
                type="dashed"
                onClick={() =>
                  add({ enabled: true, type: 'percentage', value: 5 })
                }
                icon={<PlusOutlined />}
                style={{ width: '100%' }}
              >
                添加止损规则
              </Button>
            </>
          )}
        </Form.List>
      </Form.Item>

      <Form.Item label="止盈规则" style={{ marginTop: 32 }}>
        <Form.List name={['takeProfitRules']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Card
                  key={field.key}
                  size="small"
                  style={{ marginBottom: 16 }}
                  extra={
                    <Button
                      type="link"
                      danger
                      icon={<DeleteOutlined />}
                      onClick={() => remove(field.name)}
                    >
                      删除
                    </Button>
                  }
                >
                  <Space direction="vertical" style={{ width: '100%' }}>
                    <Form.Item
                      {...field}
                      name={[field.name, 'enabled']}
                      valuePropName="checked"
                      label="启用"
                    >
                      <Switch />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'type']}
                      label="类型"
                      rules={[{ required: true, message: '请选择止盈类型' }]}
                    >
                      <Select placeholder="选择止盈类型">
                        <Option value="percentage">百分比止盈</Option>
                        <Option value="dollar">金额止盈</Option>
                        <Option value="partial">分批止盈</Option>
                      </Select>
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'value']}
                      label="止盈值"
                      rules={[{ required: true, message: '请输入止盈值' }]}
                    >
                      <InputNumber
                        min={0}
                        style={{ width: '100%' }}
                        placeholder="例如：10（表示10%或$10）"
                      />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'symbol']}
                      label="适用股票（可选）"
                    >
                      <Input placeholder="留空表示适用于所有持仓" />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'notifyBeforeTarget']}
                      valuePropName="checked"
                      label="接近目标时提醒"
                    >
                      <Switch />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'notifyThreshold']}
                      label="提醒阈值（%）"
                    >
                      <InputNumber
                        min={0}
                        max={100}
                        style={{ width: '100%' }}
                        placeholder="例如：1（还差1%时提醒）"
                      />
                    </Form.Item>

                    <Form.Item
                      {...field}
                      name={[field.name, 'description']}
                      label="描述"
                    >
                      <Input.TextArea rows={2} placeholder="规则描述" />
                    </Form.Item>
                  </Space>
                </Card>
              ))}
              <Button
                type="dashed"
                onClick={() =>
                  add({ enabled: true, type: 'percentage', value: 10, notifyBeforeTarget: false })
                }
                icon={<PlusOutlined />}
                style={{ width: '100%' }}
              >
                添加止盈规则
              </Button>
            </>
          )}
        </Form.List>
      </Form.Item>
    </div>
  );
};

export default StopLossTakeProfitForm;
