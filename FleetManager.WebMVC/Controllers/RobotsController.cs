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
    public class RobotsController : Controller
    {
        private readonly FleetContext _context;

        public RobotsController(FleetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var fleetContext = _context.Robots
                .Include(r => r.Firmware)
                .Include(r => r.Policy)
                .Include(r => r.Status)
                .AsNoTracking();
            return View(await fleetContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var robot = await _context.Robots
                .Include(r => r.Firmware)
                .Include(r => r.Policy)
                .Include(r => r.Status)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (robot == null) return NotFound();

            return View(robot);
        }

        public IActionResult Create()
        {
            ViewData["FirmwareId"] = new SelectList(_context.Firmwares, "Id", "Version");
            ViewData["PolicyId"] = new SelectList(_context.Policies, "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,SerialNumber,StatusId,IpAddress,PolicyId,FirmwareId")] Robot robot)
        {
            ModelState.Remove("Firmware");
            ModelState.Remove("Policy");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                _context.Add(robot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["FirmwareId"] = new SelectList(_context.Firmwares, "Id", "Version", robot.FirmwareId);
            ViewData["PolicyId"] = new SelectList(_context.Policies, "Id", "Name", robot.PolicyId);
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses, "Id", "Name", robot.StatusId);
            return View(robot);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var robot = await _context.Robots.FindAsync(id);
            if (robot == null) return NotFound();

            ViewData["FirmwareId"] = new SelectList(_context.Firmwares, "Id", "Version", robot.FirmwareId);
            ViewData["PolicyId"] = new SelectList(_context.Policies, "Id", "Name", robot.PolicyId);
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses, "Id", "Name", robot.StatusId);
            return View(robot);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SerialNumber,StatusId,IpAddress,PolicyId,FirmwareId")] Robot robot)
        {
            if (id != robot.Id) return NotFound();

            ModelState.Remove("Firmware");
            ModelState.Remove("Policy");
            ModelState.Remove("Status");

            if (ModelState.IsValid)
            {
                try
                {
                    var robotToUpdate = await _context.Robots.FirstOrDefaultAsync(r => r.Id == id);
                    if (robotToUpdate == null) return NotFound();

                    robotToUpdate.Name = robot.Name;
                    robotToUpdate.SerialNumber = robot.SerialNumber;
                    robotToUpdate.StatusId = robot.StatusId;
                    robotToUpdate.IpAddress = robot.IpAddress;
                    robotToUpdate.PolicyId = robot.PolicyId;
                    robotToUpdate.FirmwareId = robot.FirmwareId;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RobotExists(robot.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["FirmwareId"] = new SelectList(_context.Firmwares, "Id", "Version", robot.FirmwareId);
            ViewData["PolicyId"] = new SelectList(_context.Policies, "Id", "Name", robot.PolicyId);
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses, "Id", "Name", robot.StatusId);
            return View(robot);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var robot = await _context.Robots
                .Include(r => r.Firmware)
                .Include(r => r.Policy)
                .Include(r => r.Status)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (robot == null) return NotFound();

            return View(robot);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var robot = await _context.Robots.FindAsync(id);
            if (robot != null)
            {
                _context.Robots.Remove(robot);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool RobotExists(int id)
        {
            return _context.Robots.Any(e => e.Id == id);
        }
    }
}