/**
 * ETF Trading Page
 * ETF 交易页面
 */

import React, { useState, useEffect } from 'react';
import {
  Card,
  Row,
  Col,
  Statistic,
  Typography,
  Tag,
  Space,
  Spin,
  Alert,
  message,
} from 'antd';
import {
  ArrowUpOutlined,
  ArrowDownOutlined,
  DollarCircleOutlined,
} from '@ant-design/icons';
import StockSearch from '../components/trading/StockSearch';
import OrderForm from '../components/trading/OrderForm';
import { getStockQuote, unsubscribeFromStock } from '../api/stocksApi';
// import { useConnectionStore } from '../stores';
import type { Stock, StockQuote } from '../types/stock';
import './ETFTradingPage.css';

const { Title, Text } = Typography;

/**
 * ETFTradingPage Component
 * Provides interface for trading ETFs
 * 提供 ETF 交易界面
 */
const ETFTradingPage: React.FC = () => {
  const [loading, setLoading] = useState(false);
  const [quote, setQuote] = useState<StockQuote | null>(null);
  const [selectedSymbol, setSelectedSymbol] = useState<string>('');

  // const connection = useConnectionStore((state) => state.connection);

  /**
   * Handle ETF selection
   * 处理 ETF 选择
   */
  const handleSelectETF = async (stock: Stock) => {
    const symbol = stock.symbol;
    setSelectedSymbol(symbol);
    setLoading(true);

    try {
      // Unsubscribe from previous stock
      // if (quote && connection) {
      //   await unsubscribeFromStock(quote.symbol);
      // }

      // Get quote for new ETF
      const newQuote = await getStockQuote(symbol);
      setQuote(newQuote);

      // Subscribe to real-time updates
      // if (connection) {
      //   await subscribeToStock(symbol);
      // }
    } catch (error) {
      console.error('Failed to load ETF quote:', error);
      message.error('加载 ETF 报价失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Subscribe to real-time quote updates
   * 订阅实时报价更新
   */
  useEffect(() => {
    // TODO: Implement SignalR connection for real-time updates
    // const connection = useConnectionStore.getState().connection;
    // if (!connection || !selectedSymbol) return;

    // const handleQuoteUpdate = (_updatedQuote: StockQuote) => {
      // if (updatedQuote.symbol === selectedSymbol) {
      //   setQuote(updatedQuote);
      // }
    // };

    // connection.on('StockQuoteUpdate', handleQuoteUpdate);

    return () => {
      // connection.off('StockQuoteUpdate', handleQuoteUpdate);
      if (selectedSymbol) {
        unsubscribeFromStock(selectedSymbol);
      }
    };
  }, [selectedSymbol]);

  /**
   * Get market status tag
   * 获取市场状态标签
   */
  const getMarketStatusTag = (status?: string) => {
    const statusConfig: Record<string, { color: string; text: string }> = {
      open: { color: 'green', text: '交易中' },
      closed: { color: 'default', text: '已休市' },
      'pre-market': { color: 'blue', text: '盘前' },
      'after-hours': { color: 'orange', text: '盘后' },
    };

    const config = statusConfig[status || 'closed'];
    return <Tag color={config.color}>{config.text}</Tag>;
  };

  return (
    <div className="etf-trading-page">
      <Title level={3}>ETF 交易</Title>
      <Text type="secondary">
        交易所交易基金（ETF）可以像股票一样进行交易，提供多样化的投资机会。
      </Text>

      {/* ETF Search */}
      <Card className="search-card" style={{ marginTop: 24 }}>
        <StockSearch
          onSelect={handleSelectETF}
          placeholder="搜索 ETF 代码或名称，例如 SPY, QQQ, VOO"
        />
      </Card>

      {/* Loading State */}
      {loading && (
        <div className="loading-container">
          <Spin size="large" tip="加载中..." />
        </div>
      )}

      {/* ETF Quote Display */}
      {!loading && quote && (
        <>
          {/* Header with Symbol and Market Status */}
          <Card className="etf-header-card" style={{ marginTop: 24 }}>
            <Space size="large" align="center">
              <div>
                <Title level={2} style={{ margin: 0 }}>
                  {quote.symbol}
                </Title>
                <Text type="secondary">{quote.companyName || quote.name}</Text>
              </div>
              {getMarketStatusTag(quote.marketStatus)}
            </Space>
          </Card>

          {/* Price Statistics */}
          <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="最新价"
                  value={quote.lastPrice}
                  precision={2}
                  prefix="$"
                  valueStyle={{
                    color: (quote.change ?? 0) >= 0 ? '#3f8600' : '#cf1322',
                  }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="涨跌额"
                  value={quote.change ?? 0}
                  precision={2}
                  prefix={(quote.change ?? 0) >= 0 ? <ArrowUpOutlined /> : <ArrowDownOutlined />}
                  valueStyle={{
                    color: (quote.change ?? 0) >= 0 ? '#3f8600' : '#cf1322',
                  }}
                  suffix={
                    <span style={{ fontSize: 14 }}>
                      ({(quote.changePercent ?? 0).toFixed(2)}%)
                    </span>
                  }
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="当日最高"
                  value={quote.high ?? 0}
                  precision={2}
                  prefix="$"
                />
                <Statistic
                  title="当日最低"
                  value={quote.low ?? 0}
                  precision={2}
                  prefix="$"
                  valueStyle={{ fontSize: 14 }}
                />
              </Card>
            </Col>
            <Col xs={24} sm={12} md={6}>
              <Card>
                <Statistic
                  title="成交量"
                  value={quote.volume ?? 0}
                  suffix="股"
                />
              </Card>
            </Col>
          </Row>

          {/* Bid/Ask Prices */}
          <Card title="买卖盘" style={{ marginTop: 16 }}>
            <Row gutter={[16, 16]}>
              <Col xs={24} sm={12}>
                <Card type="inner">
                  <Statistic
                    title="买入价"
                    value={quote.bid ?? quote.bidPrice ?? 0}
                    precision={2}
                    prefix="$"
                    valueStyle={{ color: '#3f8600' }}
                    suffix={<Text type="secondary">x {quote.bidSize ?? 0}</Text>}
                  />
                </Card>
              </Col>
              <Col xs={24} sm={12}>
                <Card type="inner">
                  <Statistic
                    title="卖出价"
                    value={quote.ask ?? quote.askPrice ?? 0}
                    precision={2}
                    prefix="$"
                    valueStyle={{ color: '#cf1322' }}
                    suffix={<Text type="secondary">x {quote.askSize ?? 0}</Text>}
                  />
                </Card>
              </Col>
            </Row>
          </Card>

          {/* ETF-specific Information */}
          {quote.nav && (
            <Alert
              type="info"
              message="资产净值 (NAV)"
              description={
                <Space>
                  <DollarCircleOutlined />
                  <Text>净值：${quote.nav.toFixed(2)}</Text>
                  {quote.navChange !== undefined && (
                    <Text type={quote.navChange >= 0 ? 'success' : 'danger'}>
                      ({quote.navChange >= 0 ? '+' : ''}
                      {quote.navChange.toFixed(2)}%)
                    </Text>
                  )}
                </Space>
              }
              style={{ marginTop: 16 }}
            />
          )}

          {/* Order Form */}
          <Card title="下单" className="order-card" style={{ marginTop: 16 }}>
            <OrderForm
              symbol={quote.symbol}
              currentPrice={quote.lastPrice}
              marketStatus={quote.marketStatus}
            />
          </Card>
        </>
      )}

      {/* Empty State */}
      {!loading && !quote && (
        <Card style={{ marginTop: 24, textAlign: 'center', padding: 60 }}>
          <Text type="secondary">请搜索并选择一个 ETF 以查看详情和交易</Text>
        </Card>
      )}
    </div>
  );
};

export default ETFTradingPage;
