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
            Signal signal = new Signal();


            /// AutoTrading
            List<string> Tickers = new List<string> { "AMZN", "AAPL", "GOOG" };
            List<Instrument> instruments = new List<Instrument>();
            foreach (var item in Tickers)
            {
                instruments.Add(await marketDataCollector.GetInstrumentByTickerAsync(item));
            }
            InstrumentList instrumentList = new InstrumentList(instruments.Count, instruments);
            AutoTrading autoTrading = new AutoTrading() { CandleInterval = CandleInterval.FiveMinutes, CandlesCount = 200 };
            await autoTrading.AutoTradingInstruments(instrumentList, 2);
            /// AutoTrading

        }
    }
}
