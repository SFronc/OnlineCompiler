
using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.Models
{
    public class FileVersion
    {
        [Key]
        public int Id { get; set; }
        public byte[] Content { get; set; }
        public DateTime Version { get; set; }

        public int PublicFilesId { get; set; }
        public PublicFiles PublicFiles { get; set; }
    }
}