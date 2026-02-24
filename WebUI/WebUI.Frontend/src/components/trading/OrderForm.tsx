/**
 * Order Form Component
 * 下单表单组件
 */

import React, { useState } from 'react';
import {
  Form,
  InputNumber,
  Button,
  Radio,
  Space,
  Alert,
  message,
  Modal,
  Typography,
  Statistic,
  Row,
  Col,
} from 'antd';
import {
  DollarOutlined,
  WarningOutlined,
} from '@ant-design/icons';
import { placeMarketOrder, placeLimitOrder, estimateOrderCost } from '../../api/ordersApi';
import type { OrderSide, OrderType } from '../../types/order';
import './OrderForm.css';

const { Text } = Typography;

interface OrderFormProps {
  symbol: string;
  currentPrice?: number;
  marketStatus?: 'open' | 'closed' | 'pre-market' | 'after-hours';
}

interface OrderFormValues {
  side: OrderSide;
  orderType: OrderType;
  quantity: number;
  limitPrice?: number;
}

/**
 * OrderForm Component
 * Provides interface for placing market and limit orders
 * 提供市价单和限价单下单界面
 */
const OrderForm: React.FC<OrderFormProps> = ({
  symbol,
  currentPrice,
  marketStatus,
}) => {
  const [form] = Form.useForm<OrderFormValues>();
  const [loading, setLoading] = useState(false);
  const [estimatedCost, setEstimatedCost] = useState<number | null>(null);
  const [confirmModalVisible, setConfirmModalVisible] = useState(false);
  const [orderValues, setOrderValues] = useState<OrderFormValues | null>(null);

  const orderType = Form.useWatch('orderType', form);

  /**
   * Estimate order cost
   * 估算订单成本
   */
  const handleEstimateCost = async () => {
    try {
      const values = await form.validateFields();
      const cost = await estimateOrderCost({
        symbol,
        side: values.side,
        orderType: values.orderType,
        quantity: values.quantity,
        limitPrice: values.limitPrice,
      });
      setEstimatedCost(cost.totalCost);
    } catch (error) {
      console.error('Cost estimation error:', error);
    }
  };

  /**
   * Handle form submission
   * 处理表单提交
   */
  const handleSubmit = async (values: OrderFormValues) => {
    setOrderValues(values);
    setConfirmModalVisible(true);
  };

  /**
   * Confirm and place order
   * 确认并提交订单
   */
  const handleConfirmOrder = async () => {
    if (!orderValues) return;

    setLoading(true);
    try {
      let result;
      
      if (orderValues.orderType === 'market') {
        result = await placeMarketOrder({
          symbol,
          side: orderValues.side,
          quantity: orderValues.quantity,
        });
      } else {
        result = await placeLimitOrder({
          symbol,
          side: orderValues.side,
          quantity: orderValues.quantity,
          limitPrice: orderValues.limitPrice!,
        });
      }

      message.success(`订单已提交：${result.orderId}`);
      setConfirmModalVisible(false);
      form.resetFields();
      setEstimatedCost(null);
    } catch (error: any) {
      console.error('Order placement error:', error);
      message.error(error.response?.data?.message || '下单失败，请稍后重试');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Validate quantity
   * 验证数量
   */
  const validateQuantity = (_: any, value: number) => {
    if (!value || value < 1) {
      return Promise.reject('数量必须至少为 1');
    }
    if (!Number.isInteger(value)) {
      return Promise.reject('不支持碎股交易，请输入整数');
    }
    return Promise.resolve();
  };

  /**
   * Validate limit price
   * 验证限价
   */
  const validateLimitPrice = (_: any, value: number) => {
    if (orderType === 'limit' && (!value || value <= 0)) {
      return Promise.reject('限价必须大于 0');
    }
    return Promise.resolve();
  };

  /**
   * Get order type description
   * 获取订单类型描述
   */
  const getOrderTypeDescription = () => {
    if (orderType === 'market') {
      return '市价单将以当前市场价格立即执行';
    }
    return '限价单将在价格达到指定限价时执行';
  };

  /**
   * Get market status warning
   * 获取市场状态警告
   */
  const getMarketStatusWarning = () => {
    if (marketStatus === 'closed') {
      return (
        <Alert
          type="warning"
          message="市场已关闭"
          description="订单将在下一交易日开盘时执行"
          showIcon
          icon={<WarningOutlined />}
        />
      );
    }
    if (marketStatus === 'pre-market') {
      return (
        <Alert
          type="info"
          message="盘前交易"
          description="当前处于盘前交易时段，流动性可能较低"
          showIcon
        />
      );
    }
    if (marketStatus === 'after-hours') {
      return (
        <Alert
          type="info"
          message="盘后交易"
          description="当前处于盘后交易时段，流动性可能较低"
          showIcon
        />
      );
    }
    return null;
  };

  return (
    <div className="order-form">
      {getMarketStatusWarning()}

      <Form
        form={form}
        layout="vertical"
        initialValues={{
          side: 'buy' as OrderSide,
          orderType: 'market' as OrderType,
          quantity: 1,
        }}
        onFinish={handleSubmit}
        className="order-form-content"
      >
        {/* Order Side */}
        <Form.Item name="side" label="交易方向">
          <Radio.Group buttonStyle="solid" size="large">
            <Radio.Button value="buy" className="buy-button">
              买入
            </Radio.Button>
            <Radio.Button value="sell" className="sell-button">
              卖出
            </Radio.Button>
          </Radio.Group>
        </Form.Item>

        {/* Order Type */}
        <Form.Item name="orderType" label="订单类型">
          <Radio.Group buttonStyle="solid">
            <Radio.Button value="market">市价单</Radio.Button>
            <Radio.Button value="limit">限价单</Radio.Button>
          </Radio.Group>
        </Form.Item>

        <Text type="secondary" style={{ display: 'block', marginBottom: 16 }}>
          {getOrderTypeDescription()}
        </Text>

        {/* Quantity */}
        <Form.Item
          name="quantity"
          label="数量（股）"
          rules={[{ validator: validateQuantity }]}
        >
          <InputNumber
            min={1}
            step={1}
            precision={0}
            style={{ width: '100%' }}
            size="large"
            onChange={handleEstimateCost}
          />
        </Form.Item>

        {/* Limit Price (only for limit orders) */}
        {orderType === 'limit' && (
          <Form.Item
            name="limitPrice"
            label="限价（美元）"
            rules={[{ validator: validateLimitPrice }]}
          >
            <InputNumber
              min={0.01}
              step={0.01}
              precision={2}
              style={{ width: '100%' }}
              size="large"
              prefix="$"
              onChange={handleEstimateCost}
            />
          </Form.Item>
        )}

        {/* Current Price Reference */}
        {currentPrice && (
          <div className="price-reference">
            <Text type="secondary">当前价格：</Text>
            <Text strong>${currentPrice.toFixed(2)}</Text>
          </div>
        )}

        {/* Estimated Cost */}
        {estimatedCost !== null && (
          <Alert
            type="info"
            message={
              <Space>
                <DollarOutlined />
                预估成本：${estimatedCost.toFixed(2)}
                {orderType === 'limit' && ' (最多)'}
              </Space>
            }
            style={{ marginBottom: 16 }}
          />
        )}

        {/* Submit Button */}
        <Form.Item>
          <Button
            type="primary"
            htmlType="submit"
            size="large"
            block
            loading={loading}
          >
            提交订单
          </Button>
        </Form.Item>
      </Form>

      {/* Confirmation Modal */}
      <Modal
        title="确认订单"
        open={confirmModalVisible}
        onOk={handleConfirmOrder}
        onCancel={() => setConfirmModalVisible(false)}
        okText="确认提交"
        cancelText="取消"
        confirmLoading={loading}
        width={500}
      >
        {orderValues && (
          <div className="order-confirmation">
            <Row gutter={[16, 16]}>
              <Col span={12}>
                <Statistic
                  title="股票代码"
                  value={symbol}
                  valueStyle={{ fontSize: 20 }}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="交易方向"
                  value={orderValues.side === 'buy' ? '买入' : '卖出'}
                  valueStyle={{
                    fontSize: 20,
                    color: orderValues.side === 'buy' ? '#52c41a' : '#ff4d4f',
                  }}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="订单类型"
                  value={orderValues.orderType === 'market' ? '市价单' : '限价单'}
                />
              </Col>
              <Col span={12}>
                <Statistic
                  title="数量"
                  value={orderValues.quantity}
                  suffix="股"
                />
              </Col>
              {orderValues.orderType === 'limit' && orderValues.limitPrice && (
                <Col span={12}>
                  <Statistic
                    title="限价"
                    value={orderValues.limitPrice}
                    precision={2}
                    prefix="$"
                  />
                </Col>
              )}
              {estimatedCost !== null && (
                <Col span={12}>
                  <Statistic
                    title="预估成本"
                    value={estimatedCost}
                    precision={2}
                    prefix="$"
                    valueStyle={{ color: '#1890ff' }}
                  />
                </Col>
              )}
            </Row>

            <Alert
              type="warning"
              message="请确认订单信息"
              description={
                orderValues.orderType === 'market'
                  ? '市价单将以当前市场价格立即执行，实际成交价可能与显示价格不同。'
                  : '限价单将在价格达到指定限价时执行，可能不会立即成交。'
              }
              showIcon
              style={{ marginTop: 16 }}
            />
          </div>
        )}
      </Modal>
    </div>
  );
};

export default OrderForm;
