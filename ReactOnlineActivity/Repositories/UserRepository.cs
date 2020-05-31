using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Repositories
{
    public class UserRepository
    {
        private readonly ApplicationDbContext dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public ApplicationUser FindByName(string userName)
        {
            return dbContext.Users
                .Include(u => u.Statistics)
                .First(u => u.UserName == userName);
        }

        public void UpdateStatistics(Player player)
        {
            var user = FindByName(player.Name);
            user.Statistics.TotalScore += player.Score;
            user.Statistics.NumberOfGamesPlayed++;
            user.Statistics.NumberOfDraws += player.NumberOfDraws;
            dbContext.SaveChanges();
        }

        public void IncreaseWinsCount(Player player)
        {
            var userWinner = FindByName(player.Name);
            userWinner.Statistics.WinsCount++;
            dbContext.SaveChanges();
        }

        public List<ApplicationUser> SelectTopByStatistics(Func<UserStatistics, int> statisticSelector, int limit)
        {
            return dbContext.Users
                .Include(u => u.Statistics)
                .OrderByDescending(u => statisticSelector(u.Statistics))
                .Take(limit)
                .ToList();
        }
    }
}