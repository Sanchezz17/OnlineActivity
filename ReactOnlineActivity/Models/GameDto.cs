using System.Collections.Generic;
using Game.Domain;

namespace ReactOnlineActivity.Models
{
    public class GameDto
    {
        public int Id { get; set; }
        public List<PlayerDto> Players { get; set; }
        public List<Word> HiddenWords { get; set; }
        public bool GameIsOver { get; set; }
        public int RoundNumber { get; set; }
        public PlayerDto ExplainingPlayer { get; set; }
        public List<LineDto> Canvas { get; set; } // TODO: написать реализацию CanvasDto
        public long TimeStartGame { get; set; } // TODO: проверить как обработается DateTime
    }
}