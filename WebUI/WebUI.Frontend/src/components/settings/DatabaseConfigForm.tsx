/**
 * Database Configuration Form
 * 数据库配置表单
 */

import React, { useState } from 'react';
import {
  Form,
  Input,
  Select,
  Switch,
  InputNumber,
  Button,
  Space,
  Alert,
  message,
  Statistic,
  Card,
  Row,
  Col,
} from 'antd';
import type { FormInstance } from 'antd';
import { CheckCircleOutlined, CloseCircleOutlined } from '@ant-design/icons';
import { testDatabaseConnection, getDatabaseBackupStatus, triggerDatabaseBackup } from '../../api/settingsApi';

const { Option } = Select;

interface DatabaseConfigFormProps {
  form: FormInstance;
}

const DatabaseConfigForm: React.FC<DatabaseConfigFormProps> = ({ form }) => {
  const [testing, setTesting] = useState(false);
  const [testResult, setTestResult] = useState<{ success: boolean; message: string } | null>(null);
  const [backingUp, setBackingUp] = useState(false);
  const [backupStatus, setBackupStatus] = useState<{
    lastBackup: string | null;
    nextBackup: string | null;
    backupCount: number;
  } | null>(null);

  const provider = Form.useWatch(['database', 'provider'], form);

  // Test database connection
  const handleTestConnection = async () => {
    try {
      const values = form.getFieldsValue(['database']);
      const config = values.database;
      
      setTesting(true);
      setTestResult(null);
      
      const result = await testDatabaseConnection({
        provider: config.provider,
        connectionString: config.connectionString,
      });
      
      setTestResult(result);
      
      if (result.success) {
        message.success('数据库连接测试成功');
      } else {
        message.error('数据库连接测试失败');
      }
    } catch (error: any) {
      const errorMsg = error.response?.data?.message || '连接测试失败';
      setTestResult({ success: false, message: errorMsg });
      message.error(errorMsg);
    } finally {
      setTesting(false);
    }
  };

  // Load backup status
  const loadBackupStatus = async () => {
    try {
      const status = await getDatabaseBackupStatus();
      setBackupStatus(status);
    } catch (error: any) {
      message.error('加载备份状态失败');
    }
  };

  // Trigger manual backup
  const handleBackupNow = async () => {
    setBackingUp(true);
    try {
      const result = await triggerDatabaseBackup();
      if (result.success) {
        message.success('数据库备份已完成');
        loadBackupStatus();
      } else {
        message.error('数据库备份失败');
      }
    } catch (error: any) {
      message.error(error.response?.data?.message || '触发备份失败');
    } finally {
      setBackingUp(false);
    }
  };

  React.useEffect(() => {
    loadBackupStatus();
  }, []);

  return (
    <div>
      <Alert
        message="数据库配置"
        description="选择数据库提供程序并配置连接。PostgreSQL 推荐用于生产环境，SQLite 适合开发测试。"
        type="info"
        showIcon
        style={{ marginBottom: 24 }}
      />

      <Form.Item
        label="数据库提供程序"
        name={['database', 'provider']}
        tooltip="PostgreSQL 适合生产环境，SQLite 适合开发测试"
        rules={[{ required: true, message: '请选择数据库提供程序' }]}
      >
        <Select placeholder="选择数据库提供程序">
          <Option value="postgresql">PostgreSQL（推荐生产环境）</Option>
          <Option value="sqlite">SQLite（开发测试）</Option>
        </Select>
      </Form.Item>

      <Form.Item
        label="连接字符串"
        name={['database', 'connectionString']}
        tooltip={
          provider === 'postgresql'
            ? '例如：Host=localhost;Port=5432;Database=leanwebui;Username=postgres;Password=***'
            : '例如：Data Source=leanwebui.db'
        }
        rules={[{ required: true, message: '请输入连接字符串' }]}
      >
        <Input.TextArea
          rows={3}
          placeholder={
            provider === 'postgresql'
              ? 'Host=localhost;Port=5432;Database=leanwebui;Username=postgres;Password=your_password'
              : 'Data Source=leanwebui.db'
          }
        />
      </Form.Item>

      <Form.Item
        label="自动迁移"
        name={['database', 'autoMigrate']}
        valuePropName="checked"
        tooltip="启动时自动应用数据库迁移（推荐开发环境）"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="启用自动备份"
        name={['database', 'backupEnabled']}
        valuePropName="checked"
        tooltip="定期自动备份数据库"
      >
        <Switch />
      </Form.Item>

      <Form.Item
        label="备份间隔（小时）"
        name={['database', 'backupIntervalHours']}
        tooltip="自动备份的时间间隔"
      >
        <InputNumber
          style={{ width: '100%' }}
          placeholder="24"
          min={1}
        />
      </Form.Item>

      <Form.Item>
        <Space>
          <Button onClick={handleTestConnection} loading={testing}>
            测试连接
          </Button>
          <Button onClick={handleBackupNow} loading={backingUp}>
            立即备份
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
          style={{ marginBottom: 16 }}
        />
      )}

      {backupStatus && (
        <Card title="备份状态" style={{ marginTop: 16 }}>
          <Row gutter={16}>
            <Col span={8}>
              <Statistic
                title="上次备份"
                value={backupStatus.lastBackup ? new Date(backupStatus.lastBackup).toLocaleString('zh-CN') : '从未'}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="下次备份"
                value={backupStatus.nextBackup ? new Date(backupStatus.nextBackup).toLocaleString('zh-CN') : '未计划'}
              />
            </Col>
            <Col span={8}>
              <Statistic
                title="备份总数"
                value={backupStatus.backupCount}
              />
            </Col>
          </Row>
        </Card>
      )}
    </div>
  );
};

export default DatabaseConfigForm;
