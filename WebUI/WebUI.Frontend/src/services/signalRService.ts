/**
 * SignalR Client Configuration
 * SignalR 客户端配置
 */

import * as signalR from '@microsoft/signalr';
import { message } from 'antd';
import { ENV } from '../config/env';
import { getAccessToken } from '../utils/tokenManager';
import { useConnectionStore, ConnectionStatus } from '../stores';

/**
 * SignalR Hub Connections
 */
class SignalRService {
  private marketDataHub: signalR.HubConnection | null = null;
  private orderHub: signalR.HubConnection | null = null;
  private strategyHub: signalR.HubConnection | null = null;
  
  /**
   * Create a new hub connection
   * 创建新的 Hub 连接
   */
  private createConnection(hubName: string): signalR.HubConnection {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(`${ENV.SIGNALR_HUB_URL}/${hubName}`, {
        accessTokenFactory: () => {
          const token = getAccessToken();
          return token || '';
        },
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 1s, 2s, 4s, 8s, ..., max 60s
          const delay = Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 60000);
          console.log(`SignalR reconnecting in ${delay}ms (attempt ${retryContext.previousRetryCount + 1})`);
          return delay;
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();
    
    // Connection event handlers
    connection.onclose((error) => {
      console.error(`SignalR ${hubName} connection closed:`, error);
      useConnectionStore.getState().setSignalRStatus(
        ConnectionStatus.Disconnected,
        error?.message || 'Connection closed'
      );
    });
    
    connection.onreconnecting((error) => {
      console.warn(`SignalR ${hubName} reconnecting:`, error);
      useConnectionStore.getState().setSignalRStatus(
        ConnectionStatus.Reconnecting,
        error?.message
      );
    });
    
    connection.onreconnected((connectionId) => {
      console.log(`SignalR ${hubName} reconnected: ${connectionId}`);
      useConnectionStore.getState().setSignalRStatus(ConnectionStatus.Connected);
      message.success('实时连接已恢复');
    });
    
    return connection;
  }
  
  /**
   * Start a hub connection
   * 启动 Hub 连接
   */
  private async startConnection(connection: signalR.HubConnection, hubName: string): Promise<void> {
    try {
      useConnectionStore.getState().setSignalRStatus(ConnectionStatus.Connecting);
      await connection.start();
      console.log(`SignalR ${hubName} connected`);
      useConnectionStore.getState().setSignalRStatus(ConnectionStatus.Connected);
    } catch (error: any) {
      console.error(`SignalR ${hubName} connection failed:`, error);
      useConnectionStore.getState().setSignalRStatus(
        ConnectionStatus.Failed,
        error.message
      );
      message.error(`实时连接失败: ${error.message}`);
      throw error;
    }
  }
  
  /**
   * Get or create Market Data Hub connection
   * 获取或创建行情数据 Hub 连接
   */
  async getMarketDataHub(): Promise<signalR.HubConnection> {
    if (!this.marketDataHub) {
      this.marketDataHub = this.createConnection('market');
    }
    
    if (this.marketDataHub.state === signalR.HubConnectionState.Disconnected) {
      await this.startConnection(this.marketDataHub, 'market');
    }
    
    return this.marketDataHub;
  }
  
  /**
   * Get or create Order Hub connection
   * 获取或创建订单 Hub 连接
   */
  async getOrderHub(): Promise<signalR.HubConnection> {
    if (!this.orderHub) {
      this.orderHub = this.createConnection('order');
    }
    
    if (this.orderHub.state === signalR.HubConnectionState.Disconnected) {
      await this.startConnection(this.orderHub, 'order');
    }
    
    return this.orderHub;
  }
  
  /**
   * Get or create Strategy Hub connection
   * 获取或创建策略 Hub 连接
   */
  async getStrategyHub(): Promise<signalR.HubConnection> {
    if (!this.strategyHub) {
      this.strategyHub = this.createConnection('strategy');
    }
    
    if (this.strategyHub.state === signalR.HubConnectionState.Disconnected) {
      await this.startConnection(this.strategyHub, 'strategy');
    }
    
    return this.strategyHub;
  }
  
  /**
   * Subscribe to market data for a symbol
   * 订阅股票行情
   */
  async subscribeMarketData(symbol: string, callback: (data: any) => void): Promise<void> {
    const hub = await this.getMarketDataHub();
    
    // Register callback for this symbol
    hub.on(`MarketData_${symbol}`, callback);
    
    // Request subscription on server
    await hub.invoke('SubscribeMarketData', symbol);
    console.log(`Subscribed to market data: ${symbol}`);
  }
  
  /**
   * Unsubscribe from market data
   * 取消订阅行情
   */
  async unsubscribeMarketData(symbol: string): Promise<void> {
    if (this.marketDataHub) {
      // Remove callback
      this.marketDataHub.off(`MarketData_${symbol}`);
      
      // Request unsubscription on server
      await this.marketDataHub.invoke('UnsubscribeMarketData', symbol);
      console.log(`Unsubscribed from market data: ${symbol}`);
    }
  }
  
  /**
   * Subscribe to order updates
   * 订阅订单更新
   */
  async subscribeOrderUpdates(callback: (data: any) => void): Promise<void> {
    const hub = await this.getOrderHub();
    hub.on('OrderUpdate', callback);
    console.log('Subscribed to order updates');
  }
  
  /**
   * Subscribe to position updates
   * 订阅持仓更新
   */
  async subscribePositionUpdates(callback: (data: any) => void): Promise<void> {
    const hub = await this.getOrderHub();
    hub.on('PositionUpdate', callback);
    console.log('Subscribed to position updates');
  }
  
  /**
   * Subscribe to strategy logs
   * 订阅策略日志
   */
  async subscribeStrategyLogs(strategyId: number, callback: (log: string) => void): Promise<void> {
    const hub = await this.getStrategyHub();
    hub.on(`StrategyLog_${strategyId}`, callback);
    
    // Request subscription on server
    await hub.invoke('SubscribeStrategyLogs', strategyId);
    console.log(`Subscribed to strategy logs: ${strategyId}`);
  }
  
  /**
   * Unsubscribe from strategy logs
   * 取消订阅策略日志
   */
  async unsubscribeStrategyLogs(strategyId: number): Promise<void> {
    if (this.strategyHub) {
      this.strategyHub.off(`StrategyLog_${strategyId}`);
      await this.strategyHub.invoke('UnsubscribeStrategyLogs', strategyId);
      console.log(`Unsubscribed from strategy logs: ${strategyId}`);
    }
  }
  
  /**
   * Disconnect all hubs
   * 断开所有 Hub 连接
   */
  async disconnectAll(): Promise<void> {
    const promises: Promise<void>[] = [];
    
    if (this.marketDataHub) {
      promises.push(this.marketDataHub.stop());
      this.marketDataHub = null;
    }
    
    if (this.orderHub) {
      promises.push(this.orderHub.stop());
      this.orderHub = null;
    }
    
    if (this.strategyHub) {
      promises.push(this.strategyHub.stop());
      this.strategyHub = null;
    }
    
    await Promise.all(promises);
    useConnectionStore.getState().setSignalRStatus(ConnectionStatus.Disconnected);
    console.log('All SignalR connections disconnected');
  }
}

// Export singleton instance
export const signalRService = new SignalRService();
