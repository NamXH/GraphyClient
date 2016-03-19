using System;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public DatabaseManager(string dbName)
        {
            _dbName = dbName;

            if (!this.Exists())
            {
                DbConnection = new SQLiteConnection(DbPath);
                SetupSchema();
            }
            else
            {
                DbConnection = new SQLiteConnection(DbPath);
            }
        }

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
            var createAddress = "CREATE TABLE Address (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, StreetLine1 VARCHAR, StreetLine2 VARCHAR, City VARCHAR, Province VARCHAR, PostalCode VARCHAR, Country VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createAddress);
            var createEmail = "CREATE TABLE Email (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Address VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createEmail);
            var createSpecialDate = "CREATE TABLE SpecialDate (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Date DATETIME, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createSpecialDate);
            var createInstantMessage = "CREATE TABLE InstantMessage (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Nickname VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createInstantMessage);
            var createTag = "CREATE TABLE Tag (Id VARCHAR PRIMARY KEY NOT NULL, Name VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0)";
            DbConnection.Execute(createTag);
            var createContactTagMap = "CREATE TABLE ContactTagMap (Id VARCHAR PRIMARY KEY NOT NULL, ContactId VARCHAR, TagId VARCHAR, Detail VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY(TagId) REFERENCES Tag(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createContactTagMap);
            var createRelationshipType = "CREATE TABLE RelationshipType (Id VARCHAR PRIMARY KEY NOT NULL, Name VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0)";
            DbConnection.Execute(createRelationshipType);
            var createRelationship = "CREATE TABLE Relationship (Id VARCHAR PRIMARY KEY NOT NULL, Detail VARCHAR, FromContactId VARCHAR, ToContactId VARCHAR, RelationshipTypeId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(FromContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY(ToContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE, FOREIGN KEY(RelationshipTypeId) REFERENCES RelationshipType(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
            DbConnection.Execute(createRelationship);
//            var createUrl = "CREATE TABLE Url (Id VARCHAR PRIMARY KEY NOT NULL, Type VARCHAR, Link VARCHAR, ContactId VARCHAR, LastModified DATETIME, IsDeleted BOOL DEFAULT 0, FOREIGN KEY(ContactId) REFERENCES Contact(Id) ON DELETE CASCADE ON UPDATE CASCADE)";
//            DbConnection.Execute(createUrl);

            var createSyncOperation = "CREATE TABLE SyncOperation (Id VARCHAR PRIMARY KEY NOT NULL, Verb VARCHAR, ResourceName VARCHAR, ResourceId VARCHAR)";
            DbConnection.Execute(createSyncOperation);
        }

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

        public void DeleteContact(Guid contactId)
        {
            // PCL does not support reflection to call generic method so we have to copy&paste
            // http://stackoverflow.com/questions/232535/how-to-use-reflection-to-call-generic-method

            // Delete basic info
            var phoneNumbers = GetRowsRelatedToContact<PhoneNumber>(contactId);
            foreach (var element in phoneNumbers)
            {
                DbConnection.Delete(element);
            }
            var emails = GetRowsRelatedToContact<Email>(contactId);
            foreach (var element in emails)
            {
                DbConnection.Delete(element);
            }
            var addresses = GetRowsRelatedToContact<Address>(contactId);
            foreach (var element in addresses)
            {
                DbConnection.Delete(element);
            }
//            var urls = GetRowsRelatedToContact<Url>(contactId);
//            foreach (var element in urls)
//            {
//                DbConnection.Delete(element);
//            }
            var dates = GetRowsRelatedToContact<SpecialDate>(contactId);
            foreach (var element in dates)
            {
                DbConnection.Delete(element);
            }
            var ims = GetRowsRelatedToContact<InstantMessage>(contactId);
            foreach (var element in ims)
            {
                DbConnection.Delete(element);
            }

            // Delete contact-tag map, not delete tag even if it is only appear in this contact
            var contactTagMaps = GetRowsRelatedToContact<ContactTagMap>(contactId);
            foreach (var map in contactTagMaps)
            {
                DbConnection.Delete(map);
            }

            // Delete relationship, not delete relationship type
            var fromRelationships = GetRelationshipsFromContact(contactId);
            var toRelationships = GetRelationshipsToContact(contactId);
            foreach (var relationship in fromRelationships)
            {
                DbConnection.Delete(relationship);
            }
            foreach (var relationship in toRelationships)
            {
                DbConnection.Delete(relationship);
            }

            // Delete contact
            DbConnection.Delete<Contact>(contactId);
        }

        #endregion

        #region Create dummy data

        /// <summary>
        /// Creates the dummy data for test.
        /// </summary>
        public void CreateDummyData()
        {
            Console.WriteLine("start adding data");

            DbConnection.Insert(new Tag
                {
                    Id = Guid.NewGuid(),
                    Name = "Colleague",
                });
            DbConnection.Insert(new Tag
                { 
                    Id = Guid.NewGuid(),
                    Name = "Important",
                });
            DbConnection.Insert(new Tag
                { 
                    Id = Guid.NewGuid(),
                    Name = "Created Date",
                });
            DbConnection.Insert(new Tag
                { 
                    Id = Guid.NewGuid(),
                    Name = "Created Location",
                });

            DbConnection.Insert(new RelationshipType
                {
                    Id = Guid.NewGuid(),
                    Name = "Advisor",
                });
            DbConnection.Insert(new RelationshipType
                {
                    Id = Guid.NewGuid(),
                    Name = "Daughter",
                });

            // Contacts
            var contact1 = new Contact();
            contact1.Id = Guid.NewGuid();
            contact1.FirstName = "Andy";
            contact1.LastName = "Rubin";
            contact1.ImageName = "andy.jpg";
            //            contact1.IsActive = true;
            DbConnection.Insert(contact1);

            var contact2 = new Contact();
            contact2.Id = Guid.NewGuid();
            contact2.FirstName = "Bill";
            contact2.MiddleName = "Henry";
            contact2.LastName = "Gates";
            contact2.Organization = "Microsoft";
            contact2.ImageName = "bill.jpg";
            //            contact2.IsActive = true;
            DbConnection.Insert(contact2);

            var contact3 = new Contact();
            contact3.Id = Guid.NewGuid();
            contact3.FirstName = "ben";
            contact3.LastName = "Afflect";
            DbConnection.Insert(contact3);

            var contact4 = new Contact();
            contact4.Id = Guid.NewGuid();
            contact4.FirstName = "Satya";
            contact4.LastName = "Nandela";
            //            contact4.IsActive = true;
            DbConnection.Insert(contact4);

            var contact5 = new Contact();
            contact5.Id = Guid.NewGuid();
            contact5.FirstName = "Zelda";
            DbConnection.Insert(contact5);

            var contact6 = new Contact();
            contact6.Id = Guid.NewGuid();
            contact6.FirstName = "Jennifer";
            contact6.LastName = "Gates";
            DbConnection.Insert(contact6);

            var contact7 = new Contact();
            contact7.Id = Guid.NewGuid();
            contact7.FirstName = "Maple";
            DbConnection.Insert(contact7);

            var contact8 = new Contact();
            contact8.Id = Guid.NewGuid();
            contact8.FirstName = "George";
            DbConnection.Insert(contact8);

            var contact9 = new Contact();
            contact9.Id = Guid.NewGuid();
            contact9.Organization = "Null Org.";
            DbConnection.Insert(contact9);

            var contact10 = new Contact();
            contact10.Id = Guid.NewGuid();
            contact10.FirstName = "";
            DbConnection.Insert(contact10);

            // Phone numbers
            var number1 = new PhoneNumber();
            number1.Id = Guid.NewGuid();
            number1.ContactId = contact1.Id;
            number1.Number = "111";
            number1.Type = "work";
            DbConnection.Insert(number1);

            var number2 = new PhoneNumber();
            number2.Id = Guid.NewGuid();
            number2.ContactId = contact2.Id;
            number2.Number = "0123456789";
            number2.Type = "work";
            DbConnection.Insert(number2);

            var number3 = new PhoneNumber();
            number3.Id = Guid.NewGuid();
            number3.ContactId = contact2.Id;
            number3.Number = "0987654321";
            number3.Type = "home";
            DbConnection.Insert(number3);

            // Address
            var address1 = new Address();
            address1.Id = Guid.NewGuid();
            address1.Type = "home";
            address1.StreetLine1 = "1 Capitol Hill";
            address1.StreetLine2 = "Apt 1";
            address1.City = "Seattle";
            address1.Province = "WA";
            address1.Country = "United States";
            address1.ContactId = contact2.Id;
            DbConnection.Insert(address1);

            var address2 = new Address();
            address2.Id = Guid.NewGuid();
            address2.Type = "work";
            address2.StreetLine1 = "1 Microsoft Way";
            address2.City = "Redmond";
            address2.ContactId = contact2.Id;
            DbConnection.Insert(address2);

            // Email
            var email1 = new Email();
            email1.Id = Guid.NewGuid();
            email1.Address = "bill@microsoft.com";
            email1.Type = "work";
            email1.ContactId = contact2.Id;
            DbConnection.Insert(email1);

            // Url
//            var url1 = new Url();
//            url1.Id = Guid.NewGuid();
//            url1.Type = "home";
//            url1.Link = "billgates.com";
//            url1.ContactId = contact2.Id;
//            DbConnection.Insert(url1);
//
//            var url2 = new Url();
//            url2.Id = Guid.NewGuid();
//            url2.Type = "work";
//            url2.Link = "billandmelinda.org";
//            url2.ContactId = contact2.Id;
//            DbConnection.Insert(url2);

            // IMs
            var im1 = new InstantMessage();
            im1.Id = Guid.NewGuid();
            im1.Type = "skype";
            im1.Nickname = "billgates";
            im1.ContactId = contact2.Id;
            DbConnection.Insert(im1);

            // Special Dates
            var date1 = new SpecialDate();
            date1.Id = Guid.NewGuid();
            date1.Type = "other";
            date1.Date = new DateTime(1975, 4, 4);
            date1.ContactId = contact2.Id;
            DbConnection.Insert(date1);

            // Tags
            var tag1 = GetRowsByName<Tag>("Colleague").First();
            var tag2 = GetRowsByName<Tag>("Important").First();

            var tagMap1 = new ContactTagMap()
            {
                Id = Guid.NewGuid(),
                ContactId = contact2.Id,
                TagId = tag1.Id,
                Detail = "Chairman of Microsoft",
            };
            DbConnection.Insert(tagMap1);

            var tagMap2 = new ContactTagMap()
            {
                Id = Guid.NewGuid(),
                ContactId = contact2.Id,
                TagId = tag2.Id,
            };
            DbConnection.Insert(tagMap2);

            // Relationship
            var connType1 = GetRowsByName<RelationshipType>("Advisor").First();
            var connType2 = GetRowsByName<RelationshipType>("Daughter").First();

            var conn1 = new Relationship()
            {
                Id = Guid.NewGuid(),
                FromContactId = contact2.Id,
                ToContactId = contact4.Id,
                RelationshipTypeId = connType1.Id,
                Detail = "Bill will advise Satya with his new CEO role.",
            };
            DbConnection.Insert(conn1);
            var conn2 = new Relationship()
            {
                Id = Guid.NewGuid(),
                FromContactId = contact6.Id,
                ToContactId = contact2.Id,
                RelationshipTypeId = connType2.Id,
            };
            DbConnection.Insert(conn2);

            Console.WriteLine("stop adding data");
        }

        #endregion

        #region Create massive dummy data and Sync Queue

        public void CreateMassiveDataAndSyncQueue(string prefix, int amount)
        {
            // Contacts and directly related info
            for (var i = 1; i <= amount; i++)
            {
                var contact = new Contact
                {
                    Id = Guid.NewGuid(),
                    FirstName = String.Format("{0}_Contact_{1}", prefix, i),
                };
                this.DbConnection.Insert(contact);

                var phoneNumber = new PhoneNumber
                { 
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    Number = i.ToString(),
                };
                this.DbConnection.Insert(phoneNumber);

                var address = new Address
                {
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    StreetLine1 = String.Format("{0}_Address_{1}", prefix, i),
                };
                this.DbConnection.Insert(address);

                var email = new Email
                {
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    Address = String.Format("{0}_Email_{1}", prefix, i),
                };
                this.DbConnection.Insert(email);

                var im = new InstantMessage
                {
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    Nickname = String.Format("{0}_InstantMessage_{1}", prefix, i),
                };
                this.DbConnection.Insert(im);

                var specialDate = new SpecialDate
                {
                    Id = Guid.NewGuid(),
                    ContactId = contact.Id,
                    Date = new DateTime(1975, 4, 4),
                };
            } 
        }

        #endregion
    }
}

