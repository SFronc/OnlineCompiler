using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCompiler.Data;
using OnlineCompiler.Models;

namespace OnlineCompiler.Controllers
{
    public class UserController : Controller
    {
        private readonly DataBaseContext _context;

        public UserController(DataBaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HandleForm(string username, string password, string action)
        {
            if (action == "login")
            {
                return await Login(username, password);
            }
            else if (action == "register")
            {
                return await Register(username, password);
            }

            return BadRequest("Invalid action");
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == ComputeHash(password, MD5.Create()));

            if (user != null)
            {
                HttpContext.Session.SetString("IsLoggedIn", "true");
                HttpContext.Session.SetString("Username", username);
                HttpContext.Session.SetInt32("UserId", user.Id);

                if (user.Role == "Admin")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");
                }
                else HttpContext.Session.SetString("IsAdmin", "false");

                return RedirectToAction("Index");
            }

            ViewBag.ErrorMessage = "Invalid login or password";
            

            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Register(string username, string password)
        {
            if (_context.User.Any(u => u.Username == username))
            {
                ViewBag.ErrorMessage = "Username already exists!";
                return View("Create");
            }

            var hashedPassword = ComputeHash(password, MD5.Create());

            var newUser = new User
            {
                Username = username,
                PasswordHash = hashedPassword,
                Email = "placeholder",
                Role = "User"
            };

            _context.User.Add(newUser);
            _context.SaveChanges();

            //HttpContext.Session.SetString("IsLoggedIn", "true");
            //HttpContext.Session.SetString("Username", username);
            //HttpContext.Session.SetInt32("UserId", newUser.Id);
            return RedirectToAction("Create");

        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("IsLoggedIn");
            HttpContext.Session.Remove("Username");
            HttpContext.Session.Remove("UserId");
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            if(HttpContext.Session.GetString("IsLoggedIn") != "true"){
                return RedirectToAction(nameof(Login));
            }

            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;

            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);

            if (user != null)
            {
                ViewBag.ApiKey = user.ApiKey;
            }

            return View();
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details()
        {
            var users = _context.User.ToList();
            return View(users); 
        }

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,Email,PasswordHash,RegisterDate")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: User/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Email,PasswordHash,RegisterDate")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
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
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.User == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'DataBaseContext.User'  is null.");
            }
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return (_context.User?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        
        
        public static string ComputeHash(string input, HashAlgorithm hasher)
        {
            Encoding enc = Encoding.UTF8;
            var hashBuilder = new StringBuilder();
            byte[] result = hasher.ComputeHash(enc.GetBytes(input));
            foreach (var b in result)
                hashBuilder.Append(b.ToString("x2"));
            return hashBuilder.ToString();
        }
    }
}
