using System;
using SQLite;

namespace GraphyClient
{
    public class ContactTagMap : IIdContainer, IContactIdRelated
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Detail { get; set; }

        public Guid ContactId { get; set; }

        public Guid TagId { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }
    }
}

