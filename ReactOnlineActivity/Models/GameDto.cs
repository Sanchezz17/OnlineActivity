using System.Collections.Generic;
using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class GameDto
    {
        public int Id { get; set; }
        public int MaxRoundTimeInSeconds { get; set; }
        public int MaxPlayerCount { get; set; }
        public int PointsToWin { get; set; }
        public List<Player> Players { get; set; }
        public List<Word> HiddenWords { get; set; }
        public GameState GameState { get; set; }
        public int RoundNumber { get; set; }
        public string ExplainingPlayerName { get; set; }
        public List<Line> Canvas { get; set; }
        public long CurrentRoundStartTime { get; set; }
        
        public List<Player> GuessingPlayers { get; set; }
    }
}