using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pomelo.NetCore.Node.Models;

namespace Pomelo.NetCore.Node.Hubs
{
    public partial class PomeloHub : Hub
    {
        private readonly ILogger _logger;
        public static Config Config;
        public static OmniSharp OmniSharp;

        public PomeloHub(IServiceProvider serviceProvider, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PomeloHub>();
            Config = new Config(serviceProvider, configuration);
            OmniSharp = new OmniSharp(serviceProvider, configuration, loggerFactory);
        }

        public override Task OnConnected()
        {
            _logger.LogDebug($"Connected client with ID: {Context?.ConnectionId}");
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            _logger.LogDebug($"Client with ID: {Context?.ConnectionId} Disconnected");
            return base.OnDisconnected(stopCalled);
        }

        public Task<object> SignIn(string key)
        {
            throw new NotImplementedException();
        }
    }
}
