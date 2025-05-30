
namespace OnlineCompiler.ViewModels
{
    public class ManageProjectsViewModel
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public List<ProjectSelectionViewModel> Projects { get; set; }
    }
}