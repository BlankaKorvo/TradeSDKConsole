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

using ResearchLib.SyntTradeResultDatabase.Client;
using System.Transactions;
using MarketDataModules.Trading;
using ResearchLib.SyntetResultDatabase.Model;
using Google.Protobuf.WellKnownTypes;
//using Tinkoff.InvestApi.V1;
using Tinkoff.InvestApi;
using Google.Protobuf.Collections;
using DataCollector.TinkoffAdapterGrpc;
using Microsoft.Extensions.DependencyInjection;
using Tinkoff.InvestApi.V1;
//using TinkoffLegacy.InvestApi.V1;

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
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: logTemplate)
                .WriteTo.File(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs//.log"),
                              rollingInterval: RollingInterval.Month,
                              fileSizeLimitBytes: 304857600,
                              rollOnFileSizeLimit: true,
                              retainedFileCountLimit: 7,
                              outputTemplate: logTemplate
                              )
                .CreateLogger();

            Log.Information("Start program");



            //string token = File.ReadAllLines("toksann.dll")[0].Trim();
            InvestApiClient client = GetClient.Grpc;
            var instruments = client.Instruments.Shares();
            var instr = instruments.Instruments.First(x => x.Ticker == "YNDX");
            Console.WriteLine($"{instr.ClassCode} {instr.Uid}");
            //InstrumentRequest InstrumentRequest = new InstrumentRequest() {IdType = InstrumentIdType.Ticker, ClassCode = "", Id = "YNDX" };
            //var instrument = client.Instruments.GetInstrumentBy(InstrumentRequest);
            //Console.WriteLine(instrument.Instrument.Uid);
            GetCandlesRequest getCandlesRequest = new GetCandlesRequest() {InstrumentId = "10e17a87-3bce-4a1f-9dfc-720396f98a3c", /*From = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-10)),*/ Interval = Tinkoff.InvestApi.V1.CandleInterval._1Min, To = Timestamp.FromDateTime(DateTime.UtcNow)};
            //GetCandlesResponse result = client.MarketData.GetCandles(getCandlesRequest);
            //List<HistoricCandle> candles = result.Candles.ToList();
            GetTinkoffCandles getTinkoffCandles = new GetTinkoffCandles() {Client = client, FullCandlesRequest = getCandlesRequest, CandleCount = 1000000000 };
            var candles = getTinkoffCandles.GetSetCandles();
            
            foreach ( var candle in candles)
            {
                Console.WriteLine(candle.IsComplete);
                Console.WriteLine(candle.Close);
                Console.WriteLine(candle.Time);
            }
            Console.WriteLine(candles.Count);
            Console.ReadKey();
            
            
            
            
            
            
            
            
            
            //string tickerr = "TCSG";
            //var instrumentt = await GetMarketData.GetInstrumentByTickerAsync(tickerr);
            //string figi = instrumentt.Figi;
            //var candles = await GetMarketData.GetCandles(instrumentt.Figi, CandleInterval.FiveMinutes, 400);


            ////string token = File.ReadAllLines("toksann.dll")[0].Trim();
            ////InvestApiClient client = GetClient.Grpc;
            ////GetCandlesRequest getCandlesRequest = new GetCandlesRequest() { Figi = figi, From = Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-1)), Interval = Tinkoff.InvestApi.V1.CandleInterval._5Min, To = Timestamp.FromDateTime(DateTime.UtcNow) };
            ////var result = client.MarketData.GetCandles(getCandlesRequest);
            ////List<HistoricCandle> candles = result.Candles.ToList();
            ////Console.WriteLine(candles.Count);
            //foreach (var item in candles.Candles)
            //{
            //    Console.WriteLine(item.Close + " " + item.Time);
            //}    


            Console.ReadKey();

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
            //    var candles = await GetMarketData.GetCandles(instrument.Figi, CandleInterval.FiveMinutes, 400);
            //    Orderbook orderbook = await GetMarketData.GetOrderbookAsync(instrument.Figi, 20);
            //    var result = TradeDecisions.BBPercent200_20Volume5_10(candles, orderbook);
            //    if (result == MarketDataModules.Trading.TradeTarget.toLong)
            //    {
            //        Console.WriteLine(instrument.Name);
            //    }
            //}
            //Console.WriteLine("END");

            //List<Position> portfolioPositions = new List<Position>();
            //PortfolioEmulator portfolioEmulator = new PortfolioEmulator();
            DbEdit.DeleteDb();
            
            PortfolioDbEdit.Add(MarketDataModules.Candles.CandleInterval.FiveMinutes);
            if (!PositionDbEdit.IsAvailable("figi", MarketDataModules.Candles.CandleInterval.FiveMinutes))
            {
                PositionDbEdit.Add("figi", MarketDataModules.Candles.CandleInterval.FiveMinutes, "ticker", 0.12m, 1);
                PositionDbEdit.Add("figi3", MarketDataModules.Candles.CandleInterval.FiveMinutes, "ticker", 0.12m, 1);
                PositionDbEdit.Add("figi", MarketDataModules.Candles.CandleInterval.Hour, "ticker", 0.12m, 1);
            }
            if (!PositionDbEdit.IsAvailable("figi1", MarketDataModules.Candles.CandleInterval.Minute))
            {
                PositionDbEdit.Add("figi1", MarketDataModules.Candles.CandleInterval.Minute,  "ticker", 0.12m, 1);
            }
            TransactionDbEdit.Add("figi", MarketDataModules.Candles.CandleInterval.FiveMinutes, TradeTarget.toLong, 0.25m, 52m, DateTime.Now);
            //TransactionDbEdit.Add("figi", MarketDataModules.Candles.CandleInterval.TenMinutes, TradeTarget.toLong, 0.25m, 52m, DateTime.Now);

                //if (portfolioEmulator.IsPositionAvailable("figi1", CandleInterval.FiveMinutes))
                //{
                //    portfolioEmulator.RemovePosition("figi1", CandleInterval.FiveMinutes);
                //}
                //if (portfolioEmulator.IsPositionAvailable("figi", CandleInterval.FiveMinutes))
                //{
                //    portfolioEmulator.RemovePosition("figi", CandleInterval.FiveMinutes);
                //}
                //if (portfolioEmulator.IsPositionAvailable("figi2", CandleInterval.FiveMinutes))
                //{
                //    portfolioEmulator.RemovePosition("figi2", CandleInterval.FiveMinutes);
                //}

            Console.ReadKey();


        //    #region Trade
        //    //int x = 1;
        //    string ticker = "TCSG";
        //    //var instrument = await GetMarketData.GetInstrumentByTickerAsync(ticker);
        //    //CandlesList candlesList = await GetMarketData.GetCandles(instrument.Figi, CandleInterval.Day, 1000);
        //    //using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.csv"), true, System.Text.Encoding.Default))
        //    //{
        //    //    foreach (var candle in candlesList.Candles)
        //    //    {
        //    //            sw.WriteLine($"{x++};{candle.Close}");                
        //    //    }
        //    //}

        //    //Decision decision1 = TradeDecisions.Method1;
        //    List<Decision> decisions = new List<Decision>
        //    {

        //    };

        //    var x = typeof(TradeDecisions).GetMethods();
        //    foreach (var item in x.Where(x => x.IsStatic))
        //    {
        //        if (item.DeclaringType == typeof(TradeDecisions))
        //        {
        //            decisions.Add(item.CreateDelegate<Decision>());
        //        }
        //    }
        //    Task GetOrder = new Task(async () => await LockOrderbook.GetOrderbook(instrument.Figi));
        //    //GetOrder.Priority = ThreadPriority.Highest;

        //    OnlineResearch onlineResearchMinute = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.Minute, 400, decisions);
        //    Task Minute = new Task(async () => await onlineResearchMinute.Start());

        //    OnlineResearch onlineResearchTwoMinutes = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.TwoMinutes, 400, decisions);
        //    Task TwoMinutes = new Task(async () => await onlineResearchTwoMinutes.Start());

        //    OnlineResearch onlineResearchThreeMinutes = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.ThreeMinutes, 400, decisions);
        //    Task ThreeMinutes = new Task(async () => await onlineResearchThreeMinutes.Start());

        //    OnlineResearch onlineResearchFiveMinutes = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.FiveMinutes, 400, decisions);
        //    Task FiveMinutes = new Task(async () => await onlineResearchFiveMinutes.Start());


        //    OnlineResearch onlineResearchTenMinutes = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.TenMinutes, 400, decisions);
        //    Task TenMinutes = new Task(async () => await onlineResearchTenMinutes.Start());

        //    OnlineResearch onlineResearchQuarterHour = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.QuarterHour, 400, decisions);
        //    Task QuarterHour = new Task(async () => await onlineResearchQuarterHour.Start());

        //    OnlineResearch onlineResearchHalfHour = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.HalfHour, 400, decisions);
        //    Task HalfHour = new Task(async () => await onlineResearchHalfHour.Start());

        //    OnlineResearch onlineResearchHour = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.Hour, 400, decisions);
        //    Task Hour = new Task(async () => await onlineResearchHour.Start());

        //    OnlineResearch onlineResearchDay = new(instrument.Figi, MarketDataModules.Candles.CandleInterval.Day, 400, decisions);
        //    Task Day = new Task(async () => await onlineResearchDay.Start());



        //    GetOrder.Start();
        //    Thread.Sleep(100);
        //    Minute.Start();
        //    TwoMinutes.Start();
        //    ThreeMinutes.Start();
        //    FiveMinutes.Start();
        //    //TenMinutes.Start();
        //    //QuarterHour.Start();
        //    //HalfHour.Start();
        //    //Hour.Start();
        //    //Day.Start();
        //    //GetOrder.Join();
        //    //FiveMinutes.Join();

        //    //Task.WaitAny(GetOrder, FiveMinutes);
        //    while (true) { };

        //    //Log.Information("Stop program");
        //    #endregion
        }
    }
}
