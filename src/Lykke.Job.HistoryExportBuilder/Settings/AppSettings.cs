using JetBrains.Annotations;
using Lykke.Job.HistoryExportBuilder.Settings.JobSettings;
using Lykke.Sdk.Settings;
using Lykke.Service.History.Client;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.HistoryExportBuilder.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public HistoryExportBuilderSettings HistoryExportBuilderJob { get; set; }
        public HistoryServiceClientSettings HistoryServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
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
