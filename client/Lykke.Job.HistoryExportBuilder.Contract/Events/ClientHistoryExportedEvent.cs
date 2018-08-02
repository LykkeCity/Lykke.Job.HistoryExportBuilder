using System;
using MessagePack;

namespace Lykke.Job.HistoryExportBuilder.Contract.Events
{
    [MessagePackObject(true)]
    public class ClientHistoryExportedEvent
    {
        public string Id { set; get; }
        public string ClientId { set; get; }
        public Uri Uri { set; get; }
    }
}
