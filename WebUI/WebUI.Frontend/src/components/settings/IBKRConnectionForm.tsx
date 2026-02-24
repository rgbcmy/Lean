/**
 * IBKR Connection Configuration Form
 * IBKR 连接配置表单
 */

import React, { useState } from 'react';
import {
  Form,
  Input,
  InputNumber,
  Switch,
  Button,
  message,
  Alert,
} from 'antd';
import type { FormInstance } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { testIBKRConnection } from '../../api/settingsApi';

interface IBKRConnectionFormProps {
  form: FormInstance;
}

const IBKRConnectionForm: React.FC<IBKRConnectionFormProps> = ({ form }) => {
  const [testing, setTesting] = useState(false);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);

  // Test IBKR connection
  const handleTestConnection = async () => {
    try {
      const values = form.getFieldsValue(['ibkr']);
      const config = values.ibkr;
      
      setTesting(true);
      setTestResult(null);
      
      const result = await testIBKRConnection({
        host: config.host,
        port: config.port,
        clientId: config.clientId,
      });
      
      setTestResult(result);
      
      if (result.success) {
        message.success('连接测试成功');
      } else {
        message.error('连接测试失败');
      }
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || '连接测试失败';
      setTestResult({ success: false, message: errorMsg });
      message.error(errorMsg);
    } finally {
      setTesting(false);
    }
  };

  return (
    <div>
      <Alert
        message="IBKR 连接配置"
        description="配置 Interactive Brokers TWS 或 Gateway 连接参数。修改后需要重启应用才能生效。"
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      <Form.Item
        label="主机地址"
        name={['ibkr', 'host']}
        tooltip="TWS/Gateway 运行的主机地址，通常为 127.0.0.1"
        rules={[{ required: true, message: '请输入主机地址' }]}
      >
        <Input placeholder="127.0.0.1" />
      </Form.Item>

      <Form.Item
        label="端口"
        name={['ibkr', 'port']}
        tooltip="TWS: 7496(实盘)/7497(模拟), Gateway: 4001(实盘)/4002(模拟)"
        rules={[
          { required: true, message: '请输入端口' },
          { type: 'number', min: 1, max: 65535, message: '端口必须在1-65535之间' },
        ]}
      >
        <InputNumber
          style={{ width: '100%' }}
          placeholder="7497"
          min={1}
          max={65535}
        />
      </Form.Item>

      <Form.Item
        label="客户端 ID"
        name={['ibkr', 'clientId']}
        tooltip="唯一标识此连接的客户端 ID（0-32）"
        rules={[
          { required: true, message: '请输入客户端 ID' },
          { type: 'number', min: 0, max: 32, message: '客户端 ID 必须在0-32之间' },
        ]}
      >
        <InputNumber
          style={{ width: '100%' }}
          placeholder="0"
          min={0}
          max={32}
        />
      </Form.Item>

      <Form.Item
        label="账户 ID"
        name={['ibkr', 'accountId']}
        tooltip="您的 IBKR 账户 ID（例如：U1234567）"
        rules={[{ required: true, message: '请输入账户 ID' }]}
      >
        <Input placeholder="U1234567" />
      </Form.Item>

      <Form.Item
        label="使用纸面交易"
        name={['ibkr', 'usePaperTrading']}
        valuePropName="checked"
        tooltip="启用模拟交易模式（推荐用于测试）"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="自动重连"
        name={['ibkr', 'autoReconnect']}
        valuePropName="checked"
        tooltip="连接断开时自动尝试重连"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="重连间隔（秒）"
        name={['ibkr', 'reconnectIntervalSeconds']}
        tooltip="自动重连的时间间隔"
      >
        <InputNumber
          style={{ width: '100%' }}
          placeholder="5"
          min={1}
        />
      </Form.Item>

      <Form.Item
        label="心跳间隔（秒）"
        name={['ibkr', 'heartbeatIntervalSeconds']}
        tooltip="发送心跳包的时间间隔"
      >
        <InputNumber
          style={{ width: '100%' }}
          placeholder="10"
          min={1}
        />
      </Form.Item>

      <Form.Item>
        <Button
          onClick={handleTestConnection}
          loading={testing}
        >
          测试连接
        </Button>
      </Form.Item>

      {testResult && (
        <Alert
          message={testResult.success ? '连接成功' : '连接失败'}
          description={testResult.message}
          type={testResult.success ? 'success' : 'error'}
          icon={testResult.success ? <CheckCircleOutlined /> : <CloseCircleOutlined />}
          showIcon
        />
      )}
    </div>
  );
};

export default IBKRConnectionForm;
