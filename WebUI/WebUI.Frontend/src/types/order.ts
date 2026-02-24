/**
 * Order Types
 * 订单类型定义
 */

export type OrderSide = 'buy' | 'sell';
export type OrderType = 'market' | 'limit';
export type OrderStatus =
  | 'submitted'
  | 'partiallyFilled'
  | 'filled'
  | 'cancelled'
  | 'rejected';

export interface Order {
  orderId: string;
  symbol: string;
  side: OrderSide;
  orderType: OrderType;
  quantity: number;
  filledQuantity: number;
  remainingQuantity: number;
  limitPrice?: number;
  avgFillPrice?: number;
  status: OrderStatus;
  submittedAt: Date;
  lastUpdateAt: Date;
  rejectionReason?: string;
}

export interface PlaceMarketOrderRequest {
  symbol: string;
  side: OrderSide;
  quantity: number;
}

export interface PlaceLimitOrderRequest {
  symbol: string;
  side: OrderSide;
  quantity: number;
  limitPrice: number;
}

export interface OrderCostEstimate {
  estimatedPrice: number;
  commission: number;
  totalCost: number;
}

export interface EstimateOrderRequest {
  symbol: string;
  side: OrderSide;
  orderType: OrderType;
  quantity: number;
  limitPrice?: number;
}
