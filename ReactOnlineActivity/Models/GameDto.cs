using System;
using System.Collections.Generic;

namespace ReactOnlineActivity.Models
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public List<PlayerDto> Players { get; set; }
        public bool GameIsOver { get; set; }
        public int RoundNumber { get; set; }
        public PlayerDto ExplainingPlayer { get; set; }
        public CanvasDto Canvas { get; set; } // TODO: написать реализацию CanvasDto
        public DateTime TimeStartGame { get; set; } // TODO: проверить как обработается DateTime
    }
}