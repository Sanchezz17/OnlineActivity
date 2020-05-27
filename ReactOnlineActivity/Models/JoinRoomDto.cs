using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class JoinRoomDto
    {
        public bool AlreadyInRoom { get; set; }
        public Player Player { get; set; }
    }
}