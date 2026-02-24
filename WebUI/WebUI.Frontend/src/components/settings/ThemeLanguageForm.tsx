/**
 * Theme and Language Configuration Form
 * 主题和语言配置表单
 */

import React from 'react';
import {
  Form,
  Select,
  Input,
  Radio,
  Typography,
  Space,
  Divider,
} from 'antd';
import type { FormInstance } from 'antd';

const { Option } = Select;
const { Text } = Typography;

interface ThemeLanguageFormProps {
  form: FormInstance;
}

const ThemeLanguageForm: React.FC<ThemeLanguageFormProps> = ({ form: _form }) => {
  return (
    <div>
      {/* Theme Configuration */}
      <Typography.Title level={4}>主题设置</Typography.Title>
      
      <Form.Item
        label="主题模式"
        name={['theme', 'mode']}
        tooltip="选择浅色或深色主题"
        rules={[{ required: true, message: '请选择主题模式' }]}
      >
        <Radio.Group>
          <Radio value="light">浅色主题</Radio>
          <Radio value="dark">深色主题</Radio>
        </Radio.Group>
      </Form.Item>

      <Form.Item
        label="主色调"
        name={['theme', 'primaryColor']}
        tooltip="主题的主要颜色（十六进制）"
        rules={[{ required: true, message: '请输入主色调' }]}
      >
        <Input
          type="color"
          style={{ width: 100, height: 40 }}
        />
      </Form.Item>

      <Form.Item
        label="强调色"
        name={['theme', 'accentColor']}
        tooltip="用于强调的颜色（十六进制）"
        rules={[{ required: true, message: '请输入强调色' }]}
      >
        <Input
          type="color"
          style={{ width: 100, height: 40 }}
        />
      </Form.Item>

      <Form.Item
        label="图表配色方案"
        name={['theme', 'chartColorScheme']}
        tooltip="图表使用的配色方案"
      >
        <Select placeholder="选择配色方案">
          <Option value="default">默认</Option>
          <Option value="colorblind">色盲友好</Option>
          <Option value="highContrast">高对比度</Option>
        </Select>
      </Form.Item>

      <Form.Item
        label="价格颜色模式"
        name={['theme', 'priceColorMode']}
        tooltip="价格涨跌的颜色表示方式"
      >
        <Radio.Group>
          <Radio value="western">
            <Space>
              <Text>西方模式</Text>
              <Text type="secondary">（绿涨红跌）</Text>
            </Space>
          </Radio>
          <Radio value="chinese">
            <Space>
              <Text>中国模式</Text>
              <Text type="secondary">（红涨绿跌）</Text>
            </Space>
          </Radio>
        </Radio.Group>
      </Form.Item>

      <Divider />

      {/* Language Configuration */}
      <Typography.Title level={4}>语言与本地化</Typography.Title>

      <Form.Item
        label="界面语言"
        name={['language', 'locale']}
        tooltip="选择界面显示语言"
        rules={[{ required: true, message: '请选择语言' }]}
      >
        <Select placeholder="选择语言">
          <Option value="zh-CN">简体中文</Option>
          <Option value="en-US">English (US)</Option>
        </Select>
      </Form.Item>

      <Form.Item
        label="日期格式"
        name={['language', 'dateFormat']}
        tooltip="日期的显示格式"
        rules={[{ required: true, message: '请输入日期格式' }]}
      >
        <Input placeholder="YYYY-MM-DD" />
      </Form.Item>

      <Form.Item
        label="时间格式"
        name={['language', 'timeFormat']}
        tooltip="时间的显示格式"
        rules={[{ required: true, message: '请选择时间格式' }]}
      >
        <Radio.Group>
          <Radio value="12h">12小时制</Radio>
          <Radio value="24h">24小时制</Radio>
        </Radio.Group>
      </Form.Item>

      <Form.Item
        label="货币符号"
        name={['language', 'currencyFormat']}
        tooltip="货币金额的显示符号"
        rules={[{ required: true, message: '请输入货币符号' }]}
      >
        <Input placeholder="$" style={{ width: 100 }} />
      </Form.Item>

      <Form.Item
        label="小数分隔符"
        name={['language', 'numberFormat', 'decimalSeparator']}
        tooltip="小数点的分隔符"
        rules={[{ required: true, message: '请输入小数分隔符' }]}
      >
        <Input placeholder="." style={{ width: 100 }} />
      </Form.Item>

      <Form.Item
        label="千位分隔符"
        name={['language', 'numberFormat', 'thousandsSeparator']}
        tooltip="千位数的分隔符"
        rules={[{ required: true, message: '请输入千位分隔符' }]}
      >
        <Input placeholder="," style={{ width: 100 }} />
      </Form.Item>
    </div>
  );
};

export default ThemeLanguageForm;
