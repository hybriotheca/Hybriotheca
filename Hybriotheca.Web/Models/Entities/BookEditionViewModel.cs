using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Hybriotheca.Web.Models.Entities
{
    public class BookEditionViewModel
    {
        public int ID { get; set; }

        [Display(Name = "Base Book")]
        [Range(1, int.MaxValue, ErrorMessage = "Base Book Field is required")]
        public int BookID { get; set; }


        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Category Field is required")]
        public int CategoryID { get; set; }

        [MaxLength(13, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Please enter only numbers.")]
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
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;


        public string Language { get; set; }


        [MaxLength(250, ErrorMessage = "The {0} value cannot exceed {1} characters.")]
        public string? Awards { get; set; }


        [Display(Name = "Book Format")]
        public string BookFormat { get; set; }


        [Display(Name = "Nº Pages")]
        [Range(1, int.MaxValue, ErrorMessage = "The number of pages must be higher than 0.")]
        public int NrPages { get; set; }


        public string? CoverImageFullPath { get; set; }

        [Display(Name = "Cover")]
        public IFormFile? CoverImageFile { get; set; }

        [Display(Name = "ePub File")]
        public IFormFile? ePubFile { get; set; }

        [FileExtensions(Extensions = ".epub", ErrorMessage = "File type not allowed. Please upload a valid ePUB file.")]
        public string? FileName => ePubFile?.FileName ?? "empty.epub";

    }
}
