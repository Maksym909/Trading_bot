using System;
using System.Threading.Tasks;
using Binance;
using System.Linq;
using System.Threading;
using Binance.Net;
using Binance.Net.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using Binance.Net.Enums;
using CryptoExchange.Net.ExchangeInterfaces;
using CryptoExchange.Net.Sockets;

namespace Trading_bot
{
    public static class AppConstants
    {
        public static CancellationTokenSource TokenSource { get; set; }
    }

    #region Candle info and indocator info
    public interface IOhlcv
    {
      
        decimal Open { get; set; }
        decimal High { get; set; }
        decimal Low { get; set; }
        decimal Close { get; set; }
        decimal Volume { get; set; }
    }

    public class Candle : IOhlcv
    {
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    public static class ConvertExtensions
    {
        public static IOhlcv ToCandle(this IBinanceKline candle)
        {
            return new Candle
            {
                Open = candle.Open,
                High = candle.High,
                Low = candle.Low,
                Close = candle.Close,
                Volume = candle.BaseVolume,
            };
        }
    }
    #region Need Talib extension for indicator
    //public static class IndicatorExtensions
    //{
    //    private static IList<decimal?> FixIndicatorOrdering(IEnumerable<decimal> source, int outBegIdx, int outNbElement)
    //    {
    //        var outValues = new List<decimal?>();
    //        var validItems = source.Take(outNbElement);

    //        for (int i = 0; i < outBegIdx; i++)
    //        {
    //            outValues.Add(null);
    //        }

    //        outValues.AddRange(validItems.Select(value => (decimal?)value));

    //        return outValues;
    //    }

    //    public static IList<decimal?> Rsi(this IList<IOhlcv> source, int period = 14)
    //    {
    //        var rsiValues = new decimal[source.Count];

    //        var closes = source.Select(e => e.Close).ToArray();

    //        var result = TALib.Core.Rsi(closes, 0, source.Count - 1, rsiValues, out int outBegIdx, out int outNbElement, period);

    //        if (result == TALib.Core.RetCode.Success)
    //        {
    //            return FixIndicatorOrdering(rsiValues.ToList(), outBegIdx, outNbElement);
    //        }

    //        throw new ArgumentException("Could not calculate RSI.", nameof(source));
    //    }

    //}
    #endregion
    #endregion
    public class DisplayBot
    {
        public decimal Price { get; set; }
        public string Symbol { get; set; }
        public string Id { get; set; }
        Action<string> TellWhichSymbolWeAreStream;
       public static int Counter { get; set; }

        private UpdateSubscription _subscription;
        private IBinanceSocketClient _socketClient;
        public Action<IBinanceStreamKlineData> OnKlineData { get; set; }
        public IBinanceStreamKlineData LastKline { get; private set; }

        private  IDictionary<string, IList<IOhlcv>> _candles = new Dictionary<string, IList<IOhlcv>>(); //List of candles necessary for indicator
        private  CancellationTokenSource _stoppingCts = new CancellationTokenSource(); //for decline async task

        public DisplayBot()
        {
            _socketClient = new BinanceSocketClient();
            Counter++;
        }
       
        public void AddSymbol(string symbol)
        {
            //var r = _socketClient.Spot.SubscribeToKlineUpdatesAsync(symbol, KlineInterval.OneMinute, data =>
            //{
            //    LastKline = data.Data;
            //    OnKlineData?.Invoke(data.Data);
            //}).Result;

            var r = _socketClient.Spot.SubscribeToKlineUpdatesAsync(symbol, KlineInterval.OneMinute,
                data => EatPrice(data.Data.Symbol, Math.Round(data.Data.Data.Close,2))).Result;


            if (r.Success)
                _subscription = r.Data;

            //return IsChangedSymbol = true;
        }
        
        
        public void UnsubsribeAllDisplayBot()
        {
            var unsubscribeAll = _socketClient.UnsubscribeAllAsync();
        }

        public async Task UnsubscribeBot()
        {
            await _socketClient.UnsubscribeAsync(_subscription);
            Counter--;
        }

      

