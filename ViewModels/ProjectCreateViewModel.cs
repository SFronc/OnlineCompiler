using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.ViewModels
{
    public class ProjectCreateViewModel
    {
        [Required(ErrorMessage = "Nazwa projektu jest wymagana")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nazwa musi mieć od 3 do 100 znaków")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Opis projektu jest wymagany")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }
    }
}