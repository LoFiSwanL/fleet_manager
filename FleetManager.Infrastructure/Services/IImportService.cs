using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FleetManager.Infrastructure.Services
{
    public interface IImportService<TEntity> where TEntity : class
    {
        Task<ImportResult> ImportFromStreamAsync(Stream stream, bool updateExisting, CancellationToken cancellationToken);
    }
}