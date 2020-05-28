using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(ReactOnlineActivity.Areas.Identity.IdentityHostingStartup))]

namespace ReactOnlineActivity.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => { });
        }
    }
}