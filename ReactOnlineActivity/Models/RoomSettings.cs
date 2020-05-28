using System.Collections.Generic;
using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class RoomSettings
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int RoundTime { get; set; } = 60;
        public int MaxPlayerCount { get; set; } = 10;

        public int MinPlayerCount { get; set; } = 2;

        public int PointsToWin { get; set; } = 100;
        public bool IsPrivateRoom { get; set; }
        public List<ThemeRoomSettings> ThemeRoomSettings { get; set; } = new List<ThemeRoomSettings>();
    }
}