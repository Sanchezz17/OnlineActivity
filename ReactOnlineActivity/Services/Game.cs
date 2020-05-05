using System;
using System.Collections.Generic;
using System.Linq;
using Timer = System.Timers.Timer;

namespace ReactOnlineActivity.Services
{
    public class Game
    {
        private const int CanvasWidth = 100;
        private const int CanvasHeight = 100;
        private const int MaxRoundTime = 5 * 60 * 1000;
        
        private readonly string[] _hiddenWords;
        private readonly Random _random;
        private readonly int _maxRound;

        public int RoundNumber { get; private set; }
        public bool GameIsOver { get; private set; }
        public Player ExplainingPlayer { get; private set; }
        public Canvas Canvas { get; }
        public Player[] Players { get; }
        public Timer Timer { get; }
        
        public Game(string[] hiddenWords, IEnumerable<Player> players)
        {
            _hiddenWords = hiddenWords ?? throw new ArgumentException("Hidden word is null");
            _maxRound = _hiddenWords.Length;
            _random = new Random();
            
            Canvas = new Canvas(CanvasWidth, CanvasHeight);
            Players = players.ToArray();
            Timer = new Timer(MaxRoundTime);
            Timer.Elapsed += (_, __) =>
            {
                GameIsOver = true;
                Timer.Close();
            };
        }

        public bool CheckWord(string wordFromPlayer)
        {
            if (GameIsOver)
                return false;
            return wordFromPlayer.Trim() == _hiddenWords[RoundNumber];
        }

        public void Paint(IEnumerable<Pixel> pixels)
        {
            if (!GameIsOver)
                Canvas.PaintOverPixels(pixels);  
        } 

        public void CompleteRound(Player guessingPlayer)
        {
            // TODO: увеличивать счет у игроков - после добавления сущности лидерборда
            if (RoundNumber >= _maxRound)
                GameIsOver = true;
            if (!GameIsOver)
                StartNewRound();
        }

        private void StartNewRound()
        {
            RoundNumber++;
            ExplainingPlayer = Players[_random.Next(Players.Length - 1)];
        }
    }
}