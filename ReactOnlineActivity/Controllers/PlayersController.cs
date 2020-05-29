using System.Collections.Generic;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api/players")]
    public class PlayersController : Controller
    {
        private readonly PlayerRepository playerRepository;

        public PlayersController(PlayerRepository playerRepository)
        {
            this.playerRepository = playerRepository;
        }

        [HttpGet("")]
        public IEnumerable<Player> GetPlayers([FromQuery] int roomId)
        {
            return playerRepository.SelectAllFromRoom(roomId);
        }
    }
}