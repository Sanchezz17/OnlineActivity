using System;
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
        private readonly RoomRepository roomRepository;
        private readonly PlayerRepository playerRepository;
        private readonly IMapper mapper;
        
        public RoomHub(RoomRepository roomRepository,
            PlayerRepository playerRepository,
            IMapper mapper)
        {
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
            this.mapper = mapper;
        }
        
        public async Task Join(string roomId, string userName, bool alreadyInRoom)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            if (!alreadyInRoom)
            {
                var joinNotification = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    From = $"{userName} вошел в игру"
                };
                await Clients.Group(roomId).SendAsync("newMessage", joinNotification); 
            }

            var room = roomRepository.FindById(int.Parse(roomId));
            var playersCount = room.Game.Players.Count;

            var gameEntity = mapper.Map<GameEntity>(room.Game);
            
            if (playersCount >= room.Settings.MinPlayerCount && gameEntity.GameState == GameState.WaitingForStart)
            {
                gameEntity.Start();

                var newGameDto = mapper.Map<GameDto>(gameEntity);

                roomRepository.UpdateGame(int.Parse(roomId), newGameDto);
                
                await Clients.Group(roomId).SendAsync("round", newGameDto.ExplainingPlayerName);
                await Clients.Group(roomId).SendAsync("timeLeft", gameEntity.GetSecondsLeft());
            }
        }

        public async Task NewPlayer(string roomId, Player player)
        {
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync("newPlayer", player);
        }

        public async Task RequestWord(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameDto, GameEntity>(room.Game);
            
            // toDo mapping не работает! удалить код ниже после исправления
            var random = new Random();
            gameEntity.HiddenWords = room.Game.HiddenWords
                .OrderBy(x => random.Next())
                .ToArray();
            // gameEntity.HiddenWords = themeRepository.GetAllThemes()
            //     .SelectMany(t => t.Words.Select(w => w.Value)).ToArray();
            // toDo перемешать слова
            var hiddenWord = gameEntity.GetCurrentHiddenWord();

            await Clients.Caller.SendAsync("newHiddenWord", hiddenWord);
        }

        public async Task RequestRound(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            await Clients.Group(roomId).SendAsync("round", room.Game.ExplainingPlayerName);
        }

        public async Task RequestTime(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            await Clients.Group(roomId).SendAsync("timeLeft", gameEntity.GetSecondsLeft());
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
            
            var leaveNotification = new Message
            {
                Id = Guid.NewGuid().ToString(),
                From = $"{userName} покинул игру",
                Text = "",
                State = 2
            };
            await Clients.Group(roomId).SendAsync("newMessage", leaveNotification);
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

        public async Task Send(string roomId, Message message)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            var currentRoundNumber = gameEntity.RoundNumber;
            gameEntity.HiddenWords = room.Game.HiddenWords.ToArray();
            var player = gameEntity.Players.First(p => p.Name == message.From);
            var playerGuessed = gameEntity.MakeStep(player, message.Text, room.Settings.MaxPlayerCount);
            var gameDto = mapper.Map<GameDto>(gameEntity);
            roomRepository.UpdateGame(int.Parse(roomId), gameDto);
            if (playerGuessed)
            {
                var guessedNotification = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    From = $"{message.From} угадал слово",
                    Text = "",
                    State = message.State
                };

                await Clients.Group(roomId).SendAsync("newMessage", guessedNotification);
                if (currentRoundNumber < gameEntity.RoundNumber)
                {
                    var newRoundNotification = new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        From = $"Раунд №{gameDto.RoundNumber + 1}",
                        Text = "",
                        State = message.State
                    };
                    await Clients.Group(roomId).SendAsync("newMessage", newRoundNotification);
                }

                if (gameEntity.GameState == GameState.Finished)
                {
                    var pointsWinner = gameEntity.Players.Max(p => p.Score);
                    var winner = gameEntity.Players.First(p => p.Score == pointsWinner);
                    gameEntity.Start();
                    gameDto = mapper.Map<GameDto>(gameEntity);
                    roomRepository.UpdateGame(int.Parse(roomId), gameDto);
                    await Clients.Group(roomId).SendAsync("gameOver",new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        From = $"{winner.Name} победил!"
                    });
                }
                else
                {
                    await Clients.Group(roomId).SendAsync("round", gameEntity.ExplainingPlayerName);
                }

            }
            else
            {
                message.Id = Guid.NewGuid().ToString();
                await Clients.Group(roomId).SendAsync("newMessage", message);
            }
        }
        
        public async Task MessageRated(string roomId, Message message)
        {
            await Clients.Group(roomId).SendAsync("messageRated", message);     
        }
    }
}