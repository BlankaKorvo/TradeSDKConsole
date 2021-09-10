using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using TinkoffAdapter.DataHelper;
using DataCollector;
using CandleInterval = MarketDataModules.CandleInterval;
using MarketDataModules;
using Analysis.Screeners;
using System.Linq;
using System.IO;
using TinkoffAdapter.Authority;
using MarketDataModules.Models.Candles;
using TinkoffData;
using Skender.Stock.Indicators;
using TradingAlgorithms.IndicatorSignals;
using Instrument = MarketDataModules.Instrument;
using LinqStatistics;
using Analysis.Screeners.CandlesScreener;
using Analysis.Screeners.Helpers;
using TradeTarget = MarketDataModules.TradeTarget;
using Analysis.TradeDecision;
using Trader;
using System.Threading;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 104857600, rollOnFileSizeLimit: true)
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
            List<string> Tickers = new List<string> { "AMZN", "AAPL", "GOOG", "CLOV" };
            List<Instrument> instruments = new List<Instrument>();
            foreach (var item in Tickers)
            {
                instruments.Add(await marketDataCollector.GetInstrumentByTickerAsync(item));
            }
            InstrumentList instrumentList = new InstrumentList(instruments.Count, instruments);
            AutoTrading autoTrading = new AutoTrading() { CandleInterval = CandleInterval.TwoMinutes, CandlesCount = 70 };
            await autoTrading.AutoTradingInstruments(instrumentList, 2);
            /// AutoTrading
            /// 


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

        }
    }
}