    # region Down below console ouput
        ///
        /// Down below console ouput 
        /// 
        public void ConsoleEverySecondPrice(string symbol, IOhlcv ohlcv)
        {
            var loclaTime = DateTime.UtcNow.ToLocalTime();

            Console.WriteLine(
                $"[{loclaTime}] Symbol: {symbol} | Price: {ohlcv.Close}");

        }
        private void ConsolePrintInfoFinal(string symbol, IOhlcv ohlcv)
        {
            var loclaTime = DateTime.UtcNow.ToLocalTime();
            
            Console.WriteLine(
                $"FINAL [{loclaTime}] Symbol: {symbol} | " +
                $"Price: {ohlcv.Close} | Candles: {_candles[symbol].Count}");
        }

        #region Code where we use Talib extension

        //public async Task StartAsync(IBinanceClient _client, IBinanceSocketClient _socketClient)
        //{

        //    var symbols = new[]
        //    {
        //        "ADAUSDT",
        //        "BTCUSDT",
        //        "XRPUSDT",
        //        "ETHUSDT",
        //    };
        //    var interval = KlineInterval.OneMinute;

        //    foreach (var symbol in symbols)
        //    {
        //        var candleResult = await _client.Spot.Market.GetKlinesAsync(symbol, interval, limit: 1000).ConfigureAwait(false);
        //        if (!candleResult.Success)
        //            continue;

        //        var candles = candleResult.Data.SkipLast(1).Select(x => x.ToCandle()).ToList();
        //        _candles[symbol] = candles;

        //    }

        //    // Subscribe socket 
        //    var subscriptionResult = await _socketClient.Spot.SubscribeToKlineUpdatesAsync(symbols, interval,
        //        data =>
        //        {
        //            var symbol = data.Data.Symbol;
        //            var ohlcv = data.Data.Data.ToCandle();

        //            if (data.Data.Data.Final) // "Final" means finished candle is pushed to us on each KlineInterval 
        //            {
        //                Task.Run(async () =>
        //                {
        //                    //we must add all "final" or indicator will be incorrect
        //                    _candles[symbol].Add(ohlcv);

        //                    ConsolePrintInfoFinal(symbol, ohlcv);
        //                    ConsolePrintRsi(symbol, 6);

        //                    await Task.Delay(10000, _stoppingCts.Token).ConfigureAwait(false);
        //                }, _stoppingCts.Token);
        //            }
        //            else // every not "Final" candle update. Here we check whether the current price met our take profit, stop loss or trailing stop loss conditions.
        //            {
        //                ConsoleEverySecondPrice(symbol, ohlcv);

        //            }
        //        })
        //        .ConfigureAwait(false);


        //}

        //private void ConsolePrintRsi(string symbol,int rsiLength)
        //{
        //    var rsi = _candles[symbol].Rsi(rsiLength);
        //    Console.WriteLine("=======================");
        //    Console.WriteLine($"// {symbol} // RSI = {rsi.Last().Value} ");
        //    Console.WriteLine("=======================");
        //}
        #endregion
        private void EatPrice(string pair,decimal price)
        {
            Price = price;
            Symbol = pair;
            Console.WriteLine($"{pair} -- {price}");
        }
        #endregion
    }
    #region Client
    //public class Client
    //{
    //    private BinanceClient _client = new BinanceClient();


    //    public decimal Balance { get; private set; }

    //    public Client()
    //    {

    //    }
    //    public Client(string key,string secret)
    //    {
    //        _client.SetApiCredentials(key,secret); // value for authentication
    //    }

    //    public async void SetOrder(string symbol,OrderSide order)
    //    {
    //        var callResult = await _client.Spot.Order.PlaceOrderAsync(symbol, order, OrderType.Limit, quantity: 10,
    //            price: 50, timeInForce: TimeInForce.GoodTillCancel);
    //        if (callResult.Success)
    //        {

    //        }


    //    }



    //}
    #endregion

    public static class Symbols
    {
        public static readonly List<string> listSymbols = new List<string> { "BTCUSDT", "ETHUSDT", "XRPUSDT", "ADAUSDT", "BNBUSDT", "DOGEUSDT", "DOTUSDT", "CAKEUSDT", "LTCUSDT" };
    }
    class Program
    {
       
        static void Main(string[] args)
        {

            DisplayBot bot = new DisplayBot();

            Console.WriteLine("Add ada");
            bot.AddSymbol("ADAUSDT");
            Thread.Sleep(2000);
            
            Console.WriteLine("===========unsubs========");
            bot.UnsubscribeBot();
            Console.WriteLine("Should stop streaming ");
            foreach (var item in Symbols.listSymbols)
            {
                Console.WriteLine(item);
            }
            Console.ReadLine();

        }
      
    }
}
