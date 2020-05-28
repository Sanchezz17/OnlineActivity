using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using ReactOnlineActivity.Data;

namespace ReactOnlineActivity.Repositories
{
    public class PlayerRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly RoomRepository roomRepository;

        public PlayerRepository(ApplicationDbContext dbContext,
            RoomRepository roomRepository)
        {
            this.dbContext = dbContext;
            this.roomRepository = roomRepository;
        }

        public IEnumerable<Player> SelectAllFromRoom(int roomId)
        {
            return roomRepository.FindById(roomId)
                ?.Game
                .Players;
        }

        public void DeletePlayerFromRoom(int roomId, string playerName)
        {
            var room = roomRepository.FindById(roomId);
            var player = room?
                .Game
                .Players
                .SingleOrDefault(p => p.Name == playerName);
            if (player == null)
                throw new Exception();
            room.Game.Players.Remove(player);
            dbContext.SaveChanges();
        }
    }
}