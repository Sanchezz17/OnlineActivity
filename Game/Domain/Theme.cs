using System.Collections.Generic;

namespace Game.Domain
{
    public class Theme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Word> Words { get; set; }
    }
}