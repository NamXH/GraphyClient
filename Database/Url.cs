using System;
using SQLite;

namespace GraphyClient
{
    public class Url
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Type { get; set; }

        public string Link { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }

        public Url()
        {
        }
    }
}

