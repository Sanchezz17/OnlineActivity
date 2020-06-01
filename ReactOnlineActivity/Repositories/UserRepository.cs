 
using System;
using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
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
            return UsersIncludeMainProperties()
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

        public List<ApplicationUser> SelectTopByStatistics(string desiredStatistics, int limit)
        {
            try
            {
                return SortTop(UsersIncludeMainProperties(), desiredStatistics)
                    .Take(limit)
                    .ToList();
            }
            catch (NotImplementedException)
            {
                return null;
            }
        }

        public int GetUserPositionInTop(string desiredStatistics, string userName)
        {
            var users = SortTop(UsersIncludeMainProperties(), desiredStatistics).ToArray();
            for (var i = 0; i < users.Length; i++)
            {
                if (users[i].UserName == userName)
                    return i;
            }

            return -1;
        }

        private static IOrderedQueryable<ApplicationUser> SortTop(IQueryable<ApplicationUser> query, string desiredStatistics)
        {
            return desiredStatistics switch
            {
                "totalScore" => query.OrderByDescending(u => u.Statistics.TotalScore),
                "numberOfGamesPlayed" => query.OrderByDescending(u => u.Statistics.NumberOfGamesPlayed),
                "numberOfDraws" => query.OrderByDescending(u => u.Statistics.NumberOfDraws),
                "winsCount" => query.OrderByDescending(u => u.Statistics.WinsCount),
                _ => throw new NotImplementedException()
            };
        }

        public IIncludableQueryable<ApplicationUser, UserStatistics> UsersIncludeMainProperties()
        {
            return dbContext.Users
                .Include(u => u.Statistics);
        }
    }
}