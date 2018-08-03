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
        private const string ContainerName = "historyexports";

        public BlobUploader(IBlobStorage blobStorage)
        {
            _blobStorage = blobStorage;
        }
        
        public async Task<Uri> UploadAsync(string id, FileType type, MemoryStream file)
        {
            var key = $"{id}.{GetExtention(type)}";

            if (await _blobStorage.HasBlobAsync(ContainerName, key))
                return new Uri(await Task.Run(() => _blobStorage.GetBlobUrl(ContainerName, key)));

            file = new MemoryStream(file.ToArray());
            return new Uri(await _blobStorage.SaveBlobAsync(ContainerName, key, file));

        }

        public async Task RemoveAsync(string id, FileType type)
        {
            var key = $"{id}.{GetExtention(type)}";

            if (await _blobStorage.HasBlobAsync(ContainerName, key))
            {
                await _blobStorage.DelBlobAsync(ContainerName, key);
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
