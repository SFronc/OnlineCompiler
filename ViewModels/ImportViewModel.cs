
using OnlineCompiler.Models;

namespace OnlineCompiler.ViewModels
{
    public class ImportViewModel
    {
        public int OriginalPublicFileId { get; set; }  
        public string OriginalFileName { get; set; }  
        public List<int> SelectedProjectIds { get; set; } = new List<int>();  
        public List<Project> Projects { get; set; }  
    }
}