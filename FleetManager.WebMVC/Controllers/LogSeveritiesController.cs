using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FleetManager.Domain.Models;
using FleetManager.Infrastructure;

namespace FleetManager.WebMVC.Controllers
{
    public class LogSeveritiesController : Controller
    {
        private readonly FleetContext _context;

        public LogSeveritiesController(FleetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.LogSeverities.Where(x => !x.IsDeleted).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logSeverity = await _context.LogSeverities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logSeverity == null)
            {
                return NotFound();
            }

            return View(logSeverity);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] LogSeverity logSeverity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(logSeverity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(logSeverity);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logSeverity = await _context.LogSeverities.FindAsync(id);
            if (logSeverity == null)
            {
                return NotFound();
            }
            return View(logSeverity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] LogSeverity logSeverity)
        {
            if (id != logSeverity.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(logSeverity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogSeverityExists(logSeverity.Id))
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
            return View(logSeverity);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var logSeverity = await _context.LogSeverities
                .FirstOrDefaultAsync(m => m.Id == id);
            if (logSeverity == null)
            {
                return NotFound();
            }

            return View(logSeverity);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.LogSeverities.FindAsync(id);
            if (item != null)
            {
                item.IsDeleted = true;
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LogSeverityExists(int id)
        {
            return _context.LogSeverities.Any(e => e.Id == id);
        }
    }
}
