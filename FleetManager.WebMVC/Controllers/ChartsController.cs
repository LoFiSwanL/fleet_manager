using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FleetManager.Domain.Models;
using FleetManager.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FleetManager.WebMVC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartsController : ControllerBase
    {
        private readonly FleetContext _context;

        public ChartsController(FleetContext context)
        {
            _context = context;
        }

        private record SeverityCountResponse(string SeverityName, int Count);

        [HttpGet("logsBySeverity")]
        public async Task<JsonResult> GetLogsBySeverity(CancellationToken cancellationToken)
        {
            var data = await _context.HardwareLogs
                .Include(l => l.Severity)
                .GroupBy(l => l.Severity.Name)
                .Select(g => new SeverityCountResponse(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            return new JsonResult(data);
        }

        private record RobotLogCountResponse(string RobotName, int Count);

        [HttpGet("logsByRobot")]
        public async Task<JsonResult> GetLogsByRobot(CancellationToken cancellationToken)
        {
            var data = await _context.HardwareLogs
                .Include(l => l.Robot)
                .GroupBy(l => l.Robot.Name)
                .Select(g => new RobotLogCountResponse(g.Key, g.Count()))
                .ToListAsync(cancellationToken);

            return new JsonResult(data);
        }
    }
}