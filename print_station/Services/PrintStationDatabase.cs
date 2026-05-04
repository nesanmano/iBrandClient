using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Database;
using NomadLogic.Common.Util;
using NomadLogic.PrintStation.Json;
// ReSharper disable ConstantConditionalAccessQualifier

namespace NomadLogic.PrintStation.Services
{
    /// <summary>
    /// Implémentation spécifique pour le Printstation.
    /// </summary>
    public class PrintStationDatabase : SqliteDatabase, IService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Indique la version actuelle de la base de données.
        /// Il est impératif d'incrémenter cette valeur à chaque variation de base de données, pour que l'on puisse détecter
        /// que la base de données est d'un format différent et qu'il soit possible de garantir l'intégrité des données.
        public static readonly int DatabaseVersion = 2;

        /// <summary>
        /// Constructeur principal.
        /// Créer la BD et tente d'établi une connexion immédiatement.
        /// </summary>
        /// <param name="fileName"></param>
        public PrintStationDatabase(string fileName) : base(fileName) {}

        /// <summary>
        /// Cette méthode vérifie l'intégrité de la base de donnée lue.
        /// Tente de détecter une ancienne versione de la base de données, et retourne FALSE si c'est le cas.
        /// L'application devrait cesser avec un message d'erreur pour tenter de sauvegarder la base de données au besoin.
        /// </summary>
        /// <returns>TRUE si la base de données sur disque est la bonne version.</returns>
        public bool Init()
        {
            return GetDatabaseVersion() == DatabaseVersion;
        }

        /// <inheritdoc />
        protected override bool CheckDatabaseIntegrity()
        {
            var tables = new[]
            {
                "metadata",
                "user",
                "post",
                "ui_file",
                "feature_config",
                "print_count",
                "print_count_social_cam",
                "lookup_directory_sources"
            };

            return tables.All(TableExists);
        }

        /// <inheritdoc />
        protected override void CreateTables()
        {
            // Ajout 22 nov 2017
            ExecuteNonQuery("CREATE TABLE metadata (Key nvarchar(45) PRIMARY KEY, Value nvarchar(45) NOT NULL)");
            ExecuteNonQuery($"INSERT INTO metadata(Key, Value) VALUES('databaseVersion', '{DatabaseVersion}')");
            
            ExecuteNonQuery("CREATE TABLE user (ID int PRIMARY KEY, FeatureID int NOT NULL, EventID int NOT NULL, Active bit DEFAULT 1)");

            ExecuteNonQuery("CREATE TABLE post (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, ServerID int NOT NULL, EventID int NOT NULL, FeedID int, ElementID nvarchar(255), ImageURL nvarchar(255), UserImageURL nvarchar(255), UserName nvarchar(255),  Service nvarchar(45), StatusID int, CreationDate nvarchar(100), ImageDownloaded bit DEFAULT 0, UserImageDownloaded bit DEFAULT 0, Hide bit DEFAULT 0, UpdateID int NOT NULL, RepondantNo int DEFAULT -1, FOREIGN KEY( EventID ) REFERENCES user( EventID ))");

            ExecuteNonQuery("CREATE TABLE ui_file (ID int PRIMARY KEY, UserID int NOT NULL, Type nvarchar(45) NOT NULL, Src nvarchar(255) NOT NULL, FeatureID int NOT NULL, FOREIGN KEY( UserID ) REFERENCES user( ID ))");

            ExecuteNonQuery("CREATE TABLE feature_config (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, FeatureID int NOT NULL, Name nvarchar(255) NOT NULL, Value nvarchar(255) NOT NULL, FOREIGN KEY( FeatureID ) REFERENCES user( FeatureID ))");

            ExecuteNonQuery("CREATE TABLE print_count (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, PostID int NOT NULL, Identifier nvarchar(255), SentToServer bit DEFAULT 0, PrintDate nvarchar(100), FOREIGN KEY( PostID ) REFERENCES post( ID ))");

            ExecuteNonQuery("CREATE TABLE print_count_social_cam (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, EventID int NOT NULL, Identifier nvarchar(255), SentToServer bit DEFAULT 0, PrintDate nvarchar(100), FOREIGN KEY( EventID ) REFERENCES user( EventID ))");

            ExecuteNonQuery("CREATE TABLE lookup_directory_sources (ID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, UserID int NOT NULL, DirectoryPath nvarchar(255) NOT NULL, LastDateCheck int NOT NULL, FOREIGN KEY( UserID ) REFERENCES user( ID ))");
        }

