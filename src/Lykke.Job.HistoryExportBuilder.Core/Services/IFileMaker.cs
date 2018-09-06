using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Service.OperationsHistory.AutorestClient.Models;

namespace Lykke.Job.HistoryExportBuilder.Core.Services
{
    public interface IFileMaker
    {
        Task<MemoryStream> MakeAsync(IEnumerable<HistoryOperation> operations);
        Task<MemoryStream> MakeAsync(IEnumerable<HistoryModel> operations);
    }
}
