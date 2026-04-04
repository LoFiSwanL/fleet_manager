using ClosedXML.Excel;
using FleetManager.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FleetManager.Infrastructure.Services
{
    public class RobotExportService : IExportService<Robot>
    {
        private readonly FleetContext _context;

        private static readonly IReadOnlyList<string> HeaderNames = new string[]
        {
            "Name",
            "Serial Number",
            "IP Address",
            "Status Name",
            "Firmware Version",
            "Policy Name"
        };

        public RobotExportService(FleetContext context)
        {
            _context = context;
        }

        public async Task WriteToAsync(Stream stream, CancellationToken cancellationToken)
        {
            if (!stream.CanWrite)
            {
                throw new ArgumentException("Input stream is not writable");
            }

            var robots = await _context.Robots
                .Include(r => r.Status)
                .Include(r => r.Firmware)
                .Include(r => r.Policy)
                .ToListAsync(cancellationToken);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Robots");

            for (int i = 0; i < HeaderNames.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = HeaderNames[i];
            }
            worksheet.Row(1).Style.Font.Bold = true;

            int rowIndex = 2;
            foreach (var robot in robots)
            {
                worksheet.Cell(rowIndex, 1).Value = robot.Name;
                worksheet.Cell(rowIndex, 2).Value = robot.SerialNumber;
                worksheet.Cell(rowIndex, 3).Value = robot.IpAddress;
                worksheet.Cell(rowIndex, 4).Value = robot.Status?.Name ?? "";
                worksheet.Cell(rowIndex, 5).Value = robot.Firmware?.Version ?? "";
                worksheet.Cell(rowIndex, 6).Value = robot.Policy?.Name ?? "";

                rowIndex++;
            }

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(stream);
        }
    }
}