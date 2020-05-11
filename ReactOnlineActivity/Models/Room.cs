namespace ReactOnlineActivity.Models
{
    public class Room
    {
        public int Id { get; set; }   
        public GameDto Game { get; set; }
        public RoomSettings Settings { get; set; }
    }
}