using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
namespace barangay_crime_compliant_api.Hubs
{
    public class ChatHub : Hub
    {
        public async Task SendMessage(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", message);
        }
    }

}
