/**
 * Notification Preferences Form
 * 通知偏好配置表单
 */

import React from 'react';
import {
  Form,
  Input,
  Switch,
  Typography,
  Divider,
  Space,
} from 'antd';
import type { FormInstance } from 'antd';

const { Text } = Typography;

interface NotificationPreferencesFormProps {
  form: FormInstance;
}

const NotificationPreferencesForm: React.FC<NotificationPreferencesFormProps> = ({ form }) => {
  const emailEnabled = Form.useWatch(['notifications', 'email', 'enabled'], form);
  const pushEnabled = Form.useWatch(['notifications', 'push', 'enabled'], form);
  const inAppEnabled = Form.useWatch(['notifications', 'inApp', 'enabled'], form);

  return (
    <div>
      {/* Email Notifications */}
      <Typography.Title level={4}>邮件通知</Typography.Title>
      
      <Form.Item
        label="启用邮件通知"
        name={['notifications', 'email', 'enabled']}
        valuePropName="checked"
        tooltip="通过邮件接收重要通知"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="邮箱地址"
        name={['notifications', 'email', 'address']}
        rules={[
          { required: emailEnabled, message: '请输入邮箱地址' },
          { type: 'email', message: '请输入有效的邮箱地址' },
        ]}
      >
        <Input placeholder="user@example.com" disabled={!emailEnabled} />
      </Form.Item>

      <Typography.Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
        通知事件：
      </Typography.Text>

      <Space direction="vertical" style={{ width: '100%', marginBottom: 24 }}>
        <Form.Item
          name={['notifications', 'email', 'events', 'orderFilled']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>订单成交</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'email', 'events', 'orderCanceled']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>订单取消</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'email', 'events', 'stopLossTriggered']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>止损触发</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'email', 'events', 'takeProfitTriggered']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>止盈触发</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'email', 'events', 'marginCall']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>追加保证金通知</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'email', 'events', 'strategyError']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>策略错误</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'email', 'events', 'dailyReport']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!emailEnabled} />
            <Text>每日报告</Text>
          </Space>
        </Form.Item>
      </Space>

      <Divider />

      {/* Push Notifications */}
      <Typography.Title level={4}>推送通知</Typography.Title>
      
      <Form.Item
        label="启用推送通知"
        name={['notifications', 'push', 'enabled']}
        valuePropName="checked"
        tooltip="通过浏览器推送接收通知"
      >
        <Switch />
      </Form.Item>

      <Typography.Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
        通知事件：
      </Typography.Text>

      <Space direction="vertical" style={{ width: '100%', marginBottom: 24 }}>
        <Form.Item
          name={['notifications', 'push', 'events', 'orderFilled']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!pushEnabled} />
            <Text>订单成交</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'push', 'events', 'orderCanceled']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!pushEnabled} />
            <Text>订单取消</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'push', 'events', 'stopLossTriggered']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!pushEnabled} />
            <Text>止损触发</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'push', 'events', 'marginCall']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!pushEnabled} />
            <Text>追加保证金通知</Text>
          </Space>
        </Form.Item>

        <Form.Item
          name={['notifications', 'push', 'events', 'riskAlert']}
          valuePropName="checked"
          style={{ marginBottom: 8 }}
        >
          <Space>
            <Switch disabled={!pushEnabled} />
            <Text>风险告警</Text>
          </Space>
        </Form.Item>
      </Space>

      <Divider />

      {/* In-App Notifications */}
      <Typography.Title level={4}>应用内通知</Typography.Title>
      
      <Form.Item
        label="启用应用内通知"
        name={['notifications', 'inApp', 'enabled']}
        valuePropName="checked"
        tooltip="在应用内显示通知消息"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="启用提示音"
        name={['notifications', 'inApp', 'sound']}
        valuePropName="checked"
        tooltip="通知时播放提示音"
      >
        <Switch disabled={!inAppEnabled} />
      </Form.Item>

      <Form.Item
        label="接收所有事件"
        name={['notifications', 'inApp', 'events', 'all']}
        valuePropName="checked"
        tooltip="在应用内接收所有类型的通知"
      >
        <Switch disabled={!inAppEnabled} />
      </Form.Item>
    </div>
  );
};

export default NotificationPreferencesForm;
