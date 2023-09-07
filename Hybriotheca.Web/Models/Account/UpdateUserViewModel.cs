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


        [Display(Name = "Upload photo")]
        public IFormFile? PhotoFile { get; set; }

        public bool HasPhoto { get; set; }

        public string? PhotoFullPath { get; set; }

        public bool DeletePhoto { get; set; }
    }
}
