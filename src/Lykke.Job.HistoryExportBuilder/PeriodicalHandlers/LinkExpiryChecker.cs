using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.HistoryExportBuilder.Contract;
using Lykke.Job.HistoryExportBuilder.Contract.Events;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;

namespace Lykke.Job.HistoryExportBuilder.PeriodicalHandlers
{
    [UsedImplicitly]
    public class LinkExpiryChecker : TimerPeriod
    {
        private readonly IExpiryWatcher _expiryWatcher;
        private readonly ICqrsEngine _cqrsEngine;
        
        public LinkExpiryChecker(
            IExpiryWatcher expiryWatcher,
            ICqrsEngine cqrsEngine,
            ILog log) :
            base(nameof(LinkExpiryChecker), (int)TimeSpan.FromSeconds(10).TotalMilliseconds, log)
        {
            _expiryWatcher = expiryWatcher;
            _cqrsEngine = cqrsEngine;
        }

        public override async Task Execute()
        {
            var soonest = await _expiryWatcher.GetSoonestAsync(100);

            foreach (var entry in soonest)
            {
                if (entry.IsDue())
                {
                    _cqrsEngine.PublishEvent(
                        new ClientHistoryExpiredEvent
                        {
                            ClientId = entry.ClientId,
                            Id = entry.RequestId
                        },
                        HistoryExportBuilderBoundedContext.Name);

                    await _expiryWatcher.RemoveAsync(entry);
                }
            }
        }
    }
}
