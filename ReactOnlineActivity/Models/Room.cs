using System.Collections.Generic;
using ReactOnlineActivity.Models;

namespace Game.Domain
{
    public class Room
    {
        public int Id { get; set; }   
        public GameDto Game { get; set; }
        public RoomSettings Settings { get; set; }
    }
}