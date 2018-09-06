﻿using JetBrains.Annotations;
using Lykke.Job.HistoryExportBuilder.Settings.JobSettings;
using Lykke.Sdk.Settings;
using Lykke.Service.History.Client;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader.Attributes;
using SlackNotificationsSettings = Lykke.Job.HistoryExportBuilder.Settings.SlackNotifications.SlackNotificationsSettings;

namespace Lykke.Job.HistoryExportBuilder.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public HistoryExportBuilderSettings HistoryExportBuilderJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }
        public HistoryServiceClientSettings HistoryServiceClient { get; set; }
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

    public class ClientAccountServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }
    }
}
