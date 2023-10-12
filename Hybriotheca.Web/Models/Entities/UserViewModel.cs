using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "User Role")]
        public string Role { get; set; }


        [EmailAddress]
        public string Email { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        public string NameAbbr
        {
            get
            {
                var firstLetters = FullName.Split(' ')
                    .Where(word => !string.IsNullOrEmpty(word))
                    .ToArray();

                if (firstLetters.Length > 0)
                {
                    string nameAbbr = string.Concat(firstLetters[0][0], firstLetters[^1][0]);
                    return nameAbbr;
                }
                return string.Empty;
            }
        }

        
        public IFormFile? PhotoFile { get; set; }

        public bool HasPhoto { get; set; }

        public string? PhotoFullPath { get; set; }

        public bool DeletePhoto { get; set; }


        [Display(Name = "Subscription Tier")]
        public int SubscriptionID { get; set; }

        public string? SubscriptionName { get; set; }


        [Display(Name = "Main Library")]
        public int MainLibraryID { get; set; }
    }
}
