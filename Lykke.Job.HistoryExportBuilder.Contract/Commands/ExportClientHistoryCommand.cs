using System;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using MessagePack;

namespace Lykke.Job.HistoryExportBuilder.Contract.Commands
{
    [MessagePackObject(true)]
    public class ExportClientHistoryCommand
    {
        public string Id { set; get; }
        public string ClientId { set; get; }
        public HistoryOperationType[] OperationTypes { set; get; }
        public string AssetId { set; get; }
        public string AssetPairId { set; get; }
        public int Skip { set; get; }
        public int? Take { set; get; }
    }
    
    
}
