using System.ComponentModel.DataAnnotations;
using OnlineCompiler.Models;

namespace OnlineCompiler.ViewModels
{
    public class FileProjectsViewModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public List<ProjectSelection> Projects { get; set; } = new List<ProjectSelection>();
    }

    public class ProjectSelection
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public bool IsSelected { get; set; }
    }


}