using System;
using System.Collections.Generic;

namespace ReactOnlineActivity.Services
{
    public class Game
    {
        private const int CanvasWidth = 100;
        private const int CanvasHeight = 100;
        private readonly string _hiddenWord;
        public Canvas Canvas { get; }
        
        public Game(string hiddenWord)
        {
            _hiddenWord = hiddenWord ?? throw new ArgumentException("Hidden word is null");
            Canvas = new Canvas(CanvasWidth, CanvasHeight);
        }

        public bool CheckWord(string wordFromPlayer) => wordFromPlayer.Trim() == _hiddenWord;

        public void Paint(IEnumerable<Pixel> pixels) => Canvas.PaintOverPixels(pixels);
    }
}