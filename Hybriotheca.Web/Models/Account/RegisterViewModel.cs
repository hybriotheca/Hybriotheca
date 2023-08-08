using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Account
{
    public class RegisterViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }


        [Required]
        public string Password { get; set; }


        [Compare("Password")]
        [Display(Name = "Confirm password")]
        [Required]
        public string ConfirmPassword { get; set; }

    }
}
