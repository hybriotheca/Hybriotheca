using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Repositories.Entities
{
    public class Category : IEntity
    {
        public int ID { get; set; }

        [MaxLength(50, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Name { get; set; } = string.Empty;

        public IEnumerable<Edition>? Editions { get; set; }
    }
}