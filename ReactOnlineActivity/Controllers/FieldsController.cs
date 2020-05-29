using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api/fields")]
    public class FieldsController : Controller
    {
        private readonly RoomRepository roomRepository;

        public FieldsController(RoomRepository roomRepository)
        {
            this.roomRepository = roomRepository;
        }


        [HttpGet("{roomId}")]
        public LineDto[] GetField([FromRoute] int roomId)
        {
            var room = roomRepository.FindById(roomId, true);
            var canvas = room.Game.Canvas;
            return canvas
                .Select(line => new LineDto
                {
                    Coordinates = line.Value
                        .OrderBy(c => c.SerialNumber)
                        .Select(c => c.Value)
                        .ToArray(),
                    Color = line.Color
                })
                .ToArray();
        }
    }
}