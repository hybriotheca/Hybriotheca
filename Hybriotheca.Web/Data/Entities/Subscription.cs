using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Subscription : IEntity
{
    public int ID { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Details { get; set; }


    public IEnumerable<AppUser>? Users { get; set; }

}