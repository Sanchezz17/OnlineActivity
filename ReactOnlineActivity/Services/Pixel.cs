using System.Drawing;

namespace ReactOnlineActivity.Services
{
    public class Pixel
    {
        public RGB Color { get; set; }
        public Point Location { get; set; }

        public Pixel(RGB color, Point location)
        {
            Color = color;
            Location = location;
        }
    }
}