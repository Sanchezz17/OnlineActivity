using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Hubs
{
    public class RoomHub : Hub
    {
        public async Task Join(string roomId, string userName, bool alreadyInRoom)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            if (!alreadyInRoom)
                await Clients.Group(roomId).SendAsync("notify", $"{userName} вошел в игру");
        }

        public async Task NewPlayer(string roomId, PlayerDto player)
        {
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("newPlayer", player);
        }
        
        public async Task Leave(string roomId, string userName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("leave", userName);
            await Clients.Group(roomId).SendAsync("notify", $"{userName} покинул игру");
        }

        public async Task Send(string roomId, string from, string text)
        {
            await Clients.Group(roomId).SendAsync("newMessage", from, text);
        }
    }
}