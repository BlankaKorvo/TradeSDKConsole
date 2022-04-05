using System;
using System.Threading.Tasks;
using Serilog;
using DataCollector;
using System.IO;
using MarketDataModules.Candles;
using System.Diagnostics;
using ResearchLib;

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
            string ticker = "Yndx";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);

            stopwatch.Start();
            //List<ParabolicSarResult> parabolicSarTrand = MapperCandlesToQuote.ConvertThisCandlesToQuote(candlesList.Candles).GetParabolicSar(accelerationStep, maxAccelerationFactor, initialFactor).ToList();
            TimeSpan timeSpan = TimeSpan.FromDays(2);
            //await new OfflineResearch(instrument.Figi,CandleInterval.Minute, timeSpan, 400).Start();
            await new OnlineResearch(instrument.Figi, CandleInterval.Minute, 200).Start();
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }

}
