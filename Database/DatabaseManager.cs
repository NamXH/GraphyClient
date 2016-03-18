using System;
using SQLite;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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
    }
}

