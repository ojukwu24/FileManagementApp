using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementApp.Areas.Admin.Models
{
    public class ManageUserModel
    {
        public EditUserModel EditUserModel { get; set; }
        public AddUserToRoleModel AddUserToRoleModel { get; set; }
        public RemoveUserFromRoleModel RemoveUserFromRoleModel { get; set; }
    }
}
