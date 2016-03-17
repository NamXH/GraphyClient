using System;
using SQLite;

namespace GraphyClient
{
    public class Tag : IIdContainer, INameContainer
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }
    }
}

