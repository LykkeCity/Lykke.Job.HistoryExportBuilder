using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;

namespace Lykke.Job.HistoryExportBuilder.AzureRepositories.ExpiriesRepository
{
    public class ExpiryEntryRepository : IExpiryWatcher
    {
        public const string TableName = "HistoryExportExpiryEntries";

        private readonly INoSQLTableStorage<ExpiryEntryEntity> _tableStorage;

        public ExpiryEntryRepository(
            INoSQLTableStorage<ExpiryEntryEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task AddAsync(IExpiryEntry entry)
        {
            var alreadyExisted = !await
                _tableStorage.TryInsertAsync(
                    new ExpiryEntryEntity
                    {
                        PartitionKey = ExpiryEntryEntity.ByClient.GeneratePartitionKey(entry.ClientId),
                        RowKey = ExpiryEntryEntity.ByClient.GenerateRowKey(entry.RequestId),
                        ClientId = entry.ClientId,
                        RequestId = entry.RequestId,
                        ExpiryDateTime = entry.ExpiryDateTime
                    });

            if (!alreadyExisted)
            {
                await _tableStorage.InsertAsync(
                    new ExpiryEntryEntity
                    {
                        PartitionKey = ExpiryEntryEntity.ByDateTime.GeneratePartitionKey(),
                        RowKey = ExpiryEntryEntity.ByDateTime.GenerateRowKey(entry.ExpiryDateTime, entry.RequestId),
                        ClientId = entry.ClientId,
                        RequestId = entry.RequestId,
                        ExpiryDateTime = entry.ExpiryDateTime
                    });
            }
        }

        public Task RemoveAsync(IExpiryEntry entry)
        {
            return Task.WhenAll(
                _tableStorage.DeleteIfExistAsync(
                    ExpiryEntryEntity.ByClient.GeneratePartitionKey(entry.ClientId),
                    ExpiryEntryEntity.ByClient.GenerateRowKey(entry.RequestId)),
                _tableStorage.DeleteIfExistAsync(
                    ExpiryEntryEntity.ByDateTime.GeneratePartitionKey(),
                    ExpiryEntryEntity.ByDateTime.GenerateRowKey(entry.ExpiryDateTime, entry.RequestId)));
        }

        public async Task<IEnumerable<IExpiryEntry>> GetSoonestAsync(int n)
        {
            return await _tableStorage.GetTopRecordsAsync(
                ExpiryEntryEntity.ByDateTime.GeneratePartitionKey(),
                n);
        }
    }
}