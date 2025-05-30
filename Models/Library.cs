using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace OnlineCompiler.Models
{
    public class Library
    {
        [Key]
        public int Id { get; set; }
    
        public int ProjectId { get; set; }
        public Project Project { get; set; }
    
        public int ImportedFileId { get; set; }
        public ImportFile ImportedFile { get; set; }
    
        public bool IsActive { get; set; } = true;
        public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
    }
}