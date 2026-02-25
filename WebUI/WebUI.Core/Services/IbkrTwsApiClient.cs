/*
 * IBKR TWS API Client
 * 使用官方 IBApi (EClientSocket / EWrapper) 与 TWS/Gateway 通信
 * Communicates with TWS/Gateway using the official IBApi EClientSocket/EWrapper.
 *
 * 架构说明 / Architecture:
 *   IbkrWrapper         实现 DefaultEWrapper，把所有 IB 回调桥接为 async/await
 *   IIbkrTwsApiClient   对外接口：连接管理 + 账户/持仓/订单/行情/历史数据
 *   IbkrTwsApiClient    持有长连接 EClientSocket + EReader 消息循环
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IBApi;
using Microsoft.Extensions.Logging;
using WebUI.Core.Models;

namespace WebUI.Core.Services
{
    // 
    // 扩展数据模型 / Extended Data Models
    // 

    /// <summary>
    /// IBKR portfolio position (from reqPositions callback)
    /// </summary>
    public class IbkrPositionData
    {
        /// <summary>Account ID the position belongs to</summary>
        public string Account { get; set; } = string.Empty;
        /// <summary>IB contract ID</summary>
        public int ContractId { get; set; }
        /// <summary>Ticker symbol</summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>Security type: STK / FUT / OPT / FX etc.</summary>
        public string SecType { get; set; } = string.Empty;
        /// <summary>Currency (e.g. USD)</summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>Primary exchange</summary>
        public string Exchange { get; set; } = string.Empty;
        /// <summary>Local symbol as displayed in TWS</summary>
        public string LocalSymbol { get; set; } = string.Empty;
        /// <summary>Net position quantity (+ long, - short)</summary>
        public decimal Quantity { get; set; }
        /// <summary>Average fill cost per share/contract</summary>
        public decimal AverageCost { get; set; }
        /// <summary>Unrealised P&amp;L (calculated by client)</summary>
        public decimal UnrealisedPnl { get; set; }
    }

    /// <summary>
    /// Single tag-value pair from reqAccountSummary / reqAccountUpdates
    /// </summary>
    public class IbkrAccountValue
    {
        /// <summary>Tag name (e.g. "NetLiquidation")</summary>
        public string Key { get; set; } = string.Empty;
        /// <summary>String value of the tag</summary>
        public string Val { get; set; } = string.Empty;
        /// <summary>Currency of the value</summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>Account name</summary>
        public string AccountName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Open order information combined from openOrder + orderStatus callbacks
    /// </summary>
    public class IbkrOrderInfo
    {
        /// <summary>Order ID assigned by TWS</summary>
        public int OrderId { get; set; }
        /// <summary>Permanent IB order ID</summary>
        public int PermId { get; set; }
        /// <summary>Account that owns the order</summary>
        public string Account { get; set; } = string.Empty;
        /// <summary>Ticker symbol</summary>
        public string Symbol { get; set; } = string.Empty;
        /// <summary>Security type</summary>
        public string SecType { get; set; } = string.Empty;
        /// <summary>Currency</summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>Buy / Sell</summary>
        public string Action { get; set; } = string.Empty;
        /// <summary>Total order quantity</summary>
        public decimal TotalQuantity { get; set; }
        /// <summary>Order type: LMT / MKT / STP etc.</summary>
        public string OrderType { get; set; } = string.Empty;
        /// <summary>Limit price (0 if not applicable)</summary>
        public double LimitPrice { get; set; }
        /// <summary>Stop price (0 if not applicable)</summary>
        public double StopPrice { get; set; }
        /// <summary>Current TWS status string</summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>Filled quantity</summary>
        public decimal Filled { get; set; }
        /// <summary>Remaining quantity</summary>
        public decimal Remaining { get; set; }
        /// <summary>Average fill price</summary>
        public double AvgFillPrice { get; set; }
    }

    /// <summary>
    /// Result of placing an order
    /// </summary>
    public class IbkrPlaceOrderResult
    {
        /// <summary>True if the order was accepted by TWS</summary>
        public bool Success { get; set; }
        /// <summary>Order ID assigned by TWS</summary>
        public int OrderId { get; set; }
        /// <summary>Error message if Success is false</summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// One bar from reqHistoricalData
    /// </summary>
    public class IbkrBar
    {
        /// <summary>Bar timestamp (TWS format: "YYYYMMDD HH:mm:ss")</summary>
        public string Time { get; set; } = string.Empty;
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public decimal Volume { get; set; }
        public double Wap { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Market data tick snapshot for a symbol
    /// </summary>
    public class IbkrMarketDataTick
    {
        /// <summary>Request ID used in reqMktData</summary>
        public int ReqId { get; set; }
        /// <summary>Bid price (-1 if unavailable)</summary>
        public double Bid { get; set; } = -1;
        /// <summary>Ask price (-1 if unavailable)</summary>
        public double Ask { get; set; } = -1;
        /// <summary>Last trade price</summary>
        public double Last { get; set; } = -1;
        /// <summary>Bid size</summary>
        public decimal BidSize { get; set; }
        /// <summary>Ask size</summary>
        public decimal AskSize { get; set; }
        /// <summary>Last trade size</summary>
        public decimal LastSize { get; set; }
        /// <summary>Daily volume</summary>
        public decimal Volume { get; set; }
        /// <summary>Last update UTC</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    // 
    // IIbkrTwsApiClient  客户端接口
    // 

    /// <summary>
    /// Client interface for TWS/Gateway communication.
    /// Wraps the official IBApi (EClientSocket / EWrapper) into async-friendly methods.
    /// 将官方 IBApi 封装为 async 友好的接口。
    /// </summary>
    public interface IIbkrTwsApiClient
    {
        /// <summary>True when the underlying EClientSocket is connected.</summary>
        bool IsConnected { get; }

        /// <summary>TWS server version reported on handshake.</summary>
        int ServerVersion { get; }

        //  连接 / Connection 

        /// <summary>
        /// Open a persistent connection to TWS/Gateway and start the API session.
        /// Throws on failure. 建立到 TWS/Gateway 的长连接并启动 API 会话。
        /// </summary>
        Task ConnectAsync(string host, int port, int clientId, CancellationToken ct = default);

        /// <summary>Gracefully disconnect from TWS/Gateway. 优雅断开连接。</summary>
        void Disconnect();

        /// <summary>
        /// Request the server's current time as a connectivity check.
        /// Returns the Unix timestamp reported by TWS.
        /// 请求 TWS 服务器时间以验证连通性。
        /// </summary>
        Task<long> RequestCurrentTimeAsync(CancellationToken ct = default);

        //  账户 / Account 

        /// <summary>
        /// Fetch a structured account summary for the specified account group.
        /// Common tags: NetLiquidation, TotalCashValue, BuyingPower,
        ///   AvailableFunds, GrossPositionValue, MaintMarginReq, ExcessLiquidity.
        /// 获取账户摘要（净值/可用资金/购买力等）。
        /// </summary>
        Task<List<IbkrAccountValue>> GetAccountSummaryAsync(
            string group = "All",
            string? tags = null,   // null = use AccountSummaryTags.GetAllTags()
            CancellationToken ct = default);

        /// <summary>
        /// Fetch all account update key-value pairs for a specific account.
        /// Includes full detail: cash, margin, P&amp;L, etc.
        /// 获取账户详细更新数据（reqAccountUpdates）。
        /// </summary>
        Task<List<IbkrAccountValue>> GetAccountUpdatesAsync(
            string accountId, CancellationToken ct = default);

        //  持仓 / Positions 

        /// <summary>Fetch all portfolio positions across all accounts. 获取所有持仓。</summary>
        Task<List<IbkrPositionData>> GetPositionsAsync(CancellationToken ct = default);

        //  订单 / Orders 

        /// <summary>
        /// Request the next valid order ID from TWS.
        /// Must be called before placing any order. 请求下一个有效订单 ID。
        /// </summary>
        Task<int> GetNextValidOrderIdAsync(CancellationToken ct = default);

        /// <summary>Place an order. Returns order ID on success. 下单。</summary>
        Task<IbkrPlaceOrderResult> PlaceOrderAsync(
            Contract contract, Order order, CancellationToken ct = default);

        /// <summary>Cancel an open order by order ID. 撤销订单。</summary>
        void CancelOrder(int orderId, string? manualOrderCancelTime = null);

        /// <summary>Fetch all open orders for the current client session. 获取未成交订单。</summary>
        Task<List<IbkrOrderInfo>> GetOpenOrdersAsync(CancellationToken ct = default);

        /// <summary>
        /// Fetch all open orders across all client IDs (requires FA/advisor account).
        /// 获取所有客户端 ID 的未成交订单（需要投顾账户权限）。
        /// </summary>
        Task<List<IbkrOrderInfo>> GetAllOpenOrdersAsync(CancellationToken ct = default);

        //  合约 / Contracts 

        /// <summary>
        /// Fetch contract details from TWS (validates symbol, gets multiplier, etc.).
        /// 获取合约详情（验证 symbol、获取乘数等）。
        /// </summary>
        Task<List<ContractDetails>> GetContractDetailsAsync(
            Contract contract, CancellationToken ct = default);

        //  历史数据 / Historical Data 

        /// <summary>
        /// Fetch historical OHLCV bars.
        /// endDateTime: "" = now; durationStr: "1 D"/"1 W"/"1 M"; barSizeSetting: "1 min"/"1 hour".
        /// 获取历史 K 线数据。
        /// </summary>
        Task<List<IbkrBar>> GetHistoricalDataAsync(
            Contract contract,
            string endDateTime,
            string durationStr,
            string barSizeSetting,
            string whatToShow = "TRADES",
            int useRTH = 1,
            CancellationToken ct = default);

        //  实时行情 / Market Data 

        /// <summary>
        /// Subscribe to real-time Level 1 market data.
        /// Returns reqId; pass it to UnsubscribeMarketData to cancel.
        /// 订阅实时行情（Level 1）。返回 reqId，用于取消订阅。
        /// </summary>
        int SubscribeMarketData(Contract contract, string genericTickList = "");

        /// <summary>Cancel a real-time market data subscription. 取消实时行情订阅。</summary>
        void UnsubscribeMarketData(int reqId);

        //  事件 / Events 

        /// <summary>Fired on every market data price/size tick. 收到行情 tick 时触发。</summary>
        event Action<IbkrMarketDataTick>? OnMarketDataTick;

        /// <summary>Fired when an order status changes. 订单状态变化时触发。</summary>
        event Action<IbkrOrderInfo>? OnOrderStatusChanged;

        /// <summary>Fired on position updates from reqPositions subscription. 持仓更新时触发。</summary>
        event Action<IbkrPositionData>? OnPositionUpdated;

        /// <summary>Fired on connection loss or fatal error. 断线或严重错误时触发。</summary>
        event Action<string>? OnConnectionLost;

        /// <summary>Fired for any TWS error (fatal and informational). TWS 报错时触发。</summary>
        event Action<int, int, string>? OnError;
    }

    // 
    // IbkrWrapper  DefaultEWrapper 子类，桥接回调  async
    // 

    /// <summary>
    /// Extends <see cref="DefaultEWrapper"/> to bridge every IB callback into either
    /// a <see cref="TaskCompletionSource{T}"/> (for one-shot requests) or an event
    /// (for streaming/real-time data).
    /// 继承 DefaultEWrapper，将所有 IB 回调桥接为 TaskCompletionSource 或事件。
    /// </summary>
    internal sealed class IbkrWrapper : DefaultEWrapper
    {
        private readonly ILogger _logger;

        //  nextValidId 
        private volatile TaskCompletionSource<int>? _nextValidIdTcs;

        //  currentTime 
        private volatile TaskCompletionSource<long>? _currentTimeTcs;

        //  reqPositions 
        private volatile TaskCompletionSource<List<IbkrPositionData>>? _positionsTcs;
        private List<IbkrPositionData>? _pendingPositions;
        private readonly object _positionsLock = new();

        //  reqOpenOrders / reqAllOpenOrders 
        private volatile TaskCompletionSource<List<IbkrOrderInfo>>? _openOrdersTcs;
        private List<IbkrOrderInfo>? _pendingOrders;
        private readonly object _ordersLock = new();

        //  reqAccountSummary (keyed by reqId) 
        private readonly ConcurrentDictionary<int,
            (List<IbkrAccountValue> Values, TaskCompletionSource<List<IbkrAccountValue>> Tcs)>
            _accountSummaryReqs = new();

        //  reqAccountUpdates 
        private volatile TaskCompletionSource<List<IbkrAccountValue>>? _accountUpdatesTcs;
        private volatile string? _accountUpdatesAccount;
        private List<IbkrAccountValue>? _pendingAccountValues;
        private readonly object _accountUpdatesLock = new();

        //  reqContractDetails (keyed by reqId) 
        private readonly ConcurrentDictionary<int,
            (List<ContractDetails> Details, TaskCompletionSource<List<ContractDetails>> Tcs)>
            _contractDetailsReqs = new();

        //  reqHistoricalData (keyed by reqId) 
        private readonly ConcurrentDictionary<int,
            (List<IbkrBar> Bars, TaskCompletionSource<List<IbkrBar>> Tcs)>
            _historicalDataReqs = new();

        //  Market data (streaming, keyed by reqId) 
        private readonly ConcurrentDictionary<int, IbkrMarketDataTick> _marketDataTicks = new();

        //  Public events 
        public event Action<IbkrMarketDataTick>? OnMarketDataTick;
        public event Action<IbkrOrderInfo>? OnOrderStatusChanged;
        public event Action<IbkrPositionData>? OnPositionUpdated;
        public event Action<string>? OnConnectionLost;
        public event Action<int, int, string>? OnError;

        public IbkrWrapper(ILogger logger) => _logger = logger;

        //  TCS registration helpers 

        public TaskCompletionSource<int> RegisterNextValidId()
        {
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            _nextValidIdTcs = tcs;
            return tcs;
        }

        public TaskCompletionSource<long> RegisterCurrentTime()
        {
            var tcs = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
            _currentTimeTcs = tcs;
            return tcs;
        }

        public TaskCompletionSource<List<IbkrPositionData>> RegisterPositions()
        {
            var tcs = new TaskCompletionSource<List<IbkrPositionData>>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_positionsLock) { _pendingPositions = new(); _positionsTcs = tcs; }
            return tcs;
        }

        public TaskCompletionSource<List<IbkrOrderInfo>> RegisterOpenOrders()
        {
            var tcs = new TaskCompletionSource<List<IbkrOrderInfo>>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_ordersLock) { _pendingOrders = new(); _openOrdersTcs = tcs; }
            return tcs;
        }

        public TaskCompletionSource<List<IbkrAccountValue>> RegisterAccountSummary(int reqId)
        {
            var tcs = new TaskCompletionSource<List<IbkrAccountValue>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _accountSummaryReqs[reqId] = (new List<IbkrAccountValue>(), tcs);
            return tcs;
        }

        public TaskCompletionSource<List<IbkrAccountValue>> RegisterAccountUpdates(string accountId)
        {
            var tcs = new TaskCompletionSource<List<IbkrAccountValue>>(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_accountUpdatesLock)
            {
                _pendingAccountValues  = new();
                _accountUpdatesAccount = accountId;
                _accountUpdatesTcs     = tcs;
            }
            return tcs;
        }

        public TaskCompletionSource<List<ContractDetails>> RegisterContractDetails(int reqId)
        {
            var tcs = new TaskCompletionSource<List<ContractDetails>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _contractDetailsReqs[reqId] = (new List<ContractDetails>(), tcs);
            return tcs;
        }

        public TaskCompletionSource<List<IbkrBar>> RegisterHistoricalData(int reqId)
        {
            var tcs = new TaskCompletionSource<List<IbkrBar>>(TaskCreationOptions.RunContinuationsAsynchronously);
            _historicalDataReqs[reqId] = (new List<IbkrBar>(), tcs);
            return tcs;
        }

        public void RemoveMarketDataTick(int reqId) => _marketDataTicks.TryRemove(reqId, out _);

        // 
        // EWrapper callbacks
        // 

        public override void nextValidId(int orderId)
        {
            _logger.LogDebug("IBApi nextValidId={OrderId}", orderId);
            _nextValidIdTcs?.TrySetResult(orderId);
        }

        public override void currentTime(long time)
        {
            _logger.LogDebug("IBApi currentTime={Time}", time);
            _currentTimeTcs?.TrySetResult(time);
        }

        public override void managedAccounts(string accountsList)
            => _logger.LogInformation("IBApi managedAccounts: {Accounts}", accountsList);

        //  Positions 

        public override void position(string account, Contract contract, decimal pos, double avgCost)
        {
            var data = new IbkrPositionData
            {
                Account     = account,
                ContractId  = contract.ConId,
                Symbol      = string.IsNullOrEmpty(contract.LocalSymbol) ? contract.Symbol : contract.LocalSymbol,
                SecType     = contract.SecType,
                Currency    = contract.Currency,
                Exchange    = contract.Exchange,
                LocalSymbol = contract.LocalSymbol,
                Quantity    = pos,
                AverageCost = (decimal)avgCost
            };
            OnPositionUpdated?.Invoke(data);
            lock (_positionsLock) { _pendingPositions?.Add(data); }
        }

        public override void positionEnd()
        {
            TaskCompletionSource<List<IbkrPositionData>>? tcs;
            List<IbkrPositionData>? result;
            lock (_positionsLock)
            {
                tcs = _positionsTcs; result = _pendingPositions ?? new();
                _positionsTcs = null; _pendingPositions = null;
            }
            tcs?.TrySetResult(result);
        }

        //  Account Summary 

        public override void accountSummary(int reqId, string account, string tag, string value, string currency)
        {
            if (_accountSummaryReqs.TryGetValue(reqId, out var entry))
                entry.Values.Add(new IbkrAccountValue { Key = tag, Val = value, Currency = currency, AccountName = account });
        }

        public override void accountSummaryEnd(int reqId)
        {
            if (_accountSummaryReqs.TryRemove(reqId, out var entry))
                entry.Tcs.TrySetResult(entry.Values);
        }

        //  Account Updates 

        public override void updateAccountValue(string key, string value, string currency, string accountName)
        {
            lock (_accountUpdatesLock)
            {
                if (_pendingAccountValues != null &&
                    (string.IsNullOrEmpty(_accountUpdatesAccount) || accountName == _accountUpdatesAccount))
                {
                    _pendingAccountValues.Add(new IbkrAccountValue { Key = key, Val = value, Currency = currency, AccountName = accountName });
                }
            }
        }

        public override void accountDownloadEnd(string account)
        {
            _logger.LogDebug("IBApi accountDownloadEnd account={Account}", account);
            TaskCompletionSource<List<IbkrAccountValue>>? tcs;
            List<IbkrAccountValue>? result;
            lock (_accountUpdatesLock)
            {
                tcs = _accountUpdatesTcs; result = _pendingAccountValues ?? new();
                _accountUpdatesTcs = null; _pendingAccountValues = null; _accountUpdatesAccount = null;
            }
            tcs?.TrySetResult(result);
        }

        //  Open Orders 

        public override void openOrder(int orderId, Contract contract, Order order, OrderState orderState)
        {
            var info = new IbkrOrderInfo
            {
                OrderId       = orderId,
                PermId        = (int)order.PermId,
                Account       = order.Account,
                Symbol        = contract.Symbol,
                SecType       = contract.SecType,
                Currency      = contract.Currency,
                Action        = order.Action,
                TotalQuantity = order.TotalQuantity,
                OrderType     = order.OrderType,
                LimitPrice    = order.LmtPrice,
                StopPrice     = order.AuxPrice,
                Status        = orderState.Status
            };
            lock (_ordersLock) { _pendingOrders?.Add(info); }
        }

        public override void openOrderEnd()
        {
            TaskCompletionSource<List<IbkrOrderInfo>>? tcs;
            List<IbkrOrderInfo>? result;
            lock (_ordersLock)
            {
                tcs = _openOrdersTcs; result = _pendingOrders ?? new();
                _openOrdersTcs = null; _pendingOrders = null;
            }
            tcs?.TrySetResult(result);
        }

        public override void orderStatus(
            int orderId, string status, decimal filled, decimal remaining,
            double avgFillPrice, long permId, int parentId,
            double lastFillPrice, int clientId, string whyHeld, double mktCapPrice)
        {
            OnOrderStatusChanged?.Invoke(new IbkrOrderInfo
            {
                OrderId      = orderId,
                PermId       = (int)permId,
                Status       = status,
                Filled       = filled,
                Remaining    = remaining,
                AvgFillPrice = avgFillPrice
            });
        }

        //  Contract Details 

        public override void contractDetails(int reqId, ContractDetails contractDetails)
        {
            if (_contractDetailsReqs.TryGetValue(reqId, out var entry))
                entry.Details.Add(contractDetails);
        }

        public override void contractDetailsEnd(int reqId)
        {
            if (_contractDetailsReqs.TryRemove(reqId, out var entry))
                entry.Tcs.TrySetResult(entry.Details);
        }

        //  Historical Data 

        public override void historicalData(int reqId, Bar bar)
        {
            if (_historicalDataReqs.TryGetValue(reqId, out var entry))
                entry.Bars.Add(new IbkrBar
                {
                    Time = bar.Time, Open = bar.Open, High = bar.High,
                    Low  = bar.Low,  Close = bar.Close, Volume = bar.Volume,
                    Wap  = (double)bar.WAP,  Count  = bar.Count
                });
        }

        public override void historicalDataEnd(int reqId, string startDateStr, string endDateStr)
        {
            if (_historicalDataReqs.TryRemove(reqId, out var entry))
                entry.Tcs.TrySetResult(entry.Bars);
        }

        //  Real-Time Market Data 

        public override void tickPrice(int tickerId, int field, double price, TickAttrib attribs)
        {
            var tick = _marketDataTicks.GetOrAdd(tickerId, id => new IbkrMarketDataTick { ReqId = id });
            if (field == TickType.BID)  tick.Bid  = price;
            if (field == TickType.ASK)  tick.Ask  = price;
            if (field == TickType.LAST) tick.Last  = price;
            tick.UpdatedAt = DateTime.UtcNow;
            OnMarketDataTick?.Invoke(tick);
        }

        public override void tickSize(int tickerId, int field, decimal size)
        {
            var tick = _marketDataTicks.GetOrAdd(tickerId, id => new IbkrMarketDataTick { ReqId = id });
            if (field == TickType.BID_SIZE)  tick.BidSize  = size;
            if (field == TickType.ASK_SIZE)  tick.AskSize  = size;
            if (field == TickType.LAST_SIZE) tick.LastSize = size;
            if (field == TickType.VOLUME)    tick.Volume   = size;
            tick.UpdatedAt = DateTime.UtcNow;
            OnMarketDataTick?.Invoke(tick);
        }

        //  Errors 

        public override void error(Exception e)
        {
            _logger.LogError(e, "IBApi unhandled exception");
            OnConnectionLost?.Invoke(e.Message);
            FailAllPendingTcs(e.Message);
        }

        public override void error(string str) => _logger.LogWarning("IBApi message: {Msg}", str);

        public override void error(int id, long errorTime, int errorCode, string errorMsg, string advancedOrderRejectJson)
        {
            // Error codes:
            //  200 = No security definition found
            //  326 = ClientId already in use
            //  502 = Couldn't connect to TWS
            //  504 = Not connected
            //  2103-2110 = Market data farm connectivity (informational)
            bool isFatal = errorCode is 502 or 504 or 326;
            bool isInfo  = errorCode is >= 2100 and <= 2110;

            if (isInfo)
                _logger.LogDebug("IBApi info code={Code}: {Msg}", errorCode, errorMsg);
            else
                _logger.LogWarning("IBApi error id={Id} code={Code}: {Msg}", id, errorCode, errorMsg);

            OnError?.Invoke(id, errorCode, errorMsg);

            if (isFatal)
            {
                OnConnectionLost?.Invoke($"[{errorCode}] {errorMsg}");
                FailAllPendingTcs($"IBApi fatal error {errorCode}: {errorMsg}");
            }
            else
            {
                // Fail the specific per-reqId TCS if one exists
                FailRequestTcs(id, errorCode, errorMsg);
            }
        }

        //  Internal helpers 

        /// <summary>Fail any per-reqId TCS that matches the given id.</summary>
        private void FailRequestTcs(int reqId, int errorCode, string msg)
        {
            var ex = new IBApiException(errorCode, msg);
            if (_contractDetailsReqs.TryRemove(reqId, out var cd)) cd.Tcs.TrySetException(ex);
            if (_historicalDataReqs.TryRemove(reqId, out var hd))  hd.Tcs.TrySetException(ex);
            if (_accountSummaryReqs.TryRemove(reqId, out var ac))  ac.Tcs.TrySetException(ex);
        }

        /// <summary>Fail all pending TCS instances on fatal disconnect.</summary>
        private void FailAllPendingTcs(string reason)
        {
            var ex = new Exception(reason);
            _nextValidIdTcs?.TrySetException(ex);
            _currentTimeTcs?.TrySetException(ex);
            lock (_positionsLock)     { _positionsTcs?.TrySetException(ex);     _positionsTcs = null; }
            lock (_ordersLock)        { _openOrdersTcs?.TrySetException(ex);    _openOrdersTcs = null; }
            lock (_accountUpdatesLock){ _accountUpdatesTcs?.TrySetException(ex); _accountUpdatesTcs = null; }
            foreach (var kv in _accountSummaryReqs)  kv.Value.Tcs.TrySetException(ex);
            foreach (var kv in _contractDetailsReqs) kv.Value.Tcs.TrySetException(ex);
            foreach (var kv in _historicalDataReqs)  kv.Value.Tcs.TrySetException(ex);
            _accountSummaryReqs.Clear();
            _contractDetailsReqs.Clear();
            _historicalDataReqs.Clear();
        }
    }

    // 
    // IBApiException  携带 IB 错误码的异常
    // 

    /// <summary>Exception carrying an IB-specific error code. / 携带 IB 错误码的异常。</summary>
    public sealed class IBApiException : Exception
    {
        /// <summary>IB error code (e.g. 200, 326, 502, 504).</summary>
        public int ErrorCode { get; }
        public IBApiException(int errorCode, string message)
            : base($"[IBApi {errorCode}] {message}") => ErrorCode = errorCode;
    }

    // 
    // IbkrTwsApiClient  主客户端实现（长连接）
    // 

    /// <summary>
    /// Manages a single persistent connection to TWS/Gateway using the official IBApi
    /// EClientSocket. Provides async wrappers for all common operations.
    ///
    /// 使用官方 IBApi EClientSocket 管理到 TWS/Gateway 的长连接，
    /// 为所有常用操作提供 async 封装。
    /// </summary>
    public sealed class IbkrTwsApiClient : IIbkrTwsApiClient, IDisposable
    {
        private readonly ILogger<IbkrTwsApiClient> _logger;
        private readonly IbkrWrapper _wrapper;
        private readonly EReaderMonitorSignal _signal;
        private EClientSocket? _client;
        private EReader? _reader;
        private Thread? _processingThread;

        /// <summary>
        /// Atomic request ID counter; starts at 1000 to avoid clashes with order IDs.
        /// 原子请求 ID 计数器，从 1000 开始以避免与订单 ID 冲突。
        /// </summary>
        private int _reqIdCounter = 1000;
        private bool _disposed;

        /// <inheritdoc />
        public bool IsConnected => _client?.IsConnected() ?? false;

        /// <inheritdoc />
        public int ServerVersion => _client?.ServerVersion ?? 0;

        //  Events forwarded from wrapper 
        public event Action<IbkrMarketDataTick>? OnMarketDataTick;
        public event Action<IbkrOrderInfo>? OnOrderStatusChanged;
        public event Action<IbkrPositionData>? OnPositionUpdated;
        public event Action<string>? OnConnectionLost;
        public event Action<int, int, string>? OnError;

        public IbkrTwsApiClient(ILogger<IbkrTwsApiClient> logger)
        {
            _logger  = logger;
            _signal  = new EReaderMonitorSignal();
            _wrapper = new IbkrWrapper(logger);

            // Forward wrapper events outward
            _wrapper.OnMarketDataTick     += tick  => OnMarketDataTick?.Invoke(tick);
            _wrapper.OnOrderStatusChanged += order => OnOrderStatusChanged?.Invoke(order);
            _wrapper.OnPositionUpdated    += pos   => OnPositionUpdated?.Invoke(pos);
            _wrapper.OnConnectionLost     += msg   => OnConnectionLost?.Invoke(msg);
            _wrapper.OnError              += (id, code, msg) => OnError?.Invoke(id, code, msg);
        }

        //  Connection 

        /// <inheritdoc />
        public Task ConnectAsync(string host, int port, int clientId, CancellationToken ct = default)
        {
            if (IsConnected)
            {
                _logger.LogWarning("ConnectAsync: already connected  disconnecting first");
                Disconnect();
            }

            _logger.LogInformation(
                "Connecting to TWS/Gateway {Host}:{Port} clientId={ClientId}", host, port, clientId);

            _client = new EClientSocket(_wrapper, _signal);

            // Register nextValidId TCS BEFORE eConnect so we never miss the first callback
            var nextIdTcs = _wrapper.RegisterNextValidId();

            _client.eConnect(host, port, clientId);

            if (!_client.IsConnected())
                throw new InvalidOperationException(
                    $"EClientSocket.eConnect failed for {host}:{port}. " +
                    "Ensure TWS/Gateway is running and API connections are enabled.");

            // Start EReader on its own background thread
            _reader = new EReader(_client, _signal);
            _reader.Start();

            _processingThread = new Thread(() =>
            {
                _logger.LogDebug("EReader processing thread started");
                while (_client.IsConnected())
                {
                    _signal.waitForSignal();
                    try { _reader.processMsgs(); }
                    catch (Exception ex) { _logger.LogError(ex, "EReader processMsgs error"); }
                }
                _logger.LogDebug("EReader processing thread stopped");
            })
            { IsBackground = true, Name = "IBApi-EReader" };
            _processingThread.Start();

            // Wait up to 10 s for nextValidId, which confirms the API session is fully ready
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct);
            linked.CancelAfter(TimeSpan.FromSeconds(10));
            return nextIdTcs.Task.WaitAsync(linked.Token);
        }

        /// <inheritdoc />
        public void Disconnect()
        {
            if (_client?.IsConnected() == true)
            {
                _logger.LogInformation("Disconnecting from TWS/Gateway");
                _client.eDisconnect();
            }
            _client = null;
            _reader = null;
        }

        //  Connectivity check 

        /// <inheritdoc />
        public async Task<long> RequestCurrentTimeAsync(CancellationToken ct = default)
        {
            EnsureConnected();
            var tcs = _wrapper.RegisterCurrentTime();
            _client!.reqCurrentTime();
            return await tcs.Task.WaitAsync(ct);
        }

        //  Account 

        /// <inheritdoc />
        public async Task<List<IbkrAccountValue>> GetAccountSummaryAsync(
            string group = "All",
            string? tags  = null,
            CancellationToken ct = default)
        {
            EnsureConnected();
            int reqId = NextReqId();
            var tcs   = _wrapper.RegisterAccountSummary(reqId);
            var resolvedTags = tags ?? AccountSummaryTags.GetAllTags();
            _logger.LogDebug("reqAccountSummary reqId={ReqId} group={Group}", reqId, group);
            _client!.reqAccountSummary(reqId, group, resolvedTags);
            try   { return await tcs.Task.WaitAsync(ct); }
            finally { try { _client?.cancelAccountSummary(reqId); } catch { /* best-effort */ } }
        }

        /// <inheritdoc />
        public async Task<List<IbkrAccountValue>> GetAccountUpdatesAsync(
            string accountId, CancellationToken ct = default)
        {
            EnsureConnected();
            var tcs = _wrapper.RegisterAccountUpdates(accountId);
            _logger.LogDebug("reqAccountUpdates account={Account}", accountId);
            _client!.reqAccountUpdates(true, accountId);
            try   { return await tcs.Task.WaitAsync(ct); }
            finally { try { _client?.reqAccountUpdates(false, accountId); } catch { /* best-effort */ } }
        }

        //  Positions 

        /// <inheritdoc />
        public async Task<List<IbkrPositionData>> GetPositionsAsync(CancellationToken ct = default)
        {
            EnsureConnected();
            var tcs = _wrapper.RegisterPositions();
            _logger.LogDebug("reqPositions");
            _client!.reqPositions();
            try   { return await tcs.Task.WaitAsync(ct); }
            finally { try { _client?.cancelPositions(); } catch { /* best-effort */ } }
        }

        //  Orders 

        /// <inheritdoc />
        public async Task<int> GetNextValidOrderIdAsync(CancellationToken ct = default)
        {
            EnsureConnected();
            var tcs = _wrapper.RegisterNextValidId();
            _client!.reqIds(-1);    // -1 = request exactly one new ID
            return await tcs.Task.WaitAsync(ct);
        }

        /// <inheritdoc />
        public async Task<IbkrPlaceOrderResult> PlaceOrderAsync(
            Contract contract, Order order, CancellationToken ct = default)
        {
            EnsureConnected();
            try
            {
                int orderId = await GetNextValidOrderIdAsync(ct);
                order.Account ??= string.Empty;

                _logger.LogInformation(
                    "placeOrder id={Id} {Action} {Qty} {Symbol} {Type} lmt={Lmt}",
                    orderId, order.Action, order.TotalQuantity,
                    contract.Symbol, order.OrderType, order.LmtPrice);

                _client!.placeOrder(orderId, contract, order);
                return new IbkrPlaceOrderResult { Success = true, OrderId = orderId };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "placeOrder failed for {Symbol}", contract.Symbol);
                return new IbkrPlaceOrderResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        /// <inheritdoc />
        public void CancelOrder(int orderId, string? manualOrderCancelTime = null)
        {
            EnsureConnected();
            _logger.LogInformation("cancelOrder id={OrderId}", orderId);
            _client!.cancelOrder(orderId, new OrderCancel
            {
                ManualOrderCancelTime = manualOrderCancelTime ?? string.Empty
            });
        }

        /// <inheritdoc />
        public async Task<List<IbkrOrderInfo>> GetOpenOrdersAsync(CancellationToken ct = default)
        {
            EnsureConnected();
            var tcs = _wrapper.RegisterOpenOrders();
            _logger.LogDebug("reqOpenOrders");
            _client!.reqOpenOrders();
            return await tcs.Task.WaitAsync(ct);
        }

        /// <inheritdoc />
        public async Task<List<IbkrOrderInfo>> GetAllOpenOrdersAsync(CancellationToken ct = default)
        {
            EnsureConnected();
            var tcs = _wrapper.RegisterOpenOrders();
            _logger.LogDebug("reqAllOpenOrders");
            _client!.reqAllOpenOrders();
            return await tcs.Task.WaitAsync(ct);
        }

        //  Contracts 

        /// <inheritdoc />
        public async Task<List<ContractDetails>> GetContractDetailsAsync(
            Contract contract, CancellationToken ct = default)
        {
            EnsureConnected();
            int reqId = NextReqId();
            var tcs   = _wrapper.RegisterContractDetails(reqId);
            _logger.LogDebug("reqContractDetails reqId={ReqId} symbol={Symbol}", reqId, contract.Symbol);
            _client!.reqContractDetails(reqId, contract);
            return await tcs.Task.WaitAsync(ct);
        }

        //  Historical Data 

        /// <inheritdoc />
        public async Task<List<IbkrBar>> GetHistoricalDataAsync(
            Contract contract,
            string endDateTime,
            string durationStr,
            string barSizeSetting,
            string whatToShow = "TRADES",
            int useRTH = 1,
            CancellationToken ct = default)
        {
            EnsureConnected();
            int reqId = NextReqId();
            var tcs   = _wrapper.RegisterHistoricalData(reqId);

            _logger.LogDebug(
                "reqHistoricalData reqId={ReqId} {Symbol} duration={D} bar={B} show={W}",
                reqId, contract.Symbol, durationStr, barSizeSetting, whatToShow);

            _client!.reqHistoricalData(
                reqId, contract, endDateTime, durationStr,
                barSizeSetting, whatToShow, useRTH,
                formatDate: 1, keepUpToDate: false,
                chartOptions: new List<TagValue>());

            return await tcs.Task.WaitAsync(ct);
        }

        //  Market Data 

        /// <inheritdoc />
        public int SubscribeMarketData(Contract contract, string genericTickList = "")
        {
            EnsureConnected();
            int reqId = NextReqId();
            _logger.LogDebug("reqMktData reqId={ReqId} symbol={Symbol}", reqId, contract.Symbol);
            _client!.reqMktData(
                reqId, contract, genericTickList,
                snapshot: false, regulatorySnaphsot: false,
                mktDataOptions: new List<TagValue>());
            return reqId;
        }

        /// <inheritdoc />
        public void UnsubscribeMarketData(int reqId)
        {
            if (_client?.IsConnected() == true)
            {
                _logger.LogDebug("cancelMktData reqId={ReqId}", reqId);
                _client.cancelMktData(reqId);
            }
            _wrapper.RemoveMarketDataTick(reqId);
        }

        //  Helpers 

        /// <summary>Get next unique request ID (thread-safe). 获取下一个唯一请求 ID（线程安全）。</summary>
        private int NextReqId() => Interlocked.Increment(ref _reqIdCounter);

        /// <summary>Throw if not connected. 未连接时抛出。</summary>
        private void EnsureConnected()
        {
            if (!IsConnected)
                throw new InvalidOperationException(
                    "IbkrTwsApiClient is not connected to TWS/Gateway.");
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Disconnect();
        }
    }
}
