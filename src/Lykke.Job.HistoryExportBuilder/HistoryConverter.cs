using System;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;

namespace Lykke.Job.HistoryExportBuilder
{
    public static class HistoryOperationToResponseConverter
    {
        public static HistoryModel ToHistoryModel(this BaseHistoryModel baseModel)
        {
            switch (baseModel)
            {
                case CashinModel cashin:
                    return cashin.ToHistoryModel();
                case CashoutModel cashout:
                    return cashout.ToHistoryModel();
                case TradeModel trade:
                    return trade.ToHistoryModel();
                case OrderEventModel orderEvent:
                    return orderEvent.ToHistoryModel();
            }
            
            throw new ArgumentException($"{baseModel.Id} has unrecognized type");
        }

        public static HistoryModel ToHistoryModel(this TradeModel trade)
        {
            return new HistoryModel
            {
                Id = trade.Id.ToString(),
                DateTime = trade.Timestamp,
                Type = HistoryType.Trade,
                Asset = trade.BaseAssetId,
                Amount = trade.BaseVolume,
                OppositeAmount = trade.QuotingVolume,
                AssetPair = trade.AssetPairId,
                Price = trade.Price,
                State = HistoryState.Finished,
                FeeSize = trade.FeeSize.GetValueOrDefault(0),
                FeeAssetId = trade.FeeAssetId
            };
        }

        public static HistoryModel ToHistoryModel(this CashinModel cashin)
        {
            return new HistoryModel
            {
                Id = cashin.Id.ToString(),
                DateTime = cashin.Timestamp,
                Type = HistoryType.CashIn,
                Asset = cashin.AssetId,
                Amount = cashin.Volume,
                State = cashin.State,
                FeeSize = cashin.FeeSize.GetValueOrDefault(0),
                FeeAssetId = cashin.AssetId,
                TransactionHash = cashin.BlockchainHash
            };
        }

        public static HistoryModel ToHistoryModel(this CashoutModel cashout)
        {
            return new HistoryModel
            {
                Id = cashout.Id.ToString(),
                DateTime = cashout.Timestamp,
                Type = HistoryType.CashOut,
                Asset = cashout.AssetId,
                Amount = cashout.Volume,
                State = cashout.State,
                FeeSize = cashout.FeeSize.GetValueOrDefault(0),
                FeeAssetId = cashout.AssetId,
                TransactionHash = cashout.BlockchainHash
            };
        }

        public static HistoryModel ToHistoryModel(this OrderEventModel orderEvent)
        {
            var status = HistoryState.InProgress;
            if (orderEvent.Status == OrderStatus.Cancelled)
                status = HistoryState.Canceled;

            return new HistoryModel
            {
                Id = orderEvent.Id.ToString(),
                DateTime = orderEvent.Timestamp,
                Type = HistoryType.OrderEvent,
                Amount = orderEvent.Volume,
                AssetPair = orderEvent.AssetPairId,
                State = status,
                Price = orderEvent.Price
            };
        }
    }
}