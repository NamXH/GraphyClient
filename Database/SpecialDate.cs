using System;
using SQLite;

namespace GraphyClient
{
    public class SpecialDate
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Type { get; set; }

        public DateTime Date { get; set; }

        public Guid ContactId { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }

        public SpecialDate()
        {
        }
    }
}

