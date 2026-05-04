using System.Collections.Generic;
using System.Data.Common;
using NomadLogic.Common.Services.Database;

namespace NomadLogic.iBrandDesktop.Datasource
{
    public class DatabaseDatasource : AbstractDatasource
    {
        private EntriesDatabase _database;
        
        public DatabaseDatasource(DatasourceInfo datasourceInfo, Project project)
            : base(datasourceInfo, project)
        {

        }

        public override void Close()
        {
            //Disconnect from database
            if (_database != null)
                _database.Dispose();
        }

        public override void Load()
        {
            var dbName = _project.ProjectDir + _datasourceInfo.Name + ".sdf";
            _database = new EntriesDatabase(dbName);
        }

        public override List<string> GetFieldValues(string indexedFieldValue, string indexedFieldName, string fieldName)
        {
            //To BE TESTED FSF
            return _database.GetFieldValues(indexedFieldValue, indexedFieldName, fieldName);
        }

        public override bool HasValue(string fieldName, string value)
        {
            return _database.HasValue(fieldName, value);
        }

        public override List<string> GetValues(string fieldName)
        {
            return _database.GetValues(fieldName);
        }

        public override string GetFieldValue(string indexedFieldValue, string indexedFieldName, string fieldName)
        {
            return _database.GetFieldValue(indexedFieldValue, indexedFieldName, fieldName);
        }

        public class EntriesDatabase : SqliteDatabase
        {
            public EntriesDatabase(string fileName)
            : base(fileName)
            {
                
            }

            /*protected override void Connect()
            {
                if (sql_connection == null)
                {
                    sql_connection = new SqlCeConnection("Data Source = " + file_name);
                    sql_connection.Open();
                }
                else
                {
                    if (sql_connection.State != ConnectionState.Open)
                    {
                        DisposeConnection();
                        Connect();
                    }
                }
            }*/

            public string GetFieldValue(string indexedFieldValue, string indexedFieldName, string fieldName)
            {
                var query = "Select " + fieldName + " From entries where " + indexedFieldName + " = '" + indexedFieldValue + "'";

                DbDataReader reader = null;

                try
                {
                    reader = ExecuteReader(query, null);
                    if (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        return value.ToString();
                    }

                    return "";

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            public List<string> GetValues(string fieldName)
            {
                var values = new List<string>();
                
                var query = "Select distinct(" + fieldName + ") From entries";

                DbDataReader reader = null;

                try
                {
                    reader = ExecuteReader(query, null);
                    while (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        values.Add(value.ToString());
                    }

                    return values;

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            public List<string> GetFieldValues(string indexedFieldValue, string indexedFieldName, string fieldName)
            {
                var values = new List<string>();

                var query = "Select distinct(" + fieldName + ") From entries Where " + indexedFieldName + " = '" + indexedFieldValue + "'";
                DbDataReader reader = null;

                try
                {
                    reader = ExecuteReader(query, null);
                    while (reader.Read())
                    {
                        var value = reader.GetValue(0);
                        values.Add(value.ToString());
                    }

                    return values;

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }

            public bool HasValue(string fieldName, string value)
            {
                var query = "Select * From entries where " + fieldName + " = '" + value + "'";

                DbDataReader reader = null;

                try
                {
                    reader = ExecuteReader(query, null);
                    if (reader.Read())
                    {
                        return true;
                    }

                    return false;

                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }

            }

            /*public List<Field> GetFields()
            {
                List<Field> fields = new List<Field>();

                SqlCeCommand command = null;
                DbDataReader reader = null;

                String query = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'entries'";

                try
                {
                    command = new SqlCeCommand(query, SqlConnection);

                    reader = command.ExecuteReader();

                    int i = 0;
                    while (reader.Read())
                    {
                        object column_name = reader.GetValue(0);
                        String field_name = column_name.ToString();

                        if (i != 0) //On ne tient pas compte de la colonne ID
                        {
                            fields.Add(new Field(i, field_name));
                        }

                        i++;
                    }

                    return fields;

                }
                finally
                {
                    if (reader != null)
                        reader.Close();

                    if (command != null)
                        command.Dispose();
                }
            }*/
        }

    }
}
