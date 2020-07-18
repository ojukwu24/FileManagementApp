using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementApp.Areas.Admin.Models
{
    public class UserDto
    {
        public string Id { get; set; }
        public string  Username { get; set; }
        public string  Email { get; set; }
        public string  PhoneNumber { get; set; }
        public bool  EmailConfirmed { get; set; }
        public List<string>  RoleNames { get; set; }
    }
}
