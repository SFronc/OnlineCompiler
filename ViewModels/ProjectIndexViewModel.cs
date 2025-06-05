using System.Collections.Generic;
using OnlineCompiler.Models;

namespace OnlineCompiler.ViewModels
{
    public class ProjectIndexViewModel
    {
        public List<Project> OwnedProjects { get; set; }
        public List<ProjectCollaborator> CollaborationProjects { get; set; }
    }
}