        /// <summary>
        /// Obtient la dernière valeur d'un répertoire local traité, pour un usager spécifique.
        /// </summary>
        /// <param name="user">L'usager Printstation.</param>
        /// <param name="localDir">Le répertoire à utilisé.</param>
        /// <param name="mostRecentUpdate">La date la plus récente de traitement. Si erreur, sera la valeur DateTime.MIN</param>
        /// <returns>TRUE s'il y avait une entrée dans la base de donnée. Sinon, false (aucune valeur existante).</returns>
        public bool GetLastLocalFileDate([NotNull] PrintStationUser user, [NotNull] string localDir, out DateTime mostRecentUpdate)
        {
            var query = $"Select LastDateCheck From lookup_directory_sources where UserID={user.Id} and DirectoryPath='{localDir}'";

            try
            {
                using (var reader = ExecuteReader(query, null))
                {
                    if (!reader.Read()) 
                    {
                        mostRecentUpdate = DateTime.MinValue;
                        return false;
                    }

                    var obValue = reader.GetInt64(0);
                    mostRecentUpdate = new DateTime(obValue);
                    return true;
                }
            }
            catch (Exception e) // Si la table n'existe pas ou tout autre erreur de validation.
            {
                log.Error(e);
                mostRecentUpdate = DateTime.MinValue;
                return false;
            }
        }

        /// <summary>
        /// Met à jour ou insère le dernier temps de mise à jour pour un traitement de répertoire source local.
        /// </summary>
        /// <param name="user">L'usager Printstation concerné.</param>
        /// <param name="localDir">Le répertoire concerné.</param>
        /// <param name="lastUpdate">Le temps local à mettre à jour.</param>
        /// <returns>Si la mise à jour ou l'insertion s'est correctement faite (une ligne ajouté/modifiée).</returns>
        public bool UpdateLastLocalFileDate([NotNull] PrintStationUser user, [NotNull] string localDir, DateTime lastUpdate)
        {
            var queryUpdate = $"UPDATE lookup_directory_sources Set LastDateCheck = {lastUpdate.Ticks} Where UserID={user.Id} and DirectoryPath='{localDir}'";
            
            try
            {
                var res = ExecuteNonQuery(queryUpdate, null);

                if (res == 1) return true;

                // On tente un ajout si un update n'a pas fonctionné.
                var sql = $"INSERT INTO lookup_directory_sources (UserID, DirectoryPath, LastDateCheck) VALUES ({user.Id}, '{localDir}', {lastUpdate.Ticks})";
                return ExecuteNonQuery(sql, null) == 1;
            }
            catch (Exception e) // Pour toute erreur d'exécution.
            {
                log.Error(e);
                return false;
            }
        }

        /// <summary>
        /// Indique si on a un usager actif de mentionné dans la base de données.
        /// </summary>
        /// <returns></returns>
        public bool HasActiveUser()
        {
            var hasUserSession = false;

            DbDataReader reader = null;

            var query = "Select * From user where Active = 1";

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    hasUserSession = true;
                }

