using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Binance.Net;
using Trading_bot;

namespace Bot_Web
{
    public static class Clients
    {
        public static readonly List<string> Ids = new List<string>();
        public static readonly List<string> Names = new List<string>();
    }
    public static class Symbols
    {
        public static readonly List<string> listSymbols = new List<string> { "BTCUSDT", "ETHUSDT", "XRPUSDT", "ADAUSDT", "BNBUSDT", "DOGEUSDT", "DOTUSDT", "CAKEUSDT", "LTCUSDT" };
    }

 
}