using System.Threading.Tasks;

namespace Lykke.Job.HistoryExportBuilder.Core.Services
{
    public interface IFileMapper
    {
        Task<string> MapAsync(string clinetId, string requestId);
    }
}
