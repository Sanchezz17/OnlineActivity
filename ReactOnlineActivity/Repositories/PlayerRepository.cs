﻿using System;
using System.Collections.Generic;
using System.Linq;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;

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

        public IEnumerable<PlayerDto> SelectAllFromRoom(int roomId)
        {
            return roomRepository.Find(roomId)
                ?.Game
                .Players;
        }

        public void DeletePlayerFromRoom(int roomId, string playerName)
        {
            var room = roomRepository.Find(roomId);
            var player = room?
                .Game
                .Players
                .SingleOrDefault(p => p.Name == playerName);
            if (player == null) 
                throw new Exception(); // todo: 
            room.Game.Players.Remove(player);
            dbContext.SaveChanges();
        }
    }
}