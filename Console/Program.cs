using Analysis.TradeDecision;
using DataCollector;
using DataCollector.TinkoffAdapterGrpc;
using Google.Protobuf;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MarketDataModules.Candles;
using MarketDataModules.Trading;
using ResearchLib.SyntTradeResultDatabase.Client;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//using Tinkoff.InvestApi.V1;
using Tinkoff.InvestApi;
using Tinkoff.InvestApi.V1;
using Tinkoff.Trading.OpenApi.Models;
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




            //string token = File.ReadAllLines("toksann.dll")[0].Trim();
            InvestApiClient client = GetClient.Grpc;
            //PortfolioStreamRequest request = new PortfolioStreamRequest();

            //var streamMarketData = client.MarketDataStream.MarketDataStream();
            //var streamPortfolio = client.OperationsStream.PortfolioStream(request);

            var instruments = client.Instruments.Shares();
            var accounts = client.Users.GetAccounts();
            var acc = accounts.Accounts;
            var id = acc.FirstOrDefault(x => x.Name == "Стратегия роста РФ").Id;
            Console.WriteLine(id);

            var portfolioReq = new PortfolioRequest() { AccountId = id };
            var portfolio = client.Operations.GetPortfolio(portfolioReq);
 
            Console.WriteLine(portfolio.Positions.FirstOrDefault(x => x.Figi == "BBG004S681M2").Quantity);
            Console.WriteLine(portfolio.Positions.FirstOrDefault(x => x.Figi == "BBG004S681M2").QuantityLots);
            Console.WriteLine(portfolio.Positions.FirstOrDefault(x => x.Figi == "BBG004S681M2").CurrentPrice);
            Console.WriteLine(portfolio.Positions.FirstOrDefault(x => x.Figi == "BBG004S681M2").CurrentPrice * portfolio.Positions.FirstOrDefault(x => x.Figi == "BBG004S681M2").Quantity);
            Console.WriteLine(portfolio.TotalAmountPortfolio);
            Console.WriteLine();

            //var position = new PositionsStreamRequest() { Accounts = new RepeatedField<string>()};
            //AsyncServerStreamingCall<PositionsStreamResponse> positionStream = client.OperationsStream.PositionsStream(position);

            //positionStream.ResponseStream.ReadAllAsync();//ResponseStream.ReadAllAsync<PortfolioStreamResponse>();

            //await foreach (var response in positionStream.ResponseStream.ReadAllAsync())
            //{
            //    if (response == null)
            //    {
            //        //x = 0;
            //        //Console.WriteLine("cont");
            //        continue;
            //    }
            //    if (response.Position == null)
            //    {
            //        //x = 0;
            //        //Console.WriteLine("cont");
            //        continue;
            //    }

            //    Console.WriteLine(response.Position.Money);
            //}

            //var portfolio = new PortfolioStreamRequest() /*{ Accounts = new RepeatedField<string> { "" } }*/;
            //AsyncServerStreamingCall<PortfolioStreamResponse> portfolioStream = client.OperationsStream.PortfolioStream(portfolio);

            //portfolioStream.ResponseStream.ReadAllAsync<PortfolioStreamResponse>();

            //await foreach (var response in portfolioStream.ResponseStream.ReadAllAsync())
            //{
            //    if (response.Portfolio == null)
            //    {
            //        //x = 0;
            //        //Console.WriteLine("cont");
            //        continue;
            //    }
            //    foreach (var item in response.Subscriptions.Accounts)
            //    {
            //        Console.WriteLine(item);
            //    }

            //    Console.WriteLine(response.Portfolio.TotalAmountShares.Currency);
            //}


            Console.WriteLine(instruments.Instruments.Count());
            var russianInstruments = instruments.Instruments.Where(x => x.ForQualInvestorFlag == false)/*.Where(x => x.Currency == "rub")*/.Where(x => x.ShortEnabledFlag == true).OrderBy(x => x.Ticker);


            Dictionary<Share, CandleListOrder> CandlesListDict = new Dictionary<Share, CandleListOrder>();

            foreach (var instrument in russianInstruments)
            {
                Console.WriteLine($"{instrument.Name} {instrument.Uid}");
                var candlesForVolumeMax = GetMarketData.GetCandles(instrument.Uid, MarketDataModules.Candles.CandleInterval.Minute, 4500);
                var averageMaxVolume = (int)Math.Round(candlesForVolumeMax.Candles.OrderBy(x => x.Volume).TakeLast(10).Average(x => x.Volume), MidpointRounding.AwayFromZero);
                CandlesListDict.Add(instrument, new CandleListOrder()
                {
                    CandleList = new MarketDataModules.Candles.CandleList(instrument.Uid, MarketDataModules.Candles.CandleInterval.Minute, default),
                    OperationResult = new OperationResult() { OperationPrice = 0, State = MarketDataModules.Trading.OperationState.NoState },
                    BbvObject = new BBV() { MaxVolume = averageMaxVolume },
                    //VolumeMax = averageMaxVolume
                });
            }
            Console.WriteLine(russianInstruments.Count());
            Thread.Sleep(1000);

            RepeatedField<CandleInstrument> subInstruments = new RepeatedField<CandleInstrument>();
            foreach (var instrument in russianInstruments)
            {
                CandleInstrument candleInstrument = new CandleInstrument() { InstrumentId = instrument.Uid, Interval = SubscriptionInterval.OneMinute };
                subInstruments.Add(candleInstrument);
            }


            // Отправляем запрос в стрим
            //await streamMarketData.RequestStream.WriteAsync(new MarketDataRequest
            //{
            //    SubscribeCandlesRequest = new SubscribeCandlesRequest
            //    {
            //        Instruments = { subInstruments },
            //        SubscriptionAction = SubscriptionAction.Subscribe,
            //        WaitingClose = false
            //    }
            //});





            var streamMarketData = await SubscribeTinkoffData.Candles(subInstruments);
            int x = 0;
            // Обрабатываем все приходящие из стрима ответы
            try
            {
                await foreach (var response in streamMarketData.ResponseStream.ReadAllAsync())
                {

                    if (response.Candle == null)
                    {
                        //x = 0;
                        //Console.WriteLine("cont");
                        continue;
                    }
                    //Console.WriteLine(x++);
                    //if (response.Candle.InstrumentUid)
                    lock (CandlesListDict)
                    {
                        CandleListOrder candleListOrder = CandlesListDict.FirstOrDefault(x => x.Key.Uid == response.Candle.InstrumentUid).Value;
                        MarketDataModules.Candles.CandleList candleList = candleListOrder.CandleList;
                        Share share = CandlesListDict.FirstOrDefault(x => x.Key.Uid == response.Candle.InstrumentUid).Key;
                        CandleStructure candleStructure = new CandleStructure(response?.Candle?.Open, response?.Candle?.Close, response?.Candle?.High, response?.Candle?.Low, response.Candle.Volume, response.Candle.Time.ToDateTime(), false);
                        if (candleList?.Candles == null)
                        {
                            //Console.WriteLine($"First if. resTime = {response.Candle.Time.ToDateTime()} canTime = {candleList?.Candles?.LastOrDefault()?.Time}");
                            candleList = GetMarketData.GetCandles(response.Candle.InstrumentUid, MarketDataModules.Candles.CandleInterval.Minute, 1030);

                        }
                        if (response.Candle.Time.ToDateTime() > candleList.Candles.LastOrDefault().Time)
                        {
                            //Console.WriteLine($"Second if resTime = {response.Candle.Time.ToDateTime()} canTime = {candleList?.Candles?.LastOrDefault()?.Time} {x++}");
                            candleList.Candles.Add(candleStructure);
                        }
                        else
                        {
                            var index = candleList.Candles.LastIndexOf(candleList.Candles.Last(y => y.Time == response.Candle.Time.ToDateTime()));
                            candleList.Candles[index] = candleStructure;
                        }

                        candleListOrder.CandleList = new MarketDataModules.Candles.CandleList(candleList.Figi, candleList.Interval, candleList.Candles.TakeLast(1030).ToList());

                        CandlesListDict.Remove(share);
                        CandlesListDict.Add(share, candleListOrder);

                        //Console.WriteLine(response.Candle.InstrumentUid);
                        //Console.WriteLine(candleList.Candles[^3].Time.ToString() + " " + candleList?.Candles?[^3].Close);
                        //Console.WriteLine(candleList.Candles[^2].Time.ToString() + " " + candleList?.Candles?[^2].Close);
                        //Console.WriteLine(candleList.Candles.LastOrDefault().Time.ToString());
                        //Console.WriteLine(candleList?.Candles?.LastOrDefault().Close);

                        ///!!! Переход на 5 минутыне свечи
                        var candleFiveMinutesList = ToFiveMinutesCandles(candleListOrder.CandleList);
                        ///!!!!
                        var oneResult = candleListOrder.BbvObject.TradeResult(candleFiveMinutesList, candleListOrder.OperationResult);
                        //var fiveResult = candleListOrder.BbvObject.TradeResult(ToFiveMinutesCandles(candleListOrder.CandleList), candleListOrder.OperationResult);

                        if (oneResult == TradeTarget.toLong)
                        {
                            //var minuteCandleList = GetMarketData.GetCandles(response.Candle.InstrumentUid, MarketDataModules.Candles.CandleInterval.FiveMinutes, 200);
                            //var twoResult = bBVMin.TradeResult(minuteCandleList, candleListOrder.operationResult);
                            //if (twoResult != TradeTarget.toLong)
                            //{
                            //    continue;
                            //}

                            candleListOrder.OperationResult.OperationPrice = candleStructure.Close;
                            candleListOrder.OperationResult.State = MarketDataModules.Trading.OperationState.Long;
                            Console.ForegroundColor = ConsoleColor.Green;

                            //PriseStory.TryAdd(response.)

                            WriteLine(share, candleStructure, candleListOrder.BbvObject, oneResult);
                        }
                        else if (oneResult == TradeTarget.toShort)
                        {
                            //var fiveCandleList = GetMarketData.GetCandles(response.Candle.InstrumentUid, MarketDataModules.Candles.CandleInterval.FiveMinutes, 200);
                            //var twoResult = bBVMin.TradeResult(fiveCandleList, candleListOrder.operationResult);
                            //if (twoResult != TradeTarget.toShort)
                            //{
                            //    continue;
                            //}
                            candleListOrder.OperationResult.OperationPrice = candleStructure.Close;
                            candleListOrder.OperationResult.State = MarketDataModules.Trading.OperationState.Short;
                            Console.ForegroundColor = ConsoleColor.Red;
                            WriteLine(share, candleStructure, candleListOrder.BbvObject, oneResult);
                        }
                        else if (oneResult == TradeTarget.fromLong)
                        {
                            decimal margin = (candleStructure.Close - candleListOrder.OperationResult.OperationPrice) - (0.0005m * (candleStructure.Close + candleListOrder.OperationResult.LastGoodPrice));
                            decimal marginPercent = margin * 100 / candleListOrder.OperationResult.OperationPrice;
                            //candleListOrder.operationResult.OperationPrice = candleStructure.Close;
                            candleListOrder.OperationResult.State = MarketDataModules.Trading.OperationState.NoState;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            WriteLine(share, candleStructure, candleListOrder.BbvObject, oneResult, margin, marginPercent);
                        }
                        else if (oneResult == TradeTarget.fromShort)
                        {
                            decimal margin = (candleListOrder.OperationResult.OperationPrice - candleStructure.Close) - (0.0005m * (candleStructure.Close + candleListOrder.OperationResult.LastGoodPrice));
                            decimal marginPercent = margin * 100 / candleListOrder.OperationResult.OperationPrice;
                            //candleListOrder.operationResult.OperationPrice = candleStructure.Close;
                            candleListOrder.OperationResult.State = MarketDataModules.Trading.OperationState.NoState;
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            WriteLine(share, candleStructure, candleListOrder.BbvObject, oneResult, margin, marginPercent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            Console.WriteLine("оба на");
            Console.ReadKey();


            static void WriteLine(Share share, CandleStructure candleStructure, BBV bBVMin, TradeTarget oneResult, decimal margin = 0, decimal marginPercent = 0)
            {
                Console.WriteLine($"{DateTime.Now} {oneResult} {candleStructure.Close} {candleStructure.Close * 0.0005m} {margin} {marginPercent} Ticket = {share.Ticker}, Name = {share.Name}, {bBVMin.Trigger} {bBVMin.BbpFirst} {bBVMin.PVOResFirst} volume = {candleStructure.Volume} {candleStructure.Time} ");
            }



        }

        private static MarketDataModules.Candles.CandleList ToFiveMinutesCandles(MarketDataModules.Candles.CandleList test)
        {
            List<CandleStructure> fiveMinutesCandles = new List<CandleStructure>();


            for (int i = test.Candles.Count - 1; i > 0; i--)
            {
                DateTime stepTime = test.Candles[i].Time.AddMinutes(-(test.Candles[i].Time.Minute % 5)).ToUniversalTime();
                List<CandleStructure> temp = test.Candles.Where(x => DateTime.Compare(x.Time, stepTime) >= 0 && DateTime.Compare(x.Time, stepTime.AddMinutes(5)) < 0).ToList();
                CandleStructure tempCandles = new CandleStructure(temp.FirstOrDefault().Open,
                    temp.LastOrDefault().Close,
                    temp.MaxBy(x => x.High).High,
                    temp.MinBy(x => x.Low).Low,
                    temp.Sum(x => x.Volume),
                    temp.FirstOrDefault().Time,
                    temp.LastOrDefault().IsComplete
                    );
                fiveMinutesCandles.Add(tempCandles);
                i -= temp.Count - 1;
            }
            fiveMinutesCandles = fiveMinutesCandles.OrderBy(x => x.Time).ToList();
            var result = new MarketDataModules.Candles.CandleList(test.Figi, MarketDataModules.Candles.CandleInterval.FiveMinutes, fiveMinutesCandles);
            return result;
        }
    }
}
