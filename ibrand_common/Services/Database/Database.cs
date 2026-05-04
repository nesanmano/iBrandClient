using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.Common.Services.Database
{
    /// <summary>
    /// Définition d'une classe d'interface entre une base de données générique selon le modèle de base pour .NET.
    /// Si cet objet est "Disposed" via sa méthode Dispose, il n'est pas possible de le ré-initialisé pour des raisons de blocage de fichier.
    /// Il faut créer une nouvelle instance.
    /// </summary>
    public abstract class Database : IDisposable
    {
        protected string _fileName;
        private readonly DbProviderFactory _dbProviderFactory;
        private DbConnection _databaseConnection;
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Méthode de création des tables.
        /// </summary>
        protected virtual void CreateTables() { }

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="fileName">Le nom de fichier de la base de données.</param>
        protected Database(string fileName)
        {
            _fileName = fileName;
            ConnectionString = $"Data Source={fileName};Pooling=true;FailIfMissing=false";

            try
            {
                _dbProviderFactory = DbProviderFactories.GetFactory("System.Data.SQLite");
            }
            catch (Exception)
            {
                var table = DbProviderFactories.GetFactoryClasses();
                if (!log.IsDebugEnabled) throw;
                // Retrieve the installed providers and factories.

                var sbProviders = new List<string>();
                // Display each row and column value.
                foreach (DataRow row in table.Rows)
                {
                    var sb = new StringBuilder();
                    foreach (DataColumn column in table.Columns)
                    {
                        sb.Append(row[column]);
                        sb.Append(" ");
                    }
                    sbProviders.Add(sb.ToString());
                }

                log.Debug(@"List of Database providers loaded into memory:" + Environment.NewLine + "\t\t" + StringUtil.FormatEnumerable(sbProviders, true, 1, "\t\t"));
                throw;

            }
        }

        /// Nom du fichier de base de données sur lequel l'instance est basé.
        public string DataseFileName => _fileName;
        /// Indique si l'instance à été détruite via l'appel de la méthode "Dispose()". Cette instance est devenue initilisable et ne se réinitialisera pas.
        public bool Disposed { get; private set; }

        /// <summary>
        /// Ferme la connection à la base de donnée. L'instance devient pas la suite inutilisable.
        /// </summary>
        public void Dispose()
        {
            if (_databaseConnection == null) return;
            _databaseConnection.Close();			// .NET documents recommend closing instead of disposing connection
            _databaseConnection = null;
            DisposeDo();
            Disposed = true;
        }

        /// S'il y a des étapes supplémentaires propres aux implémentations de cette classe pour la destruction de l'instance, elles doivent 
        /// être entrées ici.
        /// L'instance ne retourne pas encore TRUE pour la propriété "Disposed" quand cette méthode est appelée, mais la connection à la BD n'existe plus.
        protected abstract void DisposeDo();

        /// <summary>
        ///	Créer une nouvelle connection à la base de données à l'interne.
        /// Si disposée, ne fera rien.
        /// </summary>      
        protected void CreateConnection()
        {
            if (Disposed) return;

            if (_databaseConnection != null) return;
            _databaseConnection = _dbProviderFactory.CreateConnection();
            if (_databaseConnection != null) _databaseConnection.ConnectionString = ConnectionString;
        }

        /// Indique si une connection est établie avec le fichier de la base de données. Si non, l'instance de service de base de données ne peut être utilisé.
        public bool Connected => _databaseConnection != null;

        /// <summary>
        ///		Gets the connection string for this instance.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Builds a value parameter name for the current database.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>A correctly formated parameter name.</returns>
        public virtual string BuildParameterName(string name)
        {
            return name;
        }

        /// <summary>
        ///		Creates a new parameter and sets the name of the parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>
        ///		A new <see cref="DbParameter"/> instance of the correct type for this database.</returns>
        /// <remarks>
        ///		The database will automatically add the correct prefix, like "@" for SQL Server, to the
        ///		parameter name. In other words, you can just supply the name without a prefix.
        /// </remarks>
        public virtual DbParameter CreateParameter(string name)
        {
            var parameter = _dbProviderFactory.CreateParameter();
            if (parameter == null) return null;

            parameter.ParameterName = BuildParameterName(name);
            return parameter;

        }

        /// <summary>
        ///		Creates a new parameter and sets the name of the parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">
        ///		The value you want assigned to thsi parameter. A null value will be converted to
        ///		a <see cref="DBNull"/> value in the parameter.
        /// </param>
        /// <returns>
        ///		A new <see cref="DbParameter"/> instance of the correct type for this database.</returns>
        /// <remarks>
        ///		The database will automatically add the correct prefix, like "@" for SQL Server, to the
        ///		parameter name. In other words, you can just supply the name without a prefix.
        /// </remarks>
        public virtual DbParameter CreateParameter(string name, object value)
        {
            var param = CreateParameter(name);
            param.Value = value ?? DBNull.Value;
            return param;
        }

        /// <summary>
        ///		Creates a new parameter and sets the name of the parameter.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="type">The type of the parameter.</param>
        /// <param name="size">The size of this parameter.</param>
        /// <param name="value">
        ///		The value you want assigned to this parameter. A null value will be converted to
        ///		a <see cref="DBNull"/> value in the parameter.
        /// </param>
        /// <returns>
        ///		A new <see cref="DbParameter"/> instance of the correct type for this database.</returns>
        /// <remarks>
        ///		The database will automatically add the correct prefix, like "@" for SQL Server, to the
        ///		parameter name. In other words, you can just supply the name without a prefix.
        /// </remarks>
        public virtual DbParameter CreateParameter(string name, DbType type, int size, object value)
        {
            var param = CreateParameter(name);
            param.DbType = type;
            param.Size = size;
            param.Value = value ?? DBNull.Value;
            return param;
        }

        /// <summary>
        ///		The specific <see cref="DbProviderFactory"/> instance provided by a subclass of
        ///		this class.
        /// </summary>
        public DbProviderFactory DbProviderFactory => _dbProviderFactory;

        /// <summary>
        ///		Executes an SQL query with an optional set of parameters.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">Zero or more parameters for the query.</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteNonQuery(DbCommand command, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(command, "Command");
            var connection = GetConnection();
            if (connection == null)
            {
                log.Fatal("No database connection available.");
                throw new Exception("No database connection available.");
            }

            PrepareCommand(command, connection, parameters);
            try
            {
                var result = command.ExecuteNonQuery();
                return result;
            }
            catch (Exception e)
            {
                log.Error(e, "Execution of SQL failed.");
                return -1;
            }
        }

        /// <summary>
        ///		Executes an SQL query with an optional set of parameters.
        /// </summary>
        /// <param name="sqlCommand">The SQL statement to execute.</param>
        /// <param name="parameters">Zero or more parameters for the query.</param>
        /// <returns>The number of rows affected.</returns>
        public virtual int ExecuteNonQuery(string sqlCommand, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(sqlCommand, "sqlCommand");

            using (var command = _dbProviderFactory.CreateCommand())
            {
                if (command == null) return -1;
                command.CommandText = sqlCommand;
                return ExecuteNonQuery(command, parameters);
            }
        }

        /// <summary>
        ///		Execute a command and return a <see cref="DbDataReader"/> that contains the rows
        ///		returned.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="parameters">Zero or more parameters for the query.</param>
        /// <returns>A <see cref="DbDataReader"/> that contains the rows returned by the query.</returns>
        public virtual DbDataReader ExecuteReader(DbCommand command, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(command, "command");

            var connection = GetConnection();
            PrepareCommand(command, connection, parameters);

            var result = command.ExecuteReader();
            return result;
        }

        /// <summary>
        ///		Execute a command and return a <see cref="DbDataReader"/> that contains the rows
        ///		returned.
        /// </summary>
        /// <param name="sqlCommand">The SQL query to execute.</param>
        /// <param name="parameters">Zero or more parameters for the query.</param>
        /// <returns>A <see cref="DbDataReader"/> that contains the rows returned by the query.</returns>
        public virtual DbDataReader ExecuteReader(string sqlCommand, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNullOrEmptyString(sqlCommand, "sqlCommand");

            using (var command = _dbProviderFactory.CreateCommand())
            {
                if (command == null) return null;
                command.CommandText = sqlCommand;
                return ExecuteReader(command, parameters);
            }
        }

        /// <summary>
        /// <para>
        ///		Executes the <paramref name="command"/> and returns the first column of the first
        ///		row in the result set returned by the query. Extra columns or rows are ignored.
        /// </para>
        /// </summary>
        /// <param name="command">
        /// <para>
        ///		The command that contains the query to execute.
        /// </para>
        /// </param>
        /// <param name="parameters">Zero or more parameters for the query.</param>
        /// <returns>
        /// <para>
        ///		The first column of the first row in the result set.
        /// </para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(DbCommand command, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(command, "command");

            var connection = GetConnection();
            PrepareCommand(command, connection, parameters);
            var result = command.ExecuteScalar();
            return result;
        }

        /// <summary>
        ///		Executes the <paramref name="sqlCommand"/> and returns the first column of the first
        ///		row in the result set returned by the query. Extra columns or rows are ignored.
        /// </summary>
        /// <param name="sqlCommand">The SQL statement to execute.</param>
        /// <param name="parameters">Zero or more parameters for the query.</param>
        /// <returns>
        /// <para>
        ///		The first column of the first row in the result set.
        /// </para>
        /// </returns>
        /// <seealso cref="IDbCommand.ExecuteScalar"/>
        public virtual object ExecuteScalar(string sqlCommand, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(sqlCommand, "sqlCommand");

            using (var command = _dbProviderFactory.CreateCommand())
            {
                if (command == null) return null;
                command.CommandText = sqlCommand;
                return ExecuteScalar(command, parameters);
            }
        }

        /// <summary>
        /// <para>Returns the shared connection, and opens it the first time.</para>
        /// </summary>
        /// <returns>The opened connection.</returns>
        public DbConnection GetConnection()
        {
            if (!Connected) return null;

            if (_databaseConnection.State != ConnectionState.Open)
                _databaseConnection.Open();

            return _databaseConnection;
        }

        /// <summary>
        /// <para>
        ///		Assigns a <paramref name="connection"/> to the <paramref name="command"/> and 
        ///		discovers parameters if needed.
        /// </para>
        /// </summary>
        /// <param name="command">The command that contains the query to prepare.</param>
        /// <param name="connection">The connection to assign to the command.</param>
        protected static void PrepareCommand(DbCommand command, DbConnection connection)
        {
            //Guard.ArgumentNotNull(command, "command");
            //Guard.ArgumentNotNull(connection, "connection");

            command.Connection = connection;
        }

        /// <summary>
        ///		Prepares a <see cref="DbCommand"/> object for use. This involves setting the connection
        ///		and adding any parameters to the command.
        /// </summary>
        /// <param name="command">The command object you want prepared.</param>
        /// <param name="connection">The connection to use with the command.</param>
        /// <param name="parameters">Zero or more parameters to add to the command.</param>
        protected void PrepareCommand(DbCommand command, DbConnection connection, params DbParameter[] parameters)
        {
            //Guard.ArgumentNotNull(command, "command");
            //Guard.ArgumentNotNull(connection, "connection");

            command.Connection = connection;

            if (parameters == null) return;
            foreach (var t in parameters)
                command.Parameters.Add(t);
        }

        /// <summary>
        ///		Checks to see if a specific table is in the database.
        /// </summary>
        /// <param name="tableName">Name of the table to look for.</param>
        /// <returns>True if table exists and false if it doesn't.</returns>
        public abstract bool TableExists(string tableName);

        /// <summary>
        ///		This is a simple helper method that will convert a DBNull value into
        ///		a null value.
        /// </summary>
        /// <param name="value">The value you want to check for DBNull</param>
        /// <returns>Null if <paramref name="value"/> is DBNull.Value, or <paramref name="value"/>.</returns>
        public static object GetNullable(object value)
        {
            return (value is DBNull) ? null : value;
        }
    }
}
