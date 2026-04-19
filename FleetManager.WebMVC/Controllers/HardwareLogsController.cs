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
    public class HardwareLogsController : Controller
    {
        private readonly FleetContext _context;

        public HardwareLogsController(FleetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var fleetContext = _context.HardwareLogs
                .Include(h => h.Robot)
                .Include(h => h.Severity)
                .OrderByDescending(h => h.CreatedAt);

            return View(await fleetContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hardwareLog = await _context.HardwareLogs
                .Include(h => h.Robot)
                .Include(h => h.Severity)
                .Include(h => h.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hardwareLog == null) return NotFound();

            return View(hardwareLog);
        }

        public IActionResult Create()
        {
            ViewData["RobotId"] = new SelectList(_context.Robots, "Id", "Name");
            ViewData["SeverityId"] = new SelectList(_context.LogSeverities.Where(s => s.Name.ToLower() != "smth"), "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RobotId,UserId,SeverityId,Message")] HardwareLog hardwareLog)
        {
            ModelState.Remove("Robot");
            ModelState.Remove("Severity");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Add(hardwareLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["RobotId"] = new SelectList(_context.Robots, "Id", "Name", hardwareLog.RobotId);
            ViewData["SeverityId"] = new SelectList(_context.LogSeverities, "Id", "Name", hardwareLog.SeverityId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", hardwareLog.UserId);
            return View(hardwareLog);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hardwareLog = await _context.HardwareLogs.FindAsync(id);
            if (hardwareLog == null) return NotFound();

            ViewData["RobotId"] = new SelectList(_context.Robots, "Id", "Name", hardwareLog.RobotId);
            ViewData["SeverityId"] = new SelectList(_context.LogSeverities, "Id", "Name", hardwareLog.SeverityId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", hardwareLog.UserId);
            return View(hardwareLog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RobotId,UserId,SeverityId,Message")] HardwareLog hardwareLog)
        {
            if (id != hardwareLog.Id) return NotFound();

            ModelState.Remove("Robot");
            ModelState.Remove("Severity");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                try
                {
                    var logToUpdate = await _context.HardwareLogs.FirstOrDefaultAsync(h => h.Id == id);
                    if (logToUpdate == null) return NotFound();

                    logToUpdate.RobotId = hardwareLog.RobotId;
                    logToUpdate.UserId = hardwareLog.UserId;
                    logToUpdate.SeverityId = hardwareLog.SeverityId;
                    logToUpdate.Message = hardwareLog.Message;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HardwareLogExists(hardwareLog.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["RobotId"] = new SelectList(_context.Robots, "Id", "Name", hardwareLog.RobotId);
            ViewData["SeverityId"] = new SelectList(_context.LogSeverities, "Id", "Name", hardwareLog.SeverityId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName", hardwareLog.UserId);
            return View(hardwareLog);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hardwareLog = await _context.HardwareLogs
                .Include(h => h.Robot)
                .Include(h => h.Severity)
                .Include(h => h.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (hardwareLog == null) return NotFound();

            return View(hardwareLog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hardwareLog = await _context.HardwareLogs.FindAsync(id);
            if (hardwareLog != null)
            {
                _context.HardwareLogs.Remove(hardwareLog);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HardwareLogExists(int id)
        {
            return _context.HardwareLogs.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentLogs()
        {
            var logsFromDb = await _context.HardwareLogs
                .Include(h => h.Robot)
                .OrderByDescending(l => l.CreatedAt)
                .Take(8)
                .ToListAsync();

            var jsonLogs = logsFromDb.Select(l => new {
                time = l.CreatedAt.ToLocalTime().ToString("HH:mm"),
                robot = l.Robot != null ? l.Robot.Name : "Unknown",
                message = l.Message
            });

            return Json(jsonLogs);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            var allLogs = await _context.HardwareLogs.ToListAsync();

            _context.HardwareLogs.RemoveRange(allLogs);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    } 
}