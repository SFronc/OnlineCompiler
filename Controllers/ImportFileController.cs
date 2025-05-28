using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;
using OnlineCompiler.ViewModels;

namespace OnlineCompiler.Controllers
{
    public class ImportFileController : Controller
    {
        private readonly DataBaseContext _context;

        public ImportFileController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: ImportFile
        public async Task<IActionResult> Index()
        {
            var dataBaseContext = _context.ImportFile.Include(i => i.ImportedFile).Include(i => i.OriginalPublicFile).Include(i => i.Project);
            return View(await dataBaseContext.ToListAsync());
        }

        // GET: ImportFile/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ImportFile == null)
            {
                return NotFound();
            }

            var importFile = await _context.ImportFile
                .Include(i => i.ImportedFile)
                .Include(i => i.OriginalPublicFile)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (importFile == null)
            {
                return NotFound();
            }

            return View(importFile);
        }

        // GET: ImportFile/Create
        public IActionResult Create()
        {
            ViewData["ImportedFileId"] = new SelectList(_context.FileModel, "Id", "Id");
            ViewData["OriginalPublicFileId"] = new SelectList(_context.PublicFiles, "Id", "Author");
            ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description");
            return View();
        }

        // POST: ImportFile/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,ImportedFileId,OriginalPublicFileId,ImportDate,ImportedBy")] ImportFile importFile)
        {
            if (ModelState.IsValid)
            {
                _context.Add(importFile);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ImportedFileId"] = new SelectList(_context.FileModel, "Id", "Id", importFile.ImportedFileId);
            ViewData["OriginalPublicFileId"] = new SelectList(_context.PublicFiles, "Id", "Author", importFile.OriginalPublicFileId);
            //ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", importFile.ProjectId);
            return View(importFile);
        }

        // GET: ImportFile/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ImportFile == null)
            {
                return NotFound();
            }

            var importFile = await _context.ImportFile.FindAsync(id);
            if (importFile == null)
            {
                return NotFound();
            }
            ViewData["ImportedFileId"] = new SelectList(_context.FileModel, "Id", "Id", importFile.ImportedFileId);
            ViewData["OriginalPublicFileId"] = new SelectList(_context.PublicFiles, "Id", "Author", importFile.OriginalPublicFileId);
            //ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", importFile.ProjectId);
            return View(importFile);
        }

        // POST: ImportFile/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProjectId,ImportedFileId,OriginalPublicFileId,ImportDate,ImportedBy")] ImportFile importFile)
        {
            if (id != importFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(importFile);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ImportFileExists(importFile.Id))
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
            ViewData["ImportedFileId"] = new SelectList(_context.FileModel, "Id", "Id", importFile.ImportedFileId);
            ViewData["OriginalPublicFileId"] = new SelectList(_context.PublicFiles, "Id", "Author", importFile.OriginalPublicFileId);
            //ViewData["ProjectId"] = new SelectList(_context.Project, "Id", "Description", importFile.ProjectId);
            return View(importFile);
        }

        // GET: ImportFile/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ImportFile == null)
            {
                return NotFound();
            }

            var importFile = await _context.ImportFile
                .Include(i => i.ImportedFile)
                .Include(i => i.OriginalPublicFile)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (importFile == null)
            {
                return NotFound();
            }

            return View(importFile);
        }

        // POST: ImportFile/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ImportFile == null)
            {
                return Problem("Entity set 'DataBaseContext.ImportFile'  is null.");
            }
            var importFile = await _context.ImportFile.FindAsync(id);
            if (importFile != null)
            {
                _context.ImportFile.Remove(importFile);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Import(int fileId)
        {
            var username = HttpContext.Session.GetString("Username");

            var publicFile = _context.FileModel.FirstOrDefault(pf => pf.Id == fileId);

            if (publicFile == null)
            {
                return NotFound();
            }

            var importedFile = new FileModel
            {
                Name = publicFile.Name,
                Content = publicFile.Content.ToArray(),
                Type = publicFile.Type,
                LastModified = publicFile.LastModified,
                ProjectId = publicFile.ProjectId,
                ModifiedBy = publicFile.ModifiedBy
            };

            _context.FileModel.Add(importedFile);
            await _context.SaveChangesAsync(); 

          
            var importRecord = new ImportFile
            {
                UserId = (int)HttpContext.Session.GetInt32("UserId"), 
                ImportDate = DateTime.UtcNow,
                ImportedFileId = importedFile.Id, 
                OriginalPublicFileId = publicFile.Id,
                ImportedBy = username
            };

            _context.ImportFile.Add(importRecord);


            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Files imported successfully";
            return RedirectToAction("Index", "PublicFiles");
        }

/*
        public async Task<IActionResult> ManageProjects(int fileId)
        {
            var file = await _context.ImportFile
                .Include(f => f.Project)
                .Include(f => f.ImportedFile)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return NotFound();
            }

            var allProjects = await _context.Project.ToListAsync();

            var viewModel = new FileProjectsViewModel
            {
                FileId = fileId,
                FileName = file.ImportedFile.Name,
                Projects = allProjects.Select(p => new ProjectSelection
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    IsSelected = p.ImportedFile.Any(imp => imp.ImportedFileId == file.ImportedFileId)
                }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> ManageProjects(FileProjectsViewModel model)
        {
            var file = await _context.ImportFile
                .Include(f => f.ImportedFile)
                .FirstOrDefaultAsync(f => f.Id == model.FileId);

            if (file == null)
            {
                return NotFound();
            }

            var allProjects = await _context.Project
                .Include(p => p.ImportedFile)
                .ToListAsync();

            foreach (var project in allProjects)
            {
                var selection = model.Projects.FirstOrDefault(p => p.ProjectId == project.Id);
                var isCurrentlyAssigned = project.ImportedFile.Any(imp => imp.ImportedFileId == file.ImportedFileId);

                if (selection != null)
                {
                    if (selection.IsSelected && !isCurrentlyAssigned)
                    {
                        project.ImportedFile.Add(new ImportFile
                        {
                            ImportedFileId = file.ImportedFileId,
                            ProjectId = project.Id,
                            ImportDate = DateTime.Now,
                            ImportedBy = User.Identity.Name
                        });
                    }
                    else if (!selection.IsSelected && isCurrentlyAssigned)
                    {
                        var importFileToRemove = project.ImportedFiles
                            .FirstOrDefault(imp => imp.ImportedFileId == file.ImportedFileId);
                        if (importFileToRemove != null)
                        {
                            project.ImportedFiles.Remove(importFileToRemove);
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/


        private bool ImportFileExists(int id)
        {
            return (_context.ImportFile?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
