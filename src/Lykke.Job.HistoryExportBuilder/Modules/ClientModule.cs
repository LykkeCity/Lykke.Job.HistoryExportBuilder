using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.HistoryExportBuilder.Settings;
using Lykke.Job.HistoryExportBuilder.Settings.JobSettings;
using Lykke.Service.Assets.Client;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class ClientModule : Module
    {
        private readonly AppSettings _settings;
        private readonly ILog _log;

        public ClientModule(AppSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterOperationsHistoryClient(_settings.OperationsHistoryServiceClient, _log);
        }
    }
}
