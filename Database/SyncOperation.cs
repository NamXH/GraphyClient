using System;
using SQLite;

namespace GraphyClient
{
    public class SyncOperation : IIdContainer
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Verb { get; set; }

        public string ResourceEndpoint { get; set; }

        public Guid ResourceId { get; set; }
    }
}

