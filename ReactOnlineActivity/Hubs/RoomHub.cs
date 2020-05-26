using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Game.Domain;
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
        private readonly IMapper mapper;
        
        public RoomHub(UserRepository userRepository,
            RoomRepository roomRepository,
            PlayerRepository playerRepository,
            IMapper mapper)
        {
            this.userRepository = userRepository;
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
            this.mapper = mapper;
        }
        
        public async Task Join(string roomId, string userName, bool alreadyInRoom)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            if (!alreadyInRoom)
                await Clients.Group(roomId).SendAsync("notify", $"{userName} вошел в игру");

            var room = roomRepository.FindById(int.Parse(roomId));
            var playersCount = room.Game.Players.Count;

            var gameEntity = mapper.Map<GameEntity>(room.Game);
            
            if (playersCount >= room.Settings.MinPlayerCount && gameEntity.GameState == GameState.WaitingForStart)
            {
                gameEntity.Start();

                var newGameDto = mapper.Map<GameDto>(gameEntity);

                roomRepository.UpdateGame(int.Parse(roomId), newGameDto);
                
                await Clients.Group(roomId).SendAsync("round", newGameDto.ExplainingPlayerName);
            }
        }

        public async Task NewPlayer(string roomId, PlayerDto player)
        {
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("newPlayer", player);
        }

        public async Task RequestWord(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            gameEntity.HiddenWords = room.Settings.Themes
                .SelectMany(t => t.Words.Select(w => w.Value)).ToArray();
            // toDo перемешать слова
            var hiddenWord = gameEntity.GetCurrentHiddenWord();

            await Clients.Caller.SendAsync("newHiddenWord", hiddenWord);
        }

        public async Task RequestRound(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            await Clients.Group(roomId).SendAsync("round", room.Game.ExplainingPlayerName);
        }

        public async Task NewLine(string roomId, LineDto line)
        {
            if (line == null)
                return;
            var newLine = new Line {Value = new List<Coordinate>(), Color = line.Color};
            for(var i = 0; i < line.Coordinates.Length; i++)
                newLine.Value.Add(new Coordinate {Value = line.Coordinates[i], SerialNumber = i});
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
            var room = roomRepository.FindById(int.Parse(roomId));
            if (room.Game.Players.All(p => p.Name != userName))
                return;
            
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("leave", userName);
            await Clients.Group(roomId).SendAsync("notify", $"{userName} покинул игру");
            playerRepository.DeletePlayerFromRoom(int.Parse(roomId), userName);
            
            var playersCount = room.Game.Players.Count;

            var gameEntity = mapper.Map<GameEntity>(room.Game);

            if (playersCount < room.Settings.MinPlayerCount)
            {
                gameEntity.GameState = GameState.WaitingForStart;
                gameEntity.ExplainingPlayerName = null;
                gameEntity.Canvas = new List<Line>();
                var newGameDto = mapper.Map<GameDto>(gameEntity);
                roomRepository.UpdateGame(int.Parse(roomId), newGameDto);
                await Clients.Group(roomId).SendAsync("round", gameEntity.ExplainingPlayerName);
            }
            
            if (playersCount >= room.Settings.MinPlayerCount && gameEntity.GameState == GameState.Started)
            {
                gameEntity.StartNewRound();
                var newGameDto = mapper.Map<GameDto>(gameEntity);
                roomRepository.UpdateGame(int.Parse(roomId), newGameDto);
                await Clients.Group(roomId).SendAsync("round", gameEntity.ExplainingPlayerName);
            }
        }

        public async Task Send(string roomId, string from, string text)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            gameEntity.HiddenWords = room.Game.HiddenWords.Select(w => w.Value).ToArray();
            var player = gameEntity.Players.First(p => p.Name == from);
            var playerGuessed = gameEntity.MakeStep(player, text);
            var gameDto = mapper.Map<GameDto>(gameEntity);
            roomRepository.UpdateGame(int.Parse(roomId), gameDto);
            if (playerGuessed)
            {
                await Clients.Group(roomId).SendAsync("notify", $"{from} угадал слово\nРаунд №{gameDto.RoundNumber + 1}");
                await Clients.Group(roomId).SendAsync("round", gameEntity.ExplainingPlayerName);
            }
            else
            {
                await Clients.Group(roomId).SendAsync("newMessage", from, text);
            }
        }
    }
}