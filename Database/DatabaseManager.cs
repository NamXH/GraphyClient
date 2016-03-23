using System;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GraphyClient
{
    public class DatabaseManager
    {
        public SQLiteConnection DbConnection { get; private set; }

        private string _dbName = "";

        public string DbName
        {
            get { return _dbName; }
        }

        public string DbPath
        {
            get
            {
                var documentsDirectory = "/Users/Salemzzz/Documents/Xamarin/GraphyClient/GraphyClient/Actual_Databases";
                return Path.Combine(documentsDirectory, _dbName);
            }
        }

        public bool Exists()
        {
            return File.Exists(DbPath);
        }

        public bool Delete()
        {
            if (File.Exists(DbPath))
            {
                File.Delete(DbPath);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphyClient.DatabaseManager"/> class.
        /// Create data based on Amount.
        /// </summary>
        /// <param name="dbName">Db name.</param>
        /// <param name="amount">Amount must be an even number</param>
        public DatabaseManager(string dbName, int amount)
        {
            if (amount % 2 != 0)
            {
                return;
            }

            _dbName = dbName;

            if (!this.Exists())
            {
                DbConnection = new SQLiteConnection(DbPath);
                SetupSchema();
                CreateMassiveDataAndSyncQueue(dbName, amount);
            }
            else
            {
                DbConnection = new SQLiteConnection(DbPath);
            }
        }

        public DatabaseManager(string dbName)
        {
            _dbName = dbName;

            if (!this.Exists())
            {
                throw new Exception(String.Format("Database {0} does not exist", dbName));
            }
            else
            {
                DbConnection = new SQLiteConnection(DbPath);
            }
        }

        #region Setup Schema

        public void SetupSchema()
        {
            // Turn on Foreign Key support
            var foreignKeyOn = "PRAGMA foreign_keys = ON";
            DbConnection.Execute(foreignKeyOn); 

            // Create tables using SQL commands
            // It seems SQLite-net make query base on table name. Therefore, our custom tables still work with
            // their queries even the database objects may have more properties than the fields in the table.
            // For example: DbConnection.Insert(new Contact()) still insert to the Contact table.

            // ## !! IMPORTANT:
            // For fast prototyping we use Guid as Primary Key, and represent Guid in SQLite with Varchar.
            // Be aware of performance issue.
            // http://stackoverflow.com/questions/11938044/what-are-the-best-practices-for-using-a-guid-as-a-primary-key-specifically-rega
            // http://blog.codinghorror.com/primary-keys-ids-versus-guids/
            // Using Sqlite-net, we use Guid in C# code. When insert/query Guid will be automatically converted to Varchar and vice versa!!
            // If there is a weird behavior in the database, CHECK THIS CONVERSION!

            var createContact = "CREATE TABLE Contact (Id VARCHAR PRIMARY KEY NOT NULL, FirstName VARCHAR, MiddleName VARCHAR, LastName VARCHAR, Organization VARCHAR, ImageName VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0)";
            DbConnection.Execute(createContact);
            var createPhoneNumber = "CREATE TABLE PhoneNumber (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Number VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createPhoneNumber);
            var createEmail = "CREATE TABLE Email (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Address VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createEmail);

            var createTag = "CREATE TABLE Tag (Id VARCHAR PRIMARY KEY NOT NULL, Name VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0)";
            DbConnection.Execute(createTag);
            var createContactTagMap = "CREATE TABLE ContactTagMap (Id VARCHAR PRIMARY KEY NOT NULL, ContactId VARCHAR, TagId VARCHAR, Detail VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY(TagId) REFERENCES Tag(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createContactTagMap);
            var createRelationshipType = "CREATE TABLE RelationshipType (Id VARCHAR PRIMARY KEY NOT NULL, Name VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0)";
            DbConnection.Execute(createRelationshipType);
            var createRelationship = "CREATE TABLE Relationship (Id VARCHAR PRIMARY KEY NOT NULL, Detail VARCHAR, FromContactId VARCHAR, ToContactId VARCHAR, RelationshipTypeId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(FromContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY(ToContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY(RelationshipTypeId) REFERENCES RelationshipType(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createRelationship);

            // Comment out for simplicity !!
//            var createAddress = "CREATE TABLE Address (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, StreetLine1 VARCHAR, StreetLine2 VARCHAR, City VARCHAR, Province VARCHAR, PostalCode VARCHAR, Country VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
//            DbConnection.Execute(createAddress);
//            var createUrl = "CREATE TABLE Url (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Link VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
//            DbConnection.Execute(createUrl);
//            var createSpecialDate = "CREATE TABLE SpecialDate (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Date DATETIME, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
//            DbConnection.Execute(createSpecialDate);
//            var createInstantMessage = "CREATE TABLE InstantMessage (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Nickname VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
//            DbConnection.Execute(createInstantMessage);

            var createSyncOperation = "CREATE TABLE SyncOperation (Id VARCHAR PRIMARY KEY NOT NULL, Verb VARCHAR, ResourceEndpoint VARCHAR, ResourceId VARCHAR)";
            DbConnection.Execute(createSyncOperation);
        }

        #endregion

        #region Create massive dummy data and Sync Queue

        /// <summary>
        /// Creates the massive data and sync queue.
        /// Number of contacts equal to Amount. Number of tags, number of relationship types equal to amount / 2.
        /// Each contact has: first name, 1 phone number, 1 email.
        /// Each even contact has: 1 tag.
        /// Each relationship connect 1 odd contact to an even contact. E.g. 1->2, 3->4
        /// </summary>
        /// <param name="prefix">Prefix.</param>
        /// <param name="amount">Amount must be an even number.</param>
        public void CreateMassiveDataAndSyncQueue(string prefix, int amount)
        {
            if (amount % 2 != 0)
            {
                return;
            }

            Guid previousGuid = Guid.Empty;

            for (var i = 1; i <= amount; i++)
            {
                // Contacts and directly related info
                var contact = new Contact
                {
                    Id = Guid.NewGuid(),
                    FirstName = String.Format("{0}_Contact_{1}", prefix, i),
                    LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc), 
                };
                DbConnection.Insert(contact);

                DbConnection.Insert(new SyncOperation
                    {
                        Id = Guid.NewGuid(),
                        Verb = "Post",
                        ResourceEndpoint = "contacts",
                        ResourceId = contact.Id,
                    }
                );

                var phoneNumber = new PhoneNumber
                { 
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    Number = i.ToString(),
                    LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                };
                DbConnection.Insert(phoneNumber);

                DbConnection.Insert(new SyncOperation
                    {
                        Id = Guid.NewGuid(),
                        Verb = "Post",
                        ResourceEndpoint = "phone_numbers",
                        ResourceId = phoneNumber.Id,
                    }
                );

                var email = new Email
                {
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    Address = String.Format("{0}_Email_{1}", prefix, i),
                    LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                };
                DbConnection.Insert(email);

                DbConnection.Insert(new SyncOperation
                    {
                        Id = Guid.NewGuid(),
                        Verb = "Post",
                        ResourceEndpoint = "emails",
                        ResourceId = email.Id,
                    }
                );

//                var address = new Address
//                {
//                    Id = Guid.NewGuid(),
//                    ContactId = contact.Id,
//                    StreetLine1 = String.Format("{0}_Address_{1}", prefix, i),
//                    LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                };
//                DbConnection.Insert(address);
//
//                DbConnection.Insert(new SyncOperation
//                    {
//                        Id = Guid.NewGuid(),
//                        Verb = "Post",
//                        ResourceEndpoint = "addresses",
//                        ResourceId = address.Id,
//                    }
//                );

//                var im = new InstantMessage
//                {
//                    Id = Guid.NewGuid(),
//                    ContactId = contact.Id,
//                    Nickname = String.Format("{0}_InstantMessage_{1}", prefix, i),
//                    LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                };
//                DbConnection.Insert(im);
//
//                DbConnection.Insert(new SyncOperation
//                    {
//                        Id = Guid.NewGuid(),
//                        Verb = "Post",
//                        ResourceEndpoint = "instant_messages",
//                        ResourceId = im.Id,
//                    }
//                );

//                var specialDate = new SpecialDate
//                {
//                    Id = Guid.NewGuid(),
//                    ContactId = contact.Id,
//                    Date = new DateTime(1975, 4, 4),
//                    Type = String.Format("{0}_SpecialDate_{1}", prefix, i),
//                    LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
//                };
//                DbConnection.Insert(specialDate);
//
//                DbConnection.Insert(new SyncOperation
//                    {
//                        Id = Guid.NewGuid(),
//                        Verb = "Post",
//                        ResourceEndpoint = "special_dates",
//                        ResourceId = specialDate.Id,
//                    }
//                );

                // Tags and relationships
                if (i % 2 == 0)
                {
                    var currentGuid = contact.Id;

                    var tag = new Tag
                    { 
                        Id = Guid.NewGuid(),
                        Name = String.Format("{0}_Tag_{1}", prefix, i),
                        LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    };
                    DbConnection.Insert(tag);

                    DbConnection.Insert(new SyncOperation
                        {
                            Id = Guid.NewGuid(),
                            Verb = "Post",
                            ResourceEndpoint = "tags",
                            ResourceId = tag.Id,
                        }
                    );

                    var tagMap = new ContactTagMap
                    {
                        Id = Guid.NewGuid(),
                        TagId = tag.Id,
                        ContactId = currentGuid,
                        Detail = String.Format("{0}_TagMap_{1}", prefix, i),
                        LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    };
                    DbConnection.Insert(tagMap);

                    DbConnection.Insert(new SyncOperation
                        {
                            Id = Guid.NewGuid(),
                            Verb = "Post",
                            ResourceEndpoint = "contact_tag_maps",
                            ResourceId = tagMap.Id,
                        }
                    );

                    var relationshipType = new RelationshipType
                    {
                        Id = Guid.NewGuid(),
                        Name = String.Format("{0}_RelationshipType_{1}", prefix, i),
                        LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    };
                    DbConnection.Insert(relationshipType);

                    DbConnection.Insert(new SyncOperation
                        {
                            Id = Guid.NewGuid(),
                            Verb = "Post",
                            ResourceEndpoint = "relationship_types",
                            ResourceId = relationshipType.Id,
                        }
                    );

                    var relationship = new Relationship
                    {
                        Id = Guid.NewGuid(),
                        FromContactId = previousGuid,
                        ToContactId = currentGuid,
                        RelationshipTypeId = relationshipType.Id,
                        Detail = String.Format("{0}_Relationship_{1}", prefix, i),
                        LastModified = new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    };
                    DbConnection.Insert(relationship);

                    DbConnection.Insert(new SyncOperation
                        {
                            Id = Guid.NewGuid(),
                            Verb = "Post",
                            ResourceEndpoint = "relationships",
                            ResourceId = relationship.Id,
                        }
                    );
                }
                else
                {
                    previousGuid = contact.Id;
                }
            }
        }

        #endregion

        #region Utility Methods copied from project GraphyPCL

        /// <summary>
        /// Get all rows from a table
        /// </summary>
        /// <returns>The rows.</returns>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IList<T> GetRows<T>() where T : class, new()
        {
            return DbConnection.Table<T>().ToList();
        }

        /// <summary>
        /// Get a row according to its primary key
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetRow<T>(Guid id) where T : class, IIdContainer, new()
        {
            return DbConnection.Table<T>().Where(x => x.Id == id).FirstOrDefault();
        }

        public IList<T> GetRows<T>(IList<Guid> idList) where T : class, IIdContainer, new()
        {
            return DbConnection.Table<T>().Where(x => idList.Contains(x.Id)).ToList();
        }

        public IList<T> GetRowsRelatedToContact<T>(Guid contactId) where T : class, IContactIdRelated, new()
        {
            return DbConnection.Table<T>().Where(x => x.ContactId == contactId).ToList();
        }

        public IList<T> GetRowsByName<T>(string name) where T : class, INameContainer, new()
        {
            if (String.IsNullOrEmpty(name))
            {
                return new List<T>();
            }
            return DbConnection.Table<T>().Where(x => x.Name == name).ToList();
        }

        public IList<T> GetRowsContainNameIgnoreCase<T>(string name) where T : class, INameContainer, new()
        {
            if (String.IsNullOrEmpty(name))
            {
                return new List<T>();
            }

            //             String.Equals does not work
            //             return DbConnection.Table<T>().Where(x => String.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            // Workaround
            //            var result = new List<T>();
            //            var nameUpper = FirstLetterToUpper(name);
            //            var nameLower = FirstLetterToLower(name);
            //            result.AddRange(DbConnection.Table<T>().Where(x => x.Name == name));
            //            result.AddRange(DbConnection.Table<T>().Where(x => x.Name == nameUpper));
            //            result.AddRange(DbConnection.Table<T>().Where(x => x.Name == nameLower));

            // For some shocking reason: String.Contains ignore case in this situation !! It is actually what we want !!
            // We put in tolower() to prevent bugs later on. There is a faster solution using CulturalInfo. !!
            return DbConnection.Table<T>().Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();
        }

        /// <summary>
        /// Firsts the letter to upper. Helper method.
        /// </summary>
        /// <returns>The letter to upper.</returns>
        /// <param name="str">String.</param>
        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        /// <summary>
        /// Firsts the letter to lower. Helper method.
        /// </summary>
        /// <returns>The letter to lower.</returns>
        /// <param name="str">String.</param>
        public string FirstLetterToLower(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToLower(str[0]) + str.Substring(1);

            return str.ToLower();
        }


        /// <summary>
        /// Gets the relationships start from a contact to other contacts
        /// </summary>
        /// <returns>Relationships from the contact</returns>
        /// <param name="contactId">Contact identifier</param>
        public IList<Relationship> GetRelationshipsFromContact(Guid contactId)
        {
            return DbConnection.Table<Relationship>().Where(x => x.FromContactId == contactId).ToList();
        }

        /// <summary>
        /// Gets the relationships start from other contacts to a contact
        /// </summary>
        /// <returns>Relationships to the contact</returns>
        /// <param name="contactId">Contact identifier</param>
        public IList<Relationship> GetRelationshipsToContact(Guid contactId)
        {
            return DbConnection.Table<Relationship>().Where(x => x.ToContactId == contactId).ToList();
        }

        /// <summary>
        /// Inserts list of items related to a contact.
        /// </summary>
        /// <returns>The list.</returns>
        /// <param name="list">List.</param>
        /// <param name="contact">Contact.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public IList<Guid> InsertList<T>(IList<T> list, Contact contact) where T : IIdContainer, IContactIdRelated, new()
        {
            var createdGuids = new List<Guid>();
            foreach (var item in list)
            {
                item.Id = Guid.NewGuid();
                item.ContactId = contact.Id;
                DbConnection.Insert(item);
                createdGuids.Add(item.Id);
            }
            return createdGuids;
        }

        public void DeleteContactAndRelatedInfo(Guid contactId)
        {
            // PCL does not support reflection to call generic method so we have to copy&paste
            // http://stackoverflow.com/questions/232535/how-to-use-reflection-to-call-generic-method

            // Delete basic info
            var phoneNumbers = GetRowsRelatedToContact<PhoneNumber>(contactId);
            foreach (var element in phoneNumbers)
            {
                DbConnection.Delete<PhoneNumber>(element.Id);
            }
            var emails = GetRowsRelatedToContact<Email>(contactId);
            foreach (var element in emails)
            {
                DbConnection.Delete<Email>(element.Id);
            }
//            var addresses = GetRowsRelatedToContact<Address>(contactId);
//            foreach (var element in addresses)
//            {
//                DbConnection.Delete<>(element.Id);
//            }
//            var urls = GetRowsRelatedToContact<Url>(contactId);
//            foreach (var element in urls)
//            {
//                DbConnection.Delete<>(element.Id);
//            }
//            var dates = GetRowsRelatedToContact<SpecialDate>(contactId);
//            foreach (var element in dates)
//            {
//                DbConnection.Delete<>(element.Id);
//            }
//            var ims = GetRowsRelatedToContact<InstantMessage>(contactId);
//            foreach (var element in ims)
//            {
//                DbConnection.Delete<>(element.Id);
//            }

            // Delete contact-tag map, not delete tag even if it is only appear in this contact
            var contactTagMaps = GetRowsRelatedToContact<ContactTagMap>(contactId);
            foreach (var map in contactTagMaps)
            {
                DbConnection.Delete<ContactTagMap>(map.Id);
            }

            // Delete relationship, not delete relationship type
            var fromRelationships = GetRelationshipsFromContact(contactId);
            var toRelationships = GetRelationshipsToContact(contactId);
            foreach (var relationship in fromRelationships)
            {
                DbConnection.Delete<Relationship>(relationship.Id);
            }
            foreach (var relationship in toRelationships)
            {
                DbConnection.Delete<Relationship>(relationship.Id);
            }

            // Delete contact
            DbConnection.Delete<Contact>(contactId);
        }

        #endregion

        #region New Utility Methods

        /// <summary>
        /// Get a row according to its primary key
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T GetRowFast<T>(Guid id) where T : class, IIdContainer, new()
        {
            T result = null;

            try
            {
                result = DbConnection.Get<T>(id);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message); // result will be null if exception occur.
            }

            return result;
        }

        /// <summary>
        /// Get a sync op according to its resource Id. Same performance as DbConnection.Get.
        /// </summary>
        /// <returns>The row.</returns>
        /// <param name="id">Identifier.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public SyncOperation GetSyncOperationByResourceId(Guid resourceId)
        {
            return DbConnection.Table<SyncOperation>().Where(x => x.ResourceId == resourceId).SingleOrDefault(); // Each sync op has a distinct resource Id
        }

        /// <summary>
        /// Deletes the sync operation by resource identifier.
        /// </summary>
        /// <returns>Return the number of rows deleted.</returns>
        /// <param name="resourceId">Resource identifier.</param>
        public int DeleteSyncOperationByResourceId(Guid resourceId)
        {
            var syncOp = GetSyncOperationByResourceId(resourceId);
            if (syncOp != null)
            {
                return DbConnection.Delete<SyncOperation>(syncOp.Id);
            }
            else
            {
                return 0;
            }
        }

        public Type StringToType(string resourceEndpoint)
        {
            switch (resourceEndpoint)
            {
                case "contacts":
                    return typeof(Contact);
                    break;
                case "phone_numbers":
                    return typeof(PhoneNumber);
                    break;
                case "emails":
                    return typeof(Email);
                    break;
                case "tags":
                    return typeof(Tag);
                    break;
                case "contact_tag_maps":
                    return typeof(ContactTagMap);
                    break;
                case "relationship_types":
                    return typeof(RelationshipType);
                    break;
                case "relationships":
                    return typeof(Relationship);
                    break;
                default:
                    throw new Exception("Unregconized type: " + resourceEndpoint);
            }
        }

        public void DeleteContactAndRelatedInfoAndSyncOps(Guid contactId)
        {
            // PCL does not support reflection to call generic method so we have to copy&paste
            // http://stackoverflow.com/questions/232535/how-to-use-reflection-to-call-generic-method

            // Delete basic info
            var phoneNumbers = GetRowsRelatedToContact<PhoneNumber>(contactId);
            foreach (var element in phoneNumbers)
            {
                DeleteSyncOperationByResourceId(element.Id);
                DbConnection.Delete<PhoneNumber>(element.Id);
            }

            var emails = GetRowsRelatedToContact<Email>(contactId);
            foreach (var element in emails)
            {
                DeleteSyncOperationByResourceId(element.Id);
                DbConnection.Delete<Email>(element.Id);
            }

//            var addresses = GetRowsRelatedToContact<Address>(contactId);
//            foreach (var element in addresses)
//            {
//                DbConnection.Delete<>(element.Id);
//            }
//
//            var urls = GetRowsRelatedToContact<Url>(contactId);
//            foreach (var element in urls)
//            {
//                DbConnection.Delete<>(element.Id);
//            }
//            var dates = GetRowsRelatedToContact<SpecialDate>(contactId);
//            foreach (var element in dates)
//            {
//                DbConnection.Delete<>(element.Id);
//            }
//            var ims = GetRowsRelatedToContact<InstantMessage>(contactId);
//            foreach (var element in ims)
//            {
//                DbConnection.Delete<>(element.Id);
//            }

            // Delete contact-tag map, not delete tag even if it is only appear in this contact
            var contactTagMaps = GetRowsRelatedToContact<ContactTagMap>(contactId);
            foreach (var map in contactTagMaps)
            {
                DeleteSyncOperationByResourceId(map.Id);
                DbConnection.Delete<ContactTagMap>(map.Id);
            }

            // Delete relationship, not delete relationship type
            var fromRelationships = GetRelationshipsFromContact(contactId);
            var toRelationships = GetRelationshipsToContact(contactId);
            foreach (var relationship in fromRelationships)
            {
                DeleteSyncOperationByResourceId(relationship.Id);
                DbConnection.Delete<Relationship>(relationship.Id);
            }
            foreach (var relationship in toRelationships)
            {
                DeleteSyncOperationByResourceId(relationship.Id);
                DbConnection.Delete<Relationship>(relationship.Id);
            }

            // Delete contact
            DeleteSyncOperationByResourceId(contactId);
            DbConnection.Delete<Contact>(contactId);
        }

        public void DeleteResource(string resourceEndpoint, Guid resourceId)
        {
            switch (resourceEndpoint)
            {
                case "contacts":
                    DbConnection.Delete<Contact>(resourceId);
                    break;
                case "phone_numbers":
                    DbConnection.Delete<PhoneNumber>(resourceId);
                    break;
                case "emails":
                    DbConnection.Delete<Email>(resourceId);
                    break;
                case "tags":
                    DbConnection.Delete<Tag>(resourceId);
                    break;
                case "contact_tag_maps":
                    DbConnection.Delete<ContactTagMap>(resourceId);
                    break;
                case "relationship_types":
                    DbConnection.Delete<RelationshipType>(resourceId);
                    break;
                case "relationships":
                    DbConnection.Delete<Relationship>(resourceId);
                    break;
                default:
                    throw new Exception(String.Format("Unknown resource endpoint {0} ", resourceEndpoint));
                    break; 
            }
        }

        /// <returns>Tuple: The deserialized record object, IsDeleted property of the object.</returns>
        public Tuple<object, bool> DeserializeServerRecord(string jsonString, string resourceEndpoint)
        {
            switch (resourceEndpoint)
            {
                case "contacts":
                    {
                        var record = JsonConvert.DeserializeObject<Contact>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                case "phone_numbers":
                    {
                        var record = JsonConvert.DeserializeObject<PhoneNumber>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                case "emails":
                    {
                        var record = JsonConvert.DeserializeObject<Email>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                case "tags":
                    {
                        var record = JsonConvert.DeserializeObject<Tag>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                case "contact_tag_maps":
                    {
                        var record = JsonConvert.DeserializeObject<ContactTagMap>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                case "relationship_types":
                    {
                        var record = JsonConvert.DeserializeObject<RelationshipType>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                case "relationships":
                    {
                        var record = JsonConvert.DeserializeObject<Relationship>(jsonString);
                        return new Tuple<object, bool>(record, record.IsDeleted);
                    }
                    break;
                default:
                    throw new Exception(String.Format("Unknown resource endpoint {0} ", resourceEndpoint));
                    break; 
            } 
        }

        #endregion

        public async Task GetServerRecordsAsync()
        {
            #region Contacts
            Console.WriteLine("Step 1");
            var getRequestsResult = await SyncHelper.GetAsync<Contact>("contacts");
            Console.WriteLine("Step 2");

            if (getRequestsResult.Key != 200)
            {
                throw new Exception(String.Format("Get request fail with code: {0}. Phase: Get.", getRequestsResult.Key));
            }

            foreach (var serverContact in getRequestsResult.Value)
            {
//                var clientContact = GetRow<Contact>(serverContact.Id); // Old style, can be slower
                Contact clientContact = null;
                try
                {
                    clientContact = DbConnection.Get<Contact>(serverContact.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message); // clientContact will be null if exception occur.
                }

                // Server record has last-modified > client record last-modified (same ID) or client doesn't have that record?
                if ((clientContact == null) || (clientContact.LastModified < serverContact.LastModified))
                {
                    var syncOp = GetSyncOperationByResourceId(serverContact.Id);

                    // Sync Queue has an operation of that record (same ID): means there's un-synced modification on client?
                    if (syncOp != null)
                    {
                        if (syncOp.Verb == "Put")
                        {
                            if (clientContact == null)
                            {
                                throw new Exception("Record not exists while there is a assocciated Put. Phase: Get.");
                            }

                            DbConnection.Delete<SyncOperation>(syncOp.Id);

                            // Sync: Server record has lazy-delete=true?
                            if (serverContact.IsDeleted)
                            {
                                DeleteContactAndRelatedInfoAndSyncOps(clientContact.Id);
                            }
                            else
                            {
                                DbConnection.Update(serverContact);
                            }
                        }
                        else
                        {
                            // verb == "Delete". Post cannot happen.
                            DbConnection.Delete<SyncOperation>(syncOp.Id);

                            // Server record has lazy-delete=true?
                            if (!serverContact.IsDeleted)
                            {
                                DbConnection.Insert(serverContact);
                            }
                        }
                    }
                    else
                    {
                        // Client has the record (Record has an ID which client also have)?
                        if (clientContact != null)
                        {
                            // Server record has lazy-delete=true?
                            if (serverContact.IsDeleted)
                            {
                                DeleteContactAndRelatedInfoAndSyncOps(clientContact.Id);
                            }
                            else
                            {
                                DbConnection.Update(serverContact);
                            }
                        }
                        else
                        {
                            DbConnection.Insert(serverContact);
                        }
                    }
                }
            }
            #endregion

            Console.WriteLine("Step 3");
        }

        public async Task PerformSyncOperations()
        {
            var ops = GetRows<SyncOperation>();

            // Create list of tasks for all ops
            var tasks = new List<Task<Tuple<int, string, string, SyncOperation>>>();
            foreach (var op in ops)
            {
                object data;
                DateTime lastModified = DateTime.MinValue;

                switch (op.ResourceEndpoint) // Do this tedius because we don't want to use reflection!!
                {
                    case "contacts":
                        data = GetRowFast<Contact>(op.ResourceId);
                        lastModified = ((Contact)data).LastModified;
                        break;
                    case "phone_numbers":
                        data = GetRowFast<PhoneNumber>(op.ResourceId);
                        lastModified = ((PhoneNumber)data).LastModified;
                        break;
                    case "emails":
                        data = GetRowFast<Email>(op.ResourceId);
                        lastModified = ((Email)data).LastModified;
                        break;
                    case "tags":
                        data = GetRowFast<Tag>(op.ResourceId);
                        lastModified = ((Tag)data).LastModified;
                        break;
                    case "contact_tag_maps":
                        data = GetRowFast<ContactTagMap>(op.ResourceId);
                        lastModified = ((ContactTagMap)data).LastModified;
                        break;
                    case "relationship_types":
                        data = GetRowFast<RelationshipType>(op.ResourceId);
                        lastModified = ((RelationshipType)data).LastModified;
                        break;
                    case "relationships":
                        data = GetRowFast<Relationship>(op.ResourceId);
                        lastModified = ((Relationship)data).LastModified;
                        break;
                    default:
                        throw new Exception(String.Format("Unknown resource endpoint {0} on sync op {1}", op.ResourceEndpoint, op.Id));
                        break;
                }

                if ((data == null) || (lastModified == DateTime.MinValue))
                {
                    throw new Exception(String.Format("Cannot retrieve resource or resource's LastModified for op {0}", op.Id));
                }

                switch (op.Verb)
                {
                    case "Post":
                        tasks.Add(SyncHelper.PostAsync(op.ResourceEndpoint, data, op));
                        break;
                    case "Put":
                        tasks.Add(SyncHelper.PutAsync(op.ResourceEndpoint, op.ResourceId.ToString(), data, op));
                        break;
                    case "Delete":
                        tasks.Add(SyncHelper.DeleteAsync(op.ResourceEndpoint, op.ResourceId.ToString(), lastModified, op));
                        break;
                    default:
                        throw new Exception(String.Format("Wrong verb for operation: {0}. Phase: PostPutDelete.", op.Verb));
                }
            }

            // Process responses from server
            foreach (var result in await Task.WhenAll(tasks))
            {
                switch (result.Item3)
                {
                ////
                    case "Post":
                        if (result.Item1 == 201)
                        {
                            DbConnection.Delete<SyncOperation>(result.Item4.Id);
                        }
                        else
                        {
                            Console.WriteLine(String.Format("{0} return unhandled status code {1}, operation {2}", result.Item1, result.Item2, result.Item3));
                        }
                        break;

                ////
                    case "Put":
                        switch (result.Item1)
                        {
                            case 410:
                                DeleteResource(result.Item4.ResourceEndpoint, result.Item4.Id);
                                break;
                            case 409:
                                var tmp = DeserializeServerRecord(result.Item2, result.Item4.ResourceEndpoint);
                                var record = tmp.Item1;
                                var serverRecordIsDeleted = tmp.Item2;
                                if (serverRecordIsDeleted)
                                {
                                    DbConnection.Delete(record);
                                }
                                else
                                {
                                    DbConnection.Update(record);
                                }
                                break;
                            case 204:
                                // do nothing
                                break;
                            default:
                                Console.WriteLine(String.Format("{0} return unhandled status code {1}, operation {2}", result.Item1, result.Item2, result.Item3)); 
                                break;
                        }

                        DbConnection.Delete<SyncOperation>(result.Item4.Id);
                        break;
                    
                ////
                    case "Delete":
                        switch (result.Item1)
                        {
                            case 410:
                                // do nothing
                                break;
                            case 409:
                                var tmp = DeserializeServerRecord(result.Item2, result.Item4.ResourceEndpoint);
                                var record = tmp.Item1;
                                var serverRecordIsDeleted = tmp.Item2;
                                if (!serverRecordIsDeleted)
                                {
                                    DbConnection.Insert(record);
                                }
                                break;
                            case 204:
                                // do nothing
                                break;
                            default:
                                Console.WriteLine(String.Format("{0} return unhandled status code {1}, operation {2}", result.Item1, result.Item2, result.Item3)); 
                                break;
                        }

                        DbConnection.Delete<SyncOperation>(result.Item4.Id); 
                        break;

                ////
                    default:
                        throw new Exception(String.Format("Unknown returned verb: {0}.", result.Item3));
                }
            }
        }

        public async Task Sync()
        {
            Console.WriteLine("Step 0");
            await GetServerRecordsAsync();
            Console.WriteLine("Step 4");
            await PerformSyncOperations();
        }
    }
}

