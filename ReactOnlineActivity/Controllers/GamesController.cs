using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Game.Domain;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api")]
    public class GamesController : Controller
    {
        private readonly UserRepository userRepository;
        private readonly RoomRepository roomRepository;
        private readonly PlayerRepository playerRepository;

        public GamesController(UserRepository userRepository,
            RoomRepository roomRepository,
            PlayerRepository playerRepository,
            IMapper mapper)
        {
            this.userRepository = userRepository;
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
            this.mapper = mapper;
        }

        [HttpGet("play")]
        public RedirectResult Play([FromQuery] string userName)
        {
            var decodedUserName = HttpUtility.UrlDecode(userName);
            var user = userRepository.FindByName(decodedUserName);

            var suitableRoom = roomRepository.FindSuitable();
            if (suitableRoom == null)
            {
                suitableRoom = CreateTestRoom();
                roomRepository.Insert(suitableRoom);
            }

            var player = suitableRoom
                .Game
                .Players
                .FirstOrDefault(p => p.Name == decodedUserName);
            if (player == null)
            {
                player = new PlayerDto
                {
                    Name = user.UserName,
                    PhotoUrl = user.PhotoUrl,
                    Score = 0
                };
                roomRepository.InsertPlayerIntoRoom(suitableRoom.Id, player);
            }

            return Redirect($"/rooms/{suitableRoom.Id}");
        }

        [HttpGet("players")]
        public IEnumerable<PlayerDto> GetPlayers([FromQuery] int roomId) => playerRepository.SelectAllFromRoom(roomId);

        [HttpGet("leave")]
        public void LeaveGame([FromQuery] int roomId, [FromQuery] string playerName)
        {
            playerRepository.DeletePlayerFromRoom(roomId, playerName);
        }

        [HttpGet("rooms/{roomId}")]
        public Room GetRoom([FromRoute] int roomId)
        {
            return roomRepository.Find(roomId);
        }

        private Room CreateTestRoom()
        {
            return new Room
            {
                Game = new GameDto
                {
                    HiddenWords = new List<Word> {new Word {Value = "kek"}, new Word {Value = "lol"}},
                    Players = new List<PlayerDto>(),
                    TimeStartGame = DateTime.Now.ToEpochTime()
                },
                Settings = new RoomSettings()
            };
        }
    }
}