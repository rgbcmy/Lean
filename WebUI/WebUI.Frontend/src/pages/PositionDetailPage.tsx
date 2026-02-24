/**
 * Position Detail Page
 * 持仓详情页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Descriptions,
  Table,
  Space,
  Button,
  Modal,
  message,
  Statistic,
  Row,
  Col,
  Tag,
  Typography,
  Spin,
} from 'antd';
import {
  ArrowLeftOutlined,
  StopOutlined,
  ExclamationCircleOutlined,
  ReloadOutlined,
  RiseOutlined,
  FallOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useParams, useNavigate } from 'react-router-dom';
import { getPositionDetail, closePosition } from '../api/positionsApi';
import { signalRService } from '../services/signalrService';
import type { PositionDetail, Transaction } from '../types/position';
import './PositionDetailPage.css';

const { Text, Title } = Typography;

/**
 * PositionDetailPage Component
 * Displays detailed information about a specific position
 * 显示特定持仓的详细信息
 */
const PositionDetailPage: React.FC = () => {
  const { symbol } = useParams<{ symbol: string }>();
  const navigate = useNavigate();
  const [position, setPosition] = useState<PositionDetail | null>(null);
  const [loading, setLoading] = useState(false);

  /**
   * Load position detail from API
   * 从 API 加载持仓详情
   */
  const loadPositionDetail = async () => {
    if (!symbol) return;

    setLoading(true);
    try {
      const result = await getPositionDetail(symbol);
      setPosition(result);
    } catch (error) {
      console.error('Failed to load position detail:', error);
      message.error('加载持仓详情失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle close position
   * 处理平仓
   */
  const handleClosePosition = (quantity?: number) => {
    if (!position) return;

    const isPartial = quantity && quantity < position.quantity;
    const quantityText = isPartial ? `${quantity} 股` : '全部';

    Modal.confirm({
      title: '确认平仓',
      icon: <ExclamationCircleOutlined />,
      content: `您确定要以市价卖出${quantityText} ${position.symbol} 吗？`,
      okText: '确认',
      cancelText: '取消',
      onOk: async () => {
        try {
          await closePosition(position.symbol, quantity);
          message.success('平仓订单已提交');
          navigate('/positions');
        } catch (error) {
          console.error('Failed to close position:', error);
          message.error('平仓失败');
        }
      },
    });
  };

  /**
   * Load data on mount
   * 组件挂载时加载数据
   */
  useEffect(() => {
    loadPositionDetail();
  }, [symbol]);

  /**
   * Setup SignalR for real-time price updates
   * 设置 SignalR 实时价格更新
   */
  useEffect(() => {
    if (!symbol) return;

    const handleMarketDataUpdate = (data: { symbol: string; price: number }) => {
      if (data.symbol === symbol && position) {
        const newMarketValue = data.price * position.quantity;
        const newUnrealizedPnL = newMarketValue - position.averageCost * position.quantity;
        const newUnrealizedPnLPercent =
          (newUnrealizedPnL / (position.averageCost * position.quantity)) * 100;

        setPosition({
          ...position,
          currentPrice: data.price,
          marketValue: newMarketValue,
          unrealizedPnL: newUnrealizedPnL,
          unrealizedPnLPercent: newUnrealizedPnLPercent,
        });
      }
    };

    signalRService.on('MarketData', handleMarketDataUpdate);

    return () => {
      signalRService.off('MarketData', handleMarketDataUpdate);
    };
  }, [symbol, position]);

  /**
   * Buy history table columns
   * 买入历史表格列
   */
  const buyColumns: ColumnsType<Transaction> = [
    {
      title: '日期',
      dataIndex: 'date',
      key: 'date',
      render: (date: Date) => new Date(date).toLocaleDateString('zh-CN'),
    },
    {
      title: '数量',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'right',
      render: (quantity: number) => quantity.toLocaleString(),
    },
    {
      title: '价格',
      dataIndex: 'price',
      key: 'price',
      align: 'right',
      render: (price: number) => `$${price.toFixed(2)}`,
    },
    {
      title: '手续费',
      dataIndex: 'commission',
      key: 'commission',
      align: 'right',
      render: (commission: number) => `$${commission.toFixed(2)}`,
    },
    {
      title: '总成本',
      key: 'totalCost',
      align: 'right',
      render: (_: any, record: Transaction) =>
        `$${(record.price * record.quantity + record.commission).toFixed(2)}`,
    },
  ];

  /**
   * Sell history table columns
   * 卖出历史表格列
   */
  const sellColumns: ColumnsType<Transaction> = [
    {
      title: '日期',
      dataIndex: 'date',
      key: 'date',
      render: (date: Date) => new Date(date).toLocaleDateString('zh-CN'),
    },
    {
      title: '数量',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'right',
      render: (quantity: number) => quantity.toLocaleString(),
    },
    {
      title: '价格',
      dataIndex: 'price',
      key: 'price',
      align: 'right',
      render: (price: number) => `$${price.toFixed(2)}`,
    },
    {
      title: '手续费',
      dataIndex: 'commission',
      key: 'commission',
      align: 'right',
      render: (commission: number) => `$${commission.toFixed(2)}`,
    },
    {
      title: '盈亏',
      dataIndex: 'pnL',
      key: 'pnL',
      align: 'right',
      render: (pnL: number) => (
        <Text style={{ color: pnL >= 0 ? '#3f8600' : '#cf1322' }}>
          {pnL >= 0 ? '+' : ''}${pnL.toFixed(2)}
        </Text>
      ),
    },
  ];

  if (loading || !position) {
    return (
      <div className="position-detail-page">
        <Spin size="large" tip="加载中..." />
      </div>
    );
  }

  return (
    <div className="position-detail-page">
      {/* Header */}
      <div className="page-header">
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/positions')}>
            返回
          </Button>
          <Title level={3} style={{ margin: 0 }}>
            {position.symbol} 持仓详情
          </Title>
        </Space>
        <Space>
          <Button icon={<ReloadOutlined />} onClick={loadPositionDetail}>
            刷新
          </Button>
          <Button
            type="primary"
            danger
            icon={<StopOutlined />}
            onClick={() => handleClosePosition()}
          >
            全部平仓
          </Button>
        </Space>
      </div>

      {/* Summary Statistics */}
      <Row gutter={16} style={{ marginBottom: 16 }}>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="持仓数量"
              value={position.quantity}
              suffix="股"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="当前价格"
              value={position.currentPrice}
              precision={2}
              prefix="$"
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="市值"
              value={position.marketValue}
              precision={2}
              prefix="$"
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={12} md={6}>
          <Card>
            <Statistic
              title="未实现盈亏"
              value={Math.abs(position.unrealizedPnL)}
              precision={2}
              prefix={position.unrealizedPnL >= 0 ? '+$' : '-$'}
              valueStyle={{
                color: position.unrealizedPnL >= 0 ? '#3f8600' : '#cf1322',
              }}
              suffix={
                <span style={{ fontSize: 14 }}>
                  ({position.unrealizedPnLPercent >= 0 ? '+' : ''}
                  {position.unrealizedPnLPercent.toFixed(2)}%)
                </span>
              }
            />
          </Card>
        </Col>
      </Row>

      {/* Position Details */}
      <Card title="持仓详情" style={{ marginBottom: 16 }}>
        <Descriptions bordered column={{ xs: 1, sm: 2, md: 3 }}>
          <Descriptions.Item label="股票代码">
            <Text strong>{position.symbol}</Text>
          </Descriptions.Item>
          <Descriptions.Item label="平均成本">
            ${position.averageCost.toFixed(2)}
          </Descriptions.Item>
          <Descriptions.Item label="持有天数">
            {position.holdingDays} 天
          </Descriptions.Item>
          <Descriptions.Item label="首次购买日期">
            {new Date(position.firstPurchaseDate).toLocaleDateString('zh-CN')}
          </Descriptions.Item>
          {position.sector && (
            <Descriptions.Item label="行业">
              <Tag color="blue">{position.sector}</Tag>
            </Descriptions.Item>
          )}
          {position.realizedPnL !== undefined && (
            <Descriptions.Item label="已实现盈亏">
              <Text style={{ color: position.realizedPnL >= 0 ? '#3f8600' : '#cf1322' }}>
                {position.realizedPnL >= 0 ? '+' : ''}${position.realizedPnL.toFixed(2)}
              </Text>
            </Descriptions.Item>
          )}
        </Descriptions>
      </Card>

      {/* Buy History */}
      <Card title="买入历史" style={{ marginBottom: 16 }}>
        <Table
          columns={buyColumns}
          dataSource={position.buyHistory}
          rowKey="transactionId"
          pagination={false}
          locale={{
            emptyText: '暂无买入记录',
          }}
        />
      </Card>

      {/* Sell History */}
      {position.sellHistory && position.sellHistory.length > 0 && (
        <Card title="卖出历史" style={{ marginBottom: 16 }}>
          <Table
            columns={sellColumns}
            dataSource={position.sellHistory}
            rowKey="transactionId"
            pagination={false}
            locale={{
              emptyText: '暂无卖出记录',
            }}
          />
        </Card>
      )}
    </div>
  );
};

export default PositionDetailPage;
