using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.HistoryExportBuilder.Core.Domain;

namespace Lykke.Job.HistoryExportBuilder.Core.Services
{
    public interface IExpiryWatcher
    {
        Task AddAsync(IExpiryEntry expiryEntry);
        Task RemoveAsync(IExpiryEntry expiryEntry);
        Task<IEnumerable<IExpiryEntry>> GetSoonestAsync(int n);
    }
}
