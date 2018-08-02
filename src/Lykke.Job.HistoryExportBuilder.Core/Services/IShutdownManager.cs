using System.Threading.Tasks;

namespace Lykke.Job.HistoryExportBuilder.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
