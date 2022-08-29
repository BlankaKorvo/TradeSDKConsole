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
using System.Threading;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                //.WriteTo.Console()
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"),
                              rollingInterval: RollingInterval.Month,
                              fileSizeLimitBytes: 304857600,
                              rollOnFileSizeLimit: true,
                              retainedFileCountLimit: 7
                              )
                .CreateLogger();

            //int x = 1;
            string ticker = "YNDX";
            var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
            //CandlesList candlesList = await GetMarketData.GetCandlesAsync(instrument.Figi, CandleInterval.Day, 1000);
            //using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"), true, System.Text.Encoding.Default))
            //{
            //    foreach (var candle in candlesList.Candles)
            //    {
            //            sw.WriteLine($"{x++};{candle.Close}");                
            //    }
            //}


            //Decision decision1 = TradeDecisions.Method1;
            List<Decision> decisions = new List<Decision>
            {

            };

            var x = typeof(TradeDecisions).GetMethods();
            foreach (var item in x.Where(x => x.IsStatic))
            {
                if (item.DeclaringType == typeof(TradeDecisions))
                {
                    decisions.Add(item.CreateDelegate<Decision>());
                }
            }

            OnlineResearch onlineResearchMinute = new(instrument.Figi, CandleInterval.Minute, 400, decisions);
            Thread Minute = new Thread(async () => await onlineResearchMinute.Start());

            OnlineResearch onlineResearchTwoMinutes = new(instrument.Figi, CandleInterval.TwoMinutes, 400, decisions);
            Thread TwoMinutes = new Thread(async () => await onlineResearchTwoMinutes.Start());

            OnlineResearch onlineResearchThreeMinutes = new(instrument.Figi, CandleInterval.ThreeMinutes, 400, decisions);
            Thread ThreeMinutes = new Thread(async () => await onlineResearchThreeMinutes.Start());

            OnlineResearch onlineResearchFiveMinutes = new(instrument.Figi, CandleInterval.FiveMinutes, 400, decisions);
            Thread FiveMinutes = new Thread(async () => await onlineResearchFiveMinutes.Start());

            OnlineResearch onlineResearchTenMinutes = new(instrument.Figi, CandleInterval.TenMinutes, 400, decisions);
            Thread TenMinutes = new Thread(async () => await onlineResearchTenMinutes.Start());

            OnlineResearch onlineResearchQuarterHour = new(instrument.Figi, CandleInterval.QuarterHour, 400, decisions);
            Thread QuarterHour = new Thread(async () => await onlineResearchQuarterHour.Start());

            OnlineResearch onlineResearchHalfHour = new(instrument.Figi, CandleInterval.HalfHour, 400, decisions);
            Thread HalfHour = new Thread(async () => await onlineResearchHalfHour.Start());

            OnlineResearch onlineResearchHour = new(instrument.Figi, CandleInterval.Hour, 400, decisions);
            Thread Hour = new Thread(async () => await onlineResearchHour.Start());

            OnlineResearch onlineResearchDay = new(instrument.Figi, CandleInterval.Day, 400, decisions);
            Thread Day = new Thread(async () => await onlineResearchDay.Start());

            try
            {
                    Minute.Start();
                    TwoMinutes.Start();
                    ThreeMinutes.Start();
                    FiveMinutes.Start();
                    TenMinutes.Start();
                    QuarterHour.Start();
                    HalfHour.Start();
                    Hour.Start();
                    Day.Start();
                while (true) { };
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.StackTrace);
            }
        }
    }
}
