using System;
using System.Collections.Generic;

namespace ReactOnlineActivity.Services
{
    public class Game
    {
        private readonly string _hiddenWord;
        public Canvas _canvas { get; }
        
        public Game(string hiddenWord)
        {
            _hiddenWord = hiddenWord ?? throw new ArgumentException("Hidden word is null");
        }

        public bool CheckWord(string wordFromPlayer) => wordFromPlayer.Trim() == _hiddenWord;

        public void Paint(IEnumerable<Pixel> pixels) => _canvas.PaintOverPixels(pixels);
    }
}