using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Repositories.Entities
{
    public class Work : IEntity
    {
        public int ID { get; set; }

        [Display(Name = "Generic Book Title for all Editions")]
        [MaxLength(500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string OriginalTitle { get; set; } = string.Empty;

        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Author { get; set; } = string.Empty;

        public IEnumerable<Edition> Editions { get; set; } = null!;
    }
}