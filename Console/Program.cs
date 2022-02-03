using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Serilog;

using DataCollector;
using CandleInterval = MarketDataModules.Candles.CandleInterval;
using MarketDataModules;

using System.Linq;
using System.IO;

using MarketDataModules.Candles;

using Instrument = MarketDataModules.Instruments.Instrument;
using LinqStatistics;


using System.Threading;
using MarketDataModules.Portfolio;
using MarketDataModules.Operation;
using System.Reflection;
//using Analysis.Screeners.Helpers;
using Analysis.Screeners.StockExchangeDataScreener;
using Analysis.Signals;
using Analysis.TradeDecision;
using MarketDataModules.Orderbooks;
using DataCollector.TinkoffAdapter;
using System.Diagnostics;
using MarketDataModules.Trading;
using Research;


namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                //.WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"), rollingInterval: RollingInterval.Month, fileSizeLimitBytes: 304857600, rollOnFileSizeLimit: true)
                .CreateLogger();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string ticker = "Amzn";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
            ICandlesList candlesList = await GetMarketData.GetCandlesAsync(instrument.Figi, CandleInterval.FiveMinutes, DateTime.Now.AddDays(-365));
            new OfflineResearch(candlesList).Research();
            stopwatch.Stop();


            Console.ReadKey();
        }
    }

}
