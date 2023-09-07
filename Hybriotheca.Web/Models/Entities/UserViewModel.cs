using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Models.Entities
{
    public class UserViewModel
    {
        public string? Id { get; set; }

        public string Role { get; set; }


        [EmailAddress]
        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }


        public string? PhoneNumber { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";


        public IFormFile? PhotoFile { get; set; }

        public bool HasPhoto { get; set; }

        public string? PhotoFullPath { get; set; }

        public bool DeletePhoto { get; set; }


        public int SubscriptionID { get; set; }

        public string? SubscriptionName { get; set; }
    }
}
