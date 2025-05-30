
using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.Models
{
    public class FileModel
    {
        [Key]
        public int Id { get; set; }
        public String Name { get; set; }
        public byte[] Content { get; set; }
        public string Type { get; set; }
        public DateTime LastModified { get; set; }
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        public bool IsShared { get; set; } = false;
        public PublicFiles? Share { get; set; }
        public string? ModifiedBy { get; set; } = string.Empty;
    }
}