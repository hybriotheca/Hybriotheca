using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class UserViewModel
    {
        public string? Id { get; set; }

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

                string nameAbbr = string.Concat(firstLetters[0][0], firstLetters[firstLetters.Length - 1][0]);

                return nameAbbr;
            }
        }

        
        public IFormFile? PhotoFile { get; set; }

        public bool HasPhoto { get; set; }

        public string? PhotoFullPath { get; set; }

        public bool DeletePhoto { get; set; }


        [Display(Name = "Subscription")]
        public int SubscriptionID { get; set; }

        public string? SubscriptionName { get; set; }

        public Guid ProfilePictureId { get; set; }

        public string ProfilePictureFullPath => ProfilePictureId == Guid.Empty ?
            "https://hybriotheca.blob.core.windows.net/userphotos/nophoto" :
            "https://hybriotheca.blob.core.windows.net/userphotos/" + ProfilePictureId;
    }
}
