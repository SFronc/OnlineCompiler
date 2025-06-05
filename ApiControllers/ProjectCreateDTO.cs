using System.ComponentModel.DataAnnotations;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers
{
    // Project DTOs
public class ProjectCreateDTO
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; }
    
    [Required]
    public string Description { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool isPublic { get; set; } = false;
}

public class ProjectUpdateDTO : ProjectCreateDTO
{
    public int Id { get; set; }
}

public class ProjectDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool isPublic { get; set; }
    public DateTime LastModified { get; set; }
}

public class ProjectDetailsDTO : ProjectDTO
{
    public string Code { get; set; }
    public List<FileModelDTO> Files { get; set; }
    public List<CollaboratorDTO> Collaborators { get; set; }
    public List<CompilationResultDTO> CompilationResults { get; set; }
}

// File DTOs
public class FileModelCreateDTO
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Content { get; set; } // Base64 encoded
    
    [Required]
    public string Type { get; set; }
}

public class FileModelUpdateDTO : FileModelCreateDTO
{
    public int Id { get; set; }
}

    public class FileModelDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public DateTime LastModified { get; set; }
        public bool IsShared { get; set; }
        public string ModifiedBy { get; set; }
    }

}