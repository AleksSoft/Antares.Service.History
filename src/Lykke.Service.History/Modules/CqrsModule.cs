﻿using System.Collections.Generic;
using Autofac;
using Lykke.Bitcoin.Contracts;
using Lykke.Bitcoin.Contracts.Events;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.BlockchainCashinDetector.Contract;
using Lykke.Job.BlockchainCashoutProcessor.Contract;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Sdk;
using Lykke.Service.History.Settings;
using Lykke.Service.History.Workflow.ExecutionProcessing;
using Lykke.Service.History.Workflow.Handlers;
using Lykke.Service.History.Workflow.Projections;
using Lykke.Service.PostProcessing.Contracts.Cqrs;
using Lykke.Service.PostProcessing.Contracts.Cqrs.Events;
using Lykke.SettingsReader;
using RabbitMQ.Client;

namespace Lykke.Service.History.Modules
{
    public class CqrsModule : Module
    {
        private readonly CqrsSettings _settings;

        public CqrsModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.HistoryService.Cqrs;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>()
                .SingleInstance();

            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = _settings.RabbitConnString
            };
            var rabbitMqEndpoint = rabbitMqSettings.Endpoint.ToString();

            builder.RegisterType<StartupManager>().As<IStartupManager>();

            builder.RegisterType<ExecutionQueueReader>()
                .WithParameter(TypedParameter.From(_settings.RabbitConnString))
                .SingleInstance();

            builder.RegisterType<OrderEventQueueReader>()
                .WithParameter(TypedParameter.From(_settings.RabbitConnString))
                .SingleInstance();

            builder.RegisterType<CashInProjection>();
            builder.RegisterType<CashOutProjection>();
            builder.RegisterType<CashTransferProjection>();
            builder.RegisterType<TransactionHashProjection>();

            builder.RegisterType<EthereumCommandHandler>();

            builder.Register(ctx =>
                {
                    var logFactory = ctx.Resolve<ILogFactory>();
                    var messagingEngine = new MessagingEngine(
                        logFactory,
                        new TransportResolver(
                            new Dictionary<string, TransportInfo>
                            {
                                {
                                    "RabbitMq",
                                    new TransportInfo(
                                        rabbitMqEndpoint,
                                        rabbitMqSettings.UserName,
                                        rabbitMqSettings.Password, "None", "RabbitMq")
                                }
                            }),
                        new RabbitMqTransportFactory(logFactory));
                    return CreateEngine(ctx, messagingEngine, logFactory);
                })
                .As<ICqrsEngine>()
                .AutoActivate()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(
            IComponentContext ctx,
            IMessagingEngine messagingEngine,
            ILogFactory logFactory)
        {
            const string boundedContext = "history";
            const string defaultRoute = "self";

            var sagasMessagePackEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                SerializationFormat.MessagePack,
                environment: "lykke");

            var sagasProtobufEndpointResolver = new RabbitMqConventionEndpointResolver(
                "RabbitMq",
                SerializationFormat.ProtoBuf,
                environment: "lykke");

            return new CqrsEngine(
                logFactory,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(sagasProtobufEndpointResolver),
                Register.BoundedContext(boundedContext)
                    .ListeningEvents(typeof(CashInProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashInProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashOutProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashOutProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashTransferProcessedEvent))
                    .From(PostProcessingBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(CashTransferProjection), PostProcessingBoundedContext.Name)

                    .ListeningEvents(typeof(CashoutCompletedEvent), typeof(CashinCompletedEvent))
                    .From(BitcoinBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BitcoinBoundedContext.Name)

                    .ListeningEvents(typeof(Job.BlockchainCashinDetector.Contract.Events.CashinCompletedEvent))
                    .From(BlockchainCashinDetectorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BlockchainCashinDetectorBoundedContext.Name)

                    .ListeningEvents(typeof(Job.BlockchainCashoutProcessor.Contract.Events.CashoutCompletedEvent))
                    .From(BlockchainCashoutProcessorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithProjection(typeof(TransactionHashProjection), BlockchainCashoutProcessorBoundedContext.Name),

                Register.BoundedContext("tx-handler.ethereum.commands")
                    .ListeningCommands(typeof(SaveEthInHistoryCommand), typeof(ProcessEthCoinEventCommand),
                        typeof(ProcessHotWalletErc20EventCommand))
                    .On("history")
                    .WithEndpointResolver(sagasMessagePackEndpointResolver)
                    .WithCommandsHandler<EthereumCommandHandler>());
        }
    }
}