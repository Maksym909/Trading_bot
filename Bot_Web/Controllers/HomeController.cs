using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Bot_Web.Models;
using Trading_bot;
using System.Threading;
using Binance.Net;
using Binance.Net.Interfaces;
using Bot_Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections;

namespace Bot_Web.Controllers
{
    public class Trader
    {
        //private int id;
        //private List<Action<string>> callback = new List<Action<string>>();
        
        public static Dictionary<string, string> showInfoToTrader = new Dictionary<string, string>();
        public void AddTrader(string symbol, string id)
        {
            showInfoToTrader.Add( symbol, id);
        }
        public string GetValue(string id)
        {
            foreach (var item in showInfoToTrader.Values)
            {
                if (item == id)
                    return item;
            }
            return null;
        }
    }
    public static class ClassSubscribers
    {
        //sub and unsub // pair symbol id-client
        //private static Dictionary<string, List<Trader>> _subscription = new Dictionary<string, List<SubscriptionClients>>();

        
        public static void AddSubscribe(string symbol)
        {
            string id = null;
            foreach (var tempId in Clients.Ids)
            {
               // if (!Clients.Ids.Contains(id) && !_subscription.ContainsKey(symbol))
                    id = tempId;
            }

           // _subscription.Add(symbol, id.);
        }
     
    }



    public class HomeController : Controller
    {
       
        public CancellationTokenSource TokenSource;
        private static string _symbol { get; set; }
        public static string _id { get; set; }
        private static bool _isCalledStream = false;
        private static DisplayBot displayBot;

        #region hub configuration

        private readonly IHubContext<BotHub> _hubContext;
       
       

        public HomeController(IHubContext<BotHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task StartStream()
        {
            AppConstants.TokenSource = new CancellationTokenSource();

            if (_isCalledStream)
            {
                await SendUpdates($"Already pressed");
            }
            else
            {
                string tempSymbol = _symbol;
                //var 
                displayBot = new DisplayBot();
                displayBot.AddSymbol(tempSymbol);
                _isCalledStream = true;
              
                while (true)
                {
                    var localTime = DateTime.UtcNow.ToLocalTime().ToString("HH:mm:ss");

                    if (!AppConstants.TokenSource.Token.IsCancellationRequested)
                    {
                        await SendUpdates(
                            $"Time : {localTime} | Symbol : {displayBot.Symbol} | Price : {displayBot.Price} $");
                        Thread.Sleep(1000);
                    }
                    else
                    {
                        await displayBot.UnsubscribeBot();
                        await SendUpdates($"stopped");
                        break;
                    }
                }
            }

        }

        [HttpPost]
        public async Task StopStream()
        {
            _isCalledStream = false;
        
            if (AppConstants.TokenSource != null)
            {
                await SendUpdates($"Stream Stop");
                AppConstants.TokenSource.Cancel();
            }
        }

        [HttpPost]
        public async Task BuyOrder()
        {
            await SendUpdates($"Set Buy Order on {_symbol}");
        }
        [HttpPost]
        public async Task SellOrder()
        {
            await SendUpdates($"Set Sell Order on {_symbol}");
        }
        private async Task SendUpdates(string msg)
        {
            Trader trd = new Trader();
            
            string iDvalue = null;
            foreach (var VARIABLE in Clients.Ids)
            {
                iDvalue = VARIABLE;
                _id = iDvalue;
                //trd.AddTrader(_id, _symbol);

                //if (Trader.showInfoToTrader.Keys.Contains(_symbol) && Trader.showInfoToTrader.Values.Contains(_id))
                    //await _hubContext.Clients.Client(_id).SendAsync("TaskUpdate", msg);
            }
            
            await _hubContext.Clients.Client(_id).SendAsync("TaskUpdate", msg);

            //string name = null;
            //foreach (var item in Clients.Names)
            //{
            //    name = item;
            //}
            //await _hubContext.Clients.User(name).SendAsync("TaskUpdate", msg);
            
        }
        

        #endregion


        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string Symbol) 
        {
            string name = null;
            foreach (var item in Clients.Names)
            {
                name = item;
            }
            //displayBot = new DisplayBot();
            _symbol = Symbol;
             ViewBag.Counter = DisplayBot.Counter;
            ViewBag.User = name;
            ViewBag.Count = Clients.Ids.Count();
            ViewBag.Symbol = _symbol;
            
            return View();
        }
    }
}

