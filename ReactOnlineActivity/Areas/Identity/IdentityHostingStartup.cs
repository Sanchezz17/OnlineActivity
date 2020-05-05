using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotosApp.Services;
using ReactOnlineActivity.Areas.Identity.Data;
using ReactOnlineActivity.Data;
using ReactOnlineActivity.Models;
using ReactOnlineActivity.Services;

[assembly: HostingStartup(typeof(ReactOnlineActivity.Areas.Identity.IdentityHostingStartup))]

namespace ReactOnlineActivity.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                // services.AddDbContext<UsersDbContext>(options =>
                //     options.UseSqlite(
                //         context.Configuration.GetConnectionString("UsersDbContextConnection")));
                //
                // services.AddDefaultIdentity<ActivityAppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //     .AddEntityFrameworkStores<UsersDbContext>();

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlite(context.Configuration.GetConnectionString("DefaultConnection")));

                services.AddDefaultIdentity<ApplicationUser>(options =>
                    {
                        options.SignIn.RequireConfirmedAccount = false;
                    })
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddPasswordValidator<UsernameAsPasswordValidator<ApplicationUser>>()
                    .AddErrorDescriber<RussianIdentityErrorDescriber>();

                services.AddIdentityServer()
                    .AddApiAuthorization<ApplicationUser, ApplicationDbContext>(options => { });

                services.AddAuthentication().AddGoogle("Google", options =>
                {
                    options.ClientId = context.Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = context.Configuration["Authentication:Google:ClientSecret"];
                });

                services.AddAuthentication().AddVkontakte(options =>
                {
                    options.ClientId = context.Configuration["Authentication:Vk:ClientId"];
                    options.ClientSecret = context.Configuration["Authentication:Vk:ClientSecret"];

                    options.Scope.Add("email");
                    options.Scope.Add("photos");

                    // Add fields https://vk.com/dev/objects/user
                    options.Fields.Add("uid");
                    options.Fields.Add("first_name");
                    options.Fields.Add("last_name");

                    // In this case email will return in OAuthTokenResponse, 
                    // but all scope values will be merged with user response
                    // so we can claim it as field
                    options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "uid");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
                    options.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "first_name");
                    options.ClaimActions.MapJsonKey(ClaimTypes.Surname, "last_name");
                });

                services.AddAuthentication()
                    .AddIdentityServerJwt();
            });
        }
    }
}