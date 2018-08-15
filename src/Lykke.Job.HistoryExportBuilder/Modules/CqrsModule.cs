using System.Collections.Generic;
using Autofac;
using Common.Log;
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
using Lykke.Service.OperationsHistory.Client;

namespace Lykke.Job.HistoryExportBuilder.Modules
{
    public class CqrsModule : Module
    {
        private readonly AppSettings _settings;
        private readonly ILog _log;

        public CqrsModule(AppSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            string selfRoute = "self";
            string commandsRoute = "commands";
            string eventsRoute = "events";
            Messaging.Serialization.MessagePackSerializerFactory.Defaults.FormatterResolver = MessagePack.Resolvers.ContractlessStandardResolver.Instance;
            var rabbitMqSagasSettings = new RabbitMQ.Client.ConnectionFactory { Uri = _settings.SagasRabbitMq.RabbitConnectionString };

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>();

            builder
                .RegisterType<ExportClientHistoryCommandHandler>()
                .SingleInstance()
                .WithParameter("ttl", _settings.HistoryExportBuilderJob.GeneratedFileTtl);

            builder.RegisterType<ExpiryProjection>().SingleInstance();
            
            builder.RegisterInstance(new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {"RabbitMq", new TransportInfo(rabbitMqSagasSettings.Endpoint.ToString(), rabbitMqSagasSettings.UserName, rabbitMqSagasSettings.Password, "None", "RabbitMq")}
                }),
                new RabbitMqTransportFactory())).As<IMessagingEngine>();;
            
            builder.Register(ctx => new CqrsEngine(_log,
                    ctx.Resolve<IDependencyResolver>(),
                    ctx.Resolve<IMessagingEngine>(),
                    new DefaultEndpointProvider(),
                    true,
                    Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                        "RabbitMq",
                        "messagepack",
                        environment: "lykke",
                        exclusiveQueuePostfix: "k8s")),
                    
                    Register.BoundedContext(HistoryExportBuilderBoundedContext.Name)
                        .ListeningCommands(typeof(ExportClientHistoryCommand))
                        .On("commands")
                        .WithCommandsHandler<ExportClientHistoryCommandHandler>()
                        .PublishingEvents(
                            typeof(ClientHistoryExportedEvent),
                            typeof(ClientHistoryExpiredEvent))
                        .With("events")
                        .WithLoopback()
                        .WithProjection(typeof(ExpiryProjection), HistoryExportBuilderBoundedContext.Name)
                ))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }
    }
}
