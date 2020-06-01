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
        private static readonly Dictionary<string, Func<UserStatistics, int>> getStatisticsByName 
            = new Dictionary<string, Func<UserStatistics, int>>
            {
                ["totalScore"] = statistics => statistics.TotalScore,
                ["numberOfGamesPlayed"] = statistics => statistics.NumberOfGamesPlayed,
                ["numberOfDraws"] = statistics => statistics.NumberOfDraws,
                ["winsCount"] = statistics => statistics.WinsCount
            };

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

        public List<ApplicationUser> SelectTopByStatistics(string statisticSelector, int limit)
        {
            var query = dbContext.Users
                .Include(u => u.Statistics)
                .OrderByDescending(u => u);
            try
            {
                return SortTop(query, statisticSelector)
                    .Take(limit)
                    .ToList();
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        private static IOrderedQueryable<ApplicationUser> SortTop(IQueryable<ApplicationUser> query, string statisticSelector)
        {
            return statisticSelector switch
            {
                "totalScore" => query.OrderByDescending(u => u.Statistics.TotalScore),
                "numberOfGamesPlayed" => query.OrderByDescending(u => u.Statistics.NumberOfGamesPlayed),
                "numberOfDraws" => query.OrderByDescending(u => u.Statistics.NumberOfDraws),
                "winsCount" => query.OrderByDescending(u => u.Statistics.WinsCount),
                _ => throw new NotImplementedException()
            };
        }
    }
}