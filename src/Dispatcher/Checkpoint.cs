using System;
using System.Collections.Generic;
using System.Linq;

namespace NEventStore.NServiceBus
{
    internal class Checkpoint
    {
        private readonly List<ICommit> _commits;
        private readonly string _currentCheckpointToken;
        private readonly string _messageCatalogAssemblyName;

        public Checkpoint(IEnumerable<ICommit> commits, string currentCheckpointToken, string messageCatalogAssemblyName)
        {
            _commits = commits.ToList();
            _currentCheckpointToken = currentCheckpointToken;
            _messageCatalogAssemblyName = messageCatalogAssemblyName;
        }

        public string NewCheckpointToken => _commits.Any() ? _commits.Last().CheckpointToken : _currentCheckpointToken;

        public IEnumerable<object> EventsToPublish
        {
            get { return _commits.SelectMany(c => c.Events)
                                 .Select(e => e.Body)
                                 .Where(ShouldBePublished); }
        }

        public bool ShouldBePublished(object body)
        {
            string assemblyName = body.GetType().Assembly.GetName().Name;

            return string.Equals(assemblyName, _messageCatalogAssemblyName, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}