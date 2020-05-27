using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class ThemeRoomSettings
    {
        public int ThemeId { get; set; }
        public Theme Theme { get; set; }
        public int RoomSettingsId { get; set; }
        public RoomSettings RoomSettings { get; set; }
    }
}