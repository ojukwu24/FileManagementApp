using System;
using FileManagementApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(FileManagementApp.Areas.Identity.IdentityHostingStartup))]
namespace FileManagementApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) => {
                services.AddDbContext<FileManagementAppContext>(options =>
                    options.UseSqlServer(
                        context.Configuration.GetConnectionString("FileManagementAppContextConnection")));

                services.AddIdentity<FileManagementAppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<FileManagementAppContext>()
                    .AddDefaultTokenProviders();
            });
        }
    }
}