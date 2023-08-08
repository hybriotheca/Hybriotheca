using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Account
{
    public class UpdateUserViewModel
    {
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
