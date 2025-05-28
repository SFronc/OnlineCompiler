
using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.Models
{
    public class ImportFile
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public Project? Project { get; set; }
    
        public int ImportedFileId { get; set; }
        public FileModel ImportedFile { get; set; }
    
        public int OriginalPublicFileId { get; set; }
        public PublicFiles OriginalPublicFile { get; set; }
    
        public DateTime ImportDate { get; set; } = DateTime.UtcNow;
        public string? ImportedBy { get; set; }
    }
}