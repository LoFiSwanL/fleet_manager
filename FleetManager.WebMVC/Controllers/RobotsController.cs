using ClosedXML.Excel;
using FleetManager.Domain.Models;
using FleetManager.Infrastructure;
using FleetManager.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FleetManager.WebMVC.Controllers
{
    public class RobotsController : Controller
    {
        private readonly FleetContext _context;
        private readonly IDataPortServiceFactory<Robot> _dataPortServiceFactory;

        public RobotsController(FleetContext context, IDataPortServiceFactory<Robot> dataPortServiceFactory)
        {
            _context = context;
            _dataPortServiceFactory = dataPortServiceFactory;
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
            ViewData["FirmwareId"] = new SelectList(_context.Firmwares.Where(f => !f.IsDeleted), "Id", "Version");
            ViewData["PolicyId"] = new SelectList(_context.Policies.Where(p => !p.IsDeleted), "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses.Where(s => !s.IsDeleted), "Id", "Name");

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

            ViewData["FirmwareId"] = new SelectList(_context.Firmwares.Where(f => !f.IsDeleted), "Id", "Version");
            ViewData["PolicyId"] = new SelectList(_context.Policies.Where(p => !p.IsDeleted), "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses.Where(s => !s.IsDeleted), "Id", "Name");
            return View(robot);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var robot = await _context.Robots.FindAsync(id);
            if (robot == null) return NotFound();

            ViewData["FirmwareId"] = new SelectList(_context.Firmwares.Where(f => !f.IsDeleted), "Id", "Version");
            ViewData["PolicyId"] = new SelectList(_context.Policies.Where(p => !p.IsDeleted), "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses.Where(s => !s.IsDeleted), "Id", "Name");
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

            ViewData["FirmwareId"] = new SelectList(_context.Firmwares.Where(f => !f.IsDeleted), "Id", "Version");
            ViewData["PolicyId"] = new SelectList(_context.Policies.Where(p => !p.IsDeleted), "Id", "Name");
            ViewData["StatusId"] = new SelectList(_context.RobotStatuses.Where(s => !s.IsDeleted), "Id", "Name");
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


        [HttpGet]
        public async Task<IActionResult> Export([FromQuery] string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            CancellationToken cancellationToken = default)
        {
            var exportService = _dataPortServiceFactory.GetExportService(contentType);
            var memoryStream = new MemoryStream();

            await exportService.WriteToAsync(memoryStream, cancellationToken);
            memoryStream.Flush();
            memoryStream.Position = 0;

            return new FileStreamResult(memoryStream, contentType)
            {
                FileDownloadName = $"robots_{DateTime.UtcNow.ToShortDateString()}.xlsx"
            };
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile fileExcel, string duplicateHandling, CancellationToken cancellationToken = default)
        {
            if (fileExcel == null || fileExcel.Length == 0)
            {
                ModelState.AddModelError("", "Будь ласка, оберіть файл для завантаження.");
                return View();
            }

            bool updateExisting = (duplicateHandling == "overwrite");

            try
            {
                var importService = _dataPortServiceFactory.GetImportService(fileExcel.ContentType);
                using var stream = fileExcel.OpenReadStream();

                var result = await importService.ImportFromStreamAsync(stream, updateExisting, cancellationToken);

                if (!result.Success)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }

                    if (result.ImportedCount > 0)
                    {
                        TempData["SuccessMessage"] = $"Увага: Частковий імпорт. Збережено {result.ImportedCount} записів, але виявлено помилки.";
                    }

                    return View(); 
                }

                TempData["SuccessMessage"] = $"Успіх! Оброблено {result.ImportedCount} записів.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Системна помилка при читанні файлу: {ex.Message}");
                return View();
            }
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Template");

            var headers = new string[]
            {
        "Name", "Serial Number", "IP Address", "Status Name", "Firmware Version", "Policy Name"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            worksheet.Row(1).Style.Font.Bold = true;
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(
                content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "robots_template.xlsx"
            );
        }
    }
}