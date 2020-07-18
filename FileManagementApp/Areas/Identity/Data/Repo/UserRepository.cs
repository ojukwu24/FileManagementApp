using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileManagementApp.Areas.Admin.Models;

namespace FileManagementApp.Areas.Identity.Data.Repo
{
    public class UserRepository : IUserRepository
    {
        private readonly FileManagementAppContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(FileManagementAppContext context,
                        ILogger<UserRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public List<UserDto> GetUserListWithRoles()
        {
            try
            {
                var userList = (from user in _context.Users
                                select new UserDto
                                {
                                    Id = user.Id,
                                    Username = user.UserName,
                                    Email = user.Email,
                                    PhoneNumber = user.PhoneNumber,
                                    EmailConfirmed = user.EmailConfirmed,
                                    RoleNames = _context.UserRoles
                                                                .Where(ur => ur.UserId == user.Id)
                                                                .Select(ur => _context.Roles
                                                                .First(r => r.Id == ur.RoleId).Name).ToList()
                                });

                return userList.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUserListWithRole", ex);
                return new List<UserDto>();
            }
        }

        public async Task<UserDto> GetUserWithRoles(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                return new UserDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    RoleNames = _context.UserRoles
                                                .Where(ur => ur.UserId == user.Id)
                                                .Select(ur => _context.Roles
                                                .First(r => r.Id == ur.RoleId).Name).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("GetUserWithRoles", ex);
                return null;
            }
        }

        public void Update(FileManagementAppUser user)
        {
            try
            {
                _context.Users.Attach(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Update", ex);
            }
        }

        public async Task<bool> SaveChanges()
        {
            try
            {
                int affectedRow = await _context.SaveChangesAsync();
                return affectedRow > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError("SaveChanges", ex);
                return false;
            }
        }
    }
}
