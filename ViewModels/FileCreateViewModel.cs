using System.ComponentModel.DataAnnotations;
using OnlineCompiler.Models;

namespace OnlineCompiler.ViewModels
{
    public class FileCreateViewModel
    {
        [Required]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "File name is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Name must contain 1 - 100 characters")]
        [RegularExpression(@"^[\w\-\. ]+$", ErrorMessage = "name contains invalid characters")]
        public string FileName { get; set; }

        public string Extension { get; set; } = string.Empty;
    }
}