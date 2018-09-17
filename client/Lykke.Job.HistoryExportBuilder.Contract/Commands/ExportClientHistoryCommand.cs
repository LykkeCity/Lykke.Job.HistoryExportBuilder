using Lykke.Service.History.Contracts.Enums;
using MessagePack;

namespace Lykke.Job.HistoryExportBuilder.Contract.Commands
{
    [MessagePackObject(true)]
    public class ExportClientHistoryCommand
    {
        public string Id { set; get; }
        public string ClientId { set; get; }
        public HistoryType[] OperationTypes { set; get; }
        public string AssetId { set; get; }
        public string AssetPairId { set; get; }
    }    
}
