using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace OnlineCompiler.Models
{
    public class Library
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LibraryAccess> AccessibleBy { get; set; }
        public ICollection<ProjectLibrary> UsedInProjects { get; set; }
    }
}