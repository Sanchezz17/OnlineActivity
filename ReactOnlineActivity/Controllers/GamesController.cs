using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Game.Domain;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;
using Line = Game.Domain.Line;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api")]
    public class GamesController : Controller
    {
        private readonly UserRepository userRepository;
        private readonly RoomRepository roomRepository;
        private readonly PlayerRepository playerRepository;
        private readonly ThemeRepository themeRepository;
        private readonly IMapper mapper;

        public GamesController(
            UserManager<ApplicationUser> userManager,
            UserRepository userRepository,
            RoomRepository roomRepository,
            PlayerRepository playerRepository,
            ThemeRepository themeRepository,
            IMapper mapper)
        {
            this.userRepository = userRepository;
            this.roomRepository = roomRepository;
            this.playerRepository = playerRepository;
            this.themeRepository = themeRepository;
            this.mapper = mapper;
        }

        [HttpGet("play")]
        public RedirectResult Play()
        {
            var suitableRoom = roomRepository.FindSuitable();
            if (suitableRoom == null)
            {
                suitableRoom = CreateTestRoom();
                //roomRepository.Insert(suitableRoom);
            }

            return Redirect($"/rooms/{suitableRoom.Id}");
        }

        [HttpGet("players")]
        public IEnumerable<PlayerDto> GetPlayers([FromQuery] int roomId) => playerRepository.SelectAllFromRoom(roomId);

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
            var themes = roomSettings.ThemesIds.Count == 0 
                ? themeRepository.GetDefaultThemes()
                : roomSettings.ThemesIds.Select(id => themeRepository.FindById(id)).ToList();

            var settings = mapper.Map<RoomSettings>(roomSettings);
            var words = themes.SelectMany(t => t.Words);
            var newRoom = new Room
            {
                Game = CreateTestGame(words.ToList()),
                Settings = settings
            };

            roomRepository.Insert(newRoom);

            foreach (var theme in themes)
            {
                var themeRoomSettings = new ThemeRoomSettings
                    {RoomSettingsId = newRoom.Settings.Id, ThemeId = theme.Id};
                settings.ThemeRoomSettings.Add(themeRoomSettings);
                theme.ThemeRoomSettings.Add(themeRoomSettings);
            }

            return newRoom.Id.ToString();
        }


        [HttpGet("fields/{roomId}")]
        public LineDto[] GetField([FromRoute] int roomId)
        {
            var room = roomRepository.FindById(roomId);
            var canvas = room.Game.Canvas;
            return canvas
                .Select(line => new LineDto
                {
                    Coordinates = line.Value
                        .OrderBy(c => c.SerialNumber)
                        .Select(c => c.Value)
                        .ToArray(),
                    Color = line.Color
                })
                .ToArray();
        }

        [HttpGet("themes")]
        public List<Theme> GetThemes()
        {
            return themeRepository.GetAllThemes().ToList();
        }

        [HttpPost("themes")]
        public void AddTheme([FromBody] Theme theme)
        {
            themeRepository.Insert(theme);
        }

        private Room CreateTestRoom()
        {
            var roomSettings = new RoomSettings();
            var themes = themeRepository.GetAllThemes();
            var words = themes.SelectMany(t => t.Words);
            var newRoom = new Room
            {
                Game = CreateTestGame(words.ToList()),
                Settings = roomSettings
            };

            roomRepository.Insert(newRoom);

            foreach (var theme in themes)
            {
                newRoom.Settings.ThemeRoomSettings.Add(new ThemeRoomSettings
                    {RoomSettingsId = newRoom.Settings.Id, ThemeId = theme.Id});
            }

            return newRoom;
        }

        private GameDto CreateTestGame(List<Word> words)
        {
            return new GameDto
            {
                HiddenWords = words,
                Players = new List<PlayerDto>(),
                CurrentRoundStartTime = DateTime.Now.ToEpochTime(),
                Canvas = new List<Line>()
            };
        }
    }
}