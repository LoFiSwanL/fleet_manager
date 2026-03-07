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
    public class RobotStatusController : Controller
    {
        private readonly FleetContext _context;

        public RobotStatusController(FleetContext context)
        {
            _context = context;
        }

        // GET: RobotStatus
        public async Task<IActionResult> Index()
        {
            return View(await _context.RobotStatuses.ToListAsync());
        }

        // GET: RobotStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var robotStatus = await _context.RobotStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (robotStatus == null)
            {
                return NotFound();
            }

            return View(robotStatus);
        }

        // GET: RobotStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RobotStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] RobotStatus robotStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(robotStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(robotStatus);
        }

        // GET: RobotStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var robotStatus = await _context.RobotStatuses.FindAsync(id);
            if (robotStatus == null)
            {
                return NotFound();
            }
            return View(robotStatus);
        }

        // POST: RobotStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] RobotStatus robotStatus)
        {
            if (id != robotStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(robotStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RobotStatusExists(robotStatus.Id))
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
            return View(robotStatus);
        }

        // GET: RobotStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var robotStatus = await _context.RobotStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (robotStatus == null)
            {
                return NotFound();
            }

            return View(robotStatus);
        }

        // POST: RobotStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var robotStatus = await _context.RobotStatuses.FindAsync(id);
            if (robotStatus != null)
            {
                _context.RobotStatuses.Remove(robotStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RobotStatusExists(int id)
        {
            return _context.RobotStatuses.Any(e => e.Id == id);
        }
    }
}
