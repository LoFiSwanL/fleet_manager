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
    public class FirmwaresController : Controller
    {
        private readonly FleetContext _context;

        public FirmwaresController(FleetContext context)
        {
            _context = context;
        }

        // GET: Firmwares
        public async Task<IActionResult> Index()
        {
            return View(await _context.Firmwares.ToListAsync());
        }

        // GET: Firmwares/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmware = await _context.Firmwares
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firmware == null)
            {
                return NotFound();
            }

            return View(firmware);
        }

        // GET: Firmwares/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Firmwares/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Version,ReleaseDate,Id")] Firmware firmware)
        {
            if (ModelState.IsValid)
            {
                _context.Add(firmware);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(firmware);
        }

        // GET: Firmwares/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmware = await _context.Firmwares.FindAsync(id);
            if (firmware == null)
            {
                return NotFound();
            }
            return View(firmware);
        }

        // POST: Firmwares/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Version,ReleaseDate,Id")] Firmware firmware)
        {
            if (id != firmware.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(firmware);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FirmwareExists(firmware.Id))
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
            return View(firmware);
        }

        // GET: Firmwares/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var firmware = await _context.Firmwares
                .FirstOrDefaultAsync(m => m.Id == id);
            if (firmware == null)
            {
                return NotFound();
            }

            return View(firmware);
        }

        // POST: Firmwares/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var firmware = await _context.Firmwares.FindAsync(id);
            if (firmware != null)
            {
                _context.Firmwares.Remove(firmware);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FirmwareExists(int id)
        {
            return _context.Firmwares.Any(e => e.Id == id);
        }
    }
}
