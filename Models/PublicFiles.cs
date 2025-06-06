
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineCompiler.Models
{
    public class PublicFiles
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("FileModel")]
        public int? PublicFileId { get; set; }

        public int AuthorOriginalFileId { get; set; }
        public FileModel AuthorOriginalFile { get; set; }

        [Required]
        [StringLength(100)]
        public string Author { get; set; }

        public DateTime UpdateDate { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public List<FileVersion> Versions { get; set; } = new List<FileVersion>();
        
    }
}