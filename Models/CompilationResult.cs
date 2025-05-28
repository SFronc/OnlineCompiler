using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace OnlineCompiler.Models
{
    public class CompilationResult
    {
        [Key]
        public int Id { get; set; }
        public bool Success { get; set; }
        public string Output { get; set; }
        public string Errors { get; set; }
        public string Poczet { get; set; }
        public string ErrorFile { get; set; }
        public DateTime CompilatedAt { get; set; } = DateTime.UtcNow;

        public int ProjectId { get; set; }
        public Project Project { get; set; }
    }
}