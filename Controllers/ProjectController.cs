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
    public class ProjectController : Controller
    {
        private readonly DataBaseContext _context;

        public ProjectController(DataBaseContext context)
        {
            _context = context;
        }

        // GET: Project
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new ProjectIndexViewModel
            {
                OwnedProjects = await _context.Project
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.LastModified)
                    .ToListAsync(),

                CollaborationProjects = await _context.ProjectCollaborators
                    .Include(c => c.Project)
                    .ThenInclude(p => p.User)
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.Project.LastModified)
                    .ToListAsync()
            };

            //var projects = await _context.Project
            //    .Where(p => p.UserId == userId)
            //    .ToListAsync();

            return View(model);
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.Collaborators)
                .ThenInclude(p => p.User)
                .Include(p => p.Files)
                .ThenInclude(f => f.Share)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }
            HttpContext.Session.SetInt32("CurrProject", (int)id);

            var model = new ProjectViewModel
            {
                ProjectObj = project,
                Username = HttpContext.Session.GetString("Username")
            };

            ViewBag.UserRole = (int)CollaboratorRole.Admin;

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            ViewBag.isReadOnly = true;

            var collaborator = project.Collaborators.FirstOrDefault(c => c.UserId == currentUserId);

            if (collaborator == null) {
                return View(model);
            }
            else {
                ViewBag.UserRole = (int)collaborator.Role;
            }

            return View(model);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id");
            return View();
        }

        // POST: Project/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId");

                if (userId == null)
                {
                    TempData["ErrorMessage"] = "You need to be logged in";
                    return RedirectToAction("Login", "User");
                }

                var user = await _context.User.FindAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "user not exists";
                    return RedirectToAction("Login", "User");
                }

                var newProject = new Project
                {
                    Name = model.Name,
                    Description = model.Description,
                    UserId = userId,
                    User = user
                };

                _context.Add(newProject);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Project has been created";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errors = ModelState
                .Where(x => x.Value.Errors.Count > 0)
                .Select(x => new { x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                .ToList();

                Console.WriteLine("Validation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"{error.Key}: {string.Join(", ", error.Errors)}");
                }
            }
            // ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", project.UserId);
            return View(model);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", project.UserId);
            return View(project);
        }

        // POST: Project/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
        {
            var existingProject = await _context.Project.FindAsync(id);
            if (existingProject == null)
            {
                return NotFound();
            }

            // Tylko zaktualizuj odpowiednie właściwości
            existingProject.Name = project.Name;
            existingProject.Description = project.Description;
            existingProject.LastModified = DateTime.Now;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProjectExists(project.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.User, "Id", "Id", project.UserId);
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Project == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Project == null)
            {
                return Problem("Entity set 'DataBaseContext.Project'  is null.");
            }
            var project = await _context.Project.FindAsync(id);
            if (project != null)
            {
                _context.Project.Remove(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return (_context.Project?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> AddCollaborator(int projectId, string usernameOrEmail, string role)
        {
            //var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var project = await _context.Project
                .Include(p => p.User)
                .Include(p => p.Collaborators)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            if (project.UserId != currentUserId)
            {
                return Json(new { success = false, message = "Only project owner can add collaborators" });
            }

            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail);

            if (user == null)
            {
                return Json(new { success = false, message = "User not found" });
            }

            if (project.Collaborators.Any(c => c.UserId == user.Id))
            {
                return Json(new { success = false, message = "User is already a collaborator" });
            }


            CollaboratorRole newRole = CollaboratorRole.ReadOnly;

            switch (role)
            {
                case "1":
                    newRole = CollaboratorRole.Collaborator;
                    break;
                case "2":
                    newRole = CollaboratorRole.ReadOnly;
                    break;    
            }

            project.Collaborators.Add(new ProjectCollaborator
            {
                UserId = user.Id,
                User = user,
                Role = newRole,
                JoinDate = DateTime.UtcNow,
                Project = project,
                ProjectId = project.Id
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Collaborator added successfully" });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveCollaborator(int projectId, int userId)
        {
            //var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            var project = await _context.Project
                .Include(p => p.User)
                .Include(p => p.Collaborators)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
            {
                return Json(new { success = false, message = "Project not found" });
            }

            if (project.UserId != currentUserId)
            {
                return Json(new { success = false, message = "Only project owner can remove collaborators" });
            }

            var collaborator = project.Collaborators.FirstOrDefault(c => c.UserId == userId);
            if (collaborator == null)
            {
                return Json(new { success = false, message = "Collaborator not found" });
            }

            _context.ProjectCollaborators.Remove(collaborator);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Collaborator removed successfully" });
        }
    }
}
