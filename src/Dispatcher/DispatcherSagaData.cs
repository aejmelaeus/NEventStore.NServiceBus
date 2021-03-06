﻿using NServiceBus;

namespace NEventStore.NServiceBus
{
    public class DispatcherSagaData : ContainSagaData
    {
        public virtual string CheckpointToken { get; set; }
        public virtual bool HasStarted { get; set; }
        public virtual string BucketId { get; set; }
        public virtual int TimeOutInMilliseconds { get; set; }
        public virtual string MessageCatalogAssemblyName { get; set; }
    }
}