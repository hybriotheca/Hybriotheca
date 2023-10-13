using System.ComponentModel;

namespace Hybriotheca.Web.Data.Entities;

public class Subscription : IEntity
{
    public int ID { get; set; }

    public string Name { get; set; }

    public string Details { get; set; }

    [DisplayName("Max loan days")]
    public int MaxLoanDays { get; set; }

    [DisplayName("Max loans")]
    public int MaxLoans { get; set; }

    public IEnumerable<AppUser>? Users { get; set; }
}