using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Service.Assets.Client;
using Lykke.Service.OperationsHistory.AutorestClient.Models;

namespace Lykke.Job.HistoryExportBuilder.Services
{
    public class CsvMaker : IFileMaker
    {
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;

        public CsvMaker(
            IAssetsServiceWithCache assetsServiceWithCache)
        {
            _assetsServiceWithCache = assetsServiceWithCache;
        }
        
        public async Task<MemoryStream> MakeAsync(IEnumerable<HistoryOperation> operations)
        {
            var assets = await _assetsServiceWithCache.GetAllAssetsAsync();
            var assetPairs = await _assetsServiceWithCache.GetAllAssetPairsAsync();
            
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream) {AutoFlush = true})
                {
                    var userCsv = new CsvWriter(streamWriter);

                    userCsv.WriteRecords(operations.Select(x =>
                    {
                        var assetPair = assetPairs.FirstOrDefault(y => y.Id == x.AssetPair);

                        var otherAsset = assetPair?.BaseAssetId == x.Asset
                            ? assetPair?.QuotingAssetId
                            : assetPair?.BaseAssetId;
                        
                        var r = new HistoryOperationCsvEntry
                        {
                            Date = x.DateTime,
                            Type = x.Type.ToString(),
                            Exchange = "Lykke",
                            BaseAmount = Convert.ToDecimal(x.Amount),
                            BaseCurrency = x.Asset,
                            Fee = Convert.ToDecimal(x.FeeSize),
                            FeeCurrency = x.Asset
                        };

                        return r;
                    }));
                        
                    stream.Seek(0, SeekOrigin.Begin);
                            
                    stream.Position = 0;

                    return stream;
                }
            }
        }
    }

    internal class HistoryOperationCsvEntry
    {
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Exchange { get; set; }
        public decimal BaseAmount { get; set; }
        public string BaseCurrency { get; set; }
        public decimal QuoteAmount { get; set; }
        public string QuoteCurrency { get; set; }
        public decimal Fee { set; get; }
        public string FeeCurrency { set; get; }
    }
}
