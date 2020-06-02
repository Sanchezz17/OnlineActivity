using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.SignalR;
using ReactOnlineActivity.Hubs.Constants;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;
using ReactOnlineActivity.Services;

namespace ReactOnlineActivity.Hubs
{
    public class RoomHub : Hub
    {
        private readonly RoomRepository roomRepository;
        private readonly PlayerRepository playerRepository;
        private readonly UserRepository userRepository;
        private readonly IMapper mapper;

        public RoomHub(RoomRepository roomRepository,
            PlayerRepository playerRepository,
            UserRepository userRepository,
            IMapper mapper)
        {
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        public async Task Join(string roomId, string userName, bool alreadyInRoom)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
            if (!alreadyInRoom)
            {
                var joinNotification = MessageFactory.CreateJoinNotification(userName);
                await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, joinNotification);
            }

            var room = roomRepository.FindById(int.Parse(roomId));
            var playersCount = room.Game.Players.Count;

            var gameEntity = mapper.Map<GameEntity>(room.Game);

            if (playersCount >= room.Settings.MinPlayerCount && gameEntity.GameState == GameState.WaitingForStart)
            {
                gameEntity.Start();

                var newGameDto = mapper.Map<GameDto>(gameEntity);

                roomRepository.UpdateGame(int.Parse(roomId), newGameDto);

                await Clients.Group(roomId).SendAsync(RoomHubEvents.RoundInfo, 
                    room.Game.ExplainingPlayerName, gameEntity.GetSecondsLeft());
            }
        }

        public async Task NewPlayer(string roomId, Player player)
        {
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync(RoomHubEvents.NewPlayer, player);
        }

        public async Task RequestWord(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            var hiddenWord = gameEntity.GetCurrentHiddenWord();
            await Clients.Caller.SendAsync(RoomHubEvents.NewHiddenWord, hiddenWord);
        }

        public async Task RequestRound(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            await Clients.Group(roomId).SendAsync(RoomHubEvents.RoundInfo, 
                room.Game.ExplainingPlayerName, gameEntity.GetSecondsLeft());
        }

        public async Task NewLine(string roomId, LineDto line)
        {
            if (line == null)
                return;
            Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync(RoomHubEvents.NewLine, line);
            var newLine = new Line {Value = new List<Coordinate>(), Color = line.Color};
            for (var i = 0; i < line.Coordinates.Length; i++)
                newLine.Value.Add(new Coordinate {Value = line.Coordinates[i], SerialNumber = i});
            roomRepository.AddLineToFieldIntoRoom(int.Parse(roomId), newLine);
            
        }

        public async Task ClearField(string roomId)
        {
            roomRepository.ClearField(int.Parse(roomId));
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync(RoomHubEvents.ClearField);
        }

