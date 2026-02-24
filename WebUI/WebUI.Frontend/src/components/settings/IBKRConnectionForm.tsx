/**
 * IBKR Connection Configuration Form
 * IBKR 连接配置表单
 */

import React, { useState, useEffect } from 'react';
import {
  Form,
  Input,
  InputNumber,
  Switch,
  Button,
  message,
  Alert,
  Space,
  Tag,
} from 'antd';
import type { FormInstance } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined, ApiOutlined, DisconnectOutlined } from '@ant-design/icons';
import { testIBKRConnection } from '../../api/settingsApi';
import { getIbkrStatus, connectIbkr, disconnectIbkr } from '../../api/ibkrApi';
import { useIBKRStore } from '../../stores/ibkrStore';

interface IBKRConnectionFormProps {
  form: FormInstance;
}

const IBKRConnectionForm: React.FC<IBKRConnectionFormProps> = ({ form }) => {
  const [testing, setTesting] = useState(false);
  const [connecting, setConnecting] = useState(false);
  const [disconnecting, setDisconnecting] = useState(false);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);
  const { connectionStatus, setConnected, setDisconnected, setConnecting: storeSetConnecting } = useIBKRStore();

  // Sync current status from backend on mount
  useEffect(() => {
    getIbkrStatus().then(state => {
      if (state.status === 'Connected') setConnected(state.accountId || '');
      else if (state.status === 'Connecting' || state.status === 'Reconnecting') storeSetConnecting();
      else setDisconnected(state.lastError);
    }).catch(() => {});
  }, []);

  const statusTag = () => {
    switch (connectionStatus) {
      case 'connected':   return <Tag color="success" icon={<CheckCircleOutlined />}>已连接</Tag>;
      case 'connecting':  return <Tag color="processing">连接中...</Tag>;
      default:            return <Tag color="error" icon={<CloseCircleOutlined />}>未连接</Tag>;
    }
  };

  // Connect to IBKR
  const handleConnect = async () => {
    try {
      const values = form.getFieldsValue(['ibkr']);
      const cfg = values.ibkr;
      setConnecting(true);
      storeSetConnecting();
      const usePaper: boolean = cfg.usePaperTrading ?? true;
      const state = await connectIbkr({
        host: cfg.host,
        port: cfg.port,
        clientId: cfg.clientId,
        accountId: cfg.accountId || '',
        accountType: usePaper ? 'Paper' : 'Live',
        enableAutoReconnect: cfg.autoReconnect ?? true,
      });
      if (state.status === 'Connected') {
        setConnected(state.accountId || '');
        message.success(`已成功连接到 IBKR（账户：${state.accountId}）`);
      } else {
        setDisconnected(state.lastError);
        message.error(`连接失败：${state.lastError || '未知错误'}`);
      }
    } catch (error: any) {
      const msg = error.response?.data?.message || '连接失败';
      setDisconnected(msg);
      message.error(msg);
    } finally {
      setConnecting(false);
    }
  };

  // Disconnect from IBKR
  const handleDisconnect = async () => {
    try {
      setDisconnecting(true);
      await disconnectIbkr();
      setDisconnected();
      message.success('已断开 IBKR 连接');
    } catch (error: any) {
      message.error(error.response?.data?.message || '断开连接失败');
    } finally {
      setDisconnecting(false);
    }
  };

  // Test TCP reachability (does NOT create a persistent connection)
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
        message.success('TCP 端口可达');
      } else {
        message.error('TCP 端口不可达');
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
        description="配置 Interactive Brokers TWS 或 Gateway 连接参数。保存后将写入配置文件，下次连接时自动使用新配置。"
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

      <Form.Item label="当前状态">
        {statusTag()}
      </Form.Item>

      <Form.Item>
        <Space wrap>
          {connectionStatus !== 'connected' ? (
            <Button
              type="primary"
              icon={<ApiOutlined />}
              onClick={handleConnect}
              loading={connecting}
            >
              连接 IBKR
            </Button>
          ) : (
            <Button
              danger
              icon={<DisconnectOutlined />}
              onClick={handleDisconnect}
              loading={disconnecting}
            >
              断开连接
            </Button>
          )}
          <Button
            onClick={handleTestConnection}
            loading={testing}
          >
            测试 TCP 端口
          </Button>
        </Space>
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
