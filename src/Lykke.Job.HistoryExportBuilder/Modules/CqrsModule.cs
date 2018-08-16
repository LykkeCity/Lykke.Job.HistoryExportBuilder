using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.HistoryExportBuilder.Contract;
using Lykke.Job.HistoryExportBuilder.Contract.Commands;
using Lykke.Job.HistoryExportBuilder.Contract.Events;
using Lykke.Job.HistoryExportBuilder.Core.Services;
using Lykke.Job.HistoryExportBuilder.Cqrs.CommandHandlers;
using Lykke.Job.HistoryExportBuilder.Cqrs.Projections;
using Lykke.Job.HistoryExportBuilder.Settings;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.OperationsHistory.Client;
using Lykke.SettingsReader;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class CqrsModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;

        public CqrsModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            string selfRoute = "self";
            string commandsRoute = "commands";
            string eventsRoute = "events";
            MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSagasSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.CurrentValue.SagasRabbitMq.RabbitConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>();

            builder
                .RegisterType<ExportClientHistoryCommandHandler>()
                .SingleInstance()
                .WithParameter("ttl", _settings.CurrentValue.HistoryExportBuilderJob.GeneratedFileTtl);

            builder.RegisterType<ExpiryProjection>().SingleInstance();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();

                    return new MessagingEngine(
                        logFactory,
                        new TransportResolver(new Dictionary<string, TransportInfo>
                        {
                            {
                                "RabbitMq",
                                new TransportInfo(rabbitMqSagasSettings.Endpoint.ToString(),
                                    rabbitMqSagasSettings.UserName, rabbitMqSagasSettings.Password, "None", "RabbitMq")
                            }
                        }),
                        new RabbitMqTransportFactory(logFactory));
                })
                .As<IMessagingEngine>()
                .SingleInstance();
            
            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();
                    
                    return new CqrsEngine(logFactory,
                        ctx.Resolve<IDependencyResolver>(),
                        ctx.Resolve<IMessagingEngine>(),
                        new DefaultEndpointProvider(),
                        true,
                        Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                            "RabbitMq",
                            SerializationFormat.MessagePack,
                            environment: "lykke",
                            exclusiveQueuePostfix: "k8s")),
                        Register.BoundedContext(HistoryExportBuilderBoundedContext.Name)
                            .ListeningCommands(typeof(ExportClientHistoryCommand))
                            .On(commandsRoute)
                            .WithCommandsHandler<ExportClientHistoryCommandHandler>()
                            .PublishingEvents(
                                typeof(ClientHistoryExportedEvent),
                                typeof(ClientHistoryExpiredEvent))
                            .With(eventsRoute)
                            .WithLoopback()
                            .WithProjection(typeof(ExpiryProjection), HistoryExportBuilderBoundedContext.Name)
                    );
                })
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
