using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public String Username { get; set; }
        public String Email { get; set; }
        public String PasswordHash { get; set; }
        public DateTime RegisterDate { get; set; } = DateTime.UtcNow;

        public ICollection<Project>? Projects { get; set; }
        public ICollection<LibraryAccess>? AccessibleLibraries { get; set; }
    }
}