using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.HistoryExportBuilder.Contract.Commands;
using Lykke.Job.HistoryExportBuilder.Contract.Events;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Job.HistoryExportBuilder.Models;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.ClientAccount.Client.AutorestClient.Models;
using Lykke.Service.History.Client;
using Lykke.Service.History.Contracts.History;
using Lykke.Service.OperationsHistory.Client;

namespace Lykke.Job.HistoryExportBuilder.Cqrs.CommandHandlers
{
    public class ExportClientHistoryCommandHandler
    {
        private readonly IClientAccountClient _clientAccountClient;
        private readonly IHistoryClient _historyClient;
        private readonly IFileMaker _fileMaker;
        private readonly IFileUploader _fileUploader;
        private readonly IFileMapper _fileMapper;
        private readonly IExpiryWatcher _expiryWatcher;
        private readonly TimeSpan _ttl;

        private const int _pageSize = 1000;

        public ExportClientHistoryCommandHandler(
            IClientAccountClient clientAccountClient,
            IHistoryClient historyClient,
            IFileMaker fileMaker,
            IFileUploader fileUploader,
            IFileMapper fileMapper,
            IExpiryWatcher expiryWatcher,
            TimeSpan ttl)
        {
            _historyClient = historyClient;
            _clientAccountClient = clientAccountClient;
            _fileMaker = fileMaker;
            _fileUploader = fileUploader;
            _fileMapper = fileMapper;
            _expiryWatcher = expiryWatcher;
            _ttl = ttl;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ExportClientHistoryCommand command, IEventPublisher publisher)
        {
            var result = new List<BaseHistoryModel>();

            for (var i = 0; ; i++)
            {
                var response = await _historyClient.HistoryApi.GetHistoryByWalletAsync(Guid.Parse(command.ClientId),
                    command.OperationTypes,
                    command.AssetId,
                    command.AssetPairId,
                    i * _pageSize,
                    _pageSize);

                if (!response.Any())
                    break;

                result.AddRange(response);
            }

            var history = result.Select(x => x.ToHistoryModel()).OrderByDescending(x => x.DateTime);

            var idForUri = await _fileMapper.MapAsync(command.ClientId, command.Id);

            var file = await _fileMaker.MakeAsync(history);

            var uri = await _fileUploader.UploadAsync(idForUri, FileType.Csv, file);

            await _expiryWatcher.AddAsync(
                new ExpiryEntry
                {
                    ClientId = command.ClientId,
                    RequestId = command.Id,
                    ExpiryDateTime = DateTime.UtcNow + _ttl
                });

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