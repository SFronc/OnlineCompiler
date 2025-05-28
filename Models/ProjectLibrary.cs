using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace OnlineCompiler.Models
{
    public class ProjectLibrary
    {
        [Key]
        public int Id { get; set; }

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int LibraryId { get; set; }
        public Library Library { get; set; }
    }
}