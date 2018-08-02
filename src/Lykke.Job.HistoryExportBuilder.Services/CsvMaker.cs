using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Service.OperationsHistory.AutorestClient.Models;

namespace Lykke.Job.HistoryExportBuilder.Services
{
    public class CsvMaker : IFileMaker
    {
        public Task<MemoryStream> MakeAsync(IEnumerable<HistoryOperation> operations)
        {
            using (var stream = new MemoryStream())
            {
                using (var streamWriter = new StreamWriter(stream) {AutoFlush = true})
                {
                    var userCsv = new CsvWriter(streamWriter);

                    userCsv.WriteRecords(operations);
                        
                    stream.Seek(0, SeekOrigin.Begin);
                            
                    stream.Position = 0;

                    return Task.FromResult(stream);
                }
            }
        }
    }
}