        public async Task GiveUp(string roomId, string playerName)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            var currentRoundNumber = gameEntity.RoundNumber;
            var currentHiddenWord = gameEntity.GetCurrentHiddenWord();
            gameEntity.CompleteRound();
            var gameDto = mapper.Map<GameDto>(gameEntity);
            roomRepository.UpdateGame(int.Parse(roomId), gameDto);
            var giveUpNotification = MessageFactory.CreateGiveUpNotification(playerName);
            await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, giveUpNotification);

            OnNextStep(roomId, currentRoundNumber, currentHiddenWord, gameDto, gameEntity);
        }

        public async Task Leave(string roomId, string userName)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            if (room.Game.Players.All(p => p.Name != userName))
                return;

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
            await Clients.GroupExcept(roomId, new[] {Context.ConnectionId})
                .SendAsync(RoomHubEvents.Leave, userName);

            var leaveNotification = MessageFactory.CreateLeaveNotification(userName);
            await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, leaveNotification);
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
                await Clients.Group(roomId).SendAsync(RoomHubEvents.RoundInfo, 
                    gameEntity.ExplainingPlayerName, gameEntity.GetSecondsLeft());
            }

            if (playersCount >= room.Settings.MinPlayerCount && gameEntity.GameState == GameState.Started
                                                             && userName == gameEntity.ExplainingPlayerName)
            {
                gameEntity.StartNewRound();
                var newGameDto = mapper.Map<GameDto>(gameEntity);
                roomRepository.UpdateGame(int.Parse(roomId), newGameDto);
                await Clients.Group(roomId).SendAsync(RoomHubEvents.RoundInfo,
                    gameEntity.ExplainingPlayerName, gameEntity.GetSecondsLeft());
            }
        }

        public async Task Send(string roomId, Message message)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            var currentRoundNumber = gameEntity.RoundNumber;
            var currentHiddenWord = gameEntity.GetCurrentHiddenWord();
            var player = gameEntity.Players.First(p => p.Name == message.From);
            var playerGuessed = gameEntity.MakeStep(player, message.Text, room.Settings.MaxPlayerCount);
            var gameDto = mapper.Map<GameDto>(gameEntity);
            roomRepository.UpdateGame(int.Parse(roomId), gameDto);
            if (playerGuessed)
            {
                var guessedNotification = MessageFactory.CreateGuessedNotification(message.From);
                await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, guessedNotification);

                OnNextStep(roomId, currentRoundNumber, currentHiddenWord, gameDto, gameEntity);
            }
            else
            {
                message.Id = Guid.NewGuid().ToString();
                await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, message);
            }
        }

        private async Task OnNextStep(
            string roomId,
            int currentRoundNumber,
            string currentHiddenWord, 
            GameDto gameDto,
            GameEntity gameEntity)
        {
            if (currentRoundNumber < gameEntity.RoundNumber)
                await SendNotificationAfterEndOfRound(roomId, currentHiddenWord, currentRoundNumber + 1);

            if (gameEntity.GameState == GameState.Finished)
            {
                var pointsWinner = gameEntity.Players.Max(p => p.Score);
                var winner = gameEntity.Players.First(p => p.Score == pointsWinner);
                userRepository.IncreaseWinsCount(winner);
                foreach (var player in gameEntity.Players)
                    userRepository.UpdateStatistics(player);

                gameEntity.Start();
                gameDto = mapper.Map<GameDto>(gameEntity);
                roomRepository.UpdateGame(int.Parse(roomId), gameDto);
                var victoryNotification = MessageFactory.CreateVictoryNotification(winner.Name);
                await Clients.Group(roomId).SendAsync(RoomHubEvents.GameOver, victoryNotification);
            }
            else
            {
                await Clients.Group(roomId).SendAsync(RoomHubEvents.RoundInfo, 
                    gameEntity.ExplainingPlayerName, gameEntity.GetSecondsLeft());
            }
        }

        public async Task MessageRated(string roomId, Message message)
        {
            await Clients.Group(roomId).SendAsync(RoomHubEvents.MessageRated, message);
        }

        public async Task TimeOver(string roomId)
        {
            var room = roomRepository.FindById(int.Parse(roomId));
            var gameEntity = mapper.Map<GameEntity>(room.Game);
            gameEntity.UpdateLevel();
            var gameDto = mapper.Map<GameDto>(gameEntity);
            var currentHiddenWord = gameEntity.GetCurrentHiddenWord();
            var currentRoundNumber = gameEntity.RoundNumber;
            roomRepository.UpdateGame(int.Parse(roomId), gameDto);
            await SendNotificationAfterEndOfRound(roomId, currentHiddenWord, currentRoundNumber + 1);
            await Clients.Group(roomId).SendAsync(RoomHubEvents.RoundInfo, 
                gameEntity.ExplainingPlayerName, gameEntity.GetSecondsLeft());
        }

        private async Task SendNotificationAfterEndOfRound(string roomId, string currentHiddenWords, int newRoundNumber)
        {
            var hiddenWordNotification = MessageFactory.CreateHiddenWordNotification(currentHiddenWords);
            await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, hiddenWordNotification);
            var newRoundNotification = MessageFactory.CreateNewRoundNotification(newRoundNumber);
            await Clients.Group(roomId).SendAsync(RoomHubEvents.NewMessage, newRoundNotification);
        }
    }
}