/**
 * Orders Page
 * 订单列表页面
 */

import React, { useState, useEffect } from 'react';
import {
  Table,
  Card,
  Tag,
  Space,
  Button,
  Input,
  Select,
  DatePicker,
  Modal,
  message,
  Tooltip,
  Typography,
} from 'antd';
import {
  ReloadOutlined,
  StopOutlined,
  DeleteOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
  FilterOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import dayjs from 'dayjs';
import { getOrders, cancelOrder, cancelAllOrders } from '../api/ordersApi';
import { signalRService } from '../services/signalrService';
import type { Order, OrderStatus } from '../types/order';
import './OrdersPage.css';

const { RangePicker } = DatePicker;
const { Text } = Typography;

/**
 * OrdersPage Component
 * Displays list of orders with filtering and sorting
 * 显示订单列表，支持筛选和排序
 */
const OrdersPage: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(false);
  const [total, setTotal] = useState(0);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(20);

  // Filters
  const [symbolFilter, setSymbolFilter] = useState<string>('');
  const [statusFilter, setStatusFilter] = useState<string | undefined>(undefined);
  const [dateRange, setDateRange] = useState<[dayjs.Dayjs | null, dayjs.Dayjs | null] | null>(null);

  // const connection = useConnectionStore((state) => state.connection);

  /**
   * Load orders from API
   * 从 API 加载订单
   */
  const loadOrders = async () => {
    setLoading(true);
    try {
      const params: any = {
        page: currentPage,
        pageSize,
      };

      if (symbolFilter) {
        params.symbol = symbolFilter;
      }
      if (statusFilter) {
        params.status = statusFilter;
      }
      if (dateRange && dateRange[0] && dateRange[1]) {
        params.startDate = dateRange[0].format('YYYY-MM-DD');
        params.endDate = dateRange[1].format('YYYY-MM-DD');
      }

      const result = await getOrders(params);
      setOrders(result.orders);
      setTotal(result.total);
    } catch (error) {
      console.error('Failed to load orders:', error);
      message.error('加载订单失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle cancel order
   * 处理取消订单
   */
  const handleCancelOrder = (orderId: string) => {
    Modal.confirm({
      title: '确认取消订单',
      icon: <ExclamationCircleOutlined />,
      content: '您确定要取消此订单吗？',
      okText: '确认',
      cancelText: '取消',
      onOk: async () => {
        try {
          await cancelOrder(orderId);
          message.success('订单已取消');
          loadOrders();
        } catch (error: any) {
          console.error('Failed to cancel order:', error);
          message.error(error.response?.data?.message || '取消订单失败');
        }
      },
    });
  };

  /**
   * Handle cancel all orders
   * 处理取消所有订单
   */
  const handleCancelAllOrders = () => {
    Modal.confirm({
      title: '确认取消所有订单',
      icon: <ExclamationCircleOutlined />,
      content: '您确定要取消所有活跃订单吗？此操作不可撤销。',
      okText: '确认',
      cancelText: '取消',
      okButtonProps: { danger: true },
      onOk: async () => {
        try {
          const result = await cancelAllOrders();
          message.success(`已取消 ${result.cancelledCount} 个订单`);
          loadOrders();
        } catch (error: any) {
          console.error('Failed to cancel all orders:', error);
          message.error(error.response?.data?.message || '取消订单失败');
        }
      },
    });
  };

  /**
   * Get status tag
   * 获取状态标签
   */
  const getStatusTag = (status: OrderStatus) => {
    const statusConfig: Record<OrderStatus, { color: string; text: string }> = {
      submitted: { color: 'blue', text: '已提交' },
      partiallyFilled: { color: 'orange', text: '部分成交' },
      filled: { color: 'green', text: '完全成交' },
      cancelled: { color: 'default', text: '已取消' },
      rejected: { color: 'red', text: '已拒绝' },
    };

    const config = statusConfig[status];
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  /**
   * Get side tag
   * 获取交易方向标签
   */
  const getSideTag = (side: 'buy' | 'sell') => {
    return (
      <Tag color={side === 'buy' ? 'green' : 'red'}>
        {side === 'buy' ? '买入' : '卖出'}
      </Tag>
    );
  };

  /**
   * Get order type tag
   * 获取订单类型标签
   */
  const getOrderTypeTag = (orderType: 'market' | 'limit') => {
    return (
      <Tag color={orderType === 'market' ? 'blue' : 'purple'}>
        {orderType === 'market' ? '市价' : '限价'}
      </Tag>
    );
  };

  /**
   * Table columns definition
   * 表格列定义
   */
  const columns: ColumnsType<Order> = [
    {
      title: '订单ID',
      dataIndex: 'orderId',
      key: 'orderId',
      width: 180,
      render: (id: string) => (
        <Text copyable={{ text: id }} ellipsis={{ tooltip: id }}>
          {id.slice(0, 8)}...
        </Text>
      ),
    },
    {
      title: '股票代码',
      dataIndex: 'symbol',
      key: 'symbol',
      width: 120,
      render: (symbol: string) => <Text strong>{symbol}</Text>,
    },
    {
      title: '交易方向',
      dataIndex: 'side',
      key: 'side',
      width: 100,
      render: (side: 'buy' | 'sell') => getSideTag(side),
    },
    {
      title: '订单类型',
      dataIndex: 'orderType',
      key: 'orderType',
      width: 100,
      render: (type: 'market' | 'limit') => getOrderTypeTag(type),
    },
    {
      title: '数量',
      key: 'quantity',
      width: 150,
      render: (_, record) => (
        <div>
          <div>
            {record.filledQuantity} / {record.quantity}
          </div>
          <Text type="secondary" style={{ fontSize: 12 }}>
            {record.remainingQuantity > 0 && `剩余 ${record.remainingQuantity}`}
          </Text>
        </div>
      ),
    },
    {
      title: '价格',
      key: 'price',
      width: 120,
      render: (_, record) => (
        <div>
          {record.orderType === 'limit' && record.limitPrice && (
            <div>限价: ${record.limitPrice.toFixed(2)}</div>
          )}
          {record.avgFillPrice && (
            <div>
              <Text type="secondary">成交均价:</Text> ${record.avgFillPrice.toFixed(2)}
            </div>
          )}
          {!record.avgFillPrice && record.orderType === 'market' && (
            <Text type="secondary">市价</Text>
          )}
        </div>
      ),
    },
    {
      title: '状态',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      render: (status: OrderStatus, record) => (
        <div>
          {getStatusTag(status)}
          {status === 'rejected' && record.rejectionReason && (
            <Tooltip title={record.rejectionReason}>
              <ExclamationCircleOutlined style={{ color: '#ff4d4f', marginLeft: 4 }} />
            </Tooltip>
          )}
        </div>
      ),
    },
    {
      title: '提交时间',
      dataIndex: 'submittedAt',
      key: 'submittedAt',
      width: 180,
      render: (date: Date) => dayjs(date).format('YYYY-MM-DD HH:mm:ss'),
      sorter: (a, b) => new Date(a.submittedAt).getTime() - new Date(b.submittedAt).getTime(),
      defaultSortOrder: 'descend',
    },
    {
      title: '操作',
      key: 'actions',
      width: 100,
      fixed: 'right',
      render: (_, record) => (
        <Space>
          {(record.status === 'submitted' || record.status === 'partiallyFilled') && (
            <Button
              type="link"
              danger
              size="small"
              icon={<StopOutlined />}
              onClick={() => handleCancelOrder(record.orderId)}
            >
              取消
            </Button>
          )}
        </Space>
      ),
    },
  ];

  /**
   * Subscribe to real-time order updates
   * 订阅实时订单更新
   */
  useEffect(() => {
    let subscribed = false;

    const setupSubscription = async () => {
      try {
        await signalRService.subscribeOrderUpdates((updatedOrder: Order) => {
          console.log('Order update received:', updatedOrder);
          
          // Update the order in the list
          setOrders((prevOrders) => {
            const existingIndex = prevOrders.findIndex((o) => o.orderId === updatedOrder.orderId);
            
            if (existingIndex >= 0) {
              // Update existing order
              const newOrders = [...prevOrders];
              newOrders[existingIndex] = updatedOrder;
              return newOrders;
            } else {
              // Add new order at the beginning
              return [updatedOrder, ...prevOrders];
            }
          });
          
          // Show notification for significant status changes
          if (updatedOrder.status === 'filled') {
            message.success(`订单已完全成交: ${updatedOrder.symbol}`);
          } else if (updatedOrder.status === 'cancelled') {
            message.info(`订单已取消: ${updatedOrder.symbol}`);
          } else if (updatedOrder.status === 'rejected') {
            message.error(`订单被拒绝: ${updatedOrder.symbol} - ${updatedOrder.rejectionReason || '未知原因'}`);
          }
        });
        
        subscribed = true;
        console.log('Subscribed to real-time order updates');
      } catch (error) {
        console.error('Failed to subscribe to order updates:', error);
      }
    };

    setupSubscription();

    return () => {
      if (subscribed) {
        console.log('Cleaning up order updates subscription');
        // Note: The SignalR service manages the actual connection lifecycle
        // Individual subscriptions are cleaned up when the hub is disconnected
      }
    };
  }, []);

  /**
   * Load orders on mount and when filters change
   * 初始加载和筛选条件变化时加载订单
   */
  useEffect(() => {
    loadOrders();
  }, [currentPage, pageSize, symbolFilter, statusFilter, dateRange]);

  return (
    <div className="orders-page">
      <Card
        title="订单管理"
        extra={
          <Space>
            <Button
              icon={<ReloadOutlined />}
              onClick={loadOrders}
              loading={loading}
            >
              刷新
            </Button>
            <Button
              icon={<DeleteOutlined />}
              danger
              onClick={handleCancelAllOrders}
            >
              取消所有订单
            </Button>
          </Space>
        }
      >
        {/* Filters */}
        <div className="orders-filters">
          <Space wrap>
            <Input
              placeholder="搜索股票代码"
              prefix={<SearchOutlined />}
              value={symbolFilter}
              onChange={(e) => setSymbolFilter(e.target.value)}
              onPressEnter={loadOrders}
              style={{ width: 200 }}
              allowClear
            />
            <Select
              placeholder="订单状态"
              value={statusFilter}
              onChange={setStatusFilter}
              style={{ width: 150 }}
              allowClear
            >
              <Select.Option value="submitted">已提交</Select.Option>
              <Select.Option value="partiallyFilled">部分成交</Select.Option>
              <Select.Option value="filled">完全成交</Select.Option>
              <Select.Option value="cancelled">已取消</Select.Option>
              <Select.Option value="rejected">已拒绝</Select.Option>
            </Select>
            <RangePicker
              value={dateRange}
              onChange={setDateRange}
              format="YYYY-MM-DD"
              placeholder={['开始日期', '结束日期']}
            />
            <Button
              type="primary"
              icon={<FilterOutlined />}
              onClick={loadOrders}
            >
              筛选
            </Button>
          </Space>
        </div>

        {/* Orders Table */}
        <Table
          columns={columns}
          dataSource={orders}
          rowKey="orderId"
          loading={loading}
          pagination={{
            current: currentPage,
            pageSize,
            total,
            showSizeChanger: true,
            showQuickJumper: true,
            showTotal: (total) => `共 ${total} 笔订单`,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size);
            },
          }}
          scroll={{ x: 1200 }}
        />
      </Card>
    </div>
  );
};

export default OrdersPage;
