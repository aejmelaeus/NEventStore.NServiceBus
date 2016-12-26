using NServiceBus;
using System.Reflection;
using NServiceBus.Routing;
using System.Collections.Generic;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Transport;

namespace Dispatcher
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
