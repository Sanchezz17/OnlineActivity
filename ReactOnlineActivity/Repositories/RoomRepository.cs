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

        public Room FindSuitable()
        {
            return RoomsIncludeMainProperties().FirstOrDefault(room =>
                !room.Settings.IsPrivateRoom && room.Game.Players.Count < room.Settings.MaxPlayerCount);
        }

        public Room FindById(int roomId, bool withCoordinates = false)
        {
            return withCoordinates
                ? RoomsIncludeMainProperties().ThenInclude(l => l.Value).Single(r => r.Id == roomId)
                : RoomsIncludeMainProperties().Single(r => r.Id == roomId);
        }

        public void Insert(Room room)
        {
            dbContext.Rooms.Add(room);
            dbContext.SaveChanges();
        }

        public void AddLineToFieldIntoRoom(int roomId, Line newLine)
        {
            var room = FindById(roomId);
            room.Game.Canvas.Add(newLine);
            dbContext.SaveChanges();
        }

        public void ClearField(int roomId)
        {
            var room = FindById(roomId);
            room.Game.Canvas = new List<Line>();
            dbContext.SaveChanges();
        }

        public void UpdateGame(int roomId, GameDto game)
        {
            var room = FindById(roomId);
            room.Game.GameState = game.GameState;
            room.Game.RoundNumber = game.RoundNumber;
            room.Game.Canvas = game.Canvas;
            room.Game.ExplainingPlayerName = game.ExplainingPlayerName;
            room.Game.CurrentRoundStartTime = game.CurrentRoundStartTime;
            foreach (var player in game.Players)
                room.Game.Players.Find(p => p.Id == player.Id).Score = player.Score;
            dbContext.SaveChanges();
        }

        private IIncludableQueryable<Room, List<Line>> RoomsIncludeMainProperties()
        {
            return dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .Include(r => r.Settings)
                .Include(r => r.Game.HiddenWords)
                .Include(r => r.Settings)
                .Include(r => r.Settings.ThemeRoomSettings)
                .Include(r => r.Game.Canvas);
        }
    }
}