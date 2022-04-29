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
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"), rollingInterval: RollingInterval.Month, fileSizeLimitBytes: 304857600, rollOnFileSizeLimit: true)
                .CreateLogger();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            string ticker = "AMZN";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
            //Decision decision1 = TradeDecisions.Method1;
            List<Decision> decisions = new List<Decision> { 
                //TradeDecisions.BBBorder10_02, 
                //TradeDecisions.BBBorder10_02DeleteOrder, 
                //TradeDecisions.BBBorder20_02,
                //TradeDecisions.BBBorder20_02DeleteOrder,
                //TradeDecisions.BBPercentOverCross1040,
                //TradeDecisions.BBPercentOverCross1040DeleteOrder,
                //TradeDecisions.BBPercentOverCross1060,
                //TradeDecisions.BBPercentOverCross1060DeleteOrder,
                //TradeDecisions.BBPercentOverCross2010,
                //TradeDecisions.BBPercentOverCross2010DeleteOrder,
                //TradeDecisions.BBPercentPeriodCross1020,
                //TradeDecisions.BBPercentPeriodCross1020DeleteOrder,
                //TradeDecisions.BBPercentPeriodCross1030,
                //TradeDecisions.BBPercentPeriodCross1030DeleteOrder,
                //TradeDecisions.BBPercentOverCross2040,
                //TradeDecisions.BBPercentOverCross2040DeleteOrder,
                //TradeDecisions.BBPercentOverCross2060,
                //TradeDecisions.BBPercentOverCross2060DeleteOrder                
            };

            var x = typeof(TradeDecisions).GetMethods();
            foreach (var item in x.Where(x => x.IsStatic))
            {
                if (item.DeclaringType == typeof(TradeDecisions))
                {
                    decisions.Add(item.CreateDelegate<Decision>());
                }
            }

            OnlineResearch onlineResearch = new(instrument.Figi, CandleInterval.FiveMinutes, 400);
            try
            {
                await onlineResearch.Start(decisions);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
    }
}
