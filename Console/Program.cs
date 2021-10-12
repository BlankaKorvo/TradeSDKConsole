using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using TinkoffAdapter.DataHelper;
using DataCollector;
using CandleInterval = MarketDataModules.Models.Candles.CandleInterval;
using MarketDataModules;
using Analysis.Screeners;
using System.Linq;
using System.IO;
using TinkoffAdapter.Authority;
using MarketDataModules.Models.Candles;
using TinkoffData;
using Skender.Stock.Indicators;
using TradingAlgorithms.IndicatorSignals;
using Instrument = MarketDataModules.Models.Instruments.Instrument;
using LinqStatistics;
using Analysis.Screeners.CandlesScreener;
using Analysis.Screeners.Helpers;
using TradeTarget = MarketDataModules.Models.TradeTarget;
using Analysis.TradeDecision;
using Trader;
using System.Threading;
using MarketDataModules.Models;
using MarketDataModules.Models.Portfolio;
using MarketDataModules.Models.Operation;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Month, fileSizeLimitBytes: 304857600, rollOnFileSizeLimit: true)
                .CreateLogger();
            MarketDataCollector marketDataCollector = new MarketDataCollector();
            GetStocksHistory getStocksHistory = new GetStocksHistory();
            VolumeProfileScreener volumeProfileScreener = new VolumeProfileScreener();
            VolumeIncreaseScreener volumeIncreaseScreener = new VolumeIncreaseScreener();
            TwoEmaScreener twoEmaScreener = new TwoEmaScreener();
            StochDivScreener stochDivScreener = new StochDivScreener();
            OrderbookScreener orderbookScreener = new OrderbookScreener();
            Signal signal = new Signal();

            /// AutoTrading
            //List<string> Tickers = new List<string> { "AMZN", "AAPL", "GOOG", "CLOV" };
            //List<Instrument> instruments = new List<Instrument>();
            //foreach (var item in Tickers)
            //{
            //    instruments.Add(await marketDataCollector.GetInstrumentByTickerAsync(item));
            //}
            //InstrumentList instrumentList = new InstrumentList(instruments.Count, instruments);
            //AutoTrading autoTrading = new AutoTrading() { CandleInterval = CandleInterval.TwoMinutes, CandlesCount = 70 };
            //await autoTrading.AutoTradingInstruments(instrumentList, 2);
            /// AutoTrading

            ///// Screener OrderBook
            //InstrumentList instrumentList = await marketDataCollector.GetInstrumentListAsync();
            //List<Instrument> instrumentsUSD = (from instrument in instrumentList.Instruments
            //                                   where instrument.Currency == Currency.Usd
            //                                   select instrument)
            //                               .ToList();

            //List<Orderbook> orderbooks = new List<Orderbook>();

            //foreach (var item in instrumentsUSD)
            //{
            //    var element = await marketDataCollector.GetOrderbookAsync(item.Figi);
            //    if (element == null)
            //        continue;
            //    else
            //        orderbooks.Add(element);
            //}

            //List<string> FigiLong = orderbookScreener.OrderbookLong(orderbooks, 10);
            //foreach (var item in FigiLong)
            //{
            //    var result = await marketDataCollector.GetInstrumentByFigi(item);
            //    Console.WriteLine(result.Ticker);
            //}
            ///// Screener OrderBook

            ///Test Algo
            //List<TradeInstrument> tradeInstrumentList = new List<TradeInstrument>();
            List<Instrument> instrumentList = new List<Instrument>();
            TradeOperation tradeOperation = null;
            Portfolio.Position portfolioPosition = null;
            TradeTarget lastTradeTarget = TradeTarget.fromLong;

            CandleInterval candleInterval = CandleInterval.QuarterHour;
            int candlesCount = 100;
            List<string> Tickers = new List<string> { "AMZN"};
            foreach (var item in Tickers)
            {
                var instrument = await marketDataCollector.GetInstrumentByTickerAsync(item);
                //TradeInstrument tradeInstrument = new TradeInstrument() { figi = instrument.Figi, tradeTarget = TradeTarget.fromLong, ticker = instrument.Ticker};
                //LastTransactionModel lastTransactionModel = new LastTransactionModel() {Figi = instrument.Figi, TradeOperation = TradeOperation. }
                instrumentList.Add(instrument);
            }

            while (true)
            {
                //int hour = DateTime.Now.Hour;
                //int minutes = DateTime.Now.Minute;
                //if
                //    (
                //    hour >= 17
                //    &&
                //    hour < 23
                //    )
                //{
                TestTrading(marketDataCollector, instrumentList, candleInterval, candlesCount, ref tradeOperation, ref portfolioPosition, ref lastTradeTarget);

            }
        }

        private static void TestTrading(MarketDataCollector marketDataCollector, List<Instrument> instrumentList, CandleInterval candleInterval, int candlesCount, ref TradeOperation tradeOperation, ref Portfolio.Position portfolioPosition, ref TradeTarget lastTradeTarget)
        {
            foreach (var item in instrumentList)
            {
                Log.Information("Start trade: " + item.Figi);
                var orderbook = marketDataCollector.GetOrderbookAsync(item.Figi, Provider.Tinkoff, 50).GetAwaiter().GetResult();

                if (orderbook == null)
                {
                    Log.Information("Orderbook null");
                    continue;
                }

                var bestAsk = orderbook.Asks.FirstOrDefault().Price;
                var bestBid = orderbook.Bids.FirstOrDefault().Price;

                var candleList = marketDataCollector.GetCandlesAsync(item.Figi, candleInterval, candlesCount).GetAwaiter().GetResult();

                //Portfolio portfolio = marketDataCollector.GetPortfolioAsync().GetAwaiter().GetResult();
                //List<TradeOperation> tradeOperations = marketDataCollector.GetOperationsAsync(item.figi, DateTime.Now, DateTime.Now.AddDays(-100)).GetAwaiter().GetResult();
                //List<TradeOperation> tradeOperations = new List<TradeOperation>();
                //tradeOperations.Add(tradeOperation);

                //Portfolio.Position position = null;
                //foreach (Portfolio.Position itemP in portfolio.Positions)
                //{
                //    if (itemP.Figi == item.figi)
                //    {
                //        position = itemP;
                //    }
                //}
                List<TradeOperation> tradeOperationResult = new List<TradeOperation>();
                tradeOperationResult.Add(tradeOperation);

                //MoneyAmount averagePositionPrice = item.MoneyAmountT;
                //List<TradeOperation> tradeOperationResult = new List<TradeOperation> { tradeOperation };
                //portfolioPosition = new Portfolio.Position(portfolioPosition.Name, portfolioPosition.Figi, portfolioPosition.Ticker, portfolioPosition.Isin, portfolioPosition.InstrumentType, portfolioPosition.Balance, portfolioPosition.Blocked, portfolioPosition.ExpectedYield, portfolioPosition.Lots, averagePositionPrice, portfolioPosition.AveragePositionPriceNoNkd);
                //GmmaDecisionOneMinutes gmmaDecision = new GmmaDecisionOneMinutes() { candleList = candleList, orderbook = orderbook, bestAsk = bestAsk, bestBid = bestBid };
                GmmaDecision gmmaDecisionOneMinutes = new GmmaDecision () { candleList = candleList, orderbook = orderbook, bestAsk = bestAsk, bestBid = bestBid, portfolioPosition = portfolioPosition, tradeOperations = tradeOperationResult };
                TradeTarget tradeVariant = gmmaDecisionOneMinutes.TradeVariant();

                //var gmmaSignalResult = signal.GmmaSignal(candleList, bestAsk , bestBid);

                if (tradeVariant == TradeTarget.toLong
                    &&
                    portfolioPosition == null
                    )
                {
                    int countBalance = 1;
                    portfolioPosition = new Portfolio.Position(default, item.Figi, item.Ticker, item.Isin, default, countBalance, default, new MoneyAmount(Currency.Usd, bestAsk), countBalance, new MoneyAmount(Currency.Usd, bestAsk), default);
                    tradeOperation = new TradeOperation(default, default, default, default, default, default, bestAsk, default, default, item.Figi, default, default, DateTime.Now.ToUniversalTime(), default);

                    using (StreamWriter sw = new StreamWriter("_operation " + item.Ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" Long " + item.Ticker + "price " + bestAsk);
                        sw.WriteLine();
                    }

                    Log.Information("Stop trade: " + item.Figi + " TradeOperation.toLong");
                }

                if (tradeVariant == TradeTarget.fromLong
                    &&
                    portfolioPosition?.Balance > 0)
                   
                {
                    portfolioPosition = null;
                    tradeOperation = new TradeOperation(default, default, default, default, default, default, bestBid, default, default, item.Figi, default, default, DateTime.Now.ToUniversalTime(), default);
                    using (StreamWriter sw = new StreamWriter("_operation " + item.Ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" FromLong " + item.Ticker + "price " + bestBid);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.Figi + " TradeOperation.fromLong");
                }

                if (tradeVariant == TradeTarget.toShort
                    &&
                    portfolioPosition == null
                    )
                {

                    int countBalance = -1;
                    portfolioPosition = new Portfolio.Position(default, item.Figi, item.Ticker, item.Isin, default, countBalance, default, new MoneyAmount(Currency.Usd, bestBid), countBalance, new MoneyAmount(Currency.Usd, bestBid), default);
                    tradeOperation = new TradeOperation(default, default, default, default, default, default, bestBid, default, default, item.Figi, default, default, DateTime.Now.ToUniversalTime(), default);
                    using (StreamWriter sw = new StreamWriter("_operation " + item.Ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" ToShort " + item.Ticker + "price " + bestBid);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.Figi + " TradeOperation.toShort");
                }

                if (tradeVariant == TradeTarget.fromShort
                    &&
                    portfolioPosition?.Balance < 0
                    )
                {
                    portfolioPosition = null;
                    tradeOperation = new TradeOperation(default, default, default, default, default, default, bestAsk, default, default, item.Figi, default, default, DateTime.Now.ToUniversalTime(), default);


                    using (StreamWriter sw = new StreamWriter("_operation " + item.Ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" FromShort " + item.Ticker + "price " + bestAsk);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.Figi + " TradeOperation.fromShort");
                }
            }
        }
    }

}
