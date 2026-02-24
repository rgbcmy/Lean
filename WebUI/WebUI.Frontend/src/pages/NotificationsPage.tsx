/**
 * Notifications Page Component
 * 通知中心页面组件
 */

import React from 'react';
import {
  Typography,
  Card,
  List,
  Badge,
  Button,
  Space,
  Tag,
  Empty,
  Popconfirm,
} from 'antd';
import {
  BellOutlined,
  CheckOutlined,
  DeleteOutlined,
  InfoCircleOutlined,
  CheckCircleOutlined,
  WarningOutlined,
  CloseCircleOutlined,
} from '@ant-design/icons';
import { useNotificationStore } from '../stores/notificationStore';
import type { Notification } from '../stores/notificationStore';
import './NotificationsPage.css';

const { Title, Text } = Typography;

/**
 * Get icon for notification type
 * 获取通知类型图标
 */
const getNotificationIcon = (type: Notification['type']) => {
  switch (type) {
    case 'success':
      return <CheckCircleOutlined style={{ color: '#52c41a', fontSize: 20 }} />;
    case 'warning':
      return <WarningOutlined style={{ color: '#faad14', fontSize: 20 }} />;
    case 'error':
      return <CloseCircleOutlined style={{ color: '#ff4d4f', fontSize: 20 }} />;
    case 'info':
    default:
      return <InfoCircleOutlined style={{ color: '#1890ff', fontSize: 20 }} />;
  }
};

/**
 * Get tag color for notification type
 * 获取通知类型标签颜色
 */
const getTagColor = (type: Notification['type']) => {
  switch (type) {
    case 'success':
      return 'success';
    case 'warning':
      return 'warning';
    case 'error':
      return 'error';
    case 'info':
    default:
      return 'processing';
  }
};

/**
 * Format notification timestamp
 * 格式化通知时间戳
 */
const formatTimestamp = (date: Date) => {
  const now = new Date();
  const diff = now.getTime() - new Date(date).getTime();
  const minutes = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days = Math.floor(diff / 86400000);

  if (minutes < 1) return '刚刚';
  if (minutes < 60) return `${minutes} 分钟前`;
  if (hours < 24) return `${hours} 小时前`;
  if (days < 7) return `${days} 天前`;

  return new Date(date).toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  });
};

/**
 * Notifications Page Component
 * 通知中心页面
 */
const NotificationsPage: React.FC = () => {
  const {
    notifications,
    unreadCount,
    markAsRead,
    markAllAsRead,
    removeNotification,
    clearAll,
  } = useNotificationStore();

  const handleMarkAsRead = (id: string) => {
    markAsRead(id);
  };

  const handleDelete = (id: string) => {
    removeNotification(id);
  };

  const handleMarkAllAsRead = () => {
    markAllAsRead();
  };

  const handleClearAll = () => {
    clearAll();
  };

  return (
    <div className="notifications-page">
      {/* Header */}
      <div className="notifications-header">
        <div>
          <Title level={2} style={{ marginBottom: 8 }}>
            <BellOutlined /> 通知中心
          </Title>
          <Text type="secondary">
            {unreadCount > 0 ? `您有 ${unreadCount} 条未读通知` : '暂无未读通知'}
          </Text>
        </div>
        <Space>
          {unreadCount > 0 && (
            <Button icon={<CheckOutlined />} onClick={handleMarkAllAsRead}>
              全部标记为已读
            </Button>
          )}
          {notifications.length > 0 && (
            <Popconfirm
              title="确定要清空所有通知吗？"
              onConfirm={handleClearAll}
              okText="确定"
              cancelText="取消"
            >
              <Button danger icon={<DeleteOutlined />}>
                清空全部
              </Button>
            </Popconfirm>
          )}
        </Space>
      </div>

      {/* Notifications List */}
      <Card className="notifications-card">
        {notifications.length === 0 ? (
          <Empty
            description="暂无通知"
            image={Empty.PRESENTED_IMAGE_SIMPLE}
          />
        ) : (
          <List
            itemLayout="horizontal"
            dataSource={notifications}
            renderItem={(notification) => (
              <List.Item
                key={notification.id}
                className={`notification-item ${!notification.read ? 'unread' : ''}`}
                actions={[
                  !notification.read && (
                    <Button
                      type="link"
                      size="small"
                      onClick={() => handleMarkAsRead(notification.id)}
                    >
                      标记已读
                    </Button>
                  ),
                  <Popconfirm
                    title="确定要删除这条通知吗？"
                    onConfirm={() => handleDelete(notification.id)}
                    okText="确定"
                    cancelText="取消"
                  >
                    <Button
                      type="link"
                      size="small"
                      danger
                    >
                      删除
                    </Button>
                  </Popconfirm>,
                ].filter(Boolean)}
              >
                <List.Item.Meta
                  avatar={
                    <div className="notification-icon">
                      {getNotificationIcon(notification.type)}
                    </div>
                  }
                  title={
                    <Space>
                      {!notification.read && (
                        <Badge status="processing" />
                      )}
                      <Text strong={!notification.read}>
                        {notification.title}
                      </Text>
                      <Tag color={getTagColor(notification.type)} style={{ marginLeft: 8 }}>
                        {notification.type === 'info' && '信息'}
                        {notification.type === 'success' && '成功'}
                        {notification.type === 'warning' && '警告'}
                        {notification.type === 'error' && '错误'}
                      </Tag>
                    </Space>
                  }
                  description={
                    <div className="notification-content">
                      <div className="notification-message">
                        {notification.message}
                      </div>
                      <Text type="secondary" className="notification-time">
                        {formatTimestamp(notification.timestamp)}
                      </Text>
                    </div>
                  }
                />
              </List.Item>
            )}
          />
        )}
      </Card>
    </div>
  );
};

export default NotificationsPage;
