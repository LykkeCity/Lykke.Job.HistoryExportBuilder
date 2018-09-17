using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.HistoryExportBuilder.Settings
{
    public class CqrsSettings
    {
        [AmqpCheck]
        public string RabbitConnString { get; set; }
    }
}
