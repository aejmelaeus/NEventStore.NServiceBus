using System;
using NServiceBus;
using NServiceBus.Logging;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Dispatcher;
using NEventStore;
using NEventStore.Persistence;
using NEventStore.Persistence.Sql.SqlDialects;
using NServiceBus.Features;
using NServiceBus.Persistence;
using NServiceBus.Persistence.Legacy;
using Tests.Messages;
using IContainer = Autofac.IContainer;

namespace Tests.Acceptance.Endpoint
{
    [DesignerCategory("Code")]
    internal class ProgramService : ServiceBase
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariables().Contains("APPVEYOR")
            ? @"Server=(local)\SQL2014;Initial Catalog=Dispatcher;User ID=sa;Password=Password12!"
            : @"Data Source=SE-UTV28172; Initial Catalog=Dispatcher; Integrated Security=True";

        private IEndpointInstance _endpoint;

        private static readonly ILog Logger = LogManager.GetLogger<ProgramService>();

        private static void Main()
        {
            using (var service = new ProgramService())
            {
                // to run interactive from a console or as a windows service
                if (Environment.UserInteractive)
                {
                    Console.CancelKeyPress += (sender, e) =>
                    {
                        service.OnStop();
                    };
                    service.OnStart(null);
                    Console.WriteLine("\r\nPress enter key to stop program\r\n");
                    Console.Read();
                    service.OnStop();
                    return;
                }
                Run(service);
            }
        }

        protected override void OnStart(string[] args)
        {
            AsyncOnStart().GetAwaiter().GetResult();
        }

        private async Task AsyncOnStart()
        {
            try
            {
                var container = GetContainer();

                var endpointConfiguration = new EndpointConfiguration("Tests.Acceptance.Endpoint");

                endpointConfiguration.UseSerialization<JsonSerializer>();
                endpointConfiguration.AutoSubscribe();
                endpointConfiguration.SendFailedMessagesTo("error");
                endpointConfiguration.AuditProcessedMessagesTo("audit");
                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
                endpointConfiguration.UsePersistence<NHibernatePersistence>();
                endpointConfiguration.EnableInstallers();
                endpointConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));

                // TODO - move this to Dispatcher
                var transport = endpointConfiguration.UseTransport<MsmqTransport>();
                transport.Routing().RouteToEndpoint(typeof(StartDispatching), "Tests.Acceptance.Endpoint");
                transport.Routing().RouteToEndpoint(typeof(DispatchEvents), "Tests.Acceptance.Endpoint");
                transport.Routing().RouteToEndpoint(typeof(DispatchEventsComplete), "Tests.Acceptance.Endpoint");
                transport.Routing().RegisterPublisher(typeof(EventHappened), "Tests.Acceptance.Endpoint");

                var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
                persistence.ConnectionString(_connectionString);

                _endpoint = await NServiceBus.Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

                await StartDispatching();
            }
            catch (Exception exception)
            {
                Logger.Fatal("Failed to start", exception);
                Environment.FailFast("Failed to start", exception);
            }
        }

        private IContainer GetContainer()
        {
            var bldr = new ContainerBuilder();

            var eventStore = GetEventSource();

            bldr.RegisterInstance(eventStore)
                .As<IStoreEvents>();

            bldr.RegisterInstance(eventStore.Advanced)
                .As<IPersistStreams>();

            var container = bldr.Build();

            return container;
        }

        private IStoreEvents GetEventSource()
        {
            return Wireup
                .Init()
                .UsingSqlPersistence("SequencedAggregate", "System.Data.SqlClient", _connectionString)
                    .WithDialect(new MsSqlDialect())
                    .EnlistInAmbientTransaction()
                .InitializeStorageEngine()
                    .UsingJsonSerialization()
                    .Compress()
                .Build();
        }

        private async Task StartDispatching()
        {
            await _endpoint.SendLocal(new StartDispatching
            {
                BucketId = "default",
                TimeoutInMilliseconds = 1000,
                MessageCatalogAssemblyName = "Tests.Messages"
            }).ConfigureAwait(false);
        }

        private static Task OnCriticalError(ICriticalErrorContext context)
        {
            var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
            Logger.Fatal(fatalMessage, context.Exception);
            Environment.FailFast(fatalMessage, context.Exception);
            return Task.FromResult(0);
        }

        protected override void OnStop()
        {
            _endpoint?.Stop().GetAwaiter().GetResult();
        }
    }
}