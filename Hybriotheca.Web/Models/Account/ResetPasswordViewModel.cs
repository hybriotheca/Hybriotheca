using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Account
{
    public class ResetPasswordViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }


        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; }


        [Compare("Password")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Required]
        public string ConfirmPassword { get; set; }


        public string Token { get; set; }
    }
}
