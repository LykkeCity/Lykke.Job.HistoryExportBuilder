using System;
using System.Collections.Generic;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Service.History.Contracts.Enums;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.OperationsHistory.AutorestClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Lykke.Job.HistoryExportBuilder
{
    public static class HistoryOperationToResponseConverter
    {
        public static IEnumerable<HistoryModel> ToHistoryModel(this BaseHistoryModel baseModel)
        {
            switch (baseModel)
            {
                case CashinModel cashin:
                    yield return cashin.ToHistoryModel();
                    break;
                case CashoutModel cashout:
                    yield return cashout.ToHistoryModel();
                    break;
                case TradeModel trade:
                    yield return trade.ToHistoryModel();
                    break;
                case OrderEventModel orderEvent:
                    yield return orderEvent.ToHistoryModel();
                    break;
            }
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
                FeeType = FeeType.Absolute,
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
                FeeType = FeeType.Absolute,
                FeeAssetId = cashin.AssetId
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
                FeeType = FeeType.Absolute,
                FeeAssetId = cashout.AssetId
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