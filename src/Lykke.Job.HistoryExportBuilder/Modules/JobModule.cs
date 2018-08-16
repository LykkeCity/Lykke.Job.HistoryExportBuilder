using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.HistoryExportBuilder.AzureRepositories;
using Lykke.Job.HistoryExportBuilder.AzureRepositories.ExpiriesRepository;
using Lykke.Job.HistoryExportBuilder.AzureRepositories.IdMappingsRepository;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Job.HistoryExportBuilder.Settings.JobSettings;
using Lykke.Job.HistoryExportBuilder.Services;
using Lykke.SettingsReader;
using Lykke.Job.HistoryExportBuilder.PeriodicalHandlers;
using Lykke.Job.HistoryExportBuilder.Settings;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settingsManager;
        private readonly IServiceCollection _services;

        public JobModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settingsManager = settingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterServices(builder);

            RegisterPeriodicalHandlers(builder);

            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<LinkExpiryChecker>()
                .As<IStartable>()
                .SingleInstance();
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder
                .Register(ctx =>
                    new BlobUploader(
                        AzureBlobStorage.Create(
                            _settingsManager.ConnectionString(x => x.HistoryExportBuilderJob.Db.DataConnString))))
                .As<IFileUploader>()
                .SingleInstance();

            builder
                .Register(ctx =>
                    new IdMappingsRepository(
                        AzureTableStorage<IdMappingEntity>.Create(
                            _settingsManager.ConnectionString(x => x.HistoryExportBuilderJob.Db.DataConnString),
                            IdMappingsRepository.TableName,
                            ctx.Resolve<ILogFactory>())))
                .As<IFileMapper>()
                .SingleInstance();
            
            builder
                .Register(ctx =>
                    new ExpiryEntryRepository(
                        AzureTableStorage<ExpiryEntryEntity>.Create(
                            _settingsManager.ConnectionString(x => x.HistoryExportBuilderJob.Db.DataConnString),
                            ExpiryEntryRepository.TableName,
                            ctx.Resolve<ILogFactory>())))
                .As<IExpiryWatcher>()
                .SingleInstance();

            builder
                .RegisterType<CsvMaker>()
                .As<IFileMaker>()
                .SingleInstance();
        }
    }
}
