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

        // GET: LogSeverities
        public async Task<IActionResult> Index()
        {
            return View(await _context.LogSeverities.ToListAsync());
        }

        // GET: LogSeverities/Details/5
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

        // GET: LogSeverities/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LogSeverities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: LogSeverities/Edit/5
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

        // POST: LogSeverities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: LogSeverities/Delete/5
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

        // POST: LogSeverities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var logSeverity = await _context.LogSeverities.FindAsync(id);
            if (logSeverity != null)
            {
                _context.LogSeverities.Remove(logSeverity);
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
