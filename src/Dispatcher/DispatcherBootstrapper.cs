using System.Reflection;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;

namespace NEventStore.NServiceBus
{
    public static class DispatcherBootstrapper
    {
        public static void ConfigureDispatcherRouting(this EndpointConfiguration endpointConfiguration, RoutingSettings routingSettings)
        {
            var endpointName = endpointConfiguration.GetSettings().EndpointName();

            routingSettings.RouteToEndpoint(typeof(StartDispatching), endpointName);
            routingSettings.RouteToEndpoint(typeof(DispatchEvents), endpointName);
            routingSettings.RouteToEndpoint(typeof(DispatchEventsComplete), endpointName);
        }

        public static void StartDispatcher(this IEndpointInstance endpointInstance, int pollingIntervalMilliseconds, 
            Assembly messageCatalog, string buckedId = "default")
        {
            endpointInstance.Send(new StartDispatching
            {
                BucketId = buckedId,
                MessageCatalogAssemblyName = messageCatalog.GetName().Name,
                TimeoutInMilliseconds = pollingIntervalMilliseconds
            });
        }
    }
}
