using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Hybriotheca.Web.Data.Entities;

public class Fine : IEntity
{
    public int ID { get; set; }

    public int LoanID { get; set; }

    public Loan Loan { get; set; } = null!;

    [Display(Name = "Has fine been paid?")]
    public bool IsPaid { get; set; }

    [Display(Name = "Fine Amount")]
    [Precision(18, 2)]
    [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)]
    public decimal FineValue { get; set; }

}