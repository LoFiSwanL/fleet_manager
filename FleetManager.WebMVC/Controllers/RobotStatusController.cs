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

        public async Task<IActionResult> Index()
        {
            return View(await _context.RobotStatuses.Where(x => !x.IsDeleted).ToListAsync());
        }

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

        public IActionResult Create()
        {
            return View();
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.RobotStatuses.FindAsync(id);
            if (item != null)
            {
                item.IsDeleted = true;
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
