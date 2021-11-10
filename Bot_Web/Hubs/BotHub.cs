using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using Binance.Net;
using Binance.Net.Interfaces;
using Bot_Web.Controllers;
using Microsoft.AspNetCore.SignalR;
using Trading_bot;

namespace Bot_Web.Hubs
{

    public class BotHub : Hub
    {
        //public async Task TaskUpdate(string msg)
        //{

        //    //await Clients.All.SendAsync("TaskUpdate", msg);
        //    await Clients.Caller.SendAsync("TaskUpdate", msg);
        //}

        public override Task OnConnectedAsync()
        {
            //for id
            Bot_Web.Clients.Ids.Add(Context.ConnectionId);
            //for user
            Bot_Web.Clients.Names.Add(Context.User.Identity.Name);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            //for id
            Bot_Web.Clients.Ids.Remove(Context.ConnectionId);
            //for user
            Bot_Web.Clients.Names.Remove(Context.User.Identity.Name);
            
            return base.OnDisconnectedAsync(exception);
        }
    }
}
