/**
 * Settings Page
 * 系统设置页面
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
} from 'antd';
import { SaveOutlined, ReloadOutlined } from '@ant-design/icons';
import { getSettings, updateSettings } from '../api/settingsApi';
import type { SystemSettings, SettingsUpdateRequest } from '../types/settings';
import { useThemeStore } from '../stores';
import IBKRConnectionForm from '../components/settings/IBKRConnectionForm';
import DatabaseConfigForm from '../components/settings/DatabaseConfigForm';
import ThemeLanguageForm from '../components/settings/ThemeLanguageForm';
import NotificationPreferencesForm from '../components/settings/NotificationPreferencesForm';
import './SettingsPage.css';

const { Title, Text } = Typography;
const { TabPane } = Tabs;

const SettingsPage: React.FC = () => {
  const [form] = Form.useForm();
  const [loading, setLoading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [settings, setSettings] = useState<SystemSettings | null>(null);
  const { setTheme } = useThemeStore();

  // Load settings
  const loadSettings = async () => {
    setLoading(true);
    try {
      const data = await getSettings();
      setSettings(data);
      form.setFieldsValue(data);
    } catch (error: any) {
      message.error(error.response?.data?.message || '加载设置失败');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadSettings();
  }, []);

  // Save settings
  const handleSave = async () => {
    try {
      const values = await form.validateFields();
      setSaving(true);
      
      const updateRequest: SettingsUpdateRequest = {
        ibkr: values.ibkr,
        database: values.database,
        theme: values.theme,
        language: values.language,
        notifications: values.notifications,
      };

      const updatedSettings = await updateSettings(updateRequest);
      setSettings(updatedSettings);

      // Apply theme immediately to Ant Design ThemeProvider
      if (values.theme?.mode) {
        setTheme(values.theme.mode as 'light' | 'dark');
      }

      message.success('设置已保存');
    } catch (error: any) {
      if (error.errorFields) {
        message.error('请检查表单中的错误');
      } else {
        message.error(error.response?.data?.message || '保存设置失败');
      }
    } finally {
      setSaving(false);
    }
  };

  // Reset form
  const handleReset = () => {
    if (settings) {
      form.setFieldsValue(settings);
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
    <div className="settings-page">
      <div className="page-header">
        <div>
          <Title level={2}>系统设置</Title>
          <Text type="secondary">
            配置 IBKR 连接、数据库、主题和通知偏好
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
            保存设置
          </Button>
        </Space>
      </div>

      <Form
        form={form}
        layout="vertical"
        initialValues={settings || undefined}
      >
        <Tabs defaultActiveKey="ibkr">
          <TabPane tab="IBKR 连接" key="ibkr">
            <Card>
              <IBKRConnectionForm form={form} />
            </Card>
          </TabPane>

          <TabPane tab="数据库配置" key="database">
            <Card>
              <DatabaseConfigForm form={form} />
            </Card>
          </TabPane>

          <TabPane tab="主题与语言" key="themeLanguage">
            <Card>
              <ThemeLanguageForm form={form} />
            </Card>
          </TabPane>

          <TabPane tab="通知偏好" key="notifications">
            <Card>
              <NotificationPreferencesForm form={form} />
            </Card>
          </TabPane>
        </Tabs>
      </Form>
    </div>
  );
};

export default SettingsPage;
