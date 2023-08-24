using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Models.Entities
{
    public class BookEditionViewModel
    {
        public int ID { get; set; }


        public IEnumerable<SelectListItem>? Books { get; set; }
        [Range(1, int.MaxValue)]
        public int BookID { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }



        [MaxLength(13, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? ISBN { get; set; }


        [MaxLength(500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Title { get; set; }


        [MaxLength(2500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Synopsis { get; set; }


        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? TranslationAuthor { get; set; }


        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Publisher { get; set; }


        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
        public DateTime PublishDate { get; set; }


        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Language { get; set; }


        [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? Awards { get; set; }


        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string BookFormat { get; set; }


        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int NrPages { get; set; }


        public string? CoverImageFullPath { get; set; }

        public IFormFile? CoverImageFile { get; set; }


        public bool IsAvailableOnline { get; set; }

        public Guid? EbookID { get; set; }

    }
}
