using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lykke.Job.HistoryExportBuilder.Core.Domain;

namespace Lykke.Job.HistoryExportBuilder.Core.Services
{
    public interface IFileMaker
    {
        Task<MemoryStream> MakeAsync(IEnumerable<HistoryModel> operations);
    }
}
