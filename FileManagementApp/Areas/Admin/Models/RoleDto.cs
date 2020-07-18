using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagementApp.Areas.Admin.Models
{
    public class RoleDto
    {
        [Required]
        [DataType(DataType.Text)]
        [Display(Name = "Role Name")]
        public string Name { get; set; }
    }
}
