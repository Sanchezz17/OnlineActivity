using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly UserRepository userRepository;
        private static readonly Dictionary<string, Func<UserStatistics, int>> getStatisticsByName 
            = new Dictionary<string, Func<UserStatistics, int>>
            {
                ["totalScore"] = statistics => statistics.TotalScore,
                ["numberOfGamesPlayed"] = statistics => statistics.NumberOfGamesPlayed,
                ["numberOfDraws"] = statistics => statistics.NumberOfDraws,
                ["winsCount"] = statistics => statistics.WinsCount
            };

        public UsersController(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [Route("")]
        public List<ApplicationUser> GetTopBy([FromQuery] string desiredStatistics, [FromQuery] int limit)
        {
            return getStatisticsByName.ContainsKey(desiredStatistics) 
                ? userRepository.SelectTopByStatistics(getStatisticsByName[desiredStatistics], limit) 
                : null;
        }
    }
}