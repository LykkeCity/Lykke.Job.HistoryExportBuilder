using Microsoft.Extensions.DependencyInjection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Blob;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Job.HistoryExportBuilder.AzureRepositories;
using Lykke.Job.HistoryExportBuilder.AzureRepositories.ExpiriesRepository;
using Lykke.Job.HistoryExportBuilder.AzureRepositories.IdMappingsRepository;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Job.HistoryExportBuilder.Settings.JobSettings;
using Lykke.Job.HistoryExportBuilder.Services;
using Lykke.SettingsReader;
using Lykke.Job.HistoryExportBuilder.PeriodicalHandlers;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class JobModule : Module
    {
        private readonly HistoryExportBuilderSettings _settings;
        private readonly IReloadingManager<HistoryExportBuilderSettings> _settingsManager;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public JobModule(HistoryExportBuilderSettings settings, IReloadingManager<HistoryExportBuilderSettings> settingsManager, ILog log)
        {
            _settings = settings;
            _log = log;
            _settingsManager = settingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            RegisterServices(builder);

            RegisterPeriodicalHandlers(builder);

            // TODO: Add your dependencies here

            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<LinkExpiryChecker>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder
                .Register(ctx =>
                    new BlobUploader(
                        AzureBlobStorage.Create(
                            _settingsManager.ConnectionString(x => x.Db.DataConnString))))
                .As<IFileUploader>()
                .SingleInstance();

            builder
                .Register(ctx =>
                    new IdMappingsRepository(
                        AzureTableStorage<IdMappingEntity>.Create(
                            _settingsManager.ConnectionString(x => x.Db.DataConnString),
                            IdMappingsRepository.TableName,
                            _log)))
                .As<IFileMapper>()
                .SingleInstance();
            
            builder
                .Register(ctx =>
                    new ExpiryEntryRepository(
                        AzureTableStorage<ExpiryEntryEntity>.Create(
                            _settingsManager.ConnectionString(x => x.Db.DataConnString),
                            ExpiryEntryRepository.TableName,
                            _log)))
                .As<IExpiryWatcher>()
                .SingleInstance();

            builder
                .RegisterType<CsvMaker>()
                .As<IFileMaker>()
                .SingleInstance();
        }
    }
}
