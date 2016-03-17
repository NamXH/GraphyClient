using System;
using SQLite;

namespace GraphyClient
{
    public class PhoneNumber : IIdContainer, IContactIdRelated
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Number { get; set; }

        public Guid ContactId { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }
    }
}

