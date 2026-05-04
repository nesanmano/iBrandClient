using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using NLog;

namespace NomadLogic.Common.Services.Database
{
    /// <summary>
    /// Implémentation pour une classe d'accès de base de données SQLite
    /// </summary>
    public class SqliteDatabase : Database
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        protected string DbFileName { get; }

        /// <summary>
        ///		This is the only public constructor for this class.
        /// </summary>
        /// <param name="fileName">The db3 file to open.</param>
        public SqliteDatabase(string fileName)
            : base(fileName)
        {
            DbFileName = fileName;

            if (!File.Exists(fileName))
            {
                CreateDatabase();
            }
            else
            {
                CreateConnection();
            }
        }
        
        /// <summary>
        /// Créer le fichier de base de données et initialise la base de données à partir d'un état vide.
        /// Créé une connection durant cette étape.
        /// </summary>
        public void CreateDatabase()
        {
            SQLiteConnection.CreateFile(_fileName);
            CreateConnection();
            CreateTables();
        }

        /// <summary>
        /// Vérifie l'intégrité de la base de données. Retourne FALSE s'il manque des éléments ou si un problème existe avec les données.
        /// </summary>
        /// <returns>TRUE si tous les tests sont concluants.</returns>
        protected virtual bool CheckDatabaseIntegrity()
        {
            return true;
        }

        protected override void DisposeDo()
        {
            // Voir ici pourquoi on fait ça: https://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        ///		Builds a value parameter name for the current database by ensuring there is an '@' at the
        ///		start of the name.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>A correctly formated parameter name, which starts with an '@'.</returns>
        public override string BuildParameterName(string name)
        {
            //Guard.ArgumentNotNullOrEmptyString(name, "name");

            if (name[0] != '@')
                return "@" + name;
            return name;
        }

        /// <summary>
        ///		Allows creating a parameter with a SQL Mobile-specific type.
        /// </summary>
        /// <param name="name">Name of the parameter</param>
        /// <param name="type">The SQL Mobile type of the parameter</param>
        /// <param name="size">Size of the paramater</param>
        /// <param name="value">The value to set for this parameter</param>
        /// <returns></returns>
        public DbParameter CreateParameter(string name, SqlDbType type, int size, object value)
        {
            var param = (SqlParameter)CreateParameter(name);
            param.SqlDbType = type;
            param.Size = size;
            param.Value = value ?? DBNull.Value;
            return param;
        }

        /// <summary>
        ///		Executes and SQL command that returns a <see cref="SqlDataReader"/> that can be
        ///		scrollable and/or updateable. Make sure you dispose of the result set when you're done.
        /// </summary>
        /// <param name="command">
        ///		The command object that contains the command that will be executed.
        /// </param>
        /// <param name="parameters">
        ///		An optional set of DbParameter objects that provide additional parameters required by the command.
        /// </param>
        /// <returns></returns>
        public SqlDataReader ExecuteResultSet(SqlCommand command, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(command, "command");

            var connection = (SqlConnection)GetConnection();
            PrepareCommand(command, connection, parameters);

            return command.ExecuteReader();
        }

        /// <summary>
        ///		Executes and SQL command that returns a <see cref="SqlDataReader"/> that can be
        ///		scrollable and/or updateable. Make sure you dispose of the result set when you're done.
        /// </summary>
        /// <param name="sqlCommand">
        ///		The SQL query that will be executed.
        /// </param>
        /// <param name="parameters">
        ///		An optional set of DbParameter objects that provide additional parameters required by the SQL command.
        /// </param>
        /// <returns></returns>
        public SqlDataReader ExecuteResultSet(string sqlCommand, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNullOrEmptyString(sqlCommand, "sqlCommand");

            using (var command = (SqlCommand)DbProviderFactory.CreateCommand())
            {
                if (command == null) return null;
                command.CommandText = sqlCommand;
                return ExecuteResultSet(command, parameters);
            }
        }

        /// <summary>
        ///		Checks to see if a table exists in the open database.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        /// <returns>true if the table exists, otherwise false.</returns>
        public override bool TableExists(string tableName)
        {
            if (tableName == null)
            {
                return false;
            }

            DbDataReader reader = null;

            var query = $"SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = '{tableName}'";

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.HasRows) return true;
            }
            finally
            {
                reader?.Close();
            }

            return false;
        }
    }
}
