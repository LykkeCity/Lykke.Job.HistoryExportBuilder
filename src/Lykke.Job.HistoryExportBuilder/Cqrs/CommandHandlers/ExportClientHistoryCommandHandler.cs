using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.HistoryExportBuilder.Contract.Commands;
using Lykke.Job.HistoryExportBuilder.Contract.Events;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Job.HistoryExportBuilder.Models;
using Lykke.Service.OperationsHistory.Client;

namespace Lykke.Job.HistoryExportBuilder.Cqrs.CommandHandlers
{
    public class ExportClientHistoryCommandHandler
    {
        private readonly IOperationsHistoryClient _operationsHistory;
        private readonly IFileMaker _fileMaker;
        private readonly IFileUploader _fileUploader;
        private readonly IFileMapper _fileMapper;
        private readonly IExpiryWatcher _expiryWatcher;
        private readonly TimeSpan _ttl;

        public ExportClientHistoryCommandHandler(
            IOperationsHistoryClient operationsHistory,
            IFileMaker fileMaker,
            IFileUploader fileUploader,
            IFileMapper fileMapper,
            IExpiryWatcher expiryWatcher,
            TimeSpan ttl)
        {
            _operationsHistory = operationsHistory;
            _fileMaker = fileMaker;
            _fileUploader = fileUploader;
            _fileMapper = fileMapper;
            _expiryWatcher = expiryWatcher;
            _ttl = ttl;
        }
        
        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(ExportClientHistoryCommand command, IEventPublisher publisher)
        {
            var operationsHistoryResponse =
                await _operationsHistory.GetByClientId(
                    command.ClientId,
                    command.OperationTypes,
                    command.AssetId,
                    command.AssetPairId,
                    command.Take.Value,
                    command.Skip);
            
            if(operationsHistoryResponse.Error != null)
                throw new Exception(operationsHistoryResponse.Error.Message);

            var idForUri = await _fileMapper.MapAsync(command.ClientId, command.Id);

            var file = await _fileMaker.MakeAsync(operationsHistoryResponse.Records);

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
