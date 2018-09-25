using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using JetBrains.Annotations;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.HistoryExportBuilder.Services
{
    [UsedImplicitly]
    public class CsvMaker : IFileMaker
    {
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;

        public CsvMaker(
            IAssetsServiceWithCache assetsServiceWithCache)
        {
            _assetsServiceWithCache = assetsServiceWithCache;
        }

        public async Task<MemoryStream> MakeAsync(IEnumerable<HistoryModel> operations)
        {
            var assets = (await _assetsServiceWithCache.GetAllAssetsAsync(true)).ToDictionary(x => x.Id);
            var assetPairs = (await _assetsServiceWithCache.GetAllAssetPairsAsync()).ToDictionary(x => x.Id);

            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream) { AutoFlush = true })
                {
                    var userCsv = new CsvWriter(streamWriter);
                    userCsv.Configuration.RegisterClassMap<HistoryOperationCsvEntryMap>();

                    userCsv.WriteRecords(operations.Select(x =>
                    {
                        var assetPair = x.AssetPair != null && assetPairs.ContainsKey(x.AssetPair) ? assetPairs[x.AssetPair] : null;
                        var baseAsset = x.Asset ?? assetPair?.BaseAssetId;
                        var quoteAsset = assetPair?.QuotingAssetId;
                        var baseAssetName = baseAsset != null && assets.ContainsKey(baseAsset) ? (assets[baseAsset].DisplayId ?? baseAsset) : baseAsset;
                        var quoteAssetName = quoteAsset != null && assets.ContainsKey(quoteAsset) ? (assets[quoteAsset].DisplayId ?? quoteAsset) : quoteAsset;

                        return new HistoryOperationCsvEntry
                        {
                            Date = x.DateTime,
                            Type = x.Type.ToString(),
                            Exchange = "Lykke",
                            BaseAmount = x.Amount,
                            BaseCurrency = baseAssetName,
                            QuoteAmount = x.OppositeAmount,
                            QuoteCurrency = quoteAssetName,
                            FeeCurrency =
                                x.FeeSize != 0
                                    ? (x.FeeAssetId != null && assets.ContainsKey(x.FeeAssetId) ? (assets[x.FeeAssetId].DisplayId ?? x.FeeAssetId) : x.Asset)
                                    : null,
                            Fee = x.FeeSize
                        };
                    }));

                    stream.Seek(0, SeekOrigin.Begin);

                    stream.Position = 0;

                    return stream;
                }
            }
        }
    }

    public class HistoryOperationCsvEntry
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Exchange { get; set; }
        public decimal BaseAmount { get; set; }
        public string BaseCurrency { get; set; }
        public decimal? QuoteAmount { get; set; }
        public string QuoteCurrency { get; set; }
        public decimal Fee { set; get; }
        public string FeeCurrency { set; get; }
    }

    public sealed class HistoryOperationCsvEntryMap : ClassMap<HistoryOperationCsvEntry>
    {
        public HistoryOperationCsvEntryMap()
        {
            Map(m => m.Date).Name("Date").NameIndex(0);
            Map(m => m.Type).Name("Type").NameIndex(1);
            Map(m => m.Exchange).Name("Exchange").NameIndex(2);
            Map(m => m.BaseAmount).Name("Base Amount").NameIndex(3);
            Map(m => m.BaseCurrency).Name("Base Currency").NameIndex(4);
            Map(m => m.QuoteAmount).Name("Quote Amount").NameIndex(5);
            Map(m => m.QuoteCurrency).Name("Quote Currency").NameIndex(6);
            Map(m => m.Fee).Name("Fee").NameIndex(7);
            Map(m => m.FeeCurrency).Name("Fee Currency").NameIndex(8);
        }
    }
}