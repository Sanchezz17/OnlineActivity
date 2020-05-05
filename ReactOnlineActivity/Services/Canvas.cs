using System;
using System.Collections.Generic;
using System.Drawing;

namespace ReactOnlineActivity.Services
{
    public class Canvas
    {
        public Size Size { get; }
        public RGB[,] Pixels { get; }
        
        public Canvas(Size size)
        {
            if (size.Width <= 0 || size.Height <= 0)
                throw new ArgumentException("Height and width must be positive");
            Size = size;
            Pixels = new RGB[size.Width, size.Height];
            for (var x = 0; x < size.Width; x++)
                for (var y = 0; y < size.Height; y++)
                    Pixels[x, y] = new RGB();
        }

        public Canvas(int width, int height) : this(new Size(width, height)) {}

        public void PaintOverPixels(IEnumerable<Pixel> newPixels)
        {
            foreach (var newPixel in newPixels)
                Pixels[newPixel.Location.X, newPixel.Location.Y] = newPixel.Color;
        }
        
        
    }
}