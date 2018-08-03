using System;
using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;

namespace Lykke.Job.HistoryExportBuilder.AzureRepositories
{
    public class BlobUploader : IFileUploader
    {
        private readonly IBlobStorage _blobStorage;

        public BlobUploader(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }
        
        public async Task<Uri> UploadAsync(string id, FileType type, MemoryStream file)
        {
            file = new MemoryStream(file.ToArray());
            return new Uri(await _blobStorage.SaveBlobAsync("historyexports", $"{id}.{GetExtention(type)}", file));
        }

        public Task RemoveAsync(string id, FileType type)
        {
            return _blobStorage.DelBlobAsync("historyexports", $"{id}.{GetExtention(type)}");
        }

        private string GetMimeType(FileType type)
        {
            switch (type)
            {
                case FileType.Csv:
                    return "text/csv";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private string GetExtention(FileType type)
        {
            switch (type)
            {
                case FileType.Csv:
                    return "csv";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
