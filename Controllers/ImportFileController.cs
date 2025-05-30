using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var dataBaseContext = _context.ImportFile.Include(i => i.ImportedFile)
                .Include(i => i.OriginalPublicFile)
                .ThenInclude(i => i.Share)
                .ThenInclude(i => i.Versions);
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
        public async Task<IActionResult> Delete(int? fileId)
        {
            if (fileId == null || _context.ImportFile == null)
            {
                return NotFound();
            }

            var importFile = await _context.ImportFile
                .Include(i => i.ImportedFile)
                .Include(i => i.OriginalPublicFile)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == fileId);
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
            Console.WriteLine($"=========================DELETINGFILE ID:{id}=================");
            
            if (_context.ImportFile == null)
            {
                return Problem("Entity set 'DataBaseContext.ImportFile'  is null.");
            }
            var importFile = await _context.ImportFile.Include(f => f.ImportedFile).FirstOrDefaultAsync( f => f.Id == id);
            var importFileModel = importFile.ImportedFile;


            if (importFileModel != null)
            {
                _context.FileModel.Remove(importFileModel);
            }

            _context.ImportFile.Remove(importFile);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Import(int fileId, int versionIndex)
        {
            try
            {
                var publicFile = await _context.PublicFiles
                    .Include(pf => pf.Versions)
                    .Include(pf => pf.AuthorOriginalFile)
                    .FirstOrDefaultAsync(pf => pf.PublicFileId == fileId);

                if (publicFile == null)
                {
                    return NotFound($"PublicFile not found.  fileId: {fileId}, versionindex: {versionIndex}");
                }

                if (versionIndex < 0 || versionIndex >= publicFile.Versions.Count)
                {
                    return NotFound("Invalid version selected");
                }

                var selectedVersion = publicFile.Versions[versionIndex];

                Console.WriteLine($"===========SELECTED VERSION {selectedVersion.Version}============");

                var newFile = new FileModel
                {
                    Name = publicFile.AuthorOriginalFile.Name,
                    Content = selectedVersion.Content.ToArray(),
                    Type = publicFile.AuthorOriginalFile.Type,
                    LastModified = selectedVersion.Version,
                    ModifiedBy = publicFile.AuthorOriginalFile.ModifiedBy,
                    IsShared = false

                };

                _context.FileModel.Add(newFile);

                await _context.SaveChangesAsync();


                var importRecord = new ImportFile
                {
                    UserId = (int)HttpContext.Session.GetInt32("UserId"),
                    ImportDate = DateTime.UtcNow,
                    ImportedFileId = newFile.Id,
                    ImportedFile = newFile,
                    OriginalPublicFileId = publicFile.AuthorOriginalFileId,
                    OriginalPublicFile = publicFile.AuthorOriginalFile
                };

                _context.ImportFile.Add(importRecord);


                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Files imported successfully";


            }
            catch (Exception ex)
            { }
            
            return RedirectToAction("Index", "PublicFiles");
            
        }

        [HttpPost]
        public async Task<IActionResult> Update(int fileId)
        {
            Console.WriteLine($"========Update fileId:{fileId}===========");
            var importFile = await _context.ImportFile
                .Include(f => f.ImportedFile)
                .Include(f => f.OriginalPublicFile)
                .ThenInclude(f => f.Share)
                .ThenInclude(f => f.Versions)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            

            if (importFile == null)
            {
                return NotFound("import File is null");
            }
            if (importFile.OriginalPublicFile == null)
            {
                return NotFound("OriginalPublicFile is null");
            }
            if (importFile.OriginalPublicFile.Share == null)
            {
                return NotFound("Share is null");
            }
            if (importFile.OriginalPublicFile.Share.Versions == null)
            {
                return NotFound("Versions is null");
            }
            var latestVersion = importFile.OriginalPublicFile.Share.Versions[importFile.OriginalPublicFile.Share.Versions.Count - 1];

            //importFile.ImportDate = latestVersion.Version;
            importFile.ImportDate = latestVersion.Version;
            importFile.ImportedFile.Content = latestVersion.Content.ToArray();

            //var importFile = await _context.ImportFile.FirstOrDefaultAsync(f => f.Id == fileId);
            //var test = importFile == null;
            //Console.WriteLine($"================={test}==================");

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageProjects(int fileId)
        {
            var file = await _context.ImportFile
                .Include(f => f.ImportedFile)
                .Include(f => f.ProjectLibraries)
                .FirstOrDefaultAsync(f => f.Id == fileId);

            if (file == null)
            {
                return NotFound();
            }

            if (file.ImportedFile == null)
            {
                return NotFound("Imported file reference is missing");
            }

            var availableProjects = await _context.Project
                .Where(p => p.UserId == HttpContext.Session.GetInt32("UserId")) 
                .ToListAsync();

            var model = new ManageProjectsViewModel
            {
                FileId = fileId,
                FileName = file.ImportedFile.Name,
                Projects = availableProjects.Select(p => new ProjectSelectionViewModel
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    IsSelected = file.ProjectLibraries.Any(u => u.ProjectId == p.Id && u.IsActive)
                }).ToList()
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ManageProjects(ManageProjectsViewModel model)
        {
            var file = await _context.ImportFile
                .Include(f => f.ProjectLibraries)
                .FirstOrDefaultAsync(f => f.Id == model.FileId);

            if (file == null)
            {
                return NotFound();
            }

            foreach (var project in model.Projects)
            {
                var existingUsage = file.ProjectLibraries
                    .FirstOrDefault(u => u.ProjectId == project.ProjectId);

                if (project.IsSelected && existingUsage == null)
                {
                    _context.Librarie.Add(new Library
                    {
                        ProjectId = project.ProjectId,
                        ImportedFileId = model.FileId,
                        IsActive = true
                    });
                }
                else if (!project.IsSelected && existingUsage != null)
                {
                    existingUsage.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = "Project assignments updated successfully";
            return RedirectToAction("Index"); 
        }


        private bool ImportFileExists(int id)
        {
            return (_context.ImportFile?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
