/**
 * Stocks API
 * 股票 API 接口
 */

import { apiClient } from './apiClient';
import type { Stock, StockQuote } from '../types/stock';

/**
 * Search for stocks by symbol or company name
 * 根据代码或公司名称搜索股票
 */
export async function searchStocks(query: string): Promise<Stock[]> {
  const response = await apiClient.get<Stock[]>(`/api/v1/stocks/search`, {
    params: { q: query },
  });
  return response.data;
}

/**
 * Get stock quote by symbol
 * 根据代码获取股票行情
 */
export async function getStockQuote(symbol: string): Promise<StockQuote> {
  const response = await apiClient.get<StockQuote>(`/api/v1/market/quote/${symbol}`);
  return response.data;
}

/**
 * Subscribe to real-time stock quote
 * 订阅实时股票行情
 */
export async function subscribeToStock(symbol: string): Promise<void> {
  await apiClient.post(`/api/v1/market/subscribe`, { symbols: [symbol] });
}

/**
 * Unsubscribe from stock quote
 * 取消订阅股票行情
 */
export async function unsubscribeFromStock(symbol: string): Promise<void> {
  await apiClient.post(`/api/v1/market/unsubscribe`, { symbols: [symbol] });
}
