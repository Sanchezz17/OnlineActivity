﻿using System.Collections.Generic;
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

        [Route("")]
        public List<ApplicationUser> GetTopBy([FromQuery] string desiredStatistics, [FromQuery] int limit)
        {
            return userRepository.SelectTopByStatistics(desiredStatistics, limit);
        }
        
        [Route("{userName}")]
        public ApplicationUser GetUserByName([FromRoute] string userName)
        {
            return userRepository.FindByName(userName);
        }
        
        [Route("{userName}/position")]
        public int GetUserPositionInTop([FromRoute] string userName, [FromQuery] string desiredStatistics)
        {
            return userRepository.GetUserPositionInTop(desiredStatistics, userName);
        }
    }
}