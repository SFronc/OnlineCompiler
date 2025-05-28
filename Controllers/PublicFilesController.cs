using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.Controllers
{
    public class PublicFilesController : Controller
    {
        private readonly DataBaseContext _context;

        public PublicFilesController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: PublicFiles
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            var publicFiles = await _context.PublicFiles
            .Include(pf => pf.PublicFile)
            .Where(pf => pf.IsActive)
            .ToListAsync();

            var importedFileIds = await _context.ImportFile.Where(imp => imp.UserId == userId)
            .Select(imp => imp.OriginalPublicFileId)
            .ToListAsync();

            ViewBag.ImportedFileIds = importedFileIds;
            ViewBag.CurrentUserId = userId;

            return View(publicFiles);
        }

        // GET: PublicFiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PublicFiles == null)
            {
                return NotFound();
            }

            var publicFiles = await _context.PublicFiles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publicFiles == null)
            {
                return NotFound();
            }

            return View(publicFiles);
        }

        // GET: PublicFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PublicFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PublicFile,Author,UpdateDate")] PublicFiles publicFiles)
        {
            if (ModelState.IsValid)
            {
                _context.Add(publicFiles);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(publicFiles);
        }

        // GET: PublicFiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PublicFiles == null)
            {
                return NotFound();
            }

            var publicFiles = await _context.PublicFiles.FindAsync(id);
            if (publicFiles == null)
            {
                return NotFound();
            }
            return View(publicFiles);
        }

        // POST: PublicFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PublicFile,Author,UpdateDate")] PublicFiles publicFiles)
        {
            if (id != publicFiles.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(publicFiles);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PublicFilesExists(publicFiles.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(publicFiles);
        }

        // GET: PublicFiles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PublicFiles == null)
            {
                return NotFound();
            }

            var publicFiles = await _context.PublicFiles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (publicFiles == null)
            {
                return NotFound();
            }

            return View(publicFiles);
        }

        // POST: PublicFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PublicFiles == null)
            {
                return Problem("Entity set 'DataBaseContext.PublicFiles'  is null.");
            }
            var publicFiles = await _context.PublicFiles.FindAsync(id);
            if (publicFiles != null)
            {
                _context.PublicFiles.Remove(publicFiles);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
    public async Task<IActionResult> Share(int fileId, string username)
    {
        try
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        var file = await _context.FileModel.FindAsync(fileId);
        if (file == null)
        {
                    //return NotFound("File not found");
                    return Json(new
                    {
                        success = false,
                        message = "File not found",
                        status = 404
                    });
        }

                var isAlreadyShared = await _context.PublicFiles.AnyAsync(pf => pf.PublicFileId == fileId && pf.IsActive);

        if (isAlreadyShared)
        {
                    return Json(new
                    {
                        success = true,
                        message = "File is already shared",
                        status = 200
                    });    
        }

        file.IsShared = true;

        //var existingShare = await _context.PublicFiles
        //    .FirstOrDefaultAsync(pf => pf.PublicFileId == fileId && pf.IsActive);
            
        //if (existingShare != null)
        //{
        //    return BadRequest("File is already shared");
        //}

        var publicFile = new PublicFiles
        {
            PublicFileId = fileId,
            Author = username,
            UpdateDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.PublicFiles.Add(publicFile);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "File shared successfully",
                    status = 200
                });
    }
    catch (Exception ex)
    {
        return Json(new {
            success = false,
            message = $"Internal server error: {ex.Message}",
            status = 500
        });
    }
    }

    [HttpPost]
    public async Task<IActionResult> Unshare(int fileId)
    {
        try
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        var publicFile = await _context.PublicFiles
            .FirstOrDefaultAsync(pf => pf.PublicFileId == fileId && pf.IsActive);

        var file = await _context.FileModel.FindAsync(fileId);
            
        if (publicFile == null)
        {
            return Json(new {
                success = false,
                message = "Shared file record not found",
                status = 404
            });
        }

        publicFile.IsActive = false;
        publicFile.UpdateDate = DateTime.UtcNow;
        
        if (file != null)
        {
            file.IsShared = false;
        }

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Json(new {
            success = true,
            message = "File unshared successfully",
            status = 200
        });
    }
    catch (Exception ex)
    {
        return Json(new {
            success = false,
            message = $"Internal server error: {ex.Message}",
            status = 500
        });
    }
    }

        private bool PublicFilesExists(int id)
        {
            return (_context.PublicFiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
