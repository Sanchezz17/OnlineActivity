using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public class GameEntity
    {
        private const int MaxRoundTimeInMinutes = 5;
        private const int SecondsInMinutes = 60;
        private readonly Random _random;
        private readonly int _maxRound;

        public string[] HiddenWords { get; set; }
        public int RoundNumber { get; private set; }
        public GameState GameState { get; set; }
        public string ExplainingPlayerName { get; set; }
        public List<Line> Canvas { get; set; }
        public List<Player> Players { get; }
        public long CurrentRoundStartTime { get; private set; }

        public List<Player> GuessingPlayers { get; set; }

        public GameEntity(string[] hiddenWords, IEnumerable<Player> players)
        {
            HiddenWords = hiddenWords ?? throw new ArgumentException("Hidden word is null");
            _maxRound = HiddenWords.Length;
            _random = new Random();
            
            GuessingPlayers = new List<Player>();
            Canvas = new List<Line>();
            Players = players.ToList();
        }

        public void Start()
        {
            RoundNumber = -1;
            GameState = GameState.Started;
            StartNewRound();
        }

        public void StartNewRound()
        {
            CurrentRoundStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            GuessingPlayers = new List<Player>();
            Canvas = new List<Line>();
            RoundNumber++;
            ExplainingPlayerName = Players[_random.Next(Players.Count)].Name;
        }

        public string GetCurrentHiddenWord()
        {
            return HiddenWords[RoundNumber % HiddenWords.Length];
        }

        public bool MakeStep(Player player, string word)
        {
            CheckTime();
            if (GameState == GameState.Finished)
                return false;
            
            if (GuessingPlayers.Contains(player))
                return true;

            var playerGuessed = CheckWord(word);
            if (playerGuessed)
            {
                var secondsPassed = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CurrentRoundStartTime);
                player.Score += Math.Max(0, 60 - secondsPassed) + (Players.Count - GuessingPlayers.Count) * 10;
                var explainingPlayer = Players.First(p => p.Name == ExplainingPlayerName);
                explainingPlayer.Score += MaxRoundTimeInMinutes * SecondsInMinutes - secondsPassed;
                GuessingPlayers.Add(player);
            }

            if (GuessingPlayers.Count == Players.Count - 1)
            {
                CompleteRound();
            }

            return playerGuessed;
        }

        private void CheckTime()
        {
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CurrentRoundStartTime >
                MaxRoundTimeInMinutes * SecondsInMinutes)
                GameState = GameState.Finished;
        }


        private bool CheckWord(string wordFromPlayer)
        {
            return GameState != GameState.Finished 
                   && string.Equals(GetCurrentHiddenWord(), wordFromPlayer.Trim(), 
                       StringComparison.CurrentCultureIgnoreCase);
        }

        private void CompleteRound()
        {
            if (RoundNumber >= _maxRound)
                GameState = GameState.Finished;
            if (GameState == GameState.Started)
                StartNewRound();
        }
    }
}