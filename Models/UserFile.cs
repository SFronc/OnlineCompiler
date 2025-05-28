using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.Models
{
    public class UserFile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public byte[] Content { get; set; }

        [Required]
        public String FileType { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
        public User User { get; set; }
    }
}