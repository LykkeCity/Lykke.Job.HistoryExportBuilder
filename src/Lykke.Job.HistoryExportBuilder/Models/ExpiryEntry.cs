using System;
using Lykke.Job.HistoryExportBuilder.Core.Domain;

namespace Lykke.Job.HistoryExportBuilder.Models
{
    public class ExpiryEntry : IExpiryEntry
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public DateTime ExpiryDateTime { get; set; }
    }
}
