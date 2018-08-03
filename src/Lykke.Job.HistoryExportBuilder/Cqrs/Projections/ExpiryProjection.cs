using System;
using System.Threading.Tasks;
using Lykke.Job.HistoryExportBuilder.Contract.Events;
using Lykke.Job.HistoryExportBuilder.Core.Domain;
using Lykke.Job.HistoryExportBuilder.Core.Services;

namespace Lykke.Job.HistoryExportBuilder.Cqrs.Projections
{
    public class ExpiryProjection
    {
        private readonly IFileUploader _fileUploader;
        private readonly IFileMapper _fileMapper;

        public ExpiryProjection(
            IFileUploader fileUploader,
            IFileMapper fileMapper)
        {
            _fileMapper = fileMapper;
            _fileUploader = fileUploader;
        }

        public async Task Handle(ClientHistoryExpiredEvent evt)
        {
            var id = await _fileMapper.MapAsync(evt.ClientId, evt.Id);

            await _fileUploader.RemoveAsync(id, FileType.Csv);
        }
    }
}