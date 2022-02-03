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
        public OfflineResearch(ICandlesList _candleList, int _candlesCount = 400, decimal _comission = 0.05m)
        {
            candleList = _candleList;
            candlesCount = _candlesCount;
            comission = _comission;
        }

        ICandlesList candleList;
        int candlesCount;
        readonly decimal comission;

        decimal lastTransactPrice;

        List<(decimal priceMargin, decimal realMargin, decimal percentMargin)> margin = new();

        public void Research()
        {
            for (int i = 0; i < candleList.Candles.Count - candlesCount; i++)
            {
                CandlesList goingCandleList = new CandlesList(candleList.Figi, candleList.Interval, candleList.Candles.Take(candlesCount + i).Skip(i).ToList());
                TradeTarget tradeTarget = default;//Пока не имплементированно
                FixTradeDecision(tradeTarget, goingCandleList.Figi, goingCandleList.Candles.Last().Close, goingCandleList.Candles.Last().Time.AddHours(3), goingCandleList.Interval);
            }
        }
        private void FixTradeDecision(TradeTarget tradeTarget,string figi, decimal price, DateTime lastTime, CandleInterval candleInterval)
        {
            string operationFile = $"_operation {figi} {candleInterval}";
            string marginFile = $"_margin {figi} {candleInterval}";

            int countBalance = 0;

            if (tradeTarget == TradeTarget.toLong
                &&
                countBalance == 0
                )
            {
                countBalance = 1;
                lastTransactPrice = price;
                LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
            }

            if (tradeTarget == TradeTarget.fromLong
                &&
                countBalance > 0)
            {
                countBalance = 0;

                decimal priceMargin = price - lastTransactPrice;
                margin.Add(MarginResult(price, priceMargin));
                LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                LogToMarginFile(lastTime, marginFile);

            }

            if (tradeTarget == TradeTarget.toShort
                &&
                countBalance == 0
                )
            {
                countBalance = -1;
                lastTransactPrice = price;
                LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
            }

            if (tradeTarget == TradeTarget.fromShort
                &&
                countBalance < 0
                )
            {
                countBalance = 0;

                decimal priceMargin = lastTransactPrice - price;
                margin.Add(MarginResult(price, priceMargin));
                LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                LogToMarginFile(lastTime, marginFile);

            }

            static void LogToOperationFile(TradeTarget tradeTarget, string figi, DateTime lastTime, decimal bestAsk, string operationFile)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine(DateTime.Now + $"{tradeTarget} {figi} price {bestAsk} candleTime: {lastTime}");
                    sw.WriteLine();
                }
            }

            void LogToMarginFile(DateTime lastTime, string marginFile)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, marginFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine($"{margin.Sum(x => x.priceMargin)}  {margin.Sum(x => x.realMargin)} {margin.Sum(x => x.percentMargin)} {lastTime}");
                    sw.WriteLine();
                }
            }

            (decimal priceMargin, decimal realMargin, decimal percentMargin) MarginResult(decimal price, decimal priceMargin)
            {
                decimal comissionSum = comission * (price + lastTransactPrice) / 100;

                decimal realMargin = priceMargin - comissionSum;

                decimal percentMargin = realMargin * 100 / lastTransactPrice;
                               
                return (priceMargin, realMargin, percentMargin);
            }
        }
    }
}