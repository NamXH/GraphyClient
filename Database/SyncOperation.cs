using System;
using SQLite;

namespace GraphyClient
{
    public class SyncOperation
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Verb { get; set; }

        public string ResourceName { get; set; }

        public Guid ResourceId { get; set; }
    }
}

