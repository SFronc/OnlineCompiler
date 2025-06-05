using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers {

    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public ProjectsController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetProjects()
        {
            var user = (User)HttpContext.Items["User"];
            return await _context.Project
                .Where(p => p.UserId == user.Id || p.Collaborators.Any(c => c.UserId == user.Id))
                .Select(p => ProjectToDTO(p))
                .ToListAsync();
        }

        // GET: api/projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDetailsDTO>> GetProject(int id)
        {
            var user = (User)HttpContext.Items["User"];
            var project = await _context.Project
                .Include(p => p.Files)
                .Include(p => p.Collaborators).ThenInclude(c => c.User)
                .Include(p => p.CompilationResult)
                .FirstOrDefaultAsync(p => p.Id == id &&
                    (p.UserId == user.Id || p.Collaborators.Any(c => c.UserId == user.Id)));

            if (project == null) return NotFound();

            return ProjectToDetailsDTO(project);
        }

        // POST: api/projects
        [HttpPost]
        public async Task<ActionResult<ProjectDTO>> PostProject(ProjectCreateDTO projectDTO)
        {
            var user = (User)HttpContext.Items["User"];
            var project = new Project
            {
                Name = projectDTO.Name,
                Description = projectDTO.Description,
                Code = projectDTO.Code,
                isPublic = projectDTO.isPublic,
                UserId = user.Id,
                LastModified = DateTime.UtcNow
            };

            _context.Project.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, ProjectToDTO(project));
        }

        // PUT: api/projects/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectUpdateDTO projectDTO)
        {
            var user = (User)HttpContext.Items["User"];
            if (id != projectDTO.Id) return BadRequest();

            var project = await _context.Project.FindAsync(id);
            if (project == null || project.UserId != user.Id) return NotFound();

            project.Name = projectDTO.Name;
            project.Description = projectDTO.Description;
            project.Code = projectDTO.Code;
            project.isPublic = projectDTO.isPublic;
            project.LastModified = DateTime.UtcNow;

            _context.Entry(project).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/projects/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var user = (User)HttpContext.Items["User"];
            var project = await _context.Project.FindAsync(id);
            if (project == null || project.UserId != user.Id) return NotFound();

            _context.Project.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProjectExists(int id) => _context.Project.Any(e => e.Id == id);

        private static ProjectDTO ProjectToDTO(Project project) => new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            isPublic = project.isPublic,
            LastModified = project.LastModified
        };

        private static FileModelDTO FileModelToDTO(FileModel file) => new()
        {
            Id = file.Id,
            Name = file.Name,
            Type = file.Type,
            LastModified = file.LastModified,
            IsShared = file.IsShared,
            ModifiedBy = file.ModifiedBy
        };


        private static ProjectDetailsDTO ProjectToDetailsDTO(Project project) => new()
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Code = project.Code,
            isPublic = project.isPublic,
            LastModified = project.LastModified,
            Files = project.Files.Select(f => FileModelToDTO(f)).ToList(),
            Collaborators = project.Collaborators.Select(c => CollaboratorDTO.CollaboratorToDTO(c)).ToList(),
            CompilationResults = project.CompilationResult.Select(c => CompilationResultDTO.CompilationResultToDTO(c)).ToList()
        };
    }


}