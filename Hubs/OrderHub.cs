using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace BoxManager.Hubs
{
    public class OrderHub : Hub
    {
        public async Task UpdateOrderStatus(int orderId, string newStatus)
        {
            await Clients.All.SendAsync("ReceiveStatusUpdate", orderId, newStatus);
        }

        public async Task NotifyNewOrder(int orderId)
        {
            await Clients.All.SendAsync("ReceiveNewOrder", orderId);
        }
    }
}
