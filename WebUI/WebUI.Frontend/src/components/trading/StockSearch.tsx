/**
 * Stock Search Component
 * 股票搜索组件
 */

import React, { useState, useCallback } from 'react';
import { AutoComplete, Input, Spin, Empty } from 'antd';
import { SearchOutlined, StockOutlined } from '@ant-design/icons';
import debounce from '../../utils/debounce';
import { searchStocks } from '../../api/stocksApi';
import type { Stock } from '../../types/stock';
import './StockSearch.css';

interface StockSearchProps {
  onSelect: (stock: Stock) => void;
  placeholder?: string;
  style?: React.CSSProperties;
}

interface SearchOption {
  value: string;
  label: React.ReactNode;
  stock: Stock;
}

/**
 * StockSearch Component
 * Provides autocomplete search for stocks by symbol or company name
 * 提供按代码或公司名称搜索股票的自动完成功能
 */
const StockSearch: React.FC<StockSearchProps> = ({
  onSelect,
  placeholder = '搜索股票代码或公司名称',
  style,
}) => {
  const [options, setOptions] = useState<SearchOption[]>([]);
  const [loading, setLoading] = useState(false);
  const [searchValue, setSearchValue] = useState('');

  /**
   * Search stocks by query
   * 根据查询搜索股票
   */
  const handleSearch = useCallback(
    debounce(async (query: string) => {
      if (!query || query.trim().length < 1) {
        setOptions([]);
        return;
      }

      setLoading(true);
      try {
        const raw = await searchStocks(query);
        const results: Stock[] = Array.isArray(raw) ? raw : [];
        
        if (results.length === 0) {
          setOptions([{
            value: 'no-results',
            label: (
              <Empty
                image={Empty.PRESENTED_IMAGE_SIMPLE}
                description="未找到匹配的股票"
                style={{ padding: '12px 0' }}
              />
            ),
            stock: null as any,
          }]);
        } else {
          const searchOptions: SearchOption[] = results.map((stock) => ({
            value: stock.symbol,
            label: (
              <div className="stock-search-option">
                <div className="stock-symbol">
                  <StockOutlined />
                  <span className="symbol">{stock.symbol}</span>
                </div>
                <div className="stock-info">
                  <span className="stock-name">{stock.name}</span>
                  <span className="stock-exchange">{stock.exchange}</span>
                </div>
                {stock.lastPrice && (
                  <div className="stock-price">
                    ${stock.lastPrice.toFixed(2)}
                  </div>
                )}
              </div>
            ),
            stock,
          }));
          setOptions(searchOptions);
        }
      } catch (error) {
        console.error('Stock search error:', error);
        setOptions([{
          value: 'error',
          label: (
            <div style={{ padding: '12px', color: '#ff4d4f' }}>
              搜索失败，请稍后重试
            </div>
          ),
          stock: null as any,
        }]);
      } finally {
        setLoading(false);
      }
    }, 300),
    []
  );

  /**
   * Handle stock selection
   * 处理股票选择
   */
  const handleSelect = (value: string, option: SearchOption) => {
    if (option.stock && value !== 'no-results' && value !== 'error') {
      onSelect(option.stock);
      setSearchValue('');
      setOptions([]);
    }
  };

  /**
   * Handle search input change
   * 处理搜索输入变化
   */
  const handleChange = (value: string) => {
    setSearchValue(value);
    handleSearch(value);
  };

  return (
    <AutoComplete
      value={searchValue}
      options={options}
      onSelect={handleSelect}
      onChange={handleChange}
      style={{ width: '100%', ...style }}
      className="stock-search"
      dropdownClassName="stock-search-dropdown"
      notFoundContent={loading ? <Spin size="small" /> : null}
    >
      <Input
        size="large"
        placeholder={placeholder}
        prefix={<SearchOutlined />}
        suffix={loading && <Spin size="small" />}
        allowClear
      />
    </AutoComplete>
  );
};

export default StockSearch;
