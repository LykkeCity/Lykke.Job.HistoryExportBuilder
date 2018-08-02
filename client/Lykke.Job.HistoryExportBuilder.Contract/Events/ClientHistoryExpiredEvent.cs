using MessagePack;

namespace Lykke.Job.HistoryExportBuilder.Contract.Events
{
    [MessagePackObject(true)]
    public class ClientHistoryExpiredEvent
    {
        public string Id { set; get; }
        public string ClientId { set; get; }
    }
}
