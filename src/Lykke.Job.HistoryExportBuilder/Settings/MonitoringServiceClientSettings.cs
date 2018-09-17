using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.HistoryExportBuilder.Settings
{
    public class MonitoringServiceClientSettings
    {
        [HttpCheck("api/isalive")]
        public string MonitoringServiceUrl { get; set; }
    }
}
