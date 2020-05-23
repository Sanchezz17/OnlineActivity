using System.Collections.Generic;
using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class GameDto
    {
        public int Id { get; set; }
        public List<PlayerDto> Players { get; set; }
        public List<Word> HiddenWords { get; set; }
        public GameState GameState { get; set; }
        public int RoundNumber { get; set; }
        public string ExplainingPlayerName { get; set; }
        public List<LineDto> Canvas { get; set; } // TODO: написать реализацию CanvasDto
        public long CurrentRoundStartTime { get; set; } // TODO: проверить как обработается DateTime
    }
}