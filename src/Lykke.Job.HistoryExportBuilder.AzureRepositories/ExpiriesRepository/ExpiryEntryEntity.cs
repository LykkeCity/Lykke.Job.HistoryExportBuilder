﻿using System;
using Lykke.AzureStorage.Tables;
using Lykke.Job.HistoryExportBuilder.Core.Domain;

namespace Lykke.Job.HistoryExportBuilder.AzureRepositories.ExpiriesRepository
{
    public class ExpiryEntryEntity : AzureTableEntity, IExpiryEntry
    {
        public string ClientId { get; set; }
        public string RequestId { get; set; }
        public DateTime ExpiryDateTime { get; set; }

        public static string GeneratePartitionKey()
        {
            return "part";
        }

        public static string GenerateRowKey(DateTime expiryDateTime, string requestId)
        {
            return expiryDateTime.Ticks.ToString("D25") + requestId;
        }
    }
}
