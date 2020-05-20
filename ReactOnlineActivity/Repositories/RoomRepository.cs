using System;
using System.Collections.Generic;
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
            var rooms = dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .Include(r => r.Settings)
                .Include(r => r.Game.Canvas)
                    .ThenInclude(l => l.Value)
                .Include(r => r.Settings)
                .Include(r => r.Settings.Themes)
                    .ThenInclude(t => t.Words);
                
            return rooms.FirstOrDefault(room =>
                    !room.Settings.IsPrivateRoom && room.Game.Players.Count < room.Settings.MaxPlayerCount);
        }

        public Room FindById(int roomId)
        {
            return dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .Include(r => r.Game.Canvas)
                    .ThenInclude(l => l.Value)
                .Include(r => r.Settings)
                .Include(r => r.Settings.Themes)
                .ThenInclude(t => t.Words)
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

        public void AddLineToFieldIntoRoom(int roomId, LineDto newLine)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception(); //todo: более осмысленное исключение сделать
            room.Game.Canvas.Add(newLine);
            dbContext.SaveChanges();
        }

        public void ClearField(int roomId)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception(); //todo: более осмысленное исключение сделать]
            room.Game.Canvas = new List<LineDto>();
            dbContext.SaveChanges();
        }

        public void UpdateGame(int roomId, GameDto game)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception(); //todo: более осмысленное исключение сделать]
            room.Game.GameState = game.GameState;
            room.Game.RoundNumber = game.RoundNumber;
            room.Game.ExplainingPlayer = game.ExplainingPlayer;
            room.Game.CurrentRoundStartTime = game.CurrentRoundStartTime;
           
            dbContext.SaveChanges();
        }
    }
}