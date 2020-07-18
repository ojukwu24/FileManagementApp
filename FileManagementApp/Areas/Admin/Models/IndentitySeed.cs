using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileManagementApp.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileManagementApp.Areas.Admin.Models
{
    public class IdentitySeed : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public IdentitySeed(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // Create a new scope to retrieve scoped services
            using (var scope = _serviceProvider.CreateScope())
            {
                // Get the DbContext instance
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<FileManagementAppUser>>();
                var _logger = scope.ServiceProvider.GetRequiredService<ILogger<IdentitySeed>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

               

                var user = new FileManagementAppUser { UserName = "admin@mail.com", Email = "admin@mail.com", EmailConfirmed = true };

                var adminCheck = await userManager.FindByNameAsync(user.UserName);

                if(adminCheck == null)
                {
                    var result = await userManager.CreateAsync(user, "Password@247");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Admin user added with password.");

                        //Create Admin Role and Assign Admin user created to role
                        var existingRole = await roleManager.FindByNameAsync("Administrator");
                        if (existingRole == null)
                        {
                            var role = new IdentityRole("Administrator");
                            var adminRoleResult = await roleManager.CreateAsync(role);

                            if (adminRoleResult.Succeeded)
                            {
                                _logger.LogInformation("Admin Role added.");
                            }

                            var roleDocManger = new IdentityRole("Document Manager");
                            var DocMangerRoleResult = await roleManager.CreateAsync(roleDocManger);

                            if (DocMangerRoleResult.Succeeded)
                            {
                                _logger.LogInformation("Document Manager Role added.");
                            }

                            
                            var userRole = new IdentityRole("User");
                            var userRoleResult = await roleManager.CreateAsync(userRole);

                            if (userRoleResult.Succeeded)
                            {
                                _logger.LogInformation("User Role added.");
                            }

                            var roleResult = await userManager.AddToRoleAsync(user, role.Name);
                            if (roleResult.Succeeded)
                            {
                                _logger.LogInformation("Admin added to Admin Role.");
                            }
                        }
                    }
                }
                
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
