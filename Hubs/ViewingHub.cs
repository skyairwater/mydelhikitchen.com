using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EcommerceStore.Hubs
{
    public class ViewingHub : Hub
    {
        // Static dictionary to track connection counts per page (if needed) or just total
        // For simplicity, we'll track total connected users for the whole app OR specific groups
        private static int _totalViewers = 0;

        public override async Task OnConnectedAsync()
        {
            _totalViewers++;
            await Clients.All.SendAsync("UpdateViewerCount", _totalViewers);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _totalViewers = Math.Max(0, _totalViewers - 1);
            await Clients.All.SendAsync("UpdateViewerCount", _totalViewers);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task GetCurrentCount()
        {
            await Clients.Caller.SendAsync("UpdateViewerCount", _totalViewers);
        }
    }
}
