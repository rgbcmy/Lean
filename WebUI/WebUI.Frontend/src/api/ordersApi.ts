/**
 * Orders API
 * 订单 API 接口
 */

import { apiClient } from './apiClient';
import type {
  Order,
  PlaceMarketOrderRequest,
  PlaceLimitOrderRequest,
  OrderCostEstimate,
  EstimateOrderRequest,
} from '../types/order';

/**
 * Place a market order
 * 提交市价单
 */
export async function placeMarketOrder(
  request: PlaceMarketOrderRequest
): Promise<{ orderId: string }> {
  const response = await apiClient.post<{ orderId: string }>(
    '/api/v1/orders/market',
    request
  );
  return response.data;
}

/**
 * Place a limit order
 * 提交限价单
 */
export async function placeLimitOrder(
  request: PlaceLimitOrderRequest
): Promise<{ orderId: string }> {
  const response = await apiClient.post<{ orderId: string }>(
    '/api/v1/orders/limit',
    request
  );
  return response.data;
}

/**
 * Estimate order cost
 * 估算订单成本
 */
export async function estimateOrderCost(
  request: EstimateOrderRequest
): Promise<OrderCostEstimate> {
  const response = await apiClient.post<OrderCostEstimate>(
    '/api/v1/orders/estimate',
    request
  );
  return response.data;
}

/**
 * Get order by ID
 * 根据 ID 获取订单
 */
export async function getOrder(orderId: string): Promise<Order> {
  const response = await apiClient.get<Order>(`/api/v1/orders/${orderId}`);
  return response.data;
}

/**
 * Get all orders with optional filters
 * 获取所有订单（支持筛选）
 */
export async function getOrders(params?: {
  symbol?: string;
  status?: string;
  startDate?: string;
  endDate?: string;
  page?: number;
  pageSize?: number;
}): Promise<{ orders: Order[]; totalCount: number }> {
  const response = await apiClient.get<{ orders: Order[]; totalCount: number }>(
    '/api/v1/orders',
    { params }
  );
  return response.data;
}

/**
 * Cancel an order
 * 取消订单
 */
export async function cancelOrder(orderId: string): Promise<void> {
  await apiClient.delete(`/api/v1/orders/${orderId}`);
}

/**
 * Cancel all active orders
 * 取消所有活跃订单
 */
export async function cancelAllOrders(): Promise<{ cancelledCount: number }> {
  const response = await apiClient.post<{ cancelledCount: number }>(
    '/api/v1/orders/cancel-all'
  );
  return response.data;
}
