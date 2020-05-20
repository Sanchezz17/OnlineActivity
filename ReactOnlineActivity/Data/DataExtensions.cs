using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Game.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ReactOnlineActivity.Data
{
    public static class DataExtensions
    {
        private const string ThemesFileName = "Themes.json";

        public static void PrepareDB(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                try
                {
                    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        var appDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        appDbContext.SeedWithSampleThemesAsync().Wait();
                    }
                }
                catch (Exception e)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(e, "An error occurred while migrating or seeding the database.");
                }
            }
        }

        private static async Task SeedWithSampleThemesAsync(this ApplicationDbContext dbContext)
        {
            dbContext.Themes.RemoveRange(dbContext.Themes);
            await dbContext.SaveChangesAsync();
            var themes = GetThemesFromJson(ThemesFileName);
            dbContext.Themes.AddRange(themes);
            await dbContext.SaveChangesAsync();
        }

        private static List<Theme> GetThemesFromJson(string fileName)
        {
            try
            {
                var json = File.ReadAllText(fileName);
                return JsonConvert.DeserializeObject<List<Theme>>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}