using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.HistoryExportBuilder.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string DataConnString { get; set; }
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
