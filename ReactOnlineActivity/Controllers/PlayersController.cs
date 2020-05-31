using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api/rooms/{roomId}/players")]
    public class PlayersController : Controller
    {
        private readonly RoomRepository roomRepository;
        private readonly UserRepository userRepository;
        private readonly PlayerRepository playerRepository;

        public PlayersController(
            PlayerRepository playerRepository,
            RoomRepository roomRepository, 
            UserRepository userRepository)
        {
            this.playerRepository = playerRepository;
            this.roomRepository = roomRepository;
            this.userRepository = userRepository;
        }

        [HttpGet("")]
        public IEnumerable<Player> GetPlayers([FromRoute] int roomId)
        {
            return playerRepository.SelectAllFromRoom(roomId);
        }
        
        [HttpPost("")]
        public JoinRoomDto JoinRoom([FromRoute] int roomId, [FromQuery] string userName)
        {
            var room = roomRepository.FindById(roomId);

            var user = userRepository.FindByName(userName);

            var player = room
                .Game
                .Players
                .FirstOrDefault(p => p.Name == user.UserName);

            var joinRoomDto = new JoinRoomDto();

            if (player == null)
            {
                player = new Player
                {
                    Name = user.UserName,
                    PhotoUrl = user.PhotoUrl,
                    Score = 0
                };
                playerRepository.InsertPlayerIntoRoom(roomId, player);
            }
            else
            {
                joinRoomDto.AlreadyInRoom = true;
            }

            joinRoomDto.Player = player;

            return joinRoomDto;
        }
    }
}