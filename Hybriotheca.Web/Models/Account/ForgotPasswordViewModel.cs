using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Account
{
    public class ForgotPasswordViewModel
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
