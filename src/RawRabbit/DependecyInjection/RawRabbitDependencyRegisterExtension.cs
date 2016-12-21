﻿using System.Linq;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using RawRabbit.Channel;
using RawRabbit.Channel.Abstraction;
using RawRabbit.Common;
using RawRabbit.Configuration;
using RawRabbit.Configuration.BasicPublish;
using RawRabbit.Configuration.Consume;
using RawRabbit.Configuration.Consumer;
using RawRabbit.Configuration.Exchange;
using RawRabbit.Configuration.Publisher;
using RawRabbit.Configuration.Queue;
using RawRabbit.Consumer;
using RawRabbit.ErrorHandling;
using RawRabbit.Logging;
using RawRabbit.Pipe;
using RawRabbit.Serialization;

namespace RawRabbit.DependecyInjection
{
	public static class RawRabbitDependencyRegisterExtension
	{
		public static IDependecyRegister AddRawRabbit(this IDependecyRegister register)
		{
			register
				.AddSingleton(RawRabbitConfiguration.Local)
				.AddSingleton<IConnectionFactory, ConnectionFactory>(provider =>
				{
					var cfg = provider.GetService<RawRabbitConfiguration>();
					return new ConnectionFactory
					{
						VirtualHost = cfg.VirtualHost,
						UserName = cfg.Username,
						Password = cfg.Password,
						Port = cfg.Port,
						HostName = cfg.Hostnames.FirstOrDefault() ?? string.Empty,
						AutomaticRecoveryEnabled = cfg.AutomaticRecovery,
						TopologyRecoveryEnabled = cfg.TopologyRecovery,
						NetworkRecoveryInterval = cfg.RecoveryInterval,
						ClientProperties = provider.GetService<IClientPropertyProvider>().GetClientProperties(cfg),
						Ssl = cfg.Ssl
					};
				})
				.AddSingleton<IClientPropertyProvider, ClientPropertyProvider>()
				.AddSingleton<ILoggerFactory, LoggerFactory>()
				.AddTransient<ISerializer, Serialization.JsonSerializer>()
				.AddTransient(c => new Newtonsoft.Json.JsonSerializer
				{
					TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
					Formatting = Formatting.None,
					CheckAdditionalContent = true,
					ContractResolver = new CamelCasePropertyNamesContractResolver(),
					ObjectCreationHandling = ObjectCreationHandling.Auto,
					DefaultValueHandling = DefaultValueHandling.Ignore,
					TypeNameHandling = TypeNameHandling.All,
					ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
					MissingMemberHandling = MissingMemberHandling.Ignore,
					PreserveReferencesHandling = PreserveReferencesHandling.Objects,
					NullValueHandling = NullValueHandling.Ignore

				})
				.AddTransient<IConsumerFactory, ConsumerFactory>()
				.AddTransient<IErrorHandlingStrategy, DefaultStrategy>()
				.AddSingleton<IBasicPropertiesProvider, BasicPropertiesProvider>()
				.AddSingleton<IChannelFactory, ChannelFactory>()
				.AddSingleton<ChannelFactoryConfiguration, ChannelFactoryConfiguration>(c => ChannelFactoryConfiguration.Default)
				.AddSingleton<ITopologyProvider, TopologyProvider>()
				.AddTransient<IPublisherConfigurationFactory, PublisherConfigurationFactory>()
				.AddTransient<IBasicPublishConfigurationFactory, BasicPublishConfigurationFactory>()
				.AddTransient<IConsumerConfigurationFactory, ConsumerConfigurationFactory>()
				.AddTransient<IConsumeConfigurationFactory, ConsumeConfigurationFactory>()
				.AddTransient<IExchangeDeclarationFactory, ExchangeDeclarationFactory>()
				.AddTransient<IQueueConfigurationFactory, QueueDeclarationFactory>()
				.AddSingleton<INamingConventions, NamingConventions>()
				.AddSingleton<IExclusiveLock, ExclusiveLock>()
			
				.AddSingleton<IBusClient, BusClient>()
				.AddSingleton<IResourceDisposer, ResourceDisposer>()
				.AddSingleton<IPipeContextFactory, PipeContextFactory>()
				.AddSingleton<IExtendedPipeBuilder, PipeBuilder>()
				.AddSingleton<IPipeBuilderFactory>(provider => new CachedPipeBuilderFactory(() => new PipeBuilder(provider)));
			return register;
		}
	}
}
