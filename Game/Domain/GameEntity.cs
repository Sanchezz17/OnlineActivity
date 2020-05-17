using System;
using System.Collections.Generic;
using System.Linq;
using Timer = System.Timers.Timer;

namespace Game.Domain
{
    public class GameEntity
    {
        private const int MaxRoundTimeInMinutes = 5;
        private const int SecondsInMinutes = 60;
        private const int PointsForCorrectAnswer = 50;
        private readonly Random _random;
        private readonly int _maxRound;

        public string[] HiddenWords { get; }
        public int RoundNumber { get; private set; }
        public bool GameIsOver { get; private set; }
        public Player ExplainingPlayer { get; private set; }
        public List<Line> Canvas { get; }
        public List<Player> Players { get; }
        public DateTime TimeStartGame { get; }
        public TimeSpan TimeInGame => DateTime.Now - TimeStartGame;
        public Guid Id { get; }

        public GameEntity(Guid id, string[] hiddenWords, IEnumerable<Player> players)
        {
            HiddenWords = hiddenWords ?? throw new ArgumentException("Hidden word is null");
            _maxRound = HiddenWords.Length;
            _random = new Random();
            
            Canvas = new List<Line>();
            Players = players.ToList();
            TimeStartGame = DateTime.Now;
            Id = id;
        }
        
        public GameEntity(string[] hiddenWords, IEnumerable<Player> players) : this(Guid.Empty, hiddenWords, players) {}

        private void CheckTime()
        {
            if (TimeInGame > TimeSpan.FromMinutes(MaxRoundTimeInMinutes))
                GameIsOver = true;
        }

        public bool CheckWord(string wordFromPlayer)
        {
            CheckTime();
            if (GameIsOver)
                return false;
            return wordFromPlayer.Trim() == HiddenWords[RoundNumber];
        }

        public void Paint(IEnumerable<Pixel> pixels)
        {
            CheckTime();
            //if (!GameIsOver)
              //  Canvas.PaintOverPixels(pixels);  
        } 

        public void CompleteRound(Player guessingPlayer)
        {
            guessingPlayer.Score += PointsForCorrectAnswer;
            ExplainingPlayer.Score += MaxRoundTimeInMinutes * SecondsInMinutes - TimeInGame.Seconds;
            
            if (RoundNumber >= _maxRound)
                GameIsOver = true;
            if (!GameIsOver)
                StartNewRound();
        }

        private void StartNewRound()
        {
            RoundNumber++;
            ExplainingPlayer = Players[_random.Next(Players.Count - 1)];
        }
    }
}