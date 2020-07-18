using System.ComponentModel.DataAnnotations;

namespace FileManagementApp.Areas.Admin.Models
{
    public class EditUserModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        
    }
}
