using System;
using SQLite;

namespace GraphyClient
{
    public class DatabaseManager
    {
        public SQLiteConnection DbConnection { get; private set; }

        private string _dbName;

        public string DbName
        {
            get { return _dbName; }
        }

        public DatabaseManager(string dbName)
        {
            _dbName = dbName;
        }

        public 
    }
}

