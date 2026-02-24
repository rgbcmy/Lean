/**
 * Technical indicator calculation utilities
 */

export interface IndicatorConfig {
  ma?: number[]; // MA periods, e.g., [5, 10, 20, 30]
  macd?: {
    fastPeriod: number;
    slowPeriod: number;
    signalPeriod: number;
  };
  rsi?: {
    period: number;
  };
}

/**
 * Calculate Simple Moving Average (SMA)
 */
export function calculateMA(data: number[], period: number): (number | string)[] {
  const result: (number | string)[] = [];
  for (let i = 0; i < data.length; i++) {
    if (i < period - 1) {
      result.push('-');
      continue;
    }
    let sum = 0;
    for (let j = 0; j < period; j++) {
      sum += data[i - j];
    }
    result.push(+(sum / period).toFixed(2));
  }
  return result;
}

/**
 * Calculate Exponential Moving Average (EMA)
 */
export function calculateEMA(data: number[], period: number): number[] {
  const result: number[] = [];
  const multiplier = 2 / (period + 1);

  // Start with SMA for initial value
  let sum = 0;
  for (let i = 0; i < period; i++) {
    sum += data[i];
  }
  const initialEMA = sum / period;
  result.push(initialEMA);

  // Calculate EMA for remaining values
  for (let i = period; i < data.length; i++) {
    const ema = (data[i] - result[i - period]) * multiplier + result[i - period];
    result.push(ema);
  }

  // Pad with zeros for the initial period
  const paddedResult = new Array(period - 1).fill(0).concat(result);
  return paddedResult;
}

/**
 * Calculate MACD (Moving Average Convergence Divergence)
 * Returns: { macd, signal, histogram }
 */
export function calculateMACD(
  data: number[],
  fastPeriod: number = 12,
  slowPeriod: number = 26,
  signalPeriod: number = 9
): {
  macd: number[];
  signal: number[];
  histogram: number[];
} {
  const fastEMA = calculateEMA(data, fastPeriod);
  const slowEMA = calculateEMA(data, slowPeriod);

  // MACD line = Fast EMA - Slow EMA
  const macd = fastEMA.map((fast, i) => fast - slowEMA[i]);

  // Signal line = EMA of MACD
  const signal = calculateEMA(macd.slice(slowPeriod - 1), signalPeriod);
  const paddedSignal = new Array(slowPeriod - 1).fill(0).concat(signal);

  // Histogram = MACD - Signal
  const histogram = macd.map((val, i) => val - paddedSignal[i]);

  return { macd, signal: paddedSignal, histogram };
}

/**
 * Calculate RSI (Relative Strength Index)
 */
export function calculateRSI(data: number[], period: number = 14): number[] {
  const result: number[] = [];
  const gains: number[] = [];
  const losses: number[] = [];

  // Calculate gains and losses
  for (let i = 1; i < data.length; i++) {
    const change = data[i] - data[i - 1];
    gains.push(change > 0 ? change : 0);
    losses.push(change < 0 ? -change : 0);
  }

  // Calculate initial average gain and loss
  let avgGain = gains.slice(0, period).reduce((a, b) => a + b, 0) / period;
  let avgLoss = losses.slice(0, period).reduce((a, b) => a + b, 0) / period;

  // Pad result array
  for (let i = 0; i <= period; i++) {
    result.push(0);
  }

  // Calculate RSI
  for (let i = period; i < gains.length; i++) {
    avgGain = (avgGain * (period - 1) + gains[i]) / period;
    avgLoss = (avgLoss * (period - 1) + losses[i]) / period;

    const rs = avgGain / avgLoss;
    const rsi = 100 - 100 / (1 + rs);
    result.push(rsi);
  }

  return result;
}

/**
 * Calculate Bollinger Bands
 */
export function calculateBollingerBands(
  data: number[],
  period: number = 20,
  stdDev: number = 2
): {
  upper: (number | string)[];
  middle: (number | string)[];
  lower: (number | string)[];
} {
  const middle = calculateMA(data, period);
  const upper: (number | string)[] = [];
  const lower: (number | string)[] = [];

  for (let i = 0; i < data.length; i++) {
    if (i < period - 1) {
      upper.push('-');
      lower.push('-');
      continue;
    }

    // Calculate standard deviation
    const slice = data.slice(i - period + 1, i + 1);
    const mean = slice.reduce((a, b) => a + b, 0) / period;
    const variance = slice.reduce((sum, val) => sum + Math.pow(val - mean, 2), 0) / period;
    const sd = Math.sqrt(variance);

    upper.push(+((middle[i] as number) + stdDev * sd).toFixed(2));
    lower.push(+((middle[i] as number) - stdDev * sd).toFixed(2));
  }

  return { upper, middle, lower };
}
