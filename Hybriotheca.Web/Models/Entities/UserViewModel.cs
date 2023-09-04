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


        public int SubscriptionID { get; set; }

        public string? SubscriptionName { get; set; }
    }
}
