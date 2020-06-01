using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Game.Domain;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api/rooms")]
    public class RoomsController : Controller
    {
        private readonly RoomRepository roomRepository;
        private readonly ThemeRepository themeRepository;
        private readonly IMapper mapper;
        private readonly Random random;

        public RoomsController(
            RoomRepository roomRepository,
            ThemeRepository themeRepository,
            IMapper mapper)
        {
            this.roomRepository = roomRepository;
            this.themeRepository = themeRepository;
            this.mapper = mapper;
            this.random = new Random();
        }

        /// <summary>
        /// Получение любой свободной комнаты
        /// </summary>
        /// <returns></returns>
        [HttpGet("any")]
        public RedirectResult Play()
        {
            var suitableRoom = roomRepository.FindSuitable() ?? CreateRoom();
            return Redirect($"/rooms/{suitableRoom.Id}");
        }

        /// <summary>
        /// Создание комнаты с указанными настройками
        /// </summary>
        /// <param name="roomSettings"></param>
        /// <returns></returns>
        [HttpPost("")]
        public string CreateRoom([FromBody] RoomSettingsDto roomSettings)
        {
            var themes = roomSettings.ThemesIds.Count == 0
                ? themeRepository.GetDefaultThemes()
                : roomSettings.ThemesIds.Select(id => themeRepository.FindById(id)).ToList();

            var settings = mapper.Map<RoomSettings>(roomSettings);
            var words = GetMixedWordsFromThemes(themes);
            var newRoom = new Room
            {
                Game = CreateGame(words, settings),
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

        /// <summary>
        /// Получение комнаты по Id
        /// </summary>
        /// <param name="roomId"></param>
        /// <returns></returns>
        [HttpGet("{roomId}")]
        public Room GetRoom([FromRoute] int roomId)
        {
            return roomRepository.FindById(roomId);
        }

        private Room CreateRoom()
        {
            var roomSettings = new RoomSettings();
            var themes = themeRepository.GetAllThemes();
            var words = GetMixedWordsFromThemes(themes);
            var newRoom = new Room
            {
                Game = CreateGame(words, roomSettings),
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

        private GameDto CreateGame(List<Word> words, RoomSettings settings)
        {
            return new GameDto
            {
                HiddenWords = words,
                Players = new List<Player>(),
                CurrentRoundStartTime = DateTime.Now.ToEpochTime(),
                Canvas = new List<Line>(),
                MaxPlayerCount = settings.MaxPlayerCount,
                MaxRoundTimeInSeconds = settings.RoundTime,
                PointsToWin = settings.PointsToWin
            };
        }

        private List<Word> GetMixedWordsFromThemes(List<Theme> themes)
        {
            var words = themes
                .SelectMany(t => t.Words)
                .OrderBy(x => random.Next())
                .ToList();

            for (var i = 0; i < words.Count; i++)
                words[i].SerialNumber = i;

            return words;
        }
    }
}