using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReactOnlineActivity.Models
{
    [Table("Theme")]
    public class Theme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public List<Word> Words { get; set; }
        public List<ThemeRoomSettings> ThemeRoomSettings { get; set; } = new List<ThemeRoomSettings>();
    }
}