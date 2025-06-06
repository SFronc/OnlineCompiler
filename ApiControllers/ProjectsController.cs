using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public ProjectController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: api/project
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProject()
        {
            var curUser = HttpContext.Items["User"] as User;

            if (_context.Project == null || curUser == null)
            {
                return NotFound();
            }

            if (curUser.Role != "Admin")
            {
                return Unauthorized();
            }

            return await _context.Project
                .Where(p => p.UserId == curUser.Id || p.Collaborators.Any(c => c.UserId == curUser.Id))
                //.Select(p => ProjectToDTO(p))
                .ToListAsync();
        }

        // GET: api/project/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var curUser = HttpContext.Items["User"] as User;

            if (_context.Project == null || curUser == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);

            /*var project = await _context.Project
                .Include(p => p.Files)
                .Include(p => p.Collaborators)
                .FirstOrDefaultAsync(p => p.Id == id && 
                    (p.UserId == user.Id || p.Collaborators.Any(c => c.UserId == user.Id)));*/

            if (project == null) return NotFound();

            if (curUser.Id != project.UserId && curUser.Role != "Admin")
            {
                return Unauthorized();
            }

            return project;
        }

        // POST: api/Project
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project project)
        {
            var user = (User)HttpContext.Items["User"];

            if (_context.Project == null)
            {
                return Problem("Entry set 'ContextDb.User' is null.");
            }

            if (user.Id != project.UserId && user.Role != "Admin")
            {
                return Unauthorized();
            }

            if (project.UserId != null && !await _context.User.AnyAsync(u => u.Id == project.UserId))
            {
                return BadRequest("Invalid UserId");
            }


            _context.Project.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProject", new { id = project.Id }, project);
        }


        // PUT: api/Project/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, ProjectUpdateDto projectDTO)
        {
            var user = (User)HttpContext.Items["User"];
            //if (id != projectDTO.Id) return BadRequest();

            var project = await _context.Project
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            if (project.UserId != user.Id && user.Role != "Admin")
            {
                return Unauthorized();
            }


            project.Name = projectDTO.Name;
            project.Description = projectDTO.Description;
            project.isPublic = projectDTO.isPublic;
            project.LastModified = DateTime.UtcNow;


            if (user.Role == "Admin" && projectDTO.UserId.HasValue)
            {
                var newUser = await _context.User.FindAsync(projectDTO.UserId);
                if (newUser != null)
                {
                    project.UserId = projectDTO.UserId.Value;
                    project.User = newUser;
                }

            }

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



        // DELETE: api/Project/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var currentUser = (User)HttpContext.Items["User"];

            var project = await _context.Project
                .Include(p => p.Files)
                    .ThenInclude(f => f.Share)
                .Include(p => p.ImportedFile)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            if (project.UserId != currentUser.Id && currentUser.Role != "Admin")
            {
                return Unauthorized();
            }


            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var file in project.Files.ToList())
                {
                    if (file.Share != null)
                    {
                        _context.PublicFiles.Remove(file.Share);
                    }
                    _context.FileModel.Remove(file);
                }

                foreach (var import in project.ImportedFile.ToList())
                {
                    import.Project = null;
                    //import.ProjectId = null;
                }

                _context.Project.Remove(project);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return NoContent();
    
        }

        private bool ProjectExists(int id) => _context.Project.Any(e => e.Id == id);

        
    }

    public class ProjectUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        public bool isPublic { get; set; }

        public int? UserId { get; set; } 
    }

}