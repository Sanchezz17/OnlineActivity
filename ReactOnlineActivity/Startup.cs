using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PhotosApp.Services;
using ReactOnlineActivity.Services;
using ReactOnlineActivity.Services.Constants;

namespace ReactOnlineActivity
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddDefaultIdentity<ApplicationUser>(options =>
                {
                    options.User.AllowedUserNameCharacters += " абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
                    options.SignIn.RequireConfirmedAccount = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddPasswordValidator<UsernameAsPasswordValidator<ApplicationUser>>()
                .AddErrorDescriber<RussianIdentityErrorDescriber>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>();

            services.AddAuthentication().AddGoogle("Google", options =>
            {
                options.ClientId = Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });

            services.AddAuthentication().AddVkontakte(options =>
            {
                options.ClientId = Configuration["Authentication:Vk:ClientId"];
                options.ClientSecret = Configuration["Authentication:Vk:ClientSecret"];
                
                options.Scope.Add(VkScopes.Email);
                options.Scope.Add(VkScopes.Photos);

                // Add fields https://vk.com/dev/objects/user
                options.Fields.Add(VkUserFields.Id);
                options.Fields.Add(VkUserFields.FirstName);
                options.Fields.Add(VkUserFields.LastName);
                options.Fields.Add(VkUserFields.Photo200);

                // In this case email will return in OAuthTokenResponse, 
                // but all scope values will be merged with user response
                // so we can claim it as field
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, VkUserFields.Id);
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, VkUserFields.Email);
                options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, VkUserFields.FirstName);
                options.ClaimActions.MapJsonKey(ClaimTypes.Surname, VkUserFields.LastName);
                options.ClaimActions.MapJsonKey(CustomClaimTypes.PhotoUrl, VkUserFields.Photo200);
            });

            services.AddAuthentication()
                .AddIdentityServerJwt();
            

            services.AddControllersWithViews();
            services.AddRazorPages();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/build"; });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}