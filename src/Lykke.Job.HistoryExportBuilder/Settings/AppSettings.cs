using Lykke.Job.HistoryExportBuilder.Settings.JobSettings;
using Lykke.Job.HistoryExportBuilder.Settings.SlackNotifications;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.HistoryExportBuilder.Settings
{
    public class AppSettings
    {
        public HistoryExportBuilderSettings HistoryExportBuilderJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public OperationsHistoryServiceClientSettings OperationsHistoryServiceClient { get; set; }
        public SagasRabbitMq SagasRabbitMq { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
    }
    
    public class SagasRabbitMq
    {
        [AmqpCheck]
        public string RabbitConnectionString { get; set; }

        public string RetryDelay { get; set; }
    }

    public class AssetsServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
