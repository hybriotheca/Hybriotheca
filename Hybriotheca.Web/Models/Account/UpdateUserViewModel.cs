using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Account
{
    public class UpdateUserViewModel
    {
        [Required, DisplayName("First Name")]
        public string FirstName { get; set; }

        [Required, DisplayName("Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }
    }
}
