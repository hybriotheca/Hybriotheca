using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Account
{
    public class RegisterViewModel
    {
        [Required, DisplayName("First Name")]
        public string FirstName { get; set; }


        [Required, DisplayName("Last Name")]
        public string LastName { get; set; }


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