                return hasUserSession;
            }
            finally
            {
                reader?.Close();
            }
        }

        /// Obtient la version numérique de la base de données.
        /// Si la table n'existe pas, ou si la valeur n'est pas numérique, retourne -1.
        public int GetDatabaseVersion()
        {
            const string QUERY = "Select Value From metadata where Key='databaseVersion'";

            try
            {
                DbDataReader reader;
                using (reader = ExecuteReader(QUERY, null))
                {
                    if (!reader.Read()) return -1;
                    var obValue = reader.GetValue(0);
                    if (obValue == DBNull.Value) return -1;
                    if (!int.TryParse(obValue.ToString(), out int val))
                    {
                        return -1;
                    }
                    return val;
                }
            }
            catch (Exception) // Si la table n'existe pas ou tout autre erreur de validation.
            {
                return -1;
            }
        }

        public int GetMinPostId(int eventId)
        {
            var id = 0;

            DbDataReader reader = null;

            var query = "Select min(ID) From post where Hide = 0 And StatusID = 1  And ImageDownloaded = 1 And EventID = " + eventId;

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    var obValue = reader.GetValue(0);
                    if (obValue != DBNull.Value)
                    {
                        id = Convert.ToInt32(obValue.ToString());
                    }
                }

                return id;
            }
            finally
            {
                reader?.Close();
            }
        }

        public int GetMinPostId(string userName, int eventId)
        {
            var id = 0;

            DbDataReader reader = null;

            var query =
                "Select min(ID) From post Where Hide = 0 And StatusID = 1 And ImageDownloaded = 1 And UserName Like @UserName And EventID = " +
                eventId;

            try
            {
                var parameters = new DbParameter[1];

                var param = CreateParameter("@UserName", "%" + userName + "%");
                parameters[0] = param;

                reader = ExecuteReader(query, parameters);

                if (reader.Read())
                {
                    var obValue = reader.GetValue(0);
                    if (obValue != DBNull.Value)
                    {
                        id = Convert.ToInt32(obValue.ToString());
                    }
                }

                return id;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<FeatureConfig> GetFeatureConfig(int featureId)
        {
            var featureConfigs = new List<FeatureConfig>();

            DbDataReader reader = null;

            var query = "Select * From feature_config Where FeatureID=@FeatureID";

            try
            {
                var parameters = new DbParameter[1];

                var param = CreateParameter("@FeatureID", featureId);
                parameters[0] = param;

                reader = ExecuteReader(query, parameters);

                while (reader.Read())
                {
                    var value = (string) reader["Value"];
                    var name = (string) reader["Name"];

                    featureConfigs.Add(new FeatureConfig(name, value));
                }

                return featureConfigs;
            }
            finally
            {
                reader?.Close();
            }
        }

        public UiFile GetUiFile(string type, int userId)
        {
            DbDataReader reader = null;

            var query = "Select * From ui_file Where Type=@Type And UserID=@UserID";

            try
            {
                var parameters = new DbParameter[2];

                var param = CreateParameter("@Type", type);
                parameters[0] = param;

                param = CreateParameter("@UserID", userId);
                parameters[1] = param;

                reader = ExecuteReader(query, parameters);

                if (!reader.Read()) return null;
                var id = (int)reader["ID"];
                var src = (string)reader["Src"];

                var uiFile = new UiFile(id, userId, type, src);

                return uiFile;
            }
            finally
            {
                reader?.Close();
            }
        }

        public int GetMaxPostId(int eventId)
        {
            var id = 0;

            DbDataReader reader = null;

            var query = "Select max(ID) From post where Hide = 0 And StatusID = 1 And EventID = " + eventId;

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    var obValue = reader.GetValue(0);
                    if (obValue != DBNull.Value)
                    {
                        id = Convert.ToInt32(obValue.ToString());
                    }
                }

                return id;
            }
            finally
            {
                reader?.Close();
            }
        }

        public int GetLastUpdateId(int eventId)
        {
            var id = 0;

            DbDataReader reader = null;

            var query = "Select max(UpdateID) From post where EventID = " + eventId;

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    var obValue = reader.GetValue(0);
                    if (obValue != DBNull.Value)
                    {
                        id = Convert.ToInt32(obValue.ToString());
                    }
                }

                return id;
            }
            finally
            {
                reader?.Close();
            }
        }

        public PrintStationUser GetActiveUser()
        {
            PrintStationUser user = null;

            DbDataReader reader = null;

            var query = "Select * From user where Active = 1";

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    var id = (int) reader["ID"];
                    var featureId = (int) reader["FeatureID"];
                    var eventId = (int) reader["EventID"];

                    user = new PrintStationUser(id, featureId, eventId);
                }

                return user;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<Post> GetPostsWithImageToDownload(int eventId)
        {
            var posts = new List<Post>();

            DbDataReader reader = null;

            var query =
                "Select * From post where (ImageDownloaded = 0 or UserImageDownloaded = 0) And StatusID = 1 And EventID=" +
                eventId;

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var elementId = (string) reader["ElementID"];

                    string imageUrl = null;
                    var value = reader["ImageURL"];
                    if (value != DBNull.Value)
                        imageUrl = (string) value;

                    string userImageUrl = null;
                    value = reader["UserImageURL"];
                    if (value != DBNull.Value)
                        userImageUrl = (string) value;

                    string username = null;

                    value = reader["UserName"];
                    if (value != DBNull.Value)
                        username = (string) value;

                    var creationDate = (string) reader["CreationDate"];

                    var imageDownloaded = (bool) reader["ImageDownloaded"];
                    var userImageDownloaded = (bool) reader["UserImageDownloaded"];

                    var updateId = (int) reader["UpdateID"];

                    var statusId = (int) reader["StatusID"];

                    var serverId = (int) reader["ServerID"];

                    var repondantNo = (int)reader["RepondantNo"];

                    var post = new Post(id, elementId, imageUrl, userImageUrl, username, creationDate, imageDownloaded,
                        userImageDownloaded, updateId, statusId, serverId, repondantNo);
                    posts.Add(post);
                }

                return posts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<Post> GetPosts(long postId, bool foward, int limit, int eventId)
        {
            var posts = new List<Post>();

            DbDataReader reader = null;

            var query = "Select * From post Where Hide = 0 And StatusID = 1 And ImageDownloaded = 1  And EventID=" + eventId +
                        " And ID ";
            if (foward)
                query += ">= " + postId + " Order by ID ASC";
            else
                query += "<= " + postId + " Order by ID DESC";

            query += " Limit " + limit;

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var elementId = (string) reader["ElementID"];

                    string imageUrl = null;
                    var value = reader["ImageURL"];
                    if (value != DBNull.Value)
                        imageUrl = (string) value;

                    string userImageUrl = null;
                    value = reader["UserImageURL"];
                    if (value != DBNull.Value)
                        userImageUrl = (string) value;

                    string username = null;
                    value = reader["UserName"];
                    if (value != DBNull.Value)
                        username = (string) value;

                    var creationDate = (string) reader["CreationDate"];

                    var imageDownloaded = (bool) reader["ImageDownloaded"];
                    var userImageDownloaded = (bool) reader["UserImageDownloaded"];

                    var updateId = (int) reader["UpdateID"];
                    var statusId = (int) reader["StatusID"];

                    var serverId = (int) reader["ServerID"];
                    var repondantNo = (int)reader["RepondantNo"];

                    var post = new Post(id, elementId, imageUrl, userImageUrl, username, creationDate, imageDownloaded,
                        userImageDownloaded, updateId, statusId, serverId, repondantNo);
                    posts.Add(post);
                }

                return posts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<PrintCount> GetPrintCountToSend()
        {
            var printCounts = new List<PrintCount>();

            DbDataReader reader = null;

            var query = "Select * From print_count Where SentToServer = 0";

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var postId = (int) reader["PostID"];
                    var identifier = (string) reader["Identifier"];

                    printCounts.Add(new PrintCount(id, postId, identifier));
                }

                return printCounts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<PrintCountSocialCam> GetPrintCountSocialCamToSend()
        {
            var printCounts = new List<PrintCountSocialCam>();

            DbDataReader reader = null;

            var query = "Select * From print_count_social_cam Where SentToServer = 0";

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var eventId = (int) reader["EventID"];
                    var identifier = (string) reader["Identifier"];

                    printCounts.Add(new PrintCountSocialCam(id, eventId, identifier));
                }

                return printCounts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public void SetPrintCountSent(long id)
        {
            var sql = "UPDATE print_count Set SentToServer = 1 Where ID=@ID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@ID", id);
            parameters[0] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void SetPrintCountSocialCamSent(long id)
        {
            var sql = "UPDATE print_count_social_cam Set SentToServer = 1 Where ID=@ID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@ID", id);
            parameters[0] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void ResetPicturesToDownload(int eventId)
        {
            var sql =
                "UPDATE post Set ImageDownloaded = 1,  UserImageDownloaded = 1 Where (ImageDownloaded = 0 or UserImageDownloaded = 0) And StatusID = 1 And EventID=" +
                eventId;


            ExecuteNonQuery(sql, null);
        }

        public List<Post> GetPosts(string userName, long postId, bool foward, int limit, int eventId)
        {
            var posts = new List<Post>();

            DbDataReader reader = null;

            var query =
                "Select * From post Where StatusID=1 And UserName Like @UserName And ImageDownloaded = 1  And EventID=" +
                eventId + " And ID ";
            if (foward)
                query += ">= " + postId + " Order by ID ASC";
            else
                query += "<= " + postId + " Order by ID DESC";

            query += " Limit " + limit;
            
            try
            {
                var parameters = new DbParameter[1];

                var param = CreateParameter("@UserName", "%" + userName + "%");
                parameters[0] = param;

                reader = ExecuteReader(query, parameters);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var elementId = (string) reader["ElementID"];

                    string imageUrl = null;
                    var value = reader["ImageURL"];
                    if (value != DBNull.Value)
                        imageUrl = (string) value;

                    string userImageUrl = null;
                    value = reader["UserImageURL"];
                    if (value != DBNull.Value)
                        userImageUrl = (string) value;

                    string username = null;
                    value = reader["UserName"];
                    if (value != DBNull.Value)
                        username = (string) value;

                    var creationDate = (string) reader["CreationDate"];

                    var imageDownloaded = (bool) reader["ImageDownloaded"];
                    var userImageDownloaded = (bool) reader["UserImageDownloaded"];

                    var updateId = (int) reader["UpdateID"];
                    var statusId = (int) reader["StatusID"];
                    var serverId = (int) reader["ServerID"];
                    var repondantNo = (int)reader["RepondantNo"];

                    var post = new Post(id, elementId, imageUrl, userImageUrl, username, creationDate, imageDownloaded,
                        userImageDownloaded, updateId, statusId, serverId, repondantNo);
                    posts.Add(post);
                }

                return posts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<Post> GetLastReceivedPosts(string userName, int limit, int eventId)
        {
            var posts = new List<Post>();

            DbDataReader reader = null;

            var query =
                "Select * From post Where StatusID=1 And UserName Like @Username And ImageDownloaded = 1 And EventID=" +
                eventId + " And ID Order by ID DESC";

            query += " Limit " + limit;

            try
            {
                var parameters = new DbParameter[1];

                var param = CreateParameter("@UserName", "%" + userName + "%");
                parameters[0] = param;

                reader = ExecuteReader(query, parameters);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var elementId = (string) reader["ElementID"];

                    string imageUrl = null;
                    var value = reader["ImageURL"];
                    if (value != DBNull.Value)
                        imageUrl = (string) value;

                    string userImageUrl = null;
                    value = reader["UserImageURL"];
                    if (value != DBNull.Value)
                        userImageUrl = (string) value;

                    var username = (string) reader["UserName"];
                    var creationDate = (string) reader["CreationDate"];

                    var imageDownloaded = (bool) reader["ImageDownloaded"];
                    var userImageDownloaded = (bool) reader["UserImageDownloaded"];

                    var updateId = (int) reader["UpdateID"];
                    var statusId = (int) reader["StatusID"];
                    var serverId = (int) reader["ServerID"];
                    var repondantNo = (int)reader["RepondantNo"];

                    var post = new Post(id, elementId, imageUrl, userImageUrl, username, creationDate, imageDownloaded,
                        userImageDownloaded, updateId, statusId, serverId, repondantNo);
                    posts.Add(post);
                }

                return posts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public List<Post> GetLastReceivedPosts(int limit, int eventId)
        {
            var posts = new List<Post>();

            DbDataReader reader = null;

            var query = "Select * From post Where Hide = 0 And StatusID = 1 And ImageDownloaded = 1 And EventID=" + eventId +
                        " And ID Order by ID DESC";

            query += " Limit " + limit;

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var id = (long) reader["ID"];
                    var elementId = (string) reader["ElementID"];

                    string imageUrl = null;
                    var value = reader["ImageURL"];
                    if (value != DBNull.Value)
                        imageUrl = (string) value;

                    string userImageUrl = null;
                    value = reader["UserImageURL"];
                    if (value != DBNull.Value)
                        userImageUrl = (string) value;

                    string username = null;
                    value = reader["UserName"];
                    if (value != DBNull.Value)
                        username = (string) value;

                    var creationDate = (string) reader["CreationDate"];

                    var imageDownloaded = (bool) reader["ImageDownloaded"];
                    var userImageDownloaded = (bool) reader["UserImageDownloaded"];

                    var updateId = (int) reader["UpdateID"];
                    var statusId = (int) reader["StatusID"];
                    var serverId = (int) reader["ServerID"];
                    var repondantNo = (int)reader["RepondantNo"];

                    var post = new Post(id, elementId, imageUrl, userImageUrl, username, creationDate, imageDownloaded,
                        userImageDownloaded, updateId, statusId, serverId, repondantNo);
                    posts.Add(post);
                }

                return posts;
            }
            finally
            {
                reader?.Close();
            }
        }

        public bool IsUserInserted(int id)
        {
            var userSessionInserted = false;

            DbDataReader reader = null;

            var query = "Select * From user where ID = " + id;

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    userSessionInserted = true;
                }

                return userSessionInserted;
            }
            finally
            {
                reader?.Close();
            }
        }

        public bool IsPostInserted(int postServerId)
        {
            var postInserted = false;

            DbDataReader reader = null;

            var query = "Select * From post where ServerID = " + postServerId;

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    postInserted = true;
                }

                return postInserted;
            }
            finally
            {
                reader?.Close();
            }
        }

        public void InsertUser(PrintStationUser user)
        {
            var sql = "INSERT INTO user (ID, FeatureID, EventID) VALUES (@ID, @FeatureID, @EventID)";
            var parameters = new DbParameter[3];

            var param = CreateParameter("@ID", user.Id);
            parameters[0] = param;

            param = CreateParameter("@FeatureID", user.FeatureId);
            parameters[1] = param;

            param = CreateParameter("@EventID", user.EventId);
            parameters[2] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void AddPrintCount(int postId)
        {
            var sql = "INSERT INTO print_count (PostID, Identifier, PrintDate) VALUES (@PostID, @Identifier, @PrintDate)";
            var parameters = new[]
            {
                CreateParameter("@PostID", postId),
                CreateParameter("@Identifier", GenerateUniqueId.GetId()),
                CreateParameter("@PrintDate", DateTime.Now.ToString(Post.DATE_FORMAT))
            };

            ExecuteNonQuery(sql, parameters);
        }

        public void AddPrintCountSocialCam(int eventId)
        {
            var sql = "INSERT INTO print_count_social_cam (EventID, Identifier, PrintDate) VALUES (@EventID, @Identifier, @PrintDate)";
            var parameters = new []
            {
                CreateParameter("@EventID", eventId),
                CreateParameter("@Identifier", GenerateUniqueId.GetId()),
                CreateParameter("@PrintDate", DateTime.Now.ToString(Post.DATE_FORMAT))
            };
            
            ExecuteNonQuery(sql, parameters);
        }

        public void DeleteUiFiles(int userId)
        {
            var sql = "Delete from ui_file Where UserID=@UserID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@UserID", userId);
            parameters[0] = param;

            ExecuteNonQuery(sql, param);
        }

        public void DeleteConfig(int featureId)
        {
            var sql = "Delete from feature_config Where FeatureID=@FeatureID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@FeatureID", featureId);
            parameters[0] = param;

            ExecuteNonQuery(sql, param);
        }

        public void InsertUiFile(UiFile uiFile, int userId)
        {
            var sql =
                "INSERT INTO ui_file (ID, UserID, Type, Src, FeatureID) VALUES (@ID, @UserID, @Type, @Src, @FeatureID)";
            var parameters = new DbParameter[5];

            var param = CreateParameter("@ID", uiFile.Id);
            parameters[0] = param;

            param = CreateParameter("@UserID", userId);
            parameters[1] = param;

            param = CreateParameter("@Type", uiFile.Type);
            parameters[2] = param;

            param = CreateParameter("@Src", uiFile.Src);
            parameters[3] = param;

            param = CreateParameter("@FeatureID", uiFile.FeatureId);
            parameters[4] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void InsertConfig(FeatureConfig featureConfig, int featureId)
        {
            var sql = "INSERT INTO feature_config (Name, Value, FeatureID) VALUES ( @Name, @Value, @FeatureID)";
            var parameters = new DbParameter[3];

            var param = CreateParameter("@Name", featureConfig.Name);
            parameters[0] = param;

            param = CreateParameter("@Value", featureConfig.Value);
            parameters[1] = param;

            param = CreateParameter("@FeatureID", featureId);
            parameters[2] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void InsertPost(Post post)
        {
            var sql =
                "INSERT INTO post (ServerID, FeedID, EventID, ElementID, ImageURL, UserImageURL, UserName, Service, CreationDate, UpdateID, StatusID, RepondantNo) VALUES (@ServerID, @FeedID, @EventID, @ElementID, @ImageURL, @UserImageURL, @UserName, @Service, @CreationDate, @UpdateID, @StatusID, @RepondantNo)";
            var parameters = new DbParameter[12];

            var param = CreateParameter("@ServerID", post.ServerId);
            parameters[0] = param;

            param = CreateParameter("@FeedID", post.FeedId);
            parameters[1] = param;

            param = CreateParameter("@EventID", post.EventId);
            parameters[2] = param;

            param = CreateParameter("@ElementID", post.ElementId);
            parameters[3] = param;

            param = CreateParameter("@ImageURL", post.PictureUrl);
            parameters[4] = param;

            param = CreateParameter("@UserImageURL", post.UserPictureUrl);
            parameters[5] = param;

            param = CreateParameter("@UserName", post.SocialAppUsername);
            parameters[6] = param;

            param = CreateParameter("@Service", post.SocialAppServiceName);
            parameters[7] = param;

            param = CreateParameter("@CreationDate", post.CreationDate);
            parameters[8] = param;

            param = CreateParameter("@UpdateID", post.UpdateId);
            parameters[9] = param;

            param = CreateParameter("@StatusID", post.StatusId);
            parameters[10] = param;

            param = CreateParameter("@RepondantNo", post.RepondantNoFromServer);
            parameters[11] = param;
            ExecuteNonQuery(sql, parameters);
        }

        public void InsertIBrandPost(Post post)
        {
            var sql =
                "INSERT INTO post (ServerID, EventID, ElementID, ImageURL, UserName, Service, CreationDate, UpdateID, StatusID, ImageDownloaded, UserImageDownloaded, RepondantNo) VALUES (@ServerID, @EventID, @ElementID, @ImageURL, @UserName, @Service, @CreationDate, @UpdateID, @StatusID, 1, 1, @RepondantNo)";
            var parameters = new DbParameter[10];

            var param = CreateParameter("@ServerID", post.ServerId);
            parameters[0] = param;

            param = CreateParameter("@EventID", post.EventId);
            parameters[1] = param;

            param = CreateParameter("@ElementID", post.ElementId);
            parameters[2] = param;

            param = CreateParameter("@ImageURL", post.PictureUrl);
            parameters[3] = param;

            param = CreateParameter("@UserName", post.SocialAppUsername);
            parameters[4] = param;

            param = CreateParameter("@Service", post.SocialAppServiceName);
            parameters[5] = param;

            param = CreateParameter("@CreationDate", post.CreationDate);
            parameters[6] = param;

            param = CreateParameter("@UpdateID", post.UpdateId);
            parameters[7] = param;

            param = CreateParameter("@StatusID", post.StatusId);
            parameters[8] = param;

            param = CreateParameter("@RepondantNo", post.RepondantNoFromServer);
            parameters[9] = param;
            ExecuteNonQuery(sql, parameters);
        }

        public void UpdatePost(Post post)
        {
            var sql =
                "UPDATE post Set StatusID = @StatusID, ImageDownloaded = @ImageDownloaded, UserImageDownloaded = @UserImageDownloaded, UpdateID = @UpdateID Where ServerID=@ServerID";
            var parameters = new DbParameter[5];

            var param = CreateParameter("@StatusID", post.StatusId);
            parameters[0] = param;

            var imageDownloaded = post.PictureDownloaded ? 1 : 0;
            param = CreateParameter("@ImageDownloaded", imageDownloaded);
            parameters[1] = param;

            var userImageDownloaded = post.UserPictureDownloaded ? 1 : 0;
            param = CreateParameter("@UserImageDownloaded", userImageDownloaded);
            parameters[2] = param;

            param = CreateParameter("@UpdateID", post.UpdateId);
            parameters[3] = param;

            param = CreateParameter("@ServerID", post.ServerId);
            parameters[4] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void HidePost(long postId)
        {
            var sql =
                "UPDATE post Set Hide = 1 Where ID=@ID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@ID", postId);
            parameters[0] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void ClearAll(PrintStationUser user)
        {
            var sql = "UPDATE post Set StatusID = @StatusID Where EventID=@EventID";
            var parameters = new DbParameter[2];

            var param = CreateParameter("@StatusID", 2);
            parameters[0] = param;
            param = CreateParameter("@EventID", user.EventId);
            parameters[1] = param;

            ExecuteNonQuery(sql, parameters);
        }

        public void SetUserStatus(PrintStationUser user, bool active)
        {
            var sql = "update user Set Active = @Active Where ID = @ID";
            var parameters = new DbParameter[2];

            var param = CreateParameter("@Active", active);
            parameters[0] = param;
            param = CreateParameter("@ID", user.Id);
            parameters[1] = param;

            ExecuteNonQuery(sql, parameters);
        }
    }
}