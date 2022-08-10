using System;
using System.Threading.Tasks;
using Serilog;
using DataCollector;
using System.IO;
using MarketDataModules.Candles;
using System.Diagnostics;
using ResearchLib;
using Analysis.TradeDecision;
using System.Collections.Generic;
using System.Linq;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"),
                              rollingInterval: RollingInterval.Month,
                              fileSizeLimitBytes: 304857600,
                              rollOnFileSizeLimit: true,
                              retainedFileCountLimit: 7
                              )
                .CreateLogger();

            int x = 1;
            string ticker = "YNDX";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
            CandlesList candlesList = await GetMarketData.GetCandlesAsync(instrument.Figi, CandleInterval.Day, 1000);
            using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"), true, System.Text.Encoding.Default))
            {
                foreach (var candle in candlesList.Candles)
                {
                        sw.WriteLine($"{x++};{candle.Close}");                
                }
            }
            ////Decision decision1 = TradeDecisions.Method1;
            //List<Decision> decisions = new List<Decision> { 

            //};

            //var x = typeof(TradeDecisions).GetMethods();
            //foreach (var item in x.Where(x => x.IsStatic))
            //{
            //    if (item.DeclaringType == typeof(TradeDecisions))
            //    {
            //        decisions.Add(item.CreateDelegate<Decision>());
            //    }
            //}

            //OnlineResearch onlineResearch = new(instrument.Figi, CandleInterval.FiveMinutes, 400);
            //try
            //{
            //    await onlineResearch.Start(decisions);
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex.Message);
            //    Log.Error(ex.StackTrace);
            //}
        }
    }
}
