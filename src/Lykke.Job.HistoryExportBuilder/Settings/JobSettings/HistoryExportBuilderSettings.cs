using System;

namespace Lykke.Job.HistoryExportBuilder.Settings.JobSettings
{
    public class HistoryExportBuilderSettings
    {
        public DbSettings Db { get; set; }
        public TimeSpan GeneratedFileTtl { get; set; }
    }
}
