using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Repositories
{
    public class RoomRepository
    {
        private readonly ApplicationDbContext dbContext;

        public RoomRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Room FindSuitable()
        {
            return dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .Include(r => r.Settings)
                .FirstOrDefault(room =>
                    !room.Settings.IsPrivateRoom && room.Game.Players.Count < room.Settings.MaxPlayerCount);
        }

        public Room FindById(int roomId)
        {
            return dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .SingleOrDefault(r => r.Id == roomId);
        }

        public void Insert(Room room)
        {
            dbContext.Rooms.Add(room);
            dbContext.SaveChanges();
        }

        public void InsertPlayerIntoRoom(int roomId, PlayerDto player)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception(); //todo: более осмысленное исключение сделать
            room.Game.Players.Add(player);
            dbContext.SaveChanges();
        }
    }
}