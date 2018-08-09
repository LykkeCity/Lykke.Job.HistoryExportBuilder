using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.HistoryExportBuilder.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.OperationsHistory.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class ClientModule : Module
    {
        private readonly AppSettings _settings;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public ClientModule(AppSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
            
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterOperationsHistoryClient(_settings.OperationsHistoryServiceClient, _log);
            
            _services.RegisterAssetsClient(AssetServiceSettings.Create(new Uri(_settings.AssetsServiceClient.ServiceUrl), TimeSpan.FromMinutes(3)), _log);
            
            builder.Populate(_services);
        }
    }
}
