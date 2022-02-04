using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using DataCollector;

using System.Linq;
using System.IO;

using MarketDataModules.Candles;

using System.Diagnostics;

using Research;
using Skender.Stock.Indicators;
using Analysis;

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
            string ticker = "AMZN";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
            ICandlesList candlesList = await GetMarketData.GetCandlesAsync(instrument.Figi, CandleInterval.FiveMinutes, DateTime.Now.AddDays(-365));
            stopwatch.Start();
            //List<ParabolicSarResult> parabolicSarTrand = MapperCandlesToQuote.ConvertThisCandlesToQuote(candlesList.Candles).GetParabolicSar(accelerationStep, maxAccelerationFactor, initialFactor).ToList();
            new OfflineResearch(candlesList).Research();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);



            Console.ReadKey();
        }
    }

}
