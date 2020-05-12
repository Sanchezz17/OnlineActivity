using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ReactOnlineActivity.Hubs
{
    public class ChatHub : Hub
    {
        public async Task JoinRoomChat(string roomId, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("notify", $"{username} вошел в чат");
        }
        
        public async Task Send(string roomId, string from, string text)
        {
            await Clients.Group(roomId).SendAsync("newMessage", from, text);
        }
    }
}