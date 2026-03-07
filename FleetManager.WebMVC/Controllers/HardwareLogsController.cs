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

        // GET: HardwareLogs
        public async Task<IActionResult> Index()
        {
            var fleetContext = _context.HardwareLogs
                .Include(h => h.Robot)
                .Include(h => h.Severity)
                .Include(h => h.User)
                .AsNoTracking(); // Додано для швидкодії
            return View(await fleetContext.ToListAsync());
        }

        // GET: HardwareLogs/Details/5
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

        // GET: HardwareLogs/Create
        public IActionResult Create()
        {
            ViewData["RobotId"] = new SelectList(_context.Robots, "Id", "Name");
            ViewData["SeverityId"] = new SelectList(_context.LogSeverities, "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username");
            return View();
        }

        // POST: HardwareLogs/Create
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", hardwareLog.UserId);
            return View(hardwareLog);
        }

        // GET: HardwareLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hardwareLog = await _context.HardwareLogs.FindAsync(id);
            if (hardwareLog == null) return NotFound();

            ViewData["RobotId"] = new SelectList(_context.Robots, "Id", "Name", hardwareLog.RobotId);
            ViewData["SeverityId"] = new SelectList(_context.LogSeverities, "Id", "Name", hardwareLog.SeverityId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", hardwareLog.UserId);
            return View(hardwareLog);
        }

        // POST: HardwareLogs/Edit/5
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Username", hardwareLog.UserId);
            return View(hardwareLog);
        }

        // GET: HardwareLogs/Delete/5
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

        // POST: HardwareLogs/Delete/5
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

        // POST: HardwareLogs/SimulateTelemetry
        [HttpPost]
        public async Task<IActionResult> SimulateTelemetry()
        {
            // Знаходимо всіх роботів і статуси
            var robots = await _context.Robots.ToListAsync();
            var severities = await _context.LogSeverities.ToListAsync();

            if (!robots.Any() || !severities.Any()) return BadRequest("Немає роботів або статусів для симуляції");

            var random = new Random();
            var mockMessages = new[] {
                "Temperature spike in joint 3",
                "Vision system latency > 50ms",
                "Calibration required",
                "Network signal weak",
                "Battery level at 15%",
                "Object successfully grasped"
            };

            // Генеруємо від 1 до 3 випадкових логів
            int logsToGenerate = random.Next(1, 4);
            for (int i = 0; i < logsToGenerate; i++)
            {
                var newLog = new HardwareLog
                {
                    RobotId = robots[random.Next(robots.Count)].Id,
                    SeverityId = severities[random.Next(severities.Count)].Id,
                    Message = mockMessages[random.Next(mockMessages.Length)]
                };
                _context.Add(newLog);
            }

            await _context.SaveChangesAsync();
            return Ok(); 
        }
    } 
}