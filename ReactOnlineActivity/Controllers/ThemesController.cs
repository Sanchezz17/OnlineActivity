﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Repositories;

namespace ReactOnlineActivity.Controllers
{
    [Route("api/themes")]
    public class ThemesController : Controller
    {
        private readonly ThemeRepository themeRepository;
        private readonly IMapper mapper;

        public ThemesController(ThemeRepository themeRepository, IMapper mapper)
        {
            this.themeRepository = themeRepository;
            this.mapper = mapper;
        }

        /// <summary>
        /// Получение всех тем
        /// </summary>
        /// <returns></returns>
        [HttpGet("")]
        public List<Theme> GetThemes()
        {
            return themeRepository.GetAllThemes().ToList();
        }

        /// <summary>
        /// Добавление пользовательской темы
        /// </summary>
        /// <param name="themeDto"></param>
        /// <returns></returns>
        [HttpPost("")]
        public string AddTheme([FromBody] ThemeDto themeDto)
        {
            var theme = mapper.Map<Theme>(themeDto);
            themeRepository.Insert(theme);
            return theme.Id.ToString();
        }
    }
}