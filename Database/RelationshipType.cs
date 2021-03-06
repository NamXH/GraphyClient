﻿using System;
using SQLite;

namespace GraphyClient
{
    public class RelationshipType : IIdContainer, INameContainer
    {
        [PrimaryKey]
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime LastModified { get; set; }

        public bool IsDeleted { get; set; }
    }
}

