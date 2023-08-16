using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Category : IEntity
{
    public int ID { get; set; }

    [Required]
    public string Name { get; set; }

    public IEnumerable<Edition>? Editions { get; set; }
}