using System.Collections.Generic;

namespace Game.Domain
{
    public class Line
    {
        public int Id { get; set; }
        public List<Coordinate> Value { get; set; }

        public string Color { get; set; }
    }
}