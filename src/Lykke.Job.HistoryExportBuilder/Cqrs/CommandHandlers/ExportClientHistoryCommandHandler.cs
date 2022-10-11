using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.HistoryExportBuilder.Contract.Commands;
using Lykke.Job.HistoryExportBuilder.Contract.Events;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Job.HistoryExportBuilder.Models;
using Lykke.Service.History.Client;
using Lykke.Service.History.Contracts.History;

namespace Lykke.Job.HistoryExportBuilder.Cqrs.CommandHandlers
{
    public class ExportClientHistoryCommandHandler
    {
        private readonly ILog _log;
        private readonly IHistoryClient _historyClient;
        private readonly IFileMaker _fileMaker;
        private readonly IFileUploader _fileUploader;
        private readonly IFileMapper _fileMapper;
        private readonly IExpiryWatcher _expiryWatcher;
        private readonly TimeSpan _ttl;

        private const int PageSize = 1000;

        public ExportClientHistoryCommandHandler(
            ILogFactory logFactory,
            IHistoryClient historyClient,
            IFileMaker fileMaker,
            IFileUploader fileUploader,
            IFileMapper fileMapper,
            IExpiryWatcher expiryWatcher,
            TimeSpan ttl)
        {
            _log = logFactory.CreateLog(this);
            _historyClient = historyClient;
            _fileMaker = fileMaker;
            _fileUploader = fileUploader;
            _fileMapper = fileMapper;
            _expiryWatcher = expiryWatcher;
            _ttl = ttl;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ExportClientHistoryCommand command, IEventPublisher publisher)
        {
            _log.WriteInfo(nameof(Handle), command, "Client history report building is being started...");

            var result = new List<BaseHistoryModel>();

            await _expiryWatcher.AddAsync(new ExpiryEntry
            {
                ClientId = command.ClientId,
                RequestId = command.Id,
                ExpiryDateTime = DateTime.UtcNow + _ttl
            });

            for (var i = 0; ; i++)
            {
                _log.WriteInfo(nameof(Handle), command, $"History page {i} is being requested from the History service...");

                var response = await _historyClient.HistoryApi.GetHistoryByWalletAsync(Guid.Parse(command.ClientId),
                    command.OperationTypes,
                    command.AssetId,
                    command.AssetPairId,
                    i * PageSize,
                    PageSize);

                _log.WriteInfo(nameof(Handle), command, $"History page {i} has been obtained from the History service. {response.Count()} items read");

                if (!response.Any())
                    break;

                result.AddRange(response);
            }

            _log.WriteInfo(nameof(Handle), command, "Entire history has been read");

            var history = result.Select(x => x.ToHistoryModel()).OrderByDescending(x => x.DateTime);

            _log.WriteInfo(nameof(Handle), command, "Mapping report to the client...");

            var idForUri = await _fileMapper.MapAsync(command.ClientId, command.Id);

            _log.WriteInfo(nameof(Handle), command, "Exporting the report to CSV...");

            var file = await _fileMaker.MakeAsync(history);

            _log.WriteInfo(nameof(Handle), command, "Uploading the report to the storage...");

            var uri = await _fileUploader.UploadAsync(idForUri, FileType.Csv, file);

            _log.WriteInfo(nameof(Handle), command, "Publishing event...");

            publisher.PublishEvent(new ClientHistoryExportedEvent
            {
                Id = command.Id,
                ClientId = command.ClientId,
                Uri = uri
            });

            return CommandHandlingResult.Ok();
        }
    }
}