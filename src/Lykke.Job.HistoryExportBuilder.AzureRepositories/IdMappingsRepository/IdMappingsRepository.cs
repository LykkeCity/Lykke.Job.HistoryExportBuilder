using System;
using System.Threading.Tasks;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Job.HistoryExportBuilder.Core.Services;

namespace Lykke.Job.HistoryExportBuilder.AzureRepositories.IdMappingsRepository
{
    public class IdMappingsRepository : IFileMapper
    {
        public const string TableName = "HistoryExportIdMappings";
        
        private readonly INoSQLTableStorage<IdMappingEntity> _tableStorage;
        
        public IdMappingsRepository(
            INoSQLTableStorage<IdMappingEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public async Task<string> MapAsync(string clinetId, string requestId)
        {
            var id = Guid.NewGuid().ToString();

            var entity = await _tableStorage.GetOrInsertAsync(
                clinetId,
                requestId,
                () => new IdMappingEntity
                {
                    PartitionKey = clinetId,
                    RowKey = requestId,
                    Id = id
                });

            return entity.Id;
        }
    }
}
