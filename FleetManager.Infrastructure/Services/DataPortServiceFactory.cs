using FleetManager.Domain.Models;
using System;

namespace FleetManager.Infrastructure.Services
{
    public class DataPortServiceFactory : IDataPortServiceFactory<Robot>
    {
        private readonly FleetContext _context;

        public DataPortServiceFactory(FleetContext context)
        {
            _context = context;
        }

        public IImportService<Robot> GetImportService(string contentType)
        {
            if (contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return new RobotImportService(_context);
            }
            throw new NotImplementedException($"No import service implemented for content type {contentType}");
        }

        public IExportService<Robot> GetExportService(string contentType)
        {
            if (contentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                return new RobotExportService(_context);
            }
            throw new NotImplementedException($"No export service implemented for content type {contentType}");
        }
    }
}