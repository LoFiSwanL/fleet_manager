using FleetManager.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace FleetManager.WebMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly FleetContext _context;

        public HomeController(FleetContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Robots = await _context.Robots.Include(r => r.Status).Take(10).ToListAsync();
            ViewBag.Logs = await _context.HardwareLogs.Include(h => h.Robot).OrderByDescending(l => l.CreatedAt).Take(8).ToListAsync();
            ViewBag.Users = await _context.Users.Take(8).ToListAsync();

            return View();
        }
    }
}