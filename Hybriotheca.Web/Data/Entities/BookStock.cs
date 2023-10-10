using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Hybriotheca.Web.Data.Entities
{
    [Index(nameof(BookEditionID), nameof(LibraryID), IsUnique = true, Name = "IX_BookEdition_Library")]
    public class BookStock : IEntity
    {
        public int ID { get; set; }

        public int BookEditionID { get; set; }
        public BookEdition BookEdition { get; set; }

        public int LibraryID { get; set; }
        public Library Library { get; set; }

        public int TotalStock { get; set; }
        public int AvailableStock { get; set; }
    }
}