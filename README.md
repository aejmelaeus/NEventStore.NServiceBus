# NEventStore.NServiceBus

NServiceBus dispatcher for NEventStore. Uses a NServiceBus Saga to dispatch events from NEventStore.

## The Saga

The NServiceBus Saga implements a timeout to dispatch events from NEventStore. There can be one Saga per BucketId in NEventStore. It mimics the PollingClient example in NEventStore and uses the `CheckpointToken` to keep track of the process.

### Transactions

The `DispatchEventsHandler` is transactional since it only handles NServiceBus resources using the context.

This allows for transactional processing. All or nothing gets published.

The `CheckpointToken` is stored in the Saga Data, so whenever the Endpoint restarts, the Dispatcher will pick up where it left off.

## Usage

In a `Endpoint` of your choice, install the package from NuGet:

    Install-Package NEventStore.NServiceBus

Then head off to where you wire up your `Endpoint` to configure the `Dispatcher`:

    var transport = endpointConfiguration.UseTransport<MsmqTransport>(); // Or whatever you prefer!
    endpointConfiguration.ConfigureDispatcherRouting(transport.Routing());

Then the actual start (it sends a command to the Saga):

    var pollingIntervalMilliseconds = 1000;
    var messageCatalog = typeof (EventBase).Assembly;
    _endpointInstance.StartDispatcher(pollingIntervalMilliseconds, messageCatalog);

### Almost done!

The final step is to wire up the `IStoreEvents` instance in the container:

    IStoreEvents eventStore = GetEventStore();

    bldr.RegisterInstance(eventStore)
        .As<IStoreEvents>();

This is how it could look like if you use `AutoFac`

## Rough around the edges

The current version of the Dispatcher is a bit naive and makes some assumptions that you need to be aware of:

* The Dispatcher is designed with Eventual Consistency in mind. Given the nature of a polling, the process is not in real time, so it should probably not be used to build real time projections, but rather to Publish events to other Bounded Contexts.
* It will publish all Commits in the Event Store who's type is declared in the `MessageCatalog` Assembly. Please make sure that `NServiceBus` interprets those as events, or it will get angry.
* If something goes wrong the Saga will halt, so keep an eye on the Error queue and make sure to restart the Saga process if (when) something goes wrong.
* The query limit in the `GetCheckpoint` method in NEventStore returns `512` commits, so if you have a lot of data, the initial processing might take some time.