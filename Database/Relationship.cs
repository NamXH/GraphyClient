using System;
using SQLite;

namespace GraphyClient
{
    public class Relationship
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Detail { get; set; }

        public Guid FromContactId { get; set; }

        public Guid ToContactId { get; set; }

        public Guid RelationshipTypeId { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }

        public Relationship()
        {
        }
    }
}

