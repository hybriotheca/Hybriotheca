using Hybriotheca.Web.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Repositories.Entities;

public class Subscription : IEntity
{
    public int ID { get; set; }

    [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
    public string? Details { get; set; }

    public IEnumerable<AppUser> AppUsers { get; set; } = null!;
    
}