using System.Collections.Generic;
using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class RoomSettings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RoundTime { get; set; }
        public int MaxPlayerCount { get; set; }
        public int PointsToWin { get; set; }
        public bool IsPrivateRoom { get; set; }
        public List<Theme> Themes { get; set; }

        public RoomSettings(
            string name = "", 
            string description = "", 
            int roundTime = 60,
            int maxPlayerCount = 10,
            int pointsToWin = 100,
            bool isPrivateRoom = false)
        {
            Name = name;
            Description = description;
            RoundTime = roundTime;
            MaxPlayerCount = maxPlayerCount;
            PointsToWin = pointsToWin;
            IsPrivateRoom = isPrivateRoom;
        }
    }
}