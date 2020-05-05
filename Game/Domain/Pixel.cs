using System.Drawing;

namespace Game.Domain
{
    public class Pixel
    {
        public Rgb Color { get; }
        public Point Location { get; }

        public Pixel(Rgb color, Point location)
        {
            Color = color;
            Location = location;
        }
    }
}