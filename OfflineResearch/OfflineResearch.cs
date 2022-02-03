using MarketDataModules.Candles;
using MarketDataModules.Operation;
using MarketDataModules.Orderbooks;
using MarketDataModules.Portfolio;
using MarketDataModules.Trading;
using Serilog;

namespace OfflineResearch
{
    public class OfflineResearch
    {
        public OfflineResearch(ICandlesList _candleList, int _candlesCount = 400)
        {
            candleList = _candleList;
            candlesCount = _candlesCount;
        }
        int candlesCount;
        ICandlesList candleList;


        Portfolio.Position? portfolioPosition;
        const decimal COM = 0.0005m;
        TradeOperation tradeOperation;
        List<TradeOperation> tradeOperations;
        TradeTarget tradeTarget;
        List<(decimal, decimal, decimal)> margin = new();



        public void Research()
        {
            for (int i = 0; i < candleList.Candles.Count - candlesCount; i++)
            {
                CandlesList goingCandleList = new CandlesList(candleList.Figi, candleList.Interval, candleList.Candles.Take(candlesCount + i).Skip(i).ToList());
                tradeTarget = default;//Пока не имплементированно
                FixTradeDecision(tradeTarget, goingCandleList.Figi, goingCandleList.Candles.Last().Close, goingCandleList.Candles.Last().Time.AddHours(3), goingCandleList.Interval);
            }
        }
        private void FixTradeDecision(TradeTarget tradeTarget,string figi, decimal close, DateTime lastTime, CandleInterval candleInterval)
        {
            decimal bestAsk = close;
            decimal bestBid = close;


            string operationFile = $"_operation {figi} {candleInterval}";
            string marginFile = $"_margin {figi} {candleInterval}";

            if (tradeTarget == TradeTarget.toLong
                &&
                portfolioPosition == null
                )
            {
                int countBalance = 1;
                portfolioPosition = new Portfolio.Position(default, figi, default, default, default, countBalance, default, new MoneyAmount(Currency.Usd, bestAsk), countBalance, new MoneyAmount(Currency.Usd, bestAsk), default);
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestAsk, default, default, figi, default, default, DateTime.Now.ToUniversalTime(), default);

                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + $"Long {figi} price {bestAsk} candleTime: {lastTime}");
                    sw.WriteLine();
                }
            }

            if (tradeTarget == TradeTarget.fromLong
                &&
                portfolioPosition?.Balance > 0)

            {
                decimal aMargin =close - portfolioPosition.ExpectedYield.Value;
                Log.Information("aMargin= " + aMargin);
                decimal comis = COM * (close + portfolioPosition.ExpectedYield.Value);
                Log.Information("comis= " + comis);
                decimal rMargin = aMargin - comis;
                Log.Information("rMargin= " + rMargin);
                decimal oMargin = aMargin * 100 / portfolioPosition.ExpectedYield.Value;
                (decimal, decimal, decimal) tuple = (aMargin, rMargin, oMargin);
                margin.Add(tuple);

                portfolioPosition = null;
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestBid, default, default, figi, default, default, DateTime.Now.ToUniversalTime(), default);
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" FromLong " + figi + "price " + bestBid + "candleTime: " + lastTime);
                    sw.WriteLine();
                }

                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, marginFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(margin.Sum(x => x.Item1) + " " + margin.Sum(x => x.Item2) + " " + margin.Sum(x => x.Item3) + " " + lastTime);
                    sw.WriteLine();
                }
                Log.Information("Stop trade: " + figi + " TradeOperation.fromLong");
            }

            if (tradeTarget == TradeTarget.toShort
                &&
                portfolioPosition?.Balance == 0
                )
            {

                int countBalance = -1;
                portfolioPosition = new Portfolio.Position(default, figi, default, default, default, countBalance, default, new MoneyAmount(Currency.Usd, bestBid), countBalance, new MoneyAmount(Currency.Usd, bestBid), default);
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestBid, default, default, figi, default, default, DateTime.Now.ToUniversalTime(), default);
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" ToShort " + figi + "price " + bestBid + "candleTime: " + lastTime);
                    sw.WriteLine();
                }
                Log.Information("Stop trade: " + figi + " TradeOperation.toShort");
            }

            if (tradeTarget == TradeTarget.fromShort
                &&
                portfolioPosition?.Balance < 0
                )
            {
                decimal aMargin = portfolioPosition.ExpectedYield.Value - close;
                Log.Information("aMargin= " + aMargin);
                decimal comis = COM * (close + portfolioPosition.ExpectedYield.Value);
                Log.Information("comis= " + comis);
                decimal rMargin = aMargin - comis;
                Log.Information("rMargin= " + rMargin);
                decimal oMargin = aMargin * 100 / portfolioPosition.ExpectedYield.Value;
                (decimal, decimal, decimal) tuple = (aMargin, rMargin, oMargin);
                margin.Add(tuple);

                portfolioPosition = null;
                tradeOperation = new TradeOperation(default, default, default, default, default, default, bestAsk, default, default, figi, default, default, DateTime.Now.ToUniversalTime(), default);


                using (StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + @" FromShort " + figi + "price " + bestAsk + "candleTime: " + lastTime);
                    sw.WriteLine();
                }

                using (StreamWriter sw = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, marginFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(margin.Sum(x => x.Item1) + " " + margin.Sum(x => x.Item2) + " " + margin.Sum(x => x.Item3) + " " + lastTime);
                    sw.WriteLine();
                }
                Log.Information("Stop trade: " + figi + " TradeOperation.fromShort");
            }
            tradeOperations.Add(tradeOperation);
        }
    }
}