using System.Data.Common;
using NomadLogic.Common.Services.Database;

namespace NomadLogic.iBrandDesktop.Util
{
    public class PostalCodeDatabase : SqliteDatabase
    {
        public PostalCodeDatabase(string fileName)
            : base(fileName)
        {
                CreateConnection();
        }

        public ZipCodeEntry GetZipCodeEntry(string zipCode)
        {
            DbDataReader reader = null;

            ZipCodeEntry entry = null;

            var query = "Select * From data Where zipcode= '" + zipCode + "'";

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    var state = (string)reader["state"];
                    var city = (string)reader["city"];

                    entry = new ZipCodeEntry(city, state);
                }

                return entry;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }
    }
}
