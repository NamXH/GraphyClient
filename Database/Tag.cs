using System;
using SQLite;

namespace GraphyClient
{
    public class Tag
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }

        public Tag()
        {
        }
    }
}

