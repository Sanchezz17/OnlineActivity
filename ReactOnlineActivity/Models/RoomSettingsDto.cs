using System.Collections.Generic;

namespace ReactOnlineActivity.Models
{
    public class RoomSettingsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RoundTime { get; set; }
        public int MaxPlayerCount { get; set; }
        public int PointsToWin { get; set; }
        public bool IsPrivateRoom { get; set; }
        public List<int> ThemesIds { get; set; }
    }
}