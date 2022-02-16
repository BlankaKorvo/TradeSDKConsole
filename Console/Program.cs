using System;
using System.Threading.Tasks;
using Serilog;
using DataCollector;
using System.IO;
using MarketDataModules.Candles;
using System.Diagnostics;
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
            string ticker = "AAPL";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
            ICandlesList candlesList = await GetMarketData.GetCandlesAsync(instrument.Figi, CandleInterval.FiveMinutes, DateTime.Now.AddDays(-360));
            stopwatch.Start();
            //List<ParabolicSarResult> parabolicSarTrand = MapperCandlesToQuote.ConvertThisCandlesToQuote(candlesList.Candles).GetParabolicSar(accelerationStep, maxAccelerationFactor, initialFactor).ToList();
            new OfflineResearch(candlesList).Research();
            stopwatch.Stop();
            //Console.WriteLine(stopwatch.ElapsedMilliseconds);



            //Console.ReadKey();
        }
    }

}
