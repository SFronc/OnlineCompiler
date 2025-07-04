using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using OnlineCompiler.Data;
using OnlineCompiler.Models;
using OnlineCompiler.Services;
using OnlineCompiler.ViewModels;

namespace OnlineCompiler.Controllers
{
    public class FileModelController : Controller
    {
        private readonly DataBaseContext _context;
        private readonly ICompilerService _compiler;


        public FileModelController(DataBaseContext context, ICompilerService compiler)
        {
            _context = context;
            _compiler = compiler;
        }

        // GET: FileModel
        public async Task<IActionResult> Index(int id)
        {
            var file = await _context.FileModel.Include(f => f.Project).FirstOrDefaultAsync(f => f.Id == id);
            if (file == null) return NotFound();

            ViewBag.FileId = id;
            ViewBag.FileName = file.Name;
            ViewBag.FileContent = Encoding.UTF8.GetString(file.Content);

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var project = await _context.Project
                .Include(p => p.User)
                .Include(p => p.Collaborators)
                .FirstOrDefaultAsync(p => p.Id == file.ProjectId);

            ViewBag.isReadOnly = false;

            if (project == null || project.UserId == HttpContext.Session.GetInt32("UserId"))
            {
                return View();
            }

            var collaborator = project.Collaborators.FirstOrDefault(c => c.UserId == currentUserId);

            if (collaborator == null) {
                return View();
            }
            else {
                ViewBag.isReadOnly = collaborator.Role == CollaboratorRole.ReadOnly;
            }

            return View();
            //return View(await dataBaseContext.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Save(int id, [FromBody] string content)
        {
            var file = await _context.FileModel.FindAsync(id);
            if (file == null) return NotFound();

            file.Content = Encoding.UTF8.GetBytes(content);
            file.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Compile(int id)
        {
            var file = await _context.FileModel.FindAsync(id);
            if (file == null) return NotFound();

            var code = Encoding.UTF8.GetString(file.Content);

            int? projId = HttpContext.Session.GetInt32("CurrProject");

            if (projId == null)
            {
                return NotFound();
            }

            List<FileModel> projFiles = await _context.FileModel.Where(f => f.ProjectId == projId && f.Id != id).ToListAsync();
            List<FileModel> libFiles = await _context.Librarie
                                            .Include(l => l.ImportedFile)
                                            .ThenInclude(i => i.ImportedFile)
                                            .Where(l => l.ProjectId == projId)
                                            .Select(l => l.ImportedFile.ImportedFile)
                                            .Distinct()
                                            .ToListAsync();

            var result = await _compiler.CompileAsync(code, "cz", projFiles, libFiles,_context);

            Console.WriteLine("===========================");
             Console.WriteLine(result.Poczet);
            Console.WriteLine(result.Success);
             Console.WriteLine("===========================");

            return Json(new
            {
                success = result.Success,
                output = result.Output,
                errors = result.Errors,
                poczet = result.Poczet,
                errorFile = result.ErrorFile
            });
        }

       

        // GET: FileModel/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel
                .Include(f => f.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileModel == null)
            {
                return NotFound();
            }

            return View(fileModel);
        }

        // GET: FileModel/Create
        [HttpGet]
        public async Task<IActionResult> CreateAsync(int projectId)
        {
            var currProj = await _context.Project.FindAsync(projectId);

            if (currProj != null)
            {
                currProj.LastModified = DateTime.UtcNow;
                //currProj.ModifiedBy = userModified;
            }

            var model = new FileCreateViewModel
            {
                ProjectId = projectId,

            };
            return View(model);
        }

        // POST: FileModel/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FileCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                //var id = (int)HttpContext.Session.GetInt32("UserId");
                //var userModified = _context.User.FirstOrDefault(u => u.Id == id);
                //if (userModified == null) Console.WriteLine("UserModified is null!");

                var file = new FileModel
                {
                    Name = model.FileName,
                    ProjectId = model.ProjectId,
                    Type = model.Extension,
                    Content = Array.Empty<byte>(),
                    LastModified = DateTime.UtcNow,
                    //ModifiedBy = userModified.Username

                };
                _context.Add(file);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Empty file '{model.FileName}' has been created";
                return RedirectToAction("Details", "Project", new { id = model.ProjectId });
            }
            else
            {
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        foreach (var error in entry.Value.Errors)
                        {
                         Console.WriteLine($"Property: {entry.Key}, Error: {error.ErrorMessage}");
                        }
                    }
                }
            }
            
            return View(model);
        }

        // GET: FileModel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel.FindAsync(id);
            if (fileModel == null)
            {
                return NotFound();
            }
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", fileModel.ProjectId);
            return View(fileModel);
        }

        // POST: FileModel/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Content,Type,LastModified,ProjectId")] FileModel fileModel)
        {
            if (id != fileModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(fileModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FileModelExists(fileModel.Id))
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
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", fileModel.ProjectId);
            return View(fileModel);
        }

        // GET: FileModel/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.FileModel == null)
            {
                return NotFound();
            }

            var fileModel = await _context.FileModel
                .Include(f => f.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (fileModel == null)
            {
                return NotFound();
            }

            return View(fileModel);
        }

        // POST: FileModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.FileModel == null)
            {
                return Problem("Entity set 'DataBaseContext.FileModel'  is null.");
            }

            var file = await _context.FileModel
                .Include(f => f.Project)
                .Include(f => f.Share)
                .FirstOrDefaultAsync(f => f.Id == id);

            //var importFiles = await _context.ImportFile
            //    .Where(f => f.ImportedFileId == id || f.OriginalPublicFileId == id)
            //    .ToListAsync();

            var importFiles = await _context.ImportFile.ToListAsync();

            foreach (var importedFile in importFiles)
            {
                if (importedFile.OriginalPublicFileId == file.Id)
                {
                    importedFile.ImportedFileId = 0; 
                    importedFile.ImportedFile = null;
                }
                /*
                if (importedFile.ImportedFileId == id)
                {
                    importedFile.ImportedFileId = 0;
                    importedFile.ImportedFile = null;
                }
                if (importedFile.OriginalPublicFileId == id)
                {
                    importedFile.OriginalPublicFileId = 0;
                    importedFile.OriginalPublicFile = null;
                }*/

            }

            //await _context.SaveChangesAsync();


            if (file == null)
            {
                return NotFound();
            }

           if (file.Share != null)
            {
                _context.PublicFiles.Remove(file.Share);
            }

            _context.FileModel.Remove(file);
            
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Project", new { id = HttpContext.Session.GetInt32("CurrProject") });
        }

        [HttpGet]
        public async Task<IActionResult> GetProjectFiles()
        {
            var projectId = HttpContext.Session.GetInt32("CurrProject");
            if (projectId == null)
            {
                return Json(new List<object>());
            }

            var files = await _context.FileModel
                .Where(f => f.ProjectId == projectId)
                .Select(f => new
                {
                    id = f.Id,
                    name = f.Name,
                    lastModified = f.LastModified.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            return Json(files);
        }

        [HttpGet]
        public async Task<IActionResult> GetFileContent(int id)
        {
            var file = await _context.FileModel.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            var content = System.Text.Encoding.UTF8.GetString(file.Content);
            return Json(new { content });
        }



        private bool FileModelExists(int id)
        {
            return (_context.FileModel?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
