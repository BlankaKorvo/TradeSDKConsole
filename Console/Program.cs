using System;
using System.Threading.Tasks;
using Serilog;
using DataCollector;
using System.IO;
using MarketDataModules.Candles;
using System.Diagnostics;
using ResearchLib;
using Analysis.TradeDecision;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"), rollingInterval: RollingInterval.Month, fileSizeLimitBytes: 304857600, rollOnFileSizeLimit: true)
                .CreateLogger();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string ticker = "SBER";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);

            await new OnlineResearch(instrument.Figi, CandleInterval.Minute, 200).Start();
        }
    }

}
