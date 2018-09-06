using System;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Job.HistoryExportBuilder.Core.Domain
{
    public class HistoryModel
    {
        public string Id { get; set; }

        public DateTime DateTime { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HistoryType Type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public HistoryState State { get; set; }

        public decimal Amount { get; set; }

        public string Asset { get; set; }

        public string AssetPair { get; set; }

        public decimal? Price { get; set; }

        public decimal FeeSize { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public FeeType FeeType { get; set; }
    }
}