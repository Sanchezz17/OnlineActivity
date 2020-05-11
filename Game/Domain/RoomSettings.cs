using System.Collections.Generic;

namespace Game.Domain
{
    public class RoomSettings
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int RoundTime { get; set; }
        public int MaxPlayerCount { get; set; }
        public int PointsToWin { get; set; }
        public bool IsPrivateRoom { get; set; }
        public List<Theme> Themes { get; set; }
    }
}