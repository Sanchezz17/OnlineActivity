using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Hubs
{
    public class RoomHub : Hub
    {
        private readonly UserRepository userRepository;
        private readonly RoomRepository roomRepository;
        private readonly PlayerRepository playerRepository;
        
        public RoomHub(UserRepository userRepository,
            RoomRepository roomRepository,
            PlayerRepository playerRepository)
        {
            this.userRepository = userRepository;
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
        }
        
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
        
        public async Task NewLine(string roomId, double[] line)
        {
            var newLine = new LineDto {Value = new List<CoordinateDto>()};
            for(var i = 0; i < line.Length; i++)
                newLine.Value.Add(new CoordinateDto {Value = line[i], SerialNumber = i});
            roomRepository.AddLineToFieldIntoRoom(int.Parse(roomId), newLine);
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("newLine", line);
        }

        public async Task ClearField(string roomId)
        {
            roomRepository.ClearField(int.Parse(roomId));
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("clearField");
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