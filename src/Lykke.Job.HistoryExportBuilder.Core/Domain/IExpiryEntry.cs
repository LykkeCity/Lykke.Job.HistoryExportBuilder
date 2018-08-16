using System;

namespace Lykke.Job.HistoryExportBuilder.Core.Domain
{
    public interface IExpiryEntry
    {
        string ClientId { set; get; }
        string RequestId { set; get; }
        DateTime ExpiryDateTime { set; get; }
    }

    public static class IExpiryEntryHelper
    {
        public static bool IsDue(this IExpiryEntry entry)
        {
            return entry.ExpiryDateTime <= DateTime.UtcNow;
        }
    }
}
