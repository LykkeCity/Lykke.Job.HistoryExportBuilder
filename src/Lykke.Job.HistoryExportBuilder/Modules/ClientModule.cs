using System;
using Autofac;
using Lykke.Job.HistoryExportBuilder.Settings;
using Lykke.Service.Assets.Client;
using Lykke.Service.ClientAccount.Client;
using Lykke.Service.History.Client;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader;

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
            builder.RegisterAssetsClient(AssetServiceSettings.Create(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl), TimeSpan.FromMinutes(3)));
            
            builder.RegisterHistoryClient(_settings.CurrentValue.HistoryServiceClient);
            
            builder.RegisterLykkeServiceClient(_settings.CurrentValue.ClientAccountServiceClient.ServiceUrl);
        }
    }
}
