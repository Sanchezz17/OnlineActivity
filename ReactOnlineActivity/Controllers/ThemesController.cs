using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [Route("api/themes")]
    public class ThemesController : Controller
    {
        private readonly ThemeRepository themeRepository;

        public ThemesController(ThemeRepository themeRepository)
        {
            this.themeRepository = themeRepository;
        }
        
        [HttpGet("")]
        public List<Theme> GetThemes()
        {
            return themeRepository.GetAllThemes().ToList();
        }

        [HttpPost("")]
        public void AddTheme([FromBody] Theme theme)
        {
            themeRepository.Insert(theme);
        }
    }
}