using System;
using SQLite;

namespace GraphyClient
{
    public class Contact
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public string Organization { get; set; }

        public string ImageName { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }

        public Contact()
        {
        }
    }
}