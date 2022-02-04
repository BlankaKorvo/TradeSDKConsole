using Analysis.TradeDecision;
using MarketDataModules.Candles;
using MarketDataModules.Operation;
using MarketDataModules.Orderbooks;
using MarketDataModules.Portfolio;
using MarketDataModules.Trading;
using Serilog;

namespace Research
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
        int countBalance;
        decimal lastTransactPrice;

        List<(decimal priceMargin, decimal realMargin, decimal percentMargin)> margin = new();

        /// <summary>
        /// 
        /// </summary>
        public void Research()
        {
            for (int i = 0; i < candleList.Candles.Count - candlesCount; i++)
            {
                ICandlesList goingCandleList = new CandlesList(candleList.Figi, candleList.Interval, candleList.Candles.Take(candlesCount + i).Skip(i).ToList());
                //TradeTarget tradeTarget = new GmmaDecision(goingCandleList, goingCandleList.Candles.Last().Close, goingCandleList.Candles.Last().Close).TradeVariant();//Пока не имплементированно
                TradeTarget tradeTarget = new StopReversDecision(goingCandleList, goingCandleList.Candles.Last().Close, goingCandleList.Candles.Last().Close).TradeVariant();
                FixTradeDecision(tradeTarget, goingCandleList.Figi, goingCandleList.Candles.Last().Close, goingCandleList.Candles.Last().Time.AddHours(3), goingCandleList.Interval);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradeTarget"></param>
        /// <param name="figi"></param>
        /// <param name="price"></param>
        /// <param name="lastTime"></param>
        /// <param name="candleInterval"></param>
        private void FixTradeDecision(TradeTarget tradeTarget,string figi, decimal price, DateTime lastTime, CandleInterval candleInterval)
        {
            string operationFile = $"_operation {figi} {candleInterval}";
            string marginFile = $"_margin {figi} {candleInterval}";

            

            if (tradeTarget == TradeTarget.toLong)
            {
                if (countBalance == 0)
                {
                    LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                    lastTransactPrice = price;
                    countBalance = 1;
                    return;
                }
                if (countBalance < 0)
                {
                    decimal priceMargin = (lastTransactPrice - price);
                    margin.Add(MarginResult(price, priceMargin));
                    LogToMarginFile(lastTime, marginFile);
                    LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                    lastTransactPrice = price;
                    countBalance = 1;
                    return;
                }

            }

            if (tradeTarget == TradeTarget.fromLong
                &&
                countBalance > 0)
            {
                decimal priceMargin = price - lastTransactPrice;
                margin.Add(MarginResult(price, priceMargin));
                LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                LogToMarginFile(lastTime, marginFile);
                countBalance = 0;
                return;
            }

            if (tradeTarget == TradeTarget.toShort)
            {
                if (countBalance == 0)
                {
                    LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                    countBalance = -1;
                    lastTransactPrice = price;
                    return;
                }
                if (countBalance > 0)
                {
                    decimal priceMargin = (price - lastTransactPrice);
                    margin.Add(MarginResult(price, priceMargin));
                    LogToMarginFile(lastTime, marginFile);
                    LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                    countBalance = -1;
                    lastTransactPrice = price;
                    return;
                }

            }
            if (tradeTarget == TradeTarget.fromShort
                &&
                countBalance < 0
                )
            {
                decimal priceMargin = lastTransactPrice - price;
                margin.Add(MarginResult(price, priceMargin));                
                LogToMarginFile(lastTime, marginFile);
                LogToOperationFile(tradeTarget, figi, lastTime, price, operationFile);
                countBalance = 0;
                return;
            }
                      
            
            ///
            static void LogToOperationFile(TradeTarget tradeTarget, string figi, DateTime lastTime, decimal price, string operationFile)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, operationFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine($"{DateTime.Now} {tradeTarget} {figi} price {price} candleTime: {lastTime}");
                    sw.WriteLine();
                }
            }

            ///
            void LogToMarginFile(DateTime lastTime, string marginFile)
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, marginFile), true, System.Text.Encoding.Default))
                {
                    sw.WriteLine($"{margin.Sum(x => x.priceMargin)}  {margin.Sum(x => x.realMargin)} {margin.Sum(x => x.percentMargin)} {lastTime}");
                    sw.WriteLine();
                }
            }

            ///
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