using ClosedXML.Excel;
using FleetManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FleetManager.Infrastructure.Services
{
    public class RobotImportService : IImportService<Robot>
    {
        private readonly FleetContext _context;

        public RobotImportService(FleetContext context)
        {
            _context = context;
        }

        public async Task ImportFromStreamAsync(Stream stream, bool updateExisting, CancellationToken cancellationToken)
        {
            if (!stream.CanRead)
            {
                throw new ArgumentException("Дані не можуть бути прочитані", nameof(stream));
            }

            using (XLWorkbook workBook = new XLWorkbook(stream))
            {
                var worksheet = workBook.Worksheets.FirstOrDefault();
                if (worksheet == null) return;

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    await AddRobotAsync(row, updateExisting, cancellationToken);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task AddRobotAsync(IXLRow row, bool updateExisting, CancellationToken cancellationToken)
        {
            string name = row.Cell(1).Value.ToString();
            string serialNumber = row.Cell(2).Value.ToString();
            string ipAddress = row.Cell(3).Value.ToString();
            string statusName = row.Cell(4).Value.ToString();
            string firmwareVersion = row.Cell(5).Value.ToString();
            string policyName = row.Cell(6).Value.ToString();

            if (string.IsNullOrWhiteSpace(serialNumber)) return;

            RobotStatus status = null;
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                status = await _context.RobotStatuses.FirstOrDefaultAsync(s => s.Name == statusName && !s.IsDeleted, cancellationToken)
                         ?? _context.RobotStatuses.Local.FirstOrDefault(s => s.Name == statusName);

                if (status == null)
                {
                    status = new RobotStatus { Name = statusName };
                    _context.RobotStatuses.Add(status);
                }
            }

            Firmware firmware = null;
            if (!string.IsNullOrWhiteSpace(firmwareVersion))
            {
                firmware = await _context.Firmwares.FirstOrDefaultAsync(f => f.Version == firmwareVersion && !f.IsDeleted, cancellationToken)
                           ?? _context.Firmwares.Local.FirstOrDefault(f => f.Version == firmwareVersion);

                if (firmware == null)
                {
                    firmware = new Firmware { Version = firmwareVersion, ReleaseDate = DateTime.UtcNow };
                    _context.Firmwares.Add(firmware);
                }
            }

            Policy policy = null;
            if (!string.IsNullOrWhiteSpace(policyName))
            {
                policy = await _context.Policies.FirstOrDefaultAsync(p => p.Name == policyName && !p.IsDeleted, cancellationToken)
                         ?? _context.Policies.Local.FirstOrDefault(p => p.Name == policyName);

                if (policy == null)
                {
                    policy = new Policy { Name = policyName, Description = "Imported from Excel" };
                    _context.Policies.Add(policy);
                }
            }

            var robot = await _context.Robots
        .FirstOrDefaultAsync(r => r.SerialNumber == serialNumber, cancellationToken);

            if (robot != null)
            {
                if (!updateExisting)
                {
                    return;
                }
            }
            else
            {
                robot = new Robot { SerialNumber = serialNumber, CreatedAt = DateTime.UtcNow };
                _context.Robots.Add(robot);
            }

            robot.Name = name;
            robot.IpAddress = ipAddress;
            robot.Status = status;
            robot.Firmware = firmware;
            robot.Policy = policy;
            robot.UpdatedAt = DateTime.UtcNow;
        }
    }
}