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

        public UsersController(UserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <summary>
        /// Получение до limit первых игроков по указанному параметру desiredStatistics
        /// </summary>
        /// <param name="desiredStatistics"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("")]
        public List<ApplicationUser> GetTopBy([FromQuery] string desiredStatistics, [FromQuery] int limit)
        {
            return userRepository.SelectTopByStatistics(desiredStatistics, limit);
        }
        
        /// <summary>
        /// Получение пользователя по имени
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpGet("{userName}")]
        public ApplicationUser GetUserByName([FromRoute] string userName)
        {
            return userRepository.FindByName(userName);
        }
        
        /// <summary>
        /// Получение места пользователя по указанному параметру desiredStatistics
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="desiredStatistics"></param>
        /// <returns></returns>
        [HttpGet("{userName}/position")]
        public int GetUserPositionInTop([FromRoute] string userName, [FromQuery] string desiredStatistics)
        {
            return userRepository.GetUserPositionInTop(desiredStatistics, userName) + 1;
        }
    }
}