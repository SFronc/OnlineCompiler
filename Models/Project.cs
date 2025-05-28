using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.VisualStudio.Web.CodeGeneration;

namespace OnlineCompiler.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Project name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must have 3 - 100 characters")]
        public String Name { get; set; }

        [Required(ErrorMessage = "Project description is required")]
        public String Description { get; set; }
        public String Code { get; set; } = string.Empty;
        public bool isPublic { get; set; } = false;
        public int? UserId { get; set; }
        public User? User { get; set; }
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        //public User? ModifiedBy { get; set; }

        public ICollection<FileModel> Files { get; set; } = new List<FileModel>();
        public ICollection<CompilationResult> CompilationResult { get; set; } = new List<CompilationResult>();
        //public ICollection<ProjectLibrary> UsedLibraries { get; set; } = new List<ProjectLibrary>();
        public ICollection<ImportFile> ImportedFile { get; set; } = new List<ImportFile>();
    }
}