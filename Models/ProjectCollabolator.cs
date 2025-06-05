
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace OnlineCompiler.Models
{
    public class ProjectCollaborator
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public CollaboratorRole Role { get; set; }
        public DateTime JoinDate { get; set; }

    }

    public enum CollaboratorRole
    {
        [Description("Project Admin")]
        Admin = 0,

        [Description("Project Collabolator with full access")]
        Collaborator = 1,

        [Description("project Collabolator read only")]
        ReadOnly = 2,
    }
}