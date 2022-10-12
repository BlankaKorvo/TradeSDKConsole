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
using System.Reflection.Emit;
using MarketDataModules.Orderbooks;
using System.Timers;
using MarketDataModules.Instruments;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string logTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] <{ThreadId}> <{ThreadName}> {Message:lj} {NewLine}{Exception}";
            Log.Logger = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .Enrich.WithThreadName()
                .MinimumLevel.Information()
                //.WriteTo.Console(outputTemplate: logTemplate)
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"),
                              rollingInterval: RollingInterval.Month,
                              fileSizeLimitBytes: 304857600,
                              rollOnFileSizeLimit: true,
                              retainedFileCountLimit: 7,
                              outputTemplate: logTemplate
                              )
                .CreateLogger();

            Log.Information("Start program");



            //List<Instrument> instrumentsRub = new();
            //List<Instrument> instrumentsRubLong = new();
            //var instruments = await GetMarketData.GetInstrumentListAsync();
            //foreach (var intrument in instruments.Instruments)
            //{
            //    if (intrument.Currency == Currency.Usd && intrument.Type == InstrumentType.Stock)
            //    {
            //        instrumentsRub.Add(intrument);
            //    }
            //}

            //foreach (var instrument in instrumentsRub)
            //{
            //    var candles = await GetMarketData.GetCandlesAsync(instrument.Figi, CandleInterval.FiveMinutes, 400);
            //    Orderbook orderbook = await GetMarketData.GetOrderbookAsync(instrument.Figi, 20);
            //    var result = TradeDecisions.BBPercent200_20Volume5_10(candles, orderbook);
            //    if (result == MarketDataModules.Trading.TradeTarget.toLong)
            //    {
            //        Console.WriteLine(instrument.Name);
            //    }
            //}
            //Console.WriteLine("END");



            #region Trade
            //int x = 1;
            string ticker = "TCSG";
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
            Task GetOrder = new Task(async () => await Kostyl.GetOrderbook(instrument.Figi));
            //GetOrder.Priority = ThreadPriority.Highest;

            OnlineResearch onlineResearchMinute = new(instrument.Figi, CandleInterval.Minute, 400, decisions);
            Task Minute = new Task(async () => await onlineResearchMinute.Start());

            OnlineResearch onlineResearchTwoMinutes = new(instrument.Figi, CandleInterval.TwoMinutes, 400, decisions);
            Task TwoMinutes = new Task(async () => await onlineResearchTwoMinutes.Start());

            OnlineResearch onlineResearchThreeMinutes = new(instrument.Figi, CandleInterval.ThreeMinutes, 400, decisions);
            Task ThreeMinutes = new Task(async () => await onlineResearchThreeMinutes.Start());

            OnlineResearch onlineResearchFiveMinutes = new(instrument.Figi, CandleInterval.FiveMinutes, 400, decisions);
            Task FiveMinutes = new Task(async () => await onlineResearchFiveMinutes.Start());


            OnlineResearch onlineResearchTenMinutes = new(instrument.Figi, CandleInterval.TenMinutes, 400, decisions);
            Task TenMinutes = new Task(async () => await onlineResearchTenMinutes.Start());

            OnlineResearch onlineResearchQuarterHour = new(instrument.Figi, CandleInterval.QuarterHour, 400, decisions);
            Task QuarterHour = new Task(async () => await onlineResearchQuarterHour.Start());

            OnlineResearch onlineResearchHalfHour = new(instrument.Figi, CandleInterval.HalfHour, 400, decisions);
            Task HalfHour = new Task(async () => await onlineResearchHalfHour.Start());

            OnlineResearch onlineResearchHour = new(instrument.Figi, CandleInterval.Hour, 400, decisions);
            Task Hour = new Task(async () => await onlineResearchHour.Start());

            OnlineResearch onlineResearchDay = new(instrument.Figi, CandleInterval.Day, 400, decisions);
            Task Day = new Task(async () => await onlineResearchDay.Start());



            GetOrder.Start();
            Thread.Sleep(100);
            Minute.Start();
            TwoMinutes.Start();
            ThreeMinutes.Start();
            FiveMinutes.Start();
            TenMinutes.Start();
            QuarterHour.Start();
            HalfHour.Start();
            Hour.Start();
            Day.Start();
            //GetOrder.Join();
            //FiveMinutes.Join();

            //Task.WaitAny(GetOrder, FiveMinutes);
            while (true) { };

            //Log.Information("Stop program");
            #endregion
        }
    }
}
