using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.HistoryExportBuilder.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class ClientModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public ClientModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterOperationsHistoryClient(_settings.CurrentValue.OperationsHistoryServiceClient);
            
            builder.RegisterAssetsClient(AssetServiceSettings.Create(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl), TimeSpan.FromMinutes(3)));
        }
    }
}
