using ClosedXML.Excel;
using FleetManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FleetManager.Infrastructure.Services
{
    public class RobotImportService : IImportService<Robot>
    {
        private readonly FleetContext _context;

        private static readonly IReadOnlyList<string> ExpectedHeaders = new string[]
        {
            "Name", "Serial Number", "IP Address", "Status Name", "Firmware Version", "Policy Name"
        };

        public RobotImportService(FleetContext context)
        {
            _context = context;
        }

        public async Task<ImportResult> ImportFromStreamAsync(Stream stream, bool updateExisting, CancellationToken cancellationToken)
        {
            var result = new ImportResult();

            if (!stream.CanRead)
            {
                result.Errors.Add("Помилка доступу: файл неможливо прочитати.");
                return result;
            }

            using (XLWorkbook workBook = new XLWorkbook(stream))
            {
                var worksheet = workBook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    result.Errors.Add("Файл порожній: не знайдено жодного аркуша.");
                    return result;
                }

                // 1. ВАЛІДАЦІЯ ЗАГОЛОВКІВ (зупиняємось на першій же помилці)
                var headerRow = worksheet.Row(1);
                for (int i = 0; i < ExpectedHeaders.Count; i++)
                {
                    var cellValue = headerRow.Cell(i + 1).Value.ToString().Trim();
                    if (!string.Equals(cellValue, ExpectedHeaders[i], StringComparison.OrdinalIgnoreCase))
                    {
                        result.Errors.Add($"Імпорт скасовано. Помилка структури: на позиції {i + 1} очікувався стовпець '{ExpectedHeaders[i]}', а знайдено '{cellValue}'.");
                        break; // ЗУПИНЯЄМО ЦИКЛ! Більше спаму не буде
                    }
                }

                // Якщо знайшли помилку в заголовках - одразу виходимо
                if (!result.Success) return result;

                // 2. ЧИТАННЯ РЯДКІВ
                int rowIndex = 2;
                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    bool rowSuccess = await AddRobotAsync(row, updateExisting, result, rowIndex, cancellationToken);

                    // Якщо під час читання рядка виникла помилка - зупиняємо весь імпорт!
                    if (!result.Success)
                    {
                        break;
                    }

                    if (rowSuccess)
                    {
                        result.ImportedCount++;
                    }
                    rowIndex++;
                }
            }

            // 3. ЗБЕРЕЖЕННЯ В БАЗУ (ТІЛЬКИ ЯКЩО НЕМАЄ ЖОДНОЇ ПОМИЛКИ)
            if (result.Success && result.ImportedCount > 0)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        private async Task<bool> AddRobotAsync(IXLRow row, bool updateExisting, ImportResult result, int rowIndex, CancellationToken cancellationToken)
        {
            string name = row.Cell(1).Value.ToString().Trim();
            string serialNumber = row.Cell(2).Value.ToString().Trim();
            string ipAddress = row.Cell(3).Value.ToString().Trim();
            string statusName = row.Cell(4).Value.ToString().Trim();
            string firmwareVersion = row.Cell(5).Value.ToString().Trim();
            string policyName = row.Cell(6).Value.ToString().Trim();

            // Якщо рядок повністю порожній - просто пропускаємо
            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(serialNumber)) return false;

            // --- КОРОТКА ВАЛІДАЦІЯ РЯДКА ---
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(serialNumber))
            {
                result.Errors.Add($"Імпорт скасовано. У рядку {rowIndex} пропущено обов'язкове поле (Name або Serial Number).");
                return false;
            }

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

            var robot = await _context.Robots.FirstOrDefaultAsync(r => r.SerialNumber == serialNumber, cancellationToken);

            if (robot != null)
            {
                if (!updateExisting) return false;
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

            return true;
        }
    }
}