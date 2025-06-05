using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers
{
    [ApiController]
    [Route("api/projects/{projectId}/[controller]")]
    public class FilesController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public FilesController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: api/projects/5/files
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileModelDTO>>> GetFiles(int projectId)
        {
            var user = (User)HttpContext.Items["User"];
            if (!await HasProjectAccess(projectId, user.Id)) return Forbid();

            return await _context.FileModel
                .Where(f => f.ProjectId == projectId)
                .Select(f => FileModelToDTO(f))
                .ToListAsync();
        }

        // POST: api/projects/5/files
        [HttpPost]
        public async Task<ActionResult<FileModelDTO>> PostFile(int projectId, FileModelCreateDTO fileDTO)
        {
            var user = (User)HttpContext.Items["User"];
            if (!await HasProjectAccess(projectId, user.Id)) return Forbid();

            var file = new FileModel
            {
                Name = fileDTO.Name,
                Content = Convert.FromBase64String(fileDTO.Content),
                Type = fileDTO.Type,
                LastModified = DateTime.UtcNow,
                ProjectId = projectId,
                ModifiedBy = user.Username
            };

            _context.FileModel.Add(file);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFile", new { projectId, id = file.Id }, FileModelToDTO(file));
        }

        // PUT: api/projects/5/files/2
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFile(int projectId, int id, FileModelUpdateDTO fileDTO)
        {
            var user = (User)HttpContext.Items["User"];
            if (id != fileDTO.Id) return BadRequest();
            if (!await HasProjectAccess(projectId, user.Id)) return Forbid();

            var file = await _context.FileModel.FindAsync(id);
            if (file == null || file.ProjectId != projectId) return NotFound();

            file.Name = fileDTO.Name;
            file.Content = Convert.FromBase64String(fileDTO.Content);
            file.Type = fileDTO.Type;
            file.LastModified = DateTime.UtcNow;
            file.ModifiedBy = user.Username;

            _context.Entry(file).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/projects/5/files/2
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int projectId, int id)
        {
            var user = (User)HttpContext.Items["User"];
            if (!await HasProjectAccess(projectId, user.Id)) return Forbid();

            var file = await _context.FileModel.FindAsync(id);
            if (file == null || file.ProjectId != projectId) return NotFound();

            _context.FileModel.Remove(file);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FileExists(int id) => _context.FileModel.Any(e => e.Id == id);

        private async Task<bool> HasProjectAccess(int projectId, int userId)
        {
            return await _context.Project.AnyAsync(p => p.Id == projectId &&
                (p.UserId == userId || p.Collaborators.Any(c => c.UserId == userId)));
        }

        private static FileModelDTO FileModelToDTO(FileModel file) => new()
        {
            Id = file.Id,
            Name = file.Name,
            Type = file.Type,
            LastModified = file.LastModified,
            IsShared = file.IsShared,
            ModifiedBy = file.ModifiedBy
        };
    }

}