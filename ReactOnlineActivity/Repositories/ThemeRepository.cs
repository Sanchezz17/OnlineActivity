using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;

namespace ReactOnlineActivity.Repositories
{
    public class ThemeRepository
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HashSet<int> defaultThemesIds = new HashSet<int> {0};

        public ThemeRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Theme FindById(int id)
        {
            return dbContext.Themes
                .Include(t => t.Words)
                .SingleOrDefault(theme => theme.Id == id);
        }

        public List<Theme> GetAllThemes()
        {
            return dbContext.Themes
                .Include(t => t.Words)
                .ToList();
        }

        public List<Theme> GetDefaultThemes()
        {
            return dbContext.Themes
                .Include(t => t.Words)
                .Where(t => defaultThemesIds.Contains(t.Id))
                .ToList();
        }

        public void Insert(Theme theme)
        {
            dbContext.Themes.Add(theme);
            dbContext.SaveChanges();
        }
    }
}