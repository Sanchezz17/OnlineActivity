﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Game.Domain;
using IdentityModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api")]
    public class GamesController : Controller
    {
        private ApplicationDbContext dbContext;

        public GamesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        [HttpGet("play")]
        public RedirectResult Play([FromQuery] string userName)
        {
            var decodedUserName = HttpUtility.UrlDecode(userName);
            
            var user = dbContext.Users.First(u => u.UserName == decodedUserName);

            var suitableRoom = dbContext.Rooms
                .Include(r => r.Game)
                .Include(r => r.Game.Players)
                .Include(r => r.Settings)
                .FirstOrDefault(room => room.Game.Players.Count < room.Settings.MaxPlayerCount);

            if (suitableRoom == null)
            {
                suitableRoom = new Room
                {
                    Game = new GameDto
                    {
                        HiddenWords = new List<Word> { new Word {Value = "kek"}, new Word {Value ="lol"} },
                        Players = new List<PlayerDto>(),
                        TimeStartGame = DateTime.Now.ToEpochTime()
                    },
                    Settings = new RoomSettings()
                };

                dbContext.Rooms.Add(suitableRoom);
            }

            var player = suitableRoom.Game.Players.FirstOrDefault(p => p.Name == decodedUserName);

            if (player == null)
            {
                player = new PlayerDto
                {
                    Name = user.UserName,
                    Score = 0
                };
                
                suitableRoom.Game.Players.Add(player);
            }


            dbContext.SaveChanges();

            return Redirect($"/rooms/{suitableRoom.Id}");
        } 
    }
}