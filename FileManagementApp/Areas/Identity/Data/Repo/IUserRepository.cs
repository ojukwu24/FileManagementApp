using System.Collections.Generic;
using System.Threading.Tasks;
using FileManagementApp.Areas.Admin.Models;

namespace FileManagementApp.Areas.Identity.Data.Repo
{
    public interface IUserRepository
    {
        List<UserDto> GetUserListWithRoles();
        Task<UserDto> GetUserWithRoles(string userId);
        void Update(FileManagementAppUser user);
        Task<bool> SaveChanges();


    }
}
