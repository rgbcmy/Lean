/**
 * Positions Page
 * 持仓列表页面
 */

import React, { useState, useEffect } from 'react';
import {
  Table,
  Card,
  Space,
  Button,
  Input,
  Select,
  Statistic,
  Row,
  Col,
  Modal,
  message,
  Tooltip,
  Typography,
  Tag,
} from 'antd';
import {
  ReloadOutlined,
  StopOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
  FilterOutlined,
  ExportOutlined,
  PieChartOutlined,
  LineChartOutlined,
} from '@ant-design/icons';
import type { ColumnsType } from 'antd/es/table';
import { useNavigate } from 'react-router-dom';
import {
  getPositions,
  getPortfolioSummary,
  closePosition,
  exportPositions,
} from '../api/positionsApi';
import { signalRService } from '../services/signalrService';
import type { Position, PortfolioSummary } from '../types/position';
import './PositionsPage.css';

const { Text } = Typography;
const { Option } = Select;

/**
 * PositionsPage Component
 * Displays list of positions with real-time updates
 * 显示持仓列表，支持实时更新
 */
const PositionsPage: React.FC = () => {
  const [positions, setPositions] = useState<Position[]>([]);
  const [loading, setLoading] = useState(false);
  const [summary, setSummary] = useState<PortfolioSummary | null>(null);
  const [exporting, setExporting] = useState(false);
  const navigate = useNavigate();

  // Filters
  const [symbolFilter, setSymbolFilter] = useState<string>('');
  const [sortBy, setSortBy] = useState<'pnl' | 'value' | 'symbol'>('value');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc');
  const [onlyProfitable, setOnlyProfitable] = useState<boolean>(false);

  /**
   * Load positions from API
   * 从 API 加载持仓
   */
  const loadPositions = async () => {
    setLoading(true);
    try {
      const params: any = {
        sortBy,
        sortOrder,
        onlyProfitable: onlyProfitable || undefined,
      };

      if (symbolFilter) {
        params.symbol = symbolFilter;
      }

      const result = await getPositions(params);
      setPositions(result);
    } catch (error) {
      console.error('Failed to load positions:', error);
      message.error('加载持仓失败');
    } finally {
      setLoading(false);
    }
  };

  /**
   * Load portfolio summary
   * 加载投资组合摘要
   */
  const loadSummary = async () => {
    try {
      const result = await getPortfolioSummary();
      setSummary(result);
    } catch (error) {
      console.error('Failed to load summary:', error);
    }
  };

  /**
   * Handle close position
   * 处理平仓
   */
  const handleClosePosition = (position: Position, quantity?: number) => {
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
          loadPositions();
          loadSummary();
        } catch (error) {
          console.error('Failed to close position:', error);
          message.error('平仓失败');
        }
      },
    });
  };

  /**
   * Handle export positions
   * 处理导出持仓
   */
  const handleExport = async (format: 'csv' | 'excel') => {
    setExporting(true);
    try {
      const blob = await exportPositions({ format });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `positions_${new Date().toISOString().split('T')[0]}.${
        format === 'csv' ? 'csv' : 'xlsx'
      }`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
      message.success('导出成功');
    } catch (error) {
      console.error('Failed to export:', error);
      message.error('导出失败');
    } finally {
      setExporting(false);
    }
  };

  /**
   * Handle position click (navigate to detail)
   * 处理点击持仓（导航到详情页）
   */
  const handlePositionClick = (symbol: string) => {
    navigate(`/positions/${symbol}`);
  };

  /**
   * Load data on mount and when filters change
   * 组件挂载和筛选条件变化时加载数据
   */
  useEffect(() => {
    loadPositions();
    loadSummary();
  }, [sortBy, sortOrder, onlyProfitable]);

  /**
   * Setup SignalR for real-time updates
   * 设置 SignalR 实时更新
   */
  useEffect(() => {
    // Subscribe to position updates
    const handlePositionUpdate = (updatedPosition: Position) => {
      setPositions((prev) =>
        prev.map((pos) =>
          pos.symbol === updatedPosition.symbol ? updatedPosition : pos
        )
      );
    };

    // Subscribe to market data updates
    const handleMarketDataUpdate = (data: { symbol: string; price: number }) => {
      setPositions((prev) =>
        prev.map((pos) => {
          if (pos.symbol === data.symbol) {
            const newMarketValue = data.price * pos.quantity;
            const newUnrealizedPnL = newMarketValue - pos.averageCost * pos.quantity;
            const newUnrealizedPnLPercent =
              (newUnrealizedPnL / (pos.averageCost * pos.quantity)) * 100;

            return {
              ...pos,
              currentPrice: data.price,
              marketValue: newMarketValue,
              unrealizedPnL: newUnrealizedPnL,
              unrealizedPnLPercent: newUnrealizedPnLPercent,
            };
          }
          return pos;
        })
      );
    };

    signalRService.on('PositionUpdate', handlePositionUpdate);
    signalRService.on('MarketData', handleMarketDataUpdate);

    return () => {
      signalRService.off('PositionUpdate', handlePositionUpdate);
      signalRService.off('MarketData', handleMarketDataUpdate);
    };
  }, []);

  /**
   * Table columns definition
   * 表格列定义
   */
  const columns: ColumnsType<Position> = [
    {
      title: '股票代码',
      dataIndex: 'symbol',
      key: 'symbol',
      fixed: 'left',
      width: 120,
      render: (symbol: string) => (
        <Button
          type="link"
          onClick={() => handlePositionClick(symbol)}
          style={{ padding: 0 }}
        >
          {symbol}
        </Button>
      ),
    },
    {
      title: '数量',
      dataIndex: 'quantity',
      key: 'quantity',
      align: 'right',
      width: 100,
      render: (quantity: number) => quantity.toLocaleString(),
    },
    {
      title: '成本价',
      dataIndex: 'averageCost',
      key: 'averageCost',
      align: 'right',
      width: 120,
      render: (cost: number) => `$${cost.toFixed(2)}`,
    },
    {
      title: '当前价',
      dataIndex: 'currentPrice',
      key: 'currentPrice',
      align: 'right',
      width: 120,
      render: (price: number) => `$${price.toFixed(2)}`,
    },
    {
      title: '市值',
      dataIndex: 'marketValue',
      key: 'marketValue',
      align: 'right',
      width: 140,
      render: (value: number) => `$${value.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`,
    },
    {
      title: '未实现盈亏',
      dataIndex: 'unrealizedPnL',
      key: 'unrealizedPnL',
      align: 'right',
      width: 140,
      render: (pnl: number) => (
        <Text style={{ color: pnl >= 0 ? '#3f8600' : '#cf1322' }}>
          {pnl >= 0 ? '+' : ''}${pnl.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
        </Text>
      ),
    },
    {
      title: '盈亏比例',
      dataIndex: 'unrealizedPnLPercent',
      key: 'unrealizedPnLPercent',
      align: 'right',
      width: 120,
      render: (percent: number) => (
        <Text style={{ color: percent >= 0 ? '#3f8600' : '#cf1322' }}>
          {percent >= 0 ? '+' : ''}{percent.toFixed(2)}%
        </Text>
      ),
    },
    {
      title: '持有天数',
      dataIndex: 'holdingDays',
      key: 'holdingDays',
      align: 'right',
      width: 100,
      render: (days: number) => `${days} 天`,
    },
    {
      title: '操作',
      key: 'action',
      fixed: 'right',
      width: 100,
      render: (_: any, record: Position) => (
        <Space size="small">
          <Tooltip title="平仓">
            <Button
              type="text"
              size="small"
              icon={<StopOutlined />}
              onClick={() => handleClosePosition(record)}
            />
          </Tooltip>
        </Space>
      ),
    },
  ];

  /**
   * Filter positions based on symbol search
   * 根据股票代码搜索筛选持仓
   */
  const filteredPositions = symbolFilter
    ? positions.filter((pos) =>
        pos.symbol.toLowerCase().includes(symbolFilter.toLowerCase())
      )
    : positions;

  return (
    <div className="positions-page">
      {/* Portfolio Summary */}
      {summary && (
        <Row gutter={16} style={{ marginBottom: 16 }}>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="总市值"
                value={summary.totalMarketValue}
                precision={2}
                prefix="$"
                valueStyle={{ color: '#1890ff' }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="总盈亏"
                value={summary.totalUnrealizedPnL}
                precision={2}
                prefix={summary.totalUnrealizedPnL >= 0 ? '+$' : '-$'}
                valueStyle={{
                  color: summary.totalUnrealizedPnL >= 0 ? '#3f8600' : '#cf1322',
                }}
                suffix={
                  <span style={{ fontSize: 14 }}>
                    ({summary.totalUnrealizedPnLPercent >= 0 ? '+' : ''}
                    {summary.totalUnrealizedPnLPercent.toFixed(2)}%)
                  </span>
                }
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="已实现盈亏"
                value={Math.abs(summary.totalRealizedPnL)}
                precision={2}
                prefix={summary.totalRealizedPnL >= 0 ? '+$' : '-$'}
                valueStyle={{
                  color: summary.totalRealizedPnL >= 0 ? '#3f8600' : '#cf1322',
                }}
              />
            </Card>
          </Col>
          <Col xs={24} sm={12} md={6}>
            <Card>
              <Statistic
                title="持仓数量"
                value={summary.positions.length}
                suffix="个"
              />
            </Card>
          </Col>
        </Row>
      )}

      {/* Actions and Filters */}
      <Card style={{ marginBottom: 16 }}>
        <Space size="middle" wrap>
          <Input
            placeholder="搜索股票代码"
            prefix={<SearchOutlined />}
            value={symbolFilter}
            onChange={(e) => setSymbolFilter(e.target.value)}
            style={{ width: 200 }}
            allowClear
          />

          <Select
            value={sortBy}
            onChange={(value) => setSortBy(value)}
            style={{ width: 120 }}
          >
            <Option value="value">按市值</Option>
            <Option value="pnl">按盈亏</Option>
            <Option value="symbol">按代码</Option>
          </Select>

          <Select
            value={sortOrder}
            onChange={(value) => setSortOrder(value)}
            style={{ width: 100 }}
          >
            <Option value="desc">降序</Option>
            <Option value="asc">升序</Option>
          </Select>

          <Select
            value={onlyProfitable ? 'profitable' : 'all'}
            onChange={(value) => setOnlyProfitable(value === 'profitable')}
            style={{ width: 120 }}
          >
            <Option value="all">全部</Option>
            <Option value="profitable">仅盈利</Option>
          </Select>

          <Button icon={<ReloadOutlined />} onClick={loadPositions}>
            刷新
          </Button>

          <Button
            icon={<ExportOutlined />}
            onClick={() => handleExport('csv')}
            loading={exporting}
          >
            导出 CSV
          </Button>

          <Button
            icon={<ExportOutlined />}
            onClick={() => handleExport('excel')}
            loading={exporting}
          >
            导出 Excel
          </Button>

          <Button
            icon={<PieChartOutlined />}
            onClick={() => navigate('/portfolio/allocation')}
          >
            资产配置
          </Button>

          <Button
            icon={<LineChartOutlined />}
            onClick={() => navigate('/portfolio/analysis')}
          >
            投资组合分析
          </Button>
        </Space>
      </Card>

      {/* Positions Table */}
      <Card title={`持仓列表 (${filteredPositions.length})`}>
        <Table
          columns={columns}
          dataSource={filteredPositions}
          rowKey="symbol"
          loading={loading}
          pagination={false}
          scroll={{ x: 1200 }}
          locale={{
            emptyText: '暂无持仓',
          }}
        />
      </Card>
    </div>
  );
};

export default PositionsPage;
