using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinalProject.ModelViews
{
    public class EditProfileViewModel
    {
        [Key]
        public int CustomerId { get; set; }

        [Display(Name = "Họ và Tên")]
        [Required(ErrorMessage = "Vui lòng nhập Họ Tên")]
        public string FullName { get; set; }

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Vui lòng nhập Địa chỉ")]
        public string Address { get; set; } 
 

        [MaxLength(11)]
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Điện thoại")]
        [DataType(DataType.PhoneNumber)]
        [Remote(action: "ValidatePhone", controller: "Accounts")]
        public string Phone { get; set; }

    }
}
