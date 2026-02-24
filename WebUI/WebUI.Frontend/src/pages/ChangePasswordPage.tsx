/**
 * Change Password Page Component
 * 修改密码页面组件
 */

import React from 'react';
import { 
  Form, 
  Input, 
  Button, 
  Card, 
  Typography, 
  message,
  Space,
} from 'antd';
import { LockOutlined } from '@ant-design/icons';
import { useNavigate } from 'react-router-dom';
import { changePassword } from '../api/authApi';
import { useAuthStore } from '../stores';

const { Title, Text } = Typography;

interface ChangePasswordFormValues {
  oldPassword: string;
  newPassword: string;
  confirmPassword: string;
}

const ChangePasswordPage: React.FC = () => {
  const navigate = useNavigate();
  const clearAuth = useAuthStore((state) => state.clearAuth);
  const [form] = Form.useForm();
  const [loading, setLoading] = React.useState(false);
  
  const onFinish = async (values: ChangePasswordFormValues) => {
    setLoading(true);
    
    try {
      // Call change password API
      await changePassword({
        oldPassword: values.oldPassword,
        newPassword: values.newPassword,
      });
      
      // Show success message
      message.success('密码修改成功！请重新登录');
      
      // Wait 1 second then logout and redirect to login
      setTimeout(() => {
        clearAuth();
        navigate('/login', { replace: true });
      }, 1000);
    } catch (error: any) {
      console.error('Change password failed:', error);
      
      // Show error message
      const errorMessage = error.response?.data?.message || 
                           error.message || 
                           '修改密码失败，请检查旧密码是否正确';
      message.error(errorMessage);
    } finally {
      setLoading(false);
    }
  };
  
  const validatePassword = (_: any, value: string) => {
    if (!value) {
      return Promise.reject(new Error('请输入新密码'));
    }
    
    // Password strength validation
    const minLength = 8;
    const hasUpperCase = /[A-Z]/.test(value);
    const hasLowerCase = /[a-z]/.test(value);
    const hasNumber = /[0-9]/.test(value);
    const hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(value);
    
    if (value.length < minLength) {
      return Promise.reject(new Error(`密码至少需要 ${minLength} 个字符`));
    }
    
    if (!hasUpperCase) {
      return Promise.reject(new Error('密码必须包含至少一个大写字母'));
    }
    
    if (!hasLowerCase) {
      return Promise.reject(new Error('密码必须包含至少一个小写字母'));
    }
    
    if (!hasNumber) {
      return Promise.reject(new Error('密码必须包含至少一个数字'));
    }
    
    if (!hasSpecialChar) {
      return Promise.reject(new Error('密码必须包含至少一个特殊字符'));
    }
    
    return Promise.resolve();
  };
  
  const validateConfirmPassword = (_: any, value: string) => {
    const newPassword = form.getFieldValue('newPassword');
    
    if (!value) {
      return Promise.reject(new Error('请确认新密码'));
    }
    
    if (value !== newPassword) {
      return Promise.reject(new Error('两次输入的密码不一致'));
    }
    
    return Promise.resolve();
  };
  
  return (
    <div style={{ 
      display: 'flex', 
      justifyContent: 'center', 
      alignItems: 'center', 
      minHeight: '100vh',
      padding: '20px',
      background: '#f0f2f5',
    }}>
      <Card style={{ width: '100%', maxWidth: 500 }}>
        <Title level={2} style={{ textAlign: 'center', marginBottom: 8 }}>
          修改密码
        </Title>
        <Text type="secondary" style={{ display: 'block', textAlign: 'center', marginBottom: 32 }}>
          修改密码后需要重新登录
        </Text>
        
        <Form
          form={form}
          name="changePassword"
          onFinish={onFinish}
          layout="vertical"
          size="large"
          autoComplete="off"
        >
          <Form.Item
            name="oldPassword"
            label="旧密码"
            rules={[
              { required: true, message: '请输入旧密码' },
            ]}
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="请输入旧密码"
              autoComplete="current-password"
            />
          </Form.Item>
          
          <Form.Item
            name="newPassword"
            label="新密码"
            rules={[
              { validator: validatePassword },
            ]}
            hasFeedback
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="请输入新密码"
              autoComplete="new-password"
            />
          </Form.Item>
          
          <Form.Item
            name="confirmPassword"
            label="确认新密码"
            dependencies={['newPassword']}
            rules={[
              { validator: validateConfirmPassword },
            ]}
            hasFeedback
          >
            <Input.Password
              prefix={<LockOutlined />}
              placeholder="请再次输入新密码"
              autoComplete="new-password"
            />
          </Form.Item>
          
          <Form.Item style={{ marginBottom: 8 }}>
            <Space direction="vertical" style={{ width: '100%' }}>
              <Text type="secondary" style={{ fontSize: 12 }}>
                密码要求：
              </Text>
              <ul style={{ margin: 0, paddingLeft: 20, fontSize: 12, color: '#8c8c8c' }}>
                <li>至少 8 个字符</li>
                <li>包含大写字母、小写字母、数字和特殊字符</li>
              </ul>
            </Space>
          </Form.Item>
          
          <Form.Item style={{ marginBottom: 0 }}>
            <Space style={{ width: '100%', justifyContent: 'space-between' }}>
              <Button onClick={() => navigate(-1)}>
                取消
              </Button>
              <Button 
                type="primary" 
                htmlType="submit" 
                loading={loading}
              >
                修改密码
              </Button>
            </Space>
          </Form.Item>
        </Form>
      </Card>
    </div>
  );
};

export default ChangePasswordPage;
