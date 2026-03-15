using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetManager.Domain.Models;
using FleetManager.Infrastructure;
using System.Security.Cryptography;
using System.Text;

namespace FleetManager.WebMVC.Controllers
{
    public class UsersController : Controller
    {
        private readonly FleetContext _context;

        public UsersController(FleetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var fleetContext = _context.Users.Include(u => u.Role);
            return View(await fleetContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Create()
        {
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Username,PasswordHash,FullName,IsActive,RoleId")] User user)
        {
            ModelState.Remove("Role");

            if (!string.IsNullOrEmpty(user.Username) && user.Username.Trim().ToLower() == "admin")
            {
                var adminExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == "admin");
                if (adminExists)
                {
                    ModelState.AddModelError("Username", "[ ПОМИЛКА ] Обліковий запис суперадміністратора вже існує!");
                }
            }

            if (ModelState.IsValid)
            {
                user.PasswordHash = HashPassword(user.PasswordHash);

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", user.RoleId);
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", user.RoleId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,PasswordHash,FullName,IsActive,RoleId")] User user)
        {
            if (id != user.Id) return NotFound();

            ModelState.Remove("Role");

            // Захист від створення другого "admin"
            if (!string.IsNullOrEmpty(user.Username) && user.Username.Trim().ToLower() == "admin")
            {
                var adminExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == "admin" && u.Id != user.Id);
                if (adminExists)
                {
                    ModelState.AddModelError("Username", "[ ПОМИЛКА ] Обліковий запис суперадміністратора вже існує!");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
                    if (userToUpdate == null) return NotFound();

                    userToUpdate.Username = user.Username;

                    if (!string.IsNullOrEmpty(user.PasswordHash) && user.PasswordHash.Length < 50)
                    {
                        userToUpdate.PasswordHash = HashPassword(user.PasswordHash);
                    }

                    userToUpdate.FullName = user.FullName;
                    userToUpdate.IsActive = user.IsActive;
                    userToUpdate.RoleId = user.RoleId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", user.RoleId);
            return View(user);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (user == null) return NotFound();

            if (user.Username.ToLower() == "admin")
            {
                TempData["ErrorMessage"] = "[ СИСТЕМНА ПОМИЛКА ] Неможливо видалити кореневого адміністратора.";
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user != null)
            {
                if (user.Username.ToLower() == "admin")
                {
                    TempData["ErrorMessage"] = "[ КРИТИЧНА ПОМИЛКА ] Спроба несанкціонованого видалення суперадміна заблокована.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return password;

            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }
    }
}
