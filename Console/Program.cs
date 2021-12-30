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

using Skender.Stock.Indicators;

using Instrument = MarketDataModules.Instruments.Instrument;
using LinqStatistics;

using TradeTarget = MarketDataModules.TradeTarget;


using System.Threading;
using MarketDataModules.Portfolio;
using MarketDataModules.Operation;
using System.Reflection;
//using Analysis.Screeners.Helpers;
using Analysis.Screeners.StockExchangeDataScreener;
using Analysis.Signals;
using Analysis.TradeDecision;
using MarketDataModules.Orderbooks;

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
            //MarketDataCollector marketDataCollector = new MarketDataCollector();
            //GetStocksHistory getStocksHistory = new GetStocksHistory();
            VolumeProfileScreener volumeProfileScreener = new VolumeProfileScreener();
            VolumeIncreaseScreener volumeIncreaseScreener = new VolumeIncreaseScreener();
            TwoEmaScreener twoEmaScreener = new TwoEmaScreener();
            StochDivScreener stochDivScreener = new StochDivScreener();
            OrderbookScreener orderbookScreener = new OrderbookScreener();
            Signal signal = new Signal();

            /// AutoTrading
            //List<string> Tickers = new List<string> { "AMZN", "AAPL", "GOOG", "CLOV" };
            //List<Instrument> instruments = new List<Instrument>();
            //foreach (var item in Tickers)
            //{
            //    instruments.Add(await marketDataCollector.GetInstrumentByTickerAsync(item));
            //}
            //InstrumentList instrumentList = new InstrumentList(instruments.Count, instruments);
            //AutoTrading autoTrading = new AutoTrading() { CandleInterval = CandleInterval.TwoMinutes, CandlesCount = 70 };
            //await autoTrading.AutoTradingInstruments(instrumentList, 2);
            /// AutoTrading

            ///// Screener OrderBook
            //InstrumentList instrumentList = await marketDataCollector.GetInstrumentListAsync();
            //List<Instrument> instrumentsUSD = (from instrument in instrumentList.Instruments
            //                                   where instrument.Currency == Currency.Usd
            //                                   select instrument)
            //                               .ToList();

            //List<Orderbook> orderbooks = new List<Orderbook>();

            //foreach (var item in instrumentsUSD)
            //{
            //    var element = await marketDataCollector.GetOrderbookAsync(item.Figi);
            //    if (element == null)
            //        continue;
            //    else
            //        orderbooks.Add(element);
            //}

            //List<string> FigiLong = orderbookScreener.OrderbookLong(orderbooks, 10);
            //foreach (var item in FigiLong)
            //{
            //    var result = await marketDataCollector.GetInstrumentByFigi(item);
            //    Console.WriteLine(result.Ticker);
            //}
            ///// Screener OrderBook

            ///Test Algo
            ///
            //string tic = "APA";
            //var ins = await marketDataCollector.GetInstrumentByTickerAsync(tic);
            //var f = ins.Figi;
            //var candlesD = marketDataCollector.GetCandlesAsync(f, CandleInterval.Hour, 1000).GetAwaiter().GetResult();

            //using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testData.csv"), true, System.Text.Encoding.Default))
            //{
            //    foreach (var item in candlesD.Candles)
            //    {
            //        sw.WriteLine("{0},{1},{2},{3},{4},{5}\n", item.Time.ToString(), item.Open.ToString(), item.High.ToString(), item.Low.ToString(), item.Close.ToString(), item.Volume.ToString());
            //    }
            //}

            //Console.ReadKey();
            //List<Instrument> instrumentList = new List<Instrument>();
            TradeOperation tradeOperation = null;
            Portfolio.Position portfolioPosition = null;
            List<(decimal, decimal, decimal)> margin = new List<(decimal, decimal, decimal)>();
            //TradeTarget lastTradeTarget = TradeTarget.fromLong;

            CandleInterval candleInterval = CandleInterval.FiveMinutes;
            int candlesCount = 400;
            var instrument = await MarketDataCollector.GetInstrumentByTickerAsync("AMZN");

            CandlesList bigCandlesList = await MarketDataCollector.GetCandlesAsync(instrument.Figi, candleInterval, DateTime.Now.AddMonths(-13));
            for (int i = 0; i < bigCandlesList.Candles.Count - candlesCount; i++)
            {
                CandlesList notRealTimeCandleList = new CandlesList(bigCandlesList.Figi, bigCandlesList.Interval, bigCandlesList.Candles.Take(candlesCount + i).Skip(i).ToList());
                Log.Information("notRealTimeCandleListCount " + notRealTimeCandleList.Candles.Count + " " + notRealTimeCandleList.Candles.LastOrDefault().Time);
                Orderbook orderbook = MarketDataCollector.GetOrderbookAsync(instrument.Figi, Provider.Tinkoff, 50).GetAwaiter().GetResult();
                OrderbookEntry orderbookEntry = new OrderbookEntry(1, notRealTimeCandleList.Candles.Last().Close); 
                List<OrderbookEntry> orderbookEntries = new List<OrderbookEntry>() { orderbookEntry };
                //Orderbook orderbook = new Orderbook(1, orderbookEntries, orderbookEntries, notRealTimeCandleList.Figi, default, default, default, default, default, default, default);
                TestTrading(orderbook, notRealTimeCandleList, ref tradeOperation, ref portfolioPosition, ref margin, false);
            }

            Console.WriteLine(margin.Sum(x => x.Item1));
            Console.WriteLine(margin.Sum(x => x.Item2));

            Console.ReadKey();


            while (true)
            {
                try
                {
                    Orderbook orderbook = new Orderbook(default, default, default, default, default, default, default, default, default, default, default);
                    //Orderbook orderbook = marketDataCollector.GetOrderbookAsync(instrument.Figi, Provider.Tinkoff, 50).GetAwaiter().GetResult();
                    if (orderbook == null)
                    {
                        Log.Information("Orderbook null");
                        continue;
                    }
                    CandlesList candleList = MarketDataCollector.GetCandlesAsync(instrument.Figi, candleInterval, candlesCount).GetAwaiter().GetResult();

                    TestTrading(orderbook, candleList, ref tradeOperation, ref portfolioPosition, ref margin);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    Log.Error(ex.StackTrace);
                }

            }
        }

        private static void TestTrading(Orderbook orderbook, CandlesList candleList, ref TradeOperation tradeOperation, ref Portfolio.Position portfolioPosition, ref List<(decimal, decimal, decimal)> margin, bool realTime=true)
        {
            //foreach (var item in instrumentList)
            //{
            Log.Information("Start trade: " + candleList.Figi);
            //var orderbook = marketDataCollector.GetOrderbookAsync(item.Figi, Provider.Tinkoff, 50).GetAwaiter().GetResult();

            //if (orderbook == null)
            //{
            //    Log.Information("Orderbook null");
            //    continue;
            //}
            decimal bestAsk = candleList.Candles.Last().Close;
            decimal bestBid = bestAsk;

            if (realTime)
            {

                bestAsk = orderbook.Asks.FirstOrDefault().Price;
                bestBid = orderbook.Bids.FirstOrDefault().Price;
            }


            //var candleList = marketDataCollector.GetCandlesAsync(item.Figi, candleInterval, candlesCount).GetAwaiter().GetResult();

            //Portfolio portfolio = marketDataCollector.GetPortfolioAsync().GetAwaiter().GetResult();
            //List<TradeOperation> tradeOperations = marketDataCollector.GetOperationsAsync(item.figi, DateTime.Now, DateTime.Now.AddDays(-100)).GetAwaiter().GetResult();
            //List<TradeOperation> tradeOperations = new List<TradeOperation>();
            //tradeOperations.Add(tradeOperation);

            //Portfolio.Position position = null;
            //foreach (Portfolio.Position itemP in portfolio.Positions)
            //{
            //    if (itemP.Figi == item.figi)
            //    {
            //        position = itemP;
            //    }
            //}
            List<TradeOperation> tradeOperationResult = new List<TradeOperation>();
            tradeOperationResult.Add(tradeOperation);

            //MoneyAmount averagePositionPrice = item.MoneyAmountT;
            //List<TradeOperation> tradeOperationResult = new List<TradeOperation> { tradeOperation };
            //portfolioPosition = new Portfolio.Position(portfolioPosition.Name, portfolioPosition.Figi, portfolioPosition.Ticker, portfolioPosition.Isin, portfolioPosition.InstrumentType, portfolioPosition.Balance, portfolioPosition.Blocked, portfolioPosition.ExpectedYield, portfolioPosition.Lots, averagePositionPrice, portfolioPosition.AveragePositionPriceNoNkd);
            //GmmaDecisionOneMinutes gmmaDecision = new GmmaDecisionOneMinutes() { candleList = candleList, orderbook = orderbook, bestAsk = bestAsk, bestBid = bestBid };

            GmmaDecision tradeDecision = new GmmaDecision(candleList, orderbook);
            //Mishmash tradeDecision = new Mishmash() { candleList = candleList, deltaPrice = candleList.Candles.LastOrDefault().Close };
            TradeTarget tradeVariant = default;

            if (realTime)
            {
                tradeVariant = tradeDecision.TradeVariant();
            }
            else
            {
                tradeVariant = tradeDecision.TradeVariant(false);
            }
            //TradeTarget tradeVariant = gmmaDecision.TradeVariant();

            //var gmmaSignalResult = signal.GmmaSignal(candleList, bestAsk , bestBid);

            string _operationFile = "_operation_" + candleList.Figi + "_" + candleList.Interval.ToString();
            string _marginFile = "_margin_" + candleList.Figi + "_" + candleList.Interval.ToString();

            if (tradeVariant == TradeTarget.toLong
                &&
                portfolioPosition == null
                )
            {
                int countBalance = 1;
                portfolioPosition = new Portfolio.Position(default, candleList.Figi, default, default, default, countBalance, default, new MoneyAmount(Currency.Usd, bestAsk), countBalance, new MoneyAmount(Currency.Usd, bestAsk), default);
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestAsk, default, default, candleList.Figi, default, default, DateTime.Now.ToUniversalTime(), default);

                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" Long " + candleList.Figi + "price " + bestAsk + "candleTime: " + candleList.Candles.LastOrDefault().Time.AddHours(3));
                    sw.WriteLine();
                }

                Log.Information("Stop trade: " + candleList.Figi + " TradeOperation.toLong");
            }

            if (tradeVariant == TradeTarget.fromLong
                &&
                portfolioPosition?.Balance > 0)

            {
                const decimal com = 0.0005m;
                decimal aMargin = candleList.Candles.LastOrDefault().Close - portfolioPosition.ExpectedYield.Value;
                Log.Information("aMargin= " + aMargin);
                decimal comis = com * (candleList.Candles.LastOrDefault().Close + portfolioPosition.ExpectedYield.Value);
                Log.Information("comis= " + comis);
                decimal rMargin = aMargin - comis;
                Log.Information("rMargin= " + rMargin);
                decimal oMargin = aMargin * 100 / portfolioPosition.ExpectedYield.Value;
                (decimal, decimal, decimal) tuple = (aMargin, rMargin, oMargin);
                margin.Add(tuple);

                portfolioPosition = null;
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestBid, default, default, candleList.Figi, default, default, DateTime.Now.ToUniversalTime(), default);
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" FromLong " + candleList.Figi + "price " + bestBid + "candleTime: " + candleList.Candles.LastOrDefault().Time.AddHours(3));
                    sw.WriteLine();
                }

                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _marginFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(margin.Sum(x=>x.Item1) + " " + margin.Sum(x => x.Item2) + " " + margin.Sum(x => x.Item3) + " " + candleList.Candles.LastOrDefault().Time.AddHours(3));
                    sw.WriteLine();
                }
                Log.Information("Stop trade: " + candleList.Figi + " TradeOperation.fromLong");
            }

            if (tradeVariant == TradeTarget.toShort
                &&
                portfolioPosition == null
                )
            {

                int countBalance = -1;
                portfolioPosition = new Portfolio.Position(default, candleList.Figi, default, default, default, countBalance, default, new MoneyAmount(Currency.Usd, bestBid), countBalance, new MoneyAmount(Currency.Usd, bestBid), default);
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestBid, default, default, candleList.Figi, default, default, DateTime.Now.ToUniversalTime(), default);
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" ToShort " + candleList.Figi + "price " + bestBid + "candleTime: " + candleList.Candles.LastOrDefault().Time.AddHours(3));
                    sw.WriteLine();
                }
                Log.Information("Stop trade: " + candleList.Figi + " TradeOperation.toShort");
            }

            if (tradeVariant == TradeTarget.fromShort
                &&
                portfolioPosition?.Balance < 0
                )
            {
                decimal com = 0.00025m;
                decimal aMargin = portfolioPosition.ExpectedYield.Value - candleList.Candles.LastOrDefault().Close;
                Log.Information("aMargin= " + aMargin);
                decimal comis = com * (candleList.Candles.LastOrDefault().Close + portfolioPosition.ExpectedYield.Value);
                Log.Information("comis= " + comis);
                decimal rMargin = aMargin - comis;
                Log.Information("rMargin= " + rMargin);
                decimal oMargin = aMargin * 100 / portfolioPosition.ExpectedYield.Value;
                (decimal, decimal, decimal) tuple = (aMargin, rMargin, oMargin);
                margin.Add(tuple);

                portfolioPosition = null;
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestAsk, default, default, candleList.Figi, default, default, DateTime.Now.ToUniversalTime(), default);


                using (StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" FromShort " + candleList.Figi + "price " + bestAsk + "candleTime: " + candleList.Candles.LastOrDefault().Time.AddHours(3));
                    sw.WriteLine();
                }

                using (StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _marginFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(margin.Sum(x => x.Item1) + " " + margin.Sum(x => x.Item2) + " " + margin.Sum(x => x.Item3) + " " + candleList.Candles.LastOrDefault().Time.AddHours(3));
                    sw.WriteLine();
                }
                Log.Information("Stop trade: " + candleList.Figi + " TradeOperation.fromShort");
            }
            //}
        }
    }

}
