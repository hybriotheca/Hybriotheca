using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Models.Entities
{
    public class BookEditionViewModel
    {
        public int ID { get; set; }


        public IEnumerable<SelectListItem>? Books { get; set; }

        [Display(Name = "Base Book")]
        [Range(1, int.MaxValue)]
        public int BookID { get; set; }

        public IEnumerable<SelectListItem>? Categories { get; set; }

        [Display(Name = "Category")]
        [Range(1, int.MaxValue)]
        public int CategoryID { get; set; }



        [MaxLength(13, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? ISBN { get; set; }


        [MaxLength(500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Title { get; set; }


        [MaxLength(2500, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Synopsis { get; set; }


        [Display(Name = "Translation Author")]
        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? TranslationAuthor { get; set; }


        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Publisher { get; set; }


        [Display(Name = "Publish Date")]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = false)]
        public DateTime PublishDate { get; set; }


        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string Language { get; set; }


        [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? Awards { get; set; }


        [Display(Name = "Book Format")]
        [MaxLength(100, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string BookFormat { get; set; }


        [Display(Name = "Nº Pages")]
        [Range(1, int.MaxValue, ErrorMessage = "Value for {0} must be between {1} and {2}.")]
        public int NrPages { get; set; }


        public string? CoverImageFullPath { get; set; }

        [Display(Name = "Cover")]
        public IFormFile? CoverImageFile { get; set; }


        [Display(Name = "Available Online")]
        public bool IsAvailableOnline { get; set; }

        public Guid? EbookID { get; set; }

    }
}
