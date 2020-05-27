using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
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

        public Room FindSuitable() => GetQuery().FirstOrDefault(room =>
                    !room.Settings.IsPrivateRoom && room.Game.Players.Count < room.Settings.MaxPlayerCount);

        public Room FindById(int roomId) => GetQuery().SingleOrDefault(r => r.Id == roomId);

        public void Insert(Room room)
        {
            dbContext.Rooms.Add(room);
            dbContext.SaveChanges();
        }

        public void InsertPlayerIntoRoom(int roomId, Player player)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception();
            room.Game.Players.Add(player);
            dbContext.SaveChanges();
        }

        public void AddLineToFieldIntoRoom(int roomId, Line newLine)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception();
            room.Game.Canvas.Add(newLine);
            dbContext.SaveChanges();
        }

        public void ClearField(int roomId)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception();
            room.Game.Canvas = new List<Line>();
            dbContext.SaveChanges();
        }

        public void UpdateGame(int roomId, GameDto game)
        {
            var room = FindById(roomId);
            if (room == null)
                throw new Exception();
            room.Game.GameState = game.GameState;
            room.Game.RoundNumber = game.RoundNumber;
            room.Game.Canvas = game.Canvas;
            room.Game.ExplainingPlayerName = game.ExplainingPlayerName;
            room.Game.CurrentRoundStartTime = game.CurrentRoundStartTime;
            foreach (var player in game.Players)
                room.Game.Players.Find(p => p.Id == player.Id).Score = player.Score;
            dbContext.SaveChanges();
        }

        private IIncludableQueryable<Room, List<ThemeRoomSettings>> GetQuery()
        {
            return dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .Include(r => r.Settings)
                .Include(r => r.Game.HiddenWords)
                .Include(r => r.Game.Canvas)
                .ThenInclude(l => l.Value)
                .Include(r => r.Settings)
                .Include(r => r.Settings.ThemeRoomSettings);
        }
    }
}