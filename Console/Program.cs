using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using TinkoffAdapter.DataHelper;
using DataCollector;
using CandleInterval = MarketDataModules.CandleInterval;
using MarketDataModules;
using Analysis.Screeners;
using System.Linq;
using System.IO;
using TinkoffAdapter.Authority;
using MarketDataModules.Models.Candles;
using TinkoffData;
using Skender.Stock.Indicators;
using TradingAlgorithms.IndicatorSignals;
using Instrument = MarketDataModules.Instrument;
using LinqStatistics;
using Analysis.Screeners.CandlesScreener;
using Analysis.Screeners.Helpers;
using TradeTarget = MarketDataModules.TradeTarget;
using Analysis.TradeDecision;
using Trader;
using System.Threading;

namespace tradeSDK
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 104857600, rollOnFileSizeLimit: true)
                .CreateLogger();
            MarketDataCollector marketDataCollector = new MarketDataCollector();
            GetStocksHistory getStocksHistory = new GetStocksHistory();
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
            List<TradeInstrument> tradeInstrumentList = new List<TradeInstrument>();

            CandleInterval candleInterval = CandleInterval.Minute;
            int candlesCount = 100;
            List<string> Tickers = new List<string> { "AMZN", "AAPL", "GOOG", "CLOV" };
            foreach (var item in Tickers)
            {
                var instrument = await marketDataCollector.GetInstrumentByTickerAsync(item);
                TradeInstrument tradeInstrument = new TradeInstrument() { figi = instrument.Figi, tradeTarget = TradeTarget.fromLong, ticker = instrument.Ticker };
                tradeInstrumentList.Add(tradeInstrument);
            }


            while (true)
            {
                //int hour = DateTime.Now.Hour;
                //int minutes = DateTime.Now.Minute;
                //if
                //    (
                //    hour >= 11
                //    &&
                //    hour < 23
                //    )
                //{
                    await TestTrading(marketDataCollector, tradeInstrumentList, candleInterval, candlesCount);
                //}
                ///Test Algo
            }
        }



        private static async Task TestTrading(MarketDataCollector marketDataCollector, List<TradeInstrument> tradeInstrumentList, CandleInterval candleInterval, int candlesCount)
        {
            foreach (var item in tradeInstrumentList)
            {
                Log.Information("Start trade: " + item.figi);
                var orderbook = await marketDataCollector.GetOrderbookAsync(item.figi, Provider.Tinkoff, 20);

                if (orderbook == null)
                {
                    Log.Information("Orderbook null");
                    continue;
                }

                var candleList = await marketDataCollector.GetCandlesAsync(item.figi, candleInterval, candlesCount);

                var bestAsk = orderbook.Asks.FirstOrDefault().Price;
                var bestBid = orderbook.Bids.FirstOrDefault().Price;

                GmmaDecisionOneMinutes gmmaDecision = new GmmaDecisionOneMinutes() { candleList = candleList, orderbook = orderbook, bestAsk = bestAsk, bestBid = bestBid };

                //var gmmaSignalResult = signal.GmmaSignal(candleList, bestAsk , bestBid);

                if (gmmaDecision.TradeVariant() == TradeTarget.toLong
                    &&
                    (item.tradeTarget == TradeTarget.fromLong || item.tradeTarget == TradeTarget.fromShort)
                    )
                {
                    item.tradeTarget = TradeTarget.toLong;
                    using (StreamWriter sw = new StreamWriter("_operation " + item.ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" Long " + item.ticker + "price " + bestAsk);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.figi + " TradeOperation.toLong");
                }

                if (gmmaDecision.TradeVariant() == TradeTarget.fromLong
                    &&
                    (item.tradeTarget == TradeTarget.toLong)
                    )
                {
                    item.tradeTarget = TradeTarget.fromLong;
                    using (StreamWriter sw = new StreamWriter("_operation " + item.ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" FromLong " + item.ticker + "price " + bestBid);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.figi + " TradeOperation.fromLong");
                }

                if (gmmaDecision.TradeVariant() == TradeTarget.toShort
                    &&
                    (item.tradeTarget == TradeTarget.fromLong || item.tradeTarget == TradeTarget.fromShort)
                    )
                {
                    item.tradeTarget = TradeTarget.toShort;
                    using (StreamWriter sw = new StreamWriter("_operation " + item.ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" ToShort " + item.ticker + "price " + bestBid);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.figi + " TradeOperation.toShort");
                }

                if (gmmaDecision.TradeVariant() == TradeTarget.fromShort
                    &&
                    (item.tradeTarget == TradeTarget.toShort)
                    )
                {
                    item.tradeTarget = TradeTarget.fromShort;
                    using (StreamWriter sw = new StreamWriter("_operation " + item.ticker, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(DateTime.Now + @" FromShort " + item.ticker + "price " + bestAsk);
                        sw.WriteLine();
                    }
                    Log.Information("Stop trade: " + item.figi + " TradeOperation.fromShort");
                }
            }
        }
    }
    class TradeInstrument
    { 
        internal string figi { get; set; }
        internal string ticker { get; set; }
        internal TradeTarget tradeTarget { get; set; }
    }
}
