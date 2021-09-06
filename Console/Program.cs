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
using TradeOperation = MarketDataModules.TradeOperation;
using Analysis.TradeDecision;

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
            Signal signal = new Signal();

            var candleInterval = CandleInterval.FiveMinutes;              


            int candlesCount = 200;
            decimal budget = 9000;

            var lastOperation = TradeOperation.fromLong;
            List<string> Tickers = new List<string> { "BBG000BVPV84" };
            bool position = true;
            while (true)                
            {
                foreach (var item in Tickers)
                {
                    var candleList = await marketDataCollector.GetCandlesAsync(item, candleInterval, candlesCount);
                    var orderbook = await marketDataCollector.GetOrderbookAsync(item, Provider.Tinkoff, 1);

                    var bestAsk = orderbook.Asks.FirstOrDefault().Price;
                    var bestBid = orderbook.Bids.FirstOrDefault().Price;

                    GmmaDecision gmmaDecision = new GmmaDecision() { candleList = candleList, orderbook = orderbook, bestAsk = bestAsk, bestBid = bestBid };

                    //var gmmaSignalResult = signal.GmmaSignal(candleList, bestAsk , bestBid);

                    if (gmmaDecision.TradeVariant() == TradeOperation.toLong
                        && 
                        (lastOperation == TradeOperation.fromLong || lastOperation == TradeOperation.fromShort))
                    {
                        lastOperation = TradeOperation.toLong;
                        using (StreamWriter sw = new StreamWriter("_operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" Long " + item + "price " + bestAsk);
                            sw.WriteLine();
                        }
                    }

                    if (gmmaDecision.TradeVariant() == TradeOperation.fromLong
                        &&
                        (lastOperation == TradeOperation.toLong))
                    {
                        lastOperation = TradeOperation.fromLong;
                        using (StreamWriter sw = new StreamWriter("_operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" FromLong " + item + "price " + bestBid);
                            sw.WriteLine();
                        }
                    }

                    if (gmmaDecision.TradeVariant() == TradeOperation.toShort
                        &&
                        (lastOperation == TradeOperation.fromLong || lastOperation == TradeOperation.fromShort))
                    {
                        lastOperation = TradeOperation.toShort;
                        using (StreamWriter sw = new StreamWriter("_operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" ToShort " + item + "price " + bestBid);
                            sw.WriteLine();
                        }
                    }

                    if (gmmaDecision.TradeVariant() == TradeOperation.fromShort
                        &&
                        (lastOperation == TradeOperation.toShort))
                    {
                        lastOperation = TradeOperation.fromShort;
                        using (StreamWriter sw = new StreamWriter("_operation", true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(DateTime.Now + @" FromShort " + item + "price " + bestAsk);
                            sw.WriteLine();
                        }
                    }

                }                
            }
















            Analysis.Screeners.CandlesScreener.Operation mishMashScreener = new Analysis.Screeners.CandlesScreener.Operation();
            List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();

           // await Trading(marketDataCollector, getStocksHistory, candleInterval, candlesCount, budget, mishMashScreener);

            Console.ReadKey();

            List<CandlesList> candlesLists = new List<CandlesList>();



























            foreach (var item in instrumentList)
            {                
                CandlesList candles = await marketDataCollector.GetCandlesAsync(item.Figi, candleInterval, 60);

                if (candles == null || candles.Candles.Count == 0)
                {
                    continue;
                }
                
                candlesLists.Add(candles);
            }
            Log.Information("candlesLists Count = " + candlesLists.Count);

            //List<CandlesList> resultCandlesList = volumeIncreaseScreener.DramIncreased(candlesLists, 50, 4);

            ////Screener
            //List<CandlesList> resultCandlesList = stochDivScreener.TrandUp(candlesLists);

            //Console.WriteLine("resultCandlesList = " + resultCandlesList.Count);

            //foreach (var item in resultCandlesList)
            //{
            //    var instrument = await marketDataCollector.GetInstrumentByFigi(item.Figi);
            //    var ticker = instrument.Ticker;
            //    Console.Write(item.Figi + "  ");
            //    Console.WriteLine(ticker);
            //}

            //Console.ReadKey();


            ////End Screener


            async Task HZ(MarketDataCollector marketDataCollector)
            {
                Signal signal = new Signal();
                List<Instrument> instrumentList = await getStocksHistory.AllUsdStocksAsync();
                //List<Instrument> instrumentList = new List<Instrument>();
                //var xxx = await marketDataCollector.GetInstrumentByFigi("BBG000BPL8G3");
                //instrumentList.Add(xxx);

                var candleInterval = CandleInterval.Hour;

                List<CandlesList> candlesList = new List<CandlesList>();
                foreach (var item in instrumentList)
                {
                    var candles = await marketDataCollector.GetCandlesAsync(item.Figi, candleInterval, new DateTime(2021, 1, 1));
                    if (candles.Candles.Count == 0)
                    {
                        continue;
                    }
                    candlesList.Add(candles);
                }
                List<CandlesProfileList> profileList = volumeProfileScreener.CreateProfilesList(candlesList, 50, VolumeProfileMethod.All);

                List<CandlesProfileList> profilesList2 = volumeProfileScreener.BargainingOnPrice(profileList, 10);

                List<CandlesProfileList> profilesList1 = volumeProfileScreener.OrderVolBargaining(profilesList2);

                Log.Information("Start set ticker");
                foreach (var item in profilesList1)
                {
                    VolumeProfile maxVol = item.VolumeProfiles.OrderByDescending(x => (x.VolumeGreen + x.VolumeRed)).FirstOrDefault();
                    //Instrument instrument = await marketDataCollector.GetInstrumentByFigi(item.Figi);
                    decimal volGreenWeight = volumeProfileScreener.RevWeightGreen(maxVol);
                    decimal volRedWeight = 100 - volGreenWeight;

                    Instrument instrument = (from t in instrumentList
                                             where t.Figi == item.Figi
                                             select t).FirstOrDefault();
                    using (StreamWriter sw = new StreamWriter("TickersAll " + candleInterval, true, System.Text.Encoding.Default))
                    {
                        sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                        sw.WriteLine();
                    }
                    if ((maxVol.UpperBound + maxVol.LowerBound) / 2 < item.Candles.Last().Close)
                    {
                        using (StreamWriter sw = new StreamWriter("TickersOverPrice " + candleInterval, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                            sw.WriteLine();
                        }
                    }
                    if (volGreenWeight > 50)
                    {
                        using (StreamWriter sw = new StreamWriter("TickersOverGreenWeght " + candleInterval, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight);
                            sw.WriteLine();
                        }
                    }
                    List<AdlResult> adl = Mapper.AdlData(item, item.Candles.Last().Close, 1);
                    var AdlAngle = signal.AdlDegreeAverageAngle(adl, 10, Signal.Adl.Adl);
                    if (AdlAngle > 20 && adl.Last().Adl > 0)
                    {
                        using (StreamWriter sw = new StreamWriter("TickersOverADL " + candleInterval, true, System.Text.Encoding.Default))
                        {
                            sw.WriteLine(instrument.Ticker + " UpperBound: " + maxVol.UpperBound + " LowerBound: " + maxVol.LowerBound + " VolumeGreen: " + maxVol.VolumeGreen + " VolumeRed: " + maxVol.VolumeRed + " CandlesCount: " + maxVol.CandlesCount + " Close:" + item.Candles.Last().Close + " GreenVolRev = " + volGreenWeight + " RedVolRev = " + volRedWeight + " ADL angle = " + AdlAngle + " ADL " + adl.Last().Adl);
                            sw.WriteLine();
                        }
                    }
                }
            }


            //foreach (var x in vps)
            //{
            //    Console.WriteLine(x.UpperBound);
            //    Console.WriteLine(x.LowerBound);
            //    Console.WriteLine(x.Volume);
            //    Console.WriteLine();

            //}



        }

        private static decimal AngleCalc(decimal value1, decimal value2)
        {
            double deltaLeg = Convert.ToDouble(value2 - value1);
            double legDifference = Math.Atan(deltaLeg);
            double angle = legDifference * (180 / Math.PI);
            Log.Information("Angle: " + angle.ToString());

            return (decimal)angle;
        }
        //private static async Task Trading(MarketDataCollector marketDataCollector, GetStocksHistory getStocksHistory, CandleInterval candleInterval, int candlesCount, decimal maxMoneyForTrade, Analysis.Screeners.CandlesScreener.Operation mishMashScreener)
        //{
        //    try
        //    {
        //        List<string> tickers = new List<string> { }; //= new List<string> { "qdel", "med", "appf", "sage", "crox", "bio", "lpx", "hear", "txn", "trow", "fizz", "rgr", "bx", "coo", "vrtx", "prg", "azpn", "bpmc", "holx", "nbix" };
        //        //await NewMethod(marketDataCollector);
        //        List<Instrument> UsdinstrumentList = await getStocksHistory.AllUsdStocksAsync();
        //        //List<Instrument> RubinstrumentList = await getStocksHistory.AllRubStocksAsync();
        //        //List<Instrument> instrumentList = UsdinstrumentList.Union(RubinstrumentList).ToList();
        //        var result = await mishMashScreener.GetAllTransactionModels(candleInterval, candlesCount, maxMoneyForTrade, UsdinstrumentList);

        //        foreach (var item in result)
        //        {
        //            if (item.Operation == MarketDataModules.Operation.toLong)
        //            {
        //                tickers.Add(item.Figi);
        //                Console.WriteLine(item.Figi);
        //            }
        //        }
        //        //Console.ReadLine();
        //        List<string> figis = new List<string> { };
        //        foreach (string item in tickers)
        //        {
        //            Instrument instrument = await marketDataCollector.GetInstrumentByTicker(item, Provider.Tinkoff);
        //            figis.Add(instrument.Figi);
        //        }
        //        await mishMashScreener.CycleTrading(candleInterval, candlesCount, maxMoneyForTrade, figis);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex.Message);
        //        Log.Error(ex.StackTrace);
        //    }
        //}
    }
}
