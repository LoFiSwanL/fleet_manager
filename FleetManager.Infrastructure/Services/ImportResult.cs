using System.Collections.Generic;

namespace FleetManager.Infrastructure.Services
{
    public class ImportResult
    {
        public bool Success => Errors.Count == 0;

        public int ImportedCount { get; set; } = 0;

        public List<string> Errors { get; set; } = new List<string>();
    }
}