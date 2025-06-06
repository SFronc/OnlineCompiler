namespace OnlineCompiler.Models.DTO
{
    public class ProjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool isPublic { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class ProjectCreateDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public bool isPublic { get; set; }
    }

    public class ProjectUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Code { get; set; }
        public bool isPublic { get; set; }
    }

    public class ProjectDetailsDTO : ProjectDTO
    {
        public string Code { get; set; }
        public List<FileModelDTO> Files { get; set; }
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