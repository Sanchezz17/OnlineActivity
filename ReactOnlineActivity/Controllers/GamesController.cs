using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Game.Domain;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> userManager;
        private readonly UserRepository userRepository;
        private readonly RoomRepository roomRepository;
        private readonly PlayerRepository playerRepository;
        private readonly IMapper mapper;

        public GamesController(
            UserManager<ApplicationUser> userManager,
            UserRepository userRepository,
            RoomRepository roomRepository,
            PlayerRepository playerRepository,
            IMapper mapper)
        {
            this.userManager = userManager;
            this.userRepository = userRepository;
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
            this.mapper = mapper;
        }

        [HttpGet("play")]
        public RedirectResult Play()
        {
            var suitableRoom = roomRepository.FindSuitable();
            if (suitableRoom == null)
            {
                suitableRoom = CreateTestRoom();
                roomRepository.Insert(suitableRoom);
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
            return roomRepository.FindById(roomId);
        }

        [HttpGet("rooms/{roomId}/join")]
        public JoinRoomDto JoinRoom([FromRoute] int roomId, [FromQuery] string userName)
        {
            var room = roomRepository.FindById(roomId);

            var user = userRepository.FindByName(userName);

            var player = room
                .Game
                .Players
                .FirstOrDefault(p => p.Name == user.UserName);

            var joinRoomDto = new JoinRoomDto();

            if (player == null)
            {
                player = new PlayerDto
                {
                    Name = user.UserName,
                    PhotoUrl = user.PhotoUrl,
                    Score = 0
                };
                roomRepository.InsertPlayerIntoRoom(roomId, player);
            }
            else
            {
                joinRoomDto.AlreadyInRoom = true;
            }

            joinRoomDto.Player = player;

            return joinRoomDto;
        }

        [HttpPost("rooms")]
        public string CreateRoom([FromBody] RoomSettingsDto roomSettings)
        {

            var newRoom = new Room
            {
                Game = CreateTestGame(),
                Settings = mapper.Map<RoomSettings>(roomSettings)
            };

            roomRepository.Insert(newRoom);

            return newRoom.Id.ToString();
        }

        private List<Theme> GetThemesByIds(List<int> themeIds)
        {
            var result = new List<Theme>();
            return result;
        }


        [HttpGet("fields/{roomId}")]
        public double[][] GetField([FromRoute] int roomId)
        {
            var room = roomRepository.FindById(roomId);
            var canvas = room.Game.Canvas;
            return canvas.Select(line => line.Value.Select(c => c.Value).ToArray()).ToArray();
        }

        [HttpPost("fields/{roomId}")]
        public OkResult AddLine([FromBody] double[][] lineDto, [FromRoute] int roomId)
        {
            var newCanvas = new List<LineDto>();
            foreach (var line in lineDto)
            {
                var newLine = new LineDto {Value = new List<CoordinateDto>()};
                foreach (var coordinate in line)
                    newLine.Value.Add(new CoordinateDto {Value = coordinate});
                newCanvas.Add(newLine);
            }

            roomRepository.UpdateFieldIntoRoom(roomId, newCanvas);
            return Ok();
        }

        private Room CreateTestRoom()
        {
            return new Room
            {
                Game = CreateTestGame(),
                Settings = new RoomSettings()
            };
        }

        private GameDto CreateTestGame()
        {
            return new GameDto
            {
                HiddenWords = new List<Word> {new Word {Value = "kek"}, new Word {Value = "lol"}},
                Players = new List<PlayerDto>(),
                TimeStartGame = DateTime.Now.ToEpochTime(),
                Canvas = new List<LineDto>()
            };
        }
    }
}