using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicFilesController : ControllerBase
    {
        private readonly DataBaseContext _context;

        public PublicFilesController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: api/PublicFiles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PublicFiles>>> GetPublicFiles()
        {

            var curUser =  HttpContext.Items["User"] as User;

            if (_context.PublicFiles == null || curUser == null)
            {
                return NotFound();
            }

            if (curUser.Role != "Admin")
            {
                return Unauthorized();
            }

            return await _context.PublicFiles.ToListAsync();
        }

        
    }
}