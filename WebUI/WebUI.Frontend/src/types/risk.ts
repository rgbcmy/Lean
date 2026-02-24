/**
 * Risk Control Types
 * 风险控制类型定义
 */

/**
 * Stop Loss Rule Type
 * 止损规则类型
 */
export interface StopLossRule {
  id?: string;
  enabled: boolean;
  type: 'percentage' | 'dollar' | 'trailing';
  value: number; // Percentage (e.g., 5 for 5%) or dollar amount
  symbol?: string; // Optional: apply to specific symbol
  description?: string;
}

/**
 * Take Profit Rule Type
 * 止盈规则类型
 */
export interface TakeProfitRule {
  id?: string;
  enabled: boolean;
  type: 'percentage' | 'dollar' | 'partial';
  value: number;
  partialPercentages?: number[]; // For partial take profit (e.g., [50, 100])
  partialTargets?: number[]; // For partial targets (e.g., [10, 20])
  symbol?: string;
  notifyBeforeTarget?: boolean;
  notifyThreshold?: number; // Percentage before target to notify
  description?: string;
}

/**
 * Position Limit Configuration
 * 仓位限制配置
 */
export interface PositionLimitConfig {
  maxPositionSizePercent: number; // Maximum single position as % of total portfolio (e.g., 20)
  maxNumberOfPositions: number; // Maximum number of positions (e.g., 10)
  minCashReservePercent: number; // Minimum cash reserve percentage (e.g., 10)
  maxSectorConcentrationPercent: number; // Maximum concentration in one sector (e.g., 40)
}

/**
 * Trading Frequency Limit Configuration
 * 交易频率限制配置
 */
export interface TradingFrequencyConfig {
  maxTradesPerDay: number; // Maximum number of trades per day
  maxOrderValuePerDay: number; // Maximum order value per day in dollars
  cooldownPeriodMinutes: number; // Cooldown period between trades for same symbol
  enabled: boolean;
}

/**
 * Risk Configuration
 * 风险配置
 */
export interface RiskConfig {
  stopLossRules: StopLossRule[];
  takeProfitRules: TakeProfitRule[];
  positionLimits: PositionLimitConfig;
  tradingFrequency: TradingFrequencyConfig;
  marginWarningThreshold: number; // Percentage (e.g., 80)
  marginBlockThreshold: number; // Percentage (e.g., 90)
  circuitBreakerEnabled: boolean;
  priceMovementThreshold: number; // Percentage in 1 minute (e.g., 5)
  largeOrderThreshold: number; // Dollar amount requiring confirmation (e.g., 10000)
  dayTradingRulesEnabled: boolean;
  updatedAt?: string;
}

/**
 * Risk Metrics
 * 风险指标
 */
export interface RiskMetrics {
  valueAtRisk: number; // VaR (95% confidence)
  portfolioVolatility: number; // Annualized volatility
  sharpeRatio: number;
  maxDrawdown: number; // Percentage
  maxDrawdownDate?: string;
  currentDrawdown: number;
  marginUsagePercent: number;
  concentrationRisk: {
    topPositionPercent: number;
    topSectorPercent: number;
    topSectorName: string;
  };
  dayTradesCount: number; // In rolling 5 days
  riskLevel: 'low' | 'medium' | 'high';
}

/**
 * Risk Alert
 * 风险告警
 */
export interface RiskAlert {
  id: string;
  type: 'stop_loss' | 'take_profit' | 'margin' | 'concentration' | 'circuit_breaker' | 'day_trading';
  severity: 'info' | 'warning' | 'critical';
  message: string;
  symbol?: string;
  timestamp: string;
  acknowledged: boolean;
}

/**
 * Risk Report
 * 风险报告
 */
export interface RiskReport {
  date: string;
  metrics: RiskMetrics;
  alerts: RiskAlert[];
  positions: {
    symbol: string;
    riskScore: number;
    contribution: number; // Contribution to portfolio risk
  }[];
}
