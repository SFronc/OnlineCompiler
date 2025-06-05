namespace OnlineCompiler.Models
{
    public class CollaboratorDTO
    {
        public int Id { get; set; }
        public string Username { get; set; } 
        public string Role { get; set; }      
        public DateTime JoinDate { get; set; } 

        public static CollaboratorDTO CollaboratorToDTO(ProjectCollaborator collaborator) => new()
        {
            Id = collaborator.Id,
            Username = collaborator.User.Username,
            Role = collaborator.Role.ToString(),
            JoinDate = collaborator.JoinDate
        };
    }

}