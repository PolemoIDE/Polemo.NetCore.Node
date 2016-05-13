using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Extensions.Logging;

namespace Polemo.NetCore.Node.Hubs
{
    [HubName("polemo")]
    public partial class PolemoHub : Hub
    {
        private readonly ILogger _logger;

        public PolemoHub(ILogger<PolemoHub> logger)
        {
            _logger = logger;
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
