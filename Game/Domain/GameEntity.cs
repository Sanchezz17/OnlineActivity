using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Domain
{
    public class GameEntity
    {
        private const int SecondsInMinutes = 60;
        
        public int MaxRoundTimeInSeconds { get; set; }
        public int MaxPlayerCount { get; set; }
        public int PointsToWin { get; set; }
        public Word[] HiddenWords { get; set; }
        public int RoundNumber { get; private set; }
        public GameState GameState { get; set; }
        public string ExplainingPlayerName { get; set; }
        public List<Line> Canvas { get; set; }
        public List<Player> Players { get; }
        public long CurrentRoundStartTime { get; private set; }

        public List<Player> GuessingPlayers { get; set; }

        public GameEntity(Word[] hiddenWords, IEnumerable<Player> players)
        {
            HiddenWords = hiddenWords ?? throw new ArgumentException("Hidden word is null");
            Players = players.ToList();
            GuessingPlayers = new List<Player>();
            Canvas = new List<Line>();
        }

        public void Start()
        {
            RoundNumber = -1;
            GameState = GameState.Started;
            foreach (var player in Players)
                player.Score = 0;
            StartNewRound();
        }

        public void StartNewRound()
        {
            CurrentRoundStartTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            GuessingPlayers = new List<Player>();
            Canvas = new List<Line>();
            RoundNumber++;
            ExplainingPlayerName = Players[RoundNumber % Players.Count].Name;
        }

        public string GetCurrentHiddenWord()
        {
            return HiddenWords.First(w => w.SerialNumber == RoundNumber).Value;
        }

        public int GetSecondsLeft()
        {
            var secondsPassed = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CurrentRoundStartTime);
            return MaxRoundTimeInSeconds - secondsPassed;
        }

        public bool MakeStep(Player player, string word, int maxPlayerCount)
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
                player.Score += maxPlayerCount - GuessingPlayers.Count;
                var explainingPlayer = Players.First(p => p.Name == ExplainingPlayerName);
                explainingPlayer.Score += (MaxRoundTimeInSeconds - secondsPassed) / 20
                                          + maxPlayerCount / Players.Count;
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
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - CurrentRoundStartTime > MaxRoundTimeInSeconds)
                GameState = GameState.Finished;
        }


        private bool CheckWord(string wordFromPlayer)
        {
            return GameState != GameState.Finished 
                   && string.Equals(GetCurrentHiddenWord().ToLower(), wordFromPlayer.Trim().ToLower(), 
                       StringComparison.CurrentCultureIgnoreCase);
        }

        private void CompleteRound()
        {
            if (Players.Max(p => p.Score) >= PointsToWin)
                GameState = GameState.Finished;
            if (GameState == GameState.Started)
                StartNewRound();
        }
    }
}