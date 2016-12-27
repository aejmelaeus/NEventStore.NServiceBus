using System;
using Autofac;
using NServiceBus;
using NEventStore;
using Tests.Messages;
using NServiceBus.Logging;
using System.ComponentModel;
using System.ServiceProcess;
using System.Threading.Tasks;
using NEventStore.NServiceBus;
using NServiceBus.Persistence;
using IContainer = Autofac.IContainer;
using NEventStore.Persistence.Sql.SqlDialects;

namespace Tests.Acceptance.Endpoint
{
    [DesignerCategory("Code")]
    internal class ProgramService : ServiceBase
    {
        private readonly string _connectionString = Environment.GetEnvironmentVariables().Contains("APPVEYOR")
            ? @"Server=(local)\SQL2014;Initial Catalog=Dispatcher;User ID=sa;Password=Password12!"
            : @"Data Source=<FIX>; Initial Catalog=Dispatcher; Integrated Security=True";

        private IEndpointInstance _endpointInstance;

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

                var transport = endpointConfiguration.UseTransport<MsmqTransport>();
                endpointConfiguration.ConfigureDispatcherRouting(transport.Routing());

                endpointConfiguration.UseSerialization<JsonSerializer>();
                endpointConfiguration.AutoSubscribe();
                endpointConfiguration.SendFailedMessagesTo("error");
                endpointConfiguration.AuditProcessedMessagesTo("audit");
                endpointConfiguration.DefineCriticalErrorAction(OnCriticalError);
                endpointConfiguration.UsePersistence<NHibernatePersistence>();
                endpointConfiguration.EnableInstallers();
                endpointConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));
                

                transport.Routing().RegisterPublisher(typeof(EventHappened), "Tests.Acceptance.Endpoint");

                var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
                persistence.ConnectionString(_connectionString);

                _endpointInstance = await NServiceBus.Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

                var pollingIntervalMilliseconds = 1000;
                var messageCatalog = typeof (EventBase).Assembly;
                _endpointInstance.StartDispatcher(pollingIntervalMilliseconds, messageCatalog);
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

            IStoreEvents eventStore = GetEventStore();

            bldr.RegisterInstance(eventStore)
                .As<IStoreEvents>();

            var container = bldr.Build();

            return container;
        }

        private IStoreEvents GetEventStore()
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

        private static Task OnCriticalError(ICriticalErrorContext context)
        {
            var fatalMessage = $"The following critical error was encountered:\n{context.Error}\nProcess is shutting down.";
            Logger.Fatal(fatalMessage, context.Exception);
            Environment.FailFast(fatalMessage, context.Exception);
            return Task.FromResult(0);
        }

        protected override void OnStop()
        {
            _endpointInstance?.Stop().GetAwaiter().GetResult();
        }
    }
}