using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Book : IEntity
{
    public int ID { get; set; }

    [Required]
    public string OriginalTitle { get; set; }

    [Required]
    public string Author { get; set; }


    public IEnumerable<BookEdition>? Editions { get; set; }

}