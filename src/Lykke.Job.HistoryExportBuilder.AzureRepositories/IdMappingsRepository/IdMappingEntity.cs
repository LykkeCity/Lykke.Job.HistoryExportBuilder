using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.HistoryExportBuilder.AzureRepositories.IdMappingsRepository
{
    public class IdMappingEntity : TableEntity
    {
        public string Id { get; set; }
    }
}
