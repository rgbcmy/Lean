/**
 * Stock Trading Page Component
 * 股票交易页面组件
 */

import React, { useState, useEffect } from 'react';
import {
  Typography,
  Row,
  Col,
  Card,
  Statistic,
  Space,
  Tag,
  Spin,
  message,
} from 'antd';
import {
  ArrowUpOutlined,
  ArrowDownOutlined,
  ClockCircleOutlined,
} from '@ant-design/icons';
import { useParams, useNavigate } from 'react-router-dom';
import StockSearch from '../components/trading/StockSearch';
import OrderForm from '../components/trading/OrderForm';
import { getStockQuote, subscribeToStock, unsubscribeFromStock } from '../api/stocksApi';
import { signalRService } from '../services/signalrService';
import type { Stock, StockQuote } from '../types/stock';
import './StockTradingPage.css';

const { Title, Text } = Typography;

/**
 * StockTradingPage Component
 * Main stock trading interface with search, quote display, and order form
 * 主要股票交易界面，包含搜索、行情显示和下单表单
 */
const StockTradingPage: React.FC = () => {
  const { symbol: urlSymbol } = useParams<{ symbol?: string }>();
  const navigate = useNavigate();
  // const connection = useConnectionStore((state) => state.connection);
  
  const [selectedStock, setSelectedStock] = useState<Stock | null>(null);
  const [quote, setQuote] = useState<StockQuote | null>(null);
  const [loading, setLoading] = useState(false);

  /**
   * Load stock quote
   * 加载股票行情
   */
  const loadStockQuote = async (symbol: string) => {
    setLoading(true);
    try {
      const quoteData = await getStockQuote(symbol);
      setQuote(quoteData);
      setSelectedStock({
        symbol: quoteData.symbol,
        name: quoteData.name,
        exchange: quoteData.exchange,
        lastPrice: quoteData.lastPrice,
        change: quoteData.change,
        changePercent: quoteData.changePercent,
      });
      
      // Subscribe to real-time updates
      await subscribeToStock(symbol);
    } catch (error) {
      console.error('Failed to load stock quote:', error);
      message.error('加载股票行情失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Handle stock selection from search
   * 处理从搜索中选择股票
   */
  const handleStockSelect = (stock: Stock) => {
    navigate(`/trading/stocks/${stock.symbol}`);
  };

  /**
   * Setup SignalR subscription for real-time updates
   * 设置 SignalR 订阅以接收实时更新
   */
  useEffect(() => {
    if (!selectedStock) return;

    let subscribed = false;

    const setupMarketDataSubscription = async () => {
      try {
        // Subscribe to market data updates via SignalR
        await signalRService.subscribeMarketData(selectedStock.symbol, (data: Partial<StockQuote>) => {
          console.log('Market data update received:', data);
          
          // Update the quote with the new data
          setQuote((prevQuote) => {
            if (!prevQuote) return null;
            return {
              ...prevQuote,
              ...data,
              // Ensure symbol matches to avoid race conditions
              symbol: prevQuote.symbol,
            };
          });
        });

        subscribed = true;
        console.log(`Subscribed to market data: ${selectedStock.symbol}`);
      } catch (error) {
        console.error('Failed to subscribe to market data:', error);
      }
    };

    setupMarketDataSubscription();

    return () => {
      if (subscribed && selectedStock) {
        // Cleanup: unsubscribe from market data
        signalRService.unsubscribeMarketData(selectedStock.symbol)
          .then(() => console.log(`Unsubscribed from market data: ${selectedStock.symbol}`))
          .catch(console.error);
        
        // Also unsubscribe from backend
        unsubscribeFromStock(selectedStock.symbol).catch(console.error);
      }
    };
  }, [selectedStock]);

  /**
   * Load stock from URL parameter
   * 从 URL 参数加载股票
   */
  useEffect(() => {
    if (urlSymbol) {
      loadStockQuote(urlSymbol);
    }
  }, [urlSymbol]);

  /**
   * Get price change indicator
   * 获取价格变化指标
   */
  const getPriceChangeIndicator = () => {
    if (!quote?.change) return null;
    
    const isPositive = quote.change > 0;
    return {
      icon: isPositive ? <ArrowUpOutlined /> : <ArrowDownOutlined />,
      color: isPositive ? '#52c41a' : '#ff4d4f',
      prefix: isPositive ? '+' : '',
    };
  };

  const priceChange = getPriceChangeIndicator();

  /**
   * Get market status tag
   * 获取市场状态标签
   */
  const getMarketStatusTag = () => {
    if (!quote?.marketStatus) return null;

    const statusMap: Record<string, { text: string; color: string }> = {
      'open': { text: '开盘', color: 'green' },
      'closed': { text: '休市', color: 'default' },
      'pre-market': { text: '盘前', color: 'blue' },
      'after-hours': { text: '盘后', color: 'orange' },
    };

    const status = statusMap[quote.marketStatus];
    return <Tag color={status.color}>{status.text}</Tag>;
  };

  return (
    <div className="stock-trading-page">
      {/* Header with Search */}
      <div className="page-header">
        <div>
          <Title level={2}>股票交易</Title>
          <Text type="secondary">搜索并交易美股股票</Text>
        </div>
      </div>

      {/* Stock Search */}
      <Card className="search-card">
        <StockSearch
          onSelect={handleStockSelect}
          placeholder="输入股票代码或公司名称（如 AAPL 或 Apple）"
        />
      </Card>

      {/* Stock Quote and Order Form */}
      {loading && (
        <div className="loading-container">
          <Spin size="large" tip="加载中..." />
        </div>
      )}

      {!loading && selectedStock && quote && (
        <>
          {/* Stock Quote Card */}
          <Card className="quote-card">
            <div className="quote-header">
              <div className="stock-title">
                <Title level={3} style={{ marginBottom: 0 }}>
                  {quote.symbol}
                </Title>
                <Text type="secondary">{quote.name}</Text>
              </div>
              <Space>
                {getMarketStatusTag()}
                <Text type="secondary">{quote.exchange}</Text>
              </Space>
            </div>

            <Row gutter={[16, 16]} className="quote-stats">
              <Col xs={24} sm={12} lg={6}>
                <Statistic
                  title="最新价"
                  value={quote.lastPrice}
                  precision={2}
                  prefix="$"
                  valueStyle={{ fontSize: 28 }}
                />
              </Col>
              <Col xs={24} sm={12} lg={6}>
                <Statistic
                  title="涨跌"
                  value={quote.change}
                  precision={2}
                  prefix={priceChange?.prefix}
                  suffix={`(${quote.changePercent?.toFixed(2)}%)`}
                  valueStyle={{ color: priceChange?.color, fontSize: 20 }}
                />
              </Col>
              <Col xs={12} lg={6}>
                <Statistic
                  title="买价 / Ask"
                  value={quote.askPrice || '--'}
                  precision={2}
                  prefix={quote.askPrice ? '$' : ''}
                />
              </Col>
              <Col xs={12} lg={6}>
                <Statistic
                  title="卖价 / Bid"
                  value={quote.bidPrice || '--'}
                  precision={2}
                  prefix={quote.bidPrice ? '$' : ''}
                />
              </Col>
            </Row>

            <Row gutter={[16, 16]} className="quote-details">
              <Col xs={12} sm={6}>
                <Text type="secondary">今开</Text>
                <div>${quote.open?.toFixed(2) || '--'}</div>
              </Col>
              <Col xs={12} sm={6}>
                <Text type="secondary">昨收</Text>
                <div>${quote.previousClose?.toFixed(2) || '--'}</div>
              </Col>
              <Col xs={12} sm={6}>
                <Text type="secondary">最高</Text>
                <div>${quote.high?.toFixed(2) || '--'}</div>
              </Col>
              <Col xs={12} sm={6}>
                <Text type="secondary">最低</Text>
                <div>${quote.low?.toFixed(2) || '--'}</div>
              </Col>
            </Row>

            {quote.delayed && (
              <div className="delayed-notice">
                <ClockCircleOutlined /> 延时行情（15 分钟）
              </div>
            )}
          </Card>

          {/* Order Form */}
          <Card title="下单" className="order-card">
            <OrderForm
              symbol={quote.symbol}
              currentPrice={quote.lastPrice}
              marketStatus={quote.marketStatus}
            />
          </Card>
        </>
      )}

      {!loading && !selectedStock && (
        <Card className="empty-state">
          <div className="empty-content">
            <Text type="secondary">请使用搜索框查找要交易的股票</Text>
          </div>
        </Card>
      )}
    </div>
  );
};

export default StockTradingPage;
