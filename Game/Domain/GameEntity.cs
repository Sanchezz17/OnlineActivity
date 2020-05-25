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

        public string[] HiddenWords { get; set; }
        public int RoundNumber { get; private set; }
        public GameState GameState { get; set; }
        public string ExplainingPlayerName { get; set; }
        public List<Line> Canvas { get; set; }
        public List<Player> Players { get; }
        public long CurrentRoundStartTime { get; private set; }
        // public TimeSpan TimeInGame => DateTime.Now - TimeStartGame;
        public int Id { get; }

        public GameEntity(int id, string[] hiddenWords, IEnumerable<Player> players)
        {
            HiddenWords = hiddenWords ?? throw new ArgumentException("Hidden word is null");
            _maxRound = HiddenWords.Length;
            _random = new Random();
            
            Canvas = new List<Line>();
            Players = players.ToList();
            Id = id;
        }
        
        public GameEntity(string[] hiddenWords, IEnumerable<Player> players) : this(0, hiddenWords, players) {}

        public void Start()
        {
            RoundNumber = -1;
            GameState = GameState.Started;
            StartNewRound();
        }

        public string GetCurrentHiddenWord()
        {
            return HiddenWords[RoundNumber % HiddenWords.Length];
        }

        private void CheckTime()
        {
            //if (TimeInGame > TimeSpan.FromMinutes(MaxRoundTimeInMinutes))
               // GameIsOver = true;
        }

        public bool CheckWord(string wordFromPlayer)
        {
            CheckTime();
            if (GameState == GameState.Finished)
                return false;
            return wordFromPlayer.Trim() == HiddenWords[RoundNumber];
        }

        public void AddLine(Line line)
        {
            Canvas.Add(line);
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
            // Players.First(p => p.Name == ExplainingPlayerName).Score += MaxRoundTimeInMinutes * SecondsInMinutes - TimeInGame.Seconds;
            
            if (RoundNumber >= _maxRound)
                GameState = GameState.Finished;
            if (GameState == GameState.Started)
                StartNewRound();
        }

        public void StartNewRound()
        {
            CurrentRoundStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            RoundNumber++;
            ExplainingPlayerName = Players[_random.Next(Players.Count - 1)].Name;
        }
        
    }
}