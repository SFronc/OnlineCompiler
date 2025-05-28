using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace OnlineCompiler.Models
{
    public class LibraryAccess
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int LibraryId { get; set; }
        public Library Library { get; set; }
    }
}