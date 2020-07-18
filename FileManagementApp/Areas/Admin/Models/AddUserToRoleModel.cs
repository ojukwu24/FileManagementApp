using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementApp.Areas.Admin.Models
{
    public class AddUserToRoleModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        public string Id { get; set; }

        [Required] 
        [Display(Name = "Available Roles")]
        public string Role { get; set; }
    }
}
