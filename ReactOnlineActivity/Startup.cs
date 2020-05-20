using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using AutoMapper;
using Game.Domain;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PhotosApp.Services;
using ReactOnlineActivity.Hubs;
using ReactOnlineActivity.Repositories;
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
                    options.User.AllowedUserNameCharacters +=
                        " абвгдеёжзийклмнопрстуфхцчшщъыьэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ";
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddPasswordValidator<UsernameAsPasswordValidator<ApplicationUser>>()
                .AddErrorDescriber<RussianIdentityErrorDescriber>();

            services.AddIdentityServer()
                .AddApiAuthorization<ApplicationUser, ApplicationDbContext>()
                .AddProfileService<ProfileService>();

            DotNetEnv.Env.Load();

            services.AddAuthentication().AddGoogle("Google", options =>
            {
                options.ClientId = System.Environment.GetEnvironmentVariable("AUTHENTICATION_GOOGLE_CLIENT_ID");
                options.ClientSecret = System.Environment.GetEnvironmentVariable("AUTHENTICATION_GOOGLE_CLIENT_SECRET");

                options.Scope.Add("profile");

                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "name");
                options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "family_name");
                options.ClaimActions.MapJsonKey(CustomClaimTypes.PhotoUrl, "picture");
            });

            services.AddAuthentication().AddVkontakte(options =>
            {
                options.ClientId = System.Environment.GetEnvironmentVariable("AUTHENTICATION_VK_CLIENT_ID");
                options.ClientSecret = System.Environment.GetEnvironmentVariable("AUTHENTICATION_VK_CLIENT_SECRET");

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

            services.AddTransient<IEmailSender, SimpleEmailSender>(serviceProvider =>
                new SimpleEmailSender(
                    serviceProvider.GetRequiredService<ILogger<SimpleEmailSender>>(),
                    serviceProvider.GetRequiredService<IWebHostEnvironment>(),
                    Configuration["SimpleEmailSender:Host"],
                    Configuration.GetValue<int>("SimpleEmailSender:Port"),
                    Configuration.GetValue<bool>("SimpleEmailSender:EnableSSL"),
                    System.Environment.GetEnvironmentVariable("SIMPLE_EMAIL_SENDER_USER_NAME"),
                    System.Environment.GetEnvironmentVariable("SIMPLE_EMAIL_SENDER_PASSWORD")
                ));

            services.AddSignalR();

            services.AddScoped<RoomRepository>();
            services.AddScoped<PlayerRepository>();
            services.AddScoped<UserRepository>();
            services.AddScoped<ThemeRepository>();

            var sp = services.BuildServiceProvider();
            var themeRepository = sp.GetService<ThemeRepository>();

            services.AddAutoMapper(cfg =>
                {
                    cfg.CreateMap<GameDto, GameEntity>();
                    cfg.CreateMap<GameEntity, GameDto>();
                    cfg.CreateMap<Line, LineDto>();
                    cfg.CreateMap<LineDto, Line>();
                    cfg.CreateMap<PlayerDto, Player>();
                    cfg.CreateMap<Player, PlayerDto>();
                    cfg.CreateMap<Coordinate, CoordinateDto>();
                    cfg.CreateMap<RoomSettingsDto, RoomSettings>().ForMember(dest => dest.Themes,
                        opt =>
                            opt.MapFrom(src => src.ThemesIds.Select(
                                id => themeRepository.FindById(id))));
                }
                , new System.Reflection.Assembly[0]);
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

            // app.UseHttpsRedirection();
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
                endpoints.MapHub<RoomHub>("/room");
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