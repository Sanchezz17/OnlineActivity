using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ReactOnlineActivity.Hubs
{
    public class RoomHub : Hub
    {
        public async Task JoinRoom(string roomId, string username)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("notify", $"{username} вошел в игру");
        }
        
        public async Task LeaveRoom(string roomId, string username)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.Group(roomId).SendAsync("notify", $"{username} покинул игру");
        }
        
        public async Task Send(string roomId, string from, string text)
        {
            await Clients.Group(roomId).SendAsync("newMessage", from, text);
        }
    }
}