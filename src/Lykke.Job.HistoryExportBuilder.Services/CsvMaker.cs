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
using Lykke.Service.OperationsHistory.AutorestClient.Models;

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
        
        public async Task<MemoryStream> MakeAsync(IEnumerable<HistoryOperation> operations)
        {
            var assets = (await _assetsServiceWithCache.GetAllAssetsAsync()).ToDictionary(x => x.Id);
            var assetPairs = (await _assetsServiceWithCache.GetAllAssetPairsAsync()).ToDictionary(x => x.Id);
            
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream) {AutoFlush = true})
                {
                    var userCsv = new CsvWriter(streamWriter);
                    userCsv.Configuration.RegisterClassMap<HistoryOperationCsvEntryMap>();

                    userCsv.WriteRecords(operations.Select(x =>
                    {
                        var assetPair = x.AssetPair != null ? assetPairs[x.AssetPair] : null;
                        
                        decimal baseAmount;
                        decimal? quoteAmount;

                        if (assetPair == null)
                        {
                            baseAmount = Convert.ToDecimal(x.Amount);
                            quoteAmount = null;
                        }
                        else
                        {
                            baseAmount = assetPair.BaseAssetId == x.Asset
                                ? Convert.ToDecimal(x.Amount)
                                : Convert.ToDecimal(x.Amount) * Convert.ToDecimal(x.Price);
                            quoteAmount = assetPair.BaseAssetId == x.Asset
                                ? Convert.ToDecimal(x.Amount) * Convert.ToDecimal(x.Price)
                                : Convert.ToDecimal(x.Amount);
                        }
                        
                        return new HistoryOperationCsvEntry
                        {
                            Date = x.DateTime,
                            Type = x.Type.ToString(),
                            Exchange = "Lykke",
                            BaseAmount = baseAmount,
                            BaseCurrency = assets[assetPair != null ? assetPair.BaseAssetId : x.Asset].DisplayId,
                            QuoteAmount = quoteAmount,
                            QuoteCurrency = assetPair?.QuotingAssetId != null ? assets[assetPair.QuotingAssetId].DisplayId : null,
                            FeeCurrency = assets[x.Asset].DisplayId,
                            Fee = Convert.ToDecimal(x.FeeSize)
                        };
                    }));
                        
                    stream.Seek(0, SeekOrigin.Begin);
                            
                    stream.Position = 0;

                    return stream;
                }
            }
        }

        public async Task<MemoryStream> MakeAsync(IEnumerable<HistoryModel> operations)
        {
            var assets = (await _assetsServiceWithCache.GetAllAssetsAsync()).ToDictionary(x => x.Id);
            var assetPairs = (await _assetsServiceWithCache.GetAllAssetPairsAsync()).ToDictionary(x => x.Id);
            
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream) {AutoFlush = true})
                {
                    var userCsv = new CsvWriter(streamWriter);
                    userCsv.Configuration.RegisterClassMap<HistoryOperationCsvEntryMap>();

                    userCsv.WriteRecords(operations.Select(x =>
                    {
                        var assetPair = x.AssetPair != null ? assetPairs[x.AssetPair] : null;
                        
                        decimal baseAmount;
                        decimal? quoteAmount;

                        if (assetPair == null)
                        {
                            baseAmount = Convert.ToDecimal(x.Amount);
                            quoteAmount = null;
                        }
                        else
                        {
                            baseAmount = assetPair.BaseAssetId == x.Asset
                                ? Convert.ToDecimal(x.Amount)
                                : Convert.ToDecimal(x.Amount) * Convert.ToDecimal(x.Price);
                            quoteAmount = assetPair.BaseAssetId == x.Asset
                                ? Convert.ToDecimal(x.Amount) * Convert.ToDecimal(x.Price)
                                : Convert.ToDecimal(x.Amount);
                        }
                        
                        return new HistoryOperationCsvEntry
                        {
                            Date = x.DateTime,
                            Type = x.Type.ToString(),
                            Exchange = "Lykke",
                            BaseAmount = baseAmount,
                            BaseCurrency = assets[assetPair != null ? assetPair.BaseAssetId : x.Asset].DisplayId,
                            QuoteAmount = quoteAmount,
                            QuoteCurrency = assetPair?.QuotingAssetId != null ? assets[assetPair.QuotingAssetId].DisplayId : null,
                            FeeCurrency = assets[x.Asset].DisplayId,
                            Fee = Convert.ToDecimal(x.FeeSize)
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
            Map( m => m.Date ).Name( "Date" ).NameIndex( 0 );
            Map( m => m.Type ).Name( "Type" ).NameIndex( 1 );
            Map( m => m.Exchange ).Name( "Exchange" ).NameIndex( 2 );
            Map( m => m.BaseAmount ).Name( "Base Amount" ).NameIndex( 3 );
            Map( m => m.BaseCurrency ).Name( "Base Currency" ).NameIndex( 4 );
            Map( m => m.QuoteAmount ).Name( "Quote Amount" ).NameIndex( 5 );
            Map( m => m.QuoteCurrency ).Name( "Quote Currency" ).NameIndex( 6 );
            Map( m => m.Fee ).Name( "Fee" ).NameIndex( 7 );
            Map( m => m.FeeCurrency ).Name( "Fee Currency" ).NameIndex( 8 );
        }
    }
}
