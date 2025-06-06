using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileModelController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public FileModelController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: api/FileModel
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileModel>>> GetFileModel()
        {
            var curUser =  HttpContext.Items["User"] as User;

            if (_context.FileModel == null || curUser == null)
            {
                return NotFound();
            }

            if (curUser.Role != "Admin")
            {
                return Unauthorized();
            }

            return await _context.FileModel.ToListAsync();
        }

        // DELETE: api/File/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.FileModel
                .Include(f => f.Project)
                .Include(f => f.Share)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (file == null)
            {
                return NotFound();
            }

            if (file.IsShared && file.Share != null)
            {
                _context.PublicFiles.Remove(file.Share);
            }

            _context.FileModel.Remove(file);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, "Error deleting file: " + ex.Message);
            }

            return NoContent();
        }
    }
}