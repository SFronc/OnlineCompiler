using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImportFileController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public ImportFileController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: api/ImportFile
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ImportFile>>> GetImportFile()
        {
            var curUser =  HttpContext.Items["User"] as User;

            if (_context.ImportFile == null || curUser == null)
            {
                return NotFound();
            }

            if (curUser.Role != "Admin")
            {
                return Unauthorized();
            }

            return await _context.ImportFile.ToListAsync();
        }

        
    }
}