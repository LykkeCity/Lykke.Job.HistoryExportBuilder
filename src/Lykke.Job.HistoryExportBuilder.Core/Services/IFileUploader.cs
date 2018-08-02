using System;
using System.IO;
using System.Threading.Tasks;
using Lykke.Job.HistoryExportBuilder.Core.Domain;

namespace Lykke.Job.HistoryExportBuilder.Core.Services
{
    public interface IFileUploader
    {
        Task<Uri> UploadAsync(string id, FileType type, MemoryStream file);
        Task RemoveAsync(string id, FileType type);
    }
}
