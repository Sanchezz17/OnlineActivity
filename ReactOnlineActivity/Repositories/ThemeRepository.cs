using System.Collections.Generic;
using System.Linq;
using Game.Domain;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;

namespace ReactOnlineActivity.Repositories
{
    public class ThemeRepository
    {
        private readonly ApplicationDbContext dbContext;

        public ThemeRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public Theme FindById(int id)
        {
            return dbContext.Themes.SingleOrDefault(theme => theme.Id == id);
        }

        public List<Theme> GetAllThemes()
        {
            return dbContext.Themes
                .Include(t => t.Words)
                .ToList();
        }

        public void Insert(Theme theme)
        {
            dbContext.Themes.Add(theme);
        }
    }
}