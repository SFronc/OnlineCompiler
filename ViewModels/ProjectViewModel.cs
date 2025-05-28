using System.ComponentModel.DataAnnotations;
using OnlineCompiler.Models;

namespace OnlineCompiler.ViewModels
{
    public class ProjectViewModel
    {
        public Project ProjectObj { get; set; }
        public string Username { get; set; }
    }
}