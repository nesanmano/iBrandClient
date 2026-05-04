using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Serveur;
using NomadLogic.Common.Services;
using NomadLogic.Common.Services.Database;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.DateAndTime;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.Eshots;
using NomadLogic.Common.Util.Networking.QuestionnaireWebRequest.iBrand;
using NomadLogic.iBrandDesktop.Synchro.DownloadRestRequest;
using NomadLogic.iBrandDesktop.Synchro.JujuPrintstation;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;

// ReSharper disable InconsistentNaming

namespace NomadLogic.iBrandDesktop.Services
{
    /// <summary>
    /// Classe de base de données pour l'application iBrand.
    /// </summary>
    public class IbrandDatabase : SqliteDatabase, IService, IDatabaseProjectFromServer, IDatabaseEshots, ICondensedAnswerDatabase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// Indique la version actuelle de la base de données.
        /// Il est impératif d'incrémenter cette valeur à chaque variation de base de données, pour que l'on puisse détecter
        /// que la base de données est d'un format différent et qu'il soit possible de garantir l'intégrité des données.
        public static readonly int DatabaseVersion = 10; 
        // V10 => 2021-10-18 : Modifications des fichiers ressources pour référencer vers le terminal au lieu du questionnaire.
        // V9 => 2021-09-10 : Ajout de la tables des groupes dynamiques.
        // V8 => 2020-10-30 : On enlève le sous-choix de la réponse et de la BD.
        
        /// <summary>
        /// Événement quand on créé un nouveau répondant.
        /// </summary>
        public event EventHandler OnRespondentInserted;
        
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="fileName"></param>
        public IbrandDatabase(string fileName) : base(fileName) {}
        
        public bool Init()
        {
            // Active le PRAGMA de clé étrangère:
            // Les clé étrangères sont désactivées par défaut dans la base de données SQLite:
            // Voir ici: https://stackoverflow.com/questions/4254371/enabling-foreign-key-constraints-in-sqlite
            ExecuteNonQuery("PRAGMA foreign_keys = ON;");

            log.Trace("Vérification de l'intégrité de la base de données iBrand: {0}", DbFileName);
            var version = GetMetadataSingleValue("databaseVersion");
            if (CheckDatabaseIntegrity() &&
                int.TryParse(version, out var versionInt) &&
                versionInt == DatabaseVersion) return true;

            // Si l'initialisation failli, on tente de fermer la connexion, puis de renommer la BD.
            // Dans tout les cas, on retournera nul.
            Dispose();
            var fi = new FileInfo(DbFileName);
            var newName = FileUtil.FileNameValidation(fi);
            try
            {
                log.Warn("Tentative de sauvegarder la base de données ancienne vers le nom suivant: " + newName.FullName);
                fi.MoveTo(newName.FullName);
            }
            catch (System.Exception e)
            {
                log.Error(e, $"Le renommage de l'ancienne base de données à échoué.{Environment.NewLine}\t\tOLD: {DbFileName}{Environment.NewLine}\t\tNEW: {newName.FullName}");
            }

            return false;
        }

        /// <inheritdoc />
        protected override bool CheckDatabaseIntegrity()
        {
            var tables = new[]
            {
                "metadata",
                "projet",
                "questionnaire",
                "evenement",
                "repondant",
                "reponse",
                "synchronisation",
                "fichier_telecharge",
                "printstation_success",
                "condensed_answers",
                "eshots_login_data",
                "eshots_eventdays_data",
                "groupes_dynamiques",
                "valeurs_dynamiques"
            };

            return tables.All(TableExists);
        }

        /// <inheritdoc />
        protected override void CreateTables()
        {
            // Ajout 2020-05-28 => Toutes contraintes ON DELETE des clés étrangères.

            // Ajout 22 nov 2017
            ExecuteNonQuery("CREATE TABLE metadata (Key nvarchar(45) PRIMARY KEY, Value nvarchar(45) NOT NULL)");

            ExecuteNonQuery($"INSERT INTO metadata(Key, Value) VALUES('databaseVersion', '{DatabaseVersion}')");
            
            ExecuteNonQuery("CREATE TABLE projet (ProjetID int PRIMARY KEY, Nom nvarchar(255), Supprime bit DEFAULT 0)");
            // Modification 2019-12-13
            ExecuteNonQuery("CREATE TABLE questionnaire (QuestionnaireID int PRIMARY KEY, ProjetID int NOT NULL, Nom nvarchar(255), XmlSrc nvarchar(255), PleinEcran bit DEFAULT 0, FOREIGN KEY(ProjetID) REFERENCES projet(ProjetID) ON DELETE CASCADE)");
            
            // Ajout 2018-05-31
            ExecuteNonQuery("CREATE TABLE condensed_answers (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255), Inactif bit DEFAULT 0, LocalFile nvarchar(255), FOREIGN KEY( QuestionnaireID ) REFERENCES questionnaire( QuestionnaireID ) ON DELETE CASCADE)");
            ExecuteNonQuery("CREATE INDEX condensed_answers_numero_index ON condensed_answers ( QuestionnaireID )");
            ExecuteNonQuery("CREATE INDEX condensed_answers_date_index ON condensed_answers ( DateTime )");
            ExecuteNonQuery("CREATE INDEX condensed_answers_question_no ON condensed_answers ( QuestionNo )");

            ExecuteNonQuery("CREATE TABLE evenement (EvenementID int PRIMARY KEY, QuestionnaireID int NOT NULL, Nom nvarchar(255), FOREIGN KEY( QuestionnaireID ) REFERENCES Questionnaire( QuestionnaireID ) ON DELETE CASCADE)");

            ExecuteNonQuery("CREATE TABLE repondant (RepondantID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, TerminalID int NOT NULL, DateInsertion nvarchar(100), FuseauHoraire nvarchar(255), Agent nvarchar(255), Langue nvarchar(10), SynchroNo int, Synchronise bit DEFAULT 0, ReadyToSync bit DEFAULT 1, FOREIGN KEY( TerminalID ) REFERENCES terminal ( TerminalID ) ON DELETE CASCADE)");

            ExecuteNonQuery("CREATE TABLE reponse (RepondantID int NOT NULL, QuestionNo int NOT NULL, ChoixNo int NOT NULL, Reponse ntext, Type int, PRIMARY KEY( RepondantID, QuestionNo, ChoixNo), FOREIGN KEY( RepondantID ) REFERENCES repondant( RepondantID ) ON DELETE CASCADE)");

            ExecuteNonQuery("CREATE TABLE synchronisation (SynchroID VARCHAR(100) NOT NULL PRIMARY KEY, Date nvarchar(100), PPC nvarchar(105), Step int, No INTEGER NOT NULL, ProjetID int NOT NULL, FOREIGN KEY( ProjetID ) REFERENCES Projet( ProjetID ) ON DELETE CASCADE)");

            ExecuteNonQuery("CREATE TABLE fichier_telecharge (FichierID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, SynchroID VARCHAR(100) NOT NULL, NomFichierLocal ntext, NomFichierServeur ntext, Type int, FOREIGN KEY( SynchroID ) REFERENCES synchronisation( SynchroID ) ON DELETE CASCADE)");
            
            ExecuteNonQuery("CREATE TABLE printstation_success (respID INTEGER, ipAddress VARCHAR(20), PRIMARY KEY( respID, ipAddress), FOREIGN KEY( respID ) REFERENCES repondant( RepondantID ))");

            // Ajout Kugan, pour Juju (v8, 2020)
            ExecuteNonQuery("CREATE TABLE fichier_resource (fileName TEXT NOT NULL, size long, lastModifiedDate long, TerminalID int NOT NULL, PRIMARY KEY(fileName, TerminalID), FOREIGN KEY( TerminalID ) REFERENCES Terminal( TerminalID ) ON DELETE CASCADE)");

            ExecuteNonQuery("CREATE TABLE terminal (TerminalID int NOT NULL PRIMARY KEY, QuestionnaireID int NOT NULL, ProjetID int NOT NULL, TerminalDesc nvarchar(255), QuestionnaireDesc nvarchar(255), ProjetDesc nvarchar(255), MainFile nvarchar(255), FOREIGN KEY( QuestionnaireID ) REFERENCES Questionnaire( QuestionnaireID ) ON DELETE CASCADE, FOREIGN KEY( ProjetID ) REFERENCES projet ( ProjetID ))");

            ExecuteNonQuery("CREATE TABLE posts (RespondentToken int PRIMARY KEY, QuestionnaireId int, ImgUrl nvarchar(105), ImgLocalDest nvarchar(105))");
            
            // Ajout 2019-04-17: Eshots event days and token data.
            ExecuteNonQuery("CREATE TABLE eshots_login_data (ConfigurationDate int, LastUserName nvarchar(50), AccessToken nvarchar(700), RefreshToken nvarchar(700), DontForceRefresh bit DEFAULT 0, AllowAllDays bit DEFAULT 0, AllowPastData bit DEFAULT 0)");
            ExecuteNonQuery("CREATE TABLE eshots_eventdays_data (EventDayId int, EventDayCode nvarchar(10))");

            // Ajout 2021-09-10: Tables pour groupes dynamiques.
            ExecuteNonQuery("CREATE TABLE groupes_dynamiques (HeaderId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, RepondantID int NOT NULL, DateCreated nvarchar(40), DataKey nText, FOREIGN KEY( RepondantID ) REFERENCES repondant ( RepondantID ) ON DELETE CASCADE)");
            ExecuteNonQuery("CREATE TABLE valeurs_dynamiques (HeaderId int, Value nText, FOREIGN KEY( HeaderId ) REFERENCES groupes_dynamiques ( HeaderId ) ON DELETE CASCADE)");
        }
        
        protected string GetMetadataSingleValue([NotNull] string key)
        {
            var QUERY = $"Select Value From metadata where Key='{key}'";

            try
            {
                DbDataReader reader;
                using (reader = ExecuteReader(QUERY, null))
                {
                    if (!reader.Read()) return null;
                    var obValue = reader.GetValue(0);
                    return obValue == DBNull.Value ? null : (string)obValue;
                }
            }
            catch (System.Exception) // Si la table n'existe pas ou tout autre erreur de validation.
            {
                return null;
            }
        }

        protected int SetMetadataSingleValue(string key, string value, bool removeOnly = false)
        {
            var QUERY_DEL = $"DELETE FROM metadata where Key='{key}'";
            
            const string QUERY_ADD = "INSERT INTO metadata (Key, Value) VALUES (@Key, @Value)";

            try
            {
                // Enlève précédente valeur, si existante
                var rmNbr = ExecuteNonQuery(QUERY_DEL, null);
                if (removeOnly) return rmNbr;

                // Ajoute valeur dans la BD
                var parameters = new DbParameter[2];

                var param = CreateParameter("@Key", key);
                parameters[0] = param;

                param = CreateParameter("@Value", value);
                parameters[1] = param;

                return ExecuteNonQuery(QUERY_ADD, parameters);
            }
            catch (System.Exception e) // Si la table n'existe pas ou tout autre erreur de validation.
            {
                log.Error(e);
                return 0;
            }
        }
        
        /// <summary>
        /// Retourne le terminal qui est marqué comme actif.
        /// Null s'il n'y en a pas.
        /// </summary>
        public TerminalInfo GetActive()
        {
            var ids = GetMetadataSingleValue("active");
            if (ids == null) return null;

            if (int.TryParse(ids, out var tId))
            {
                var ter = GetTerminal(tId);
                if (ter != null) return ter;
            }

            // Mauvais identifiant.
            log.Error("Bad metadata format for starting terminal. Removing metadata: " + ids);
            ClearActive();
            return null;
        }

        /// <summary>
        /// Enlève le questionnaire/projet actif.
        /// </summary>
        public void ClearActive()
        {
            SetMetadataSingleValue("active", null, true);
        }
        
        public int SetActive(int terminalId)
        {
            // Vérification que le terminal existe
            if (!IsTerminalAlreadyInserted(terminalId)) return -1;
            return SetMetadataSingleValue("active", terminalId.ToString());
        }

        public bool GetStartFullScreen()
        {
            var ids = GetMetadataSingleValue("fullscreen");
            return ids != null;
        }
        
        public int SetStartFullScreen(bool isFullScreen = true)
        {
            return SetMetadataSingleValue("fullscreen", "true", !isFullScreen);
        }
        
        /// <summary>
        /// Assigne une métadonnée au terminal qui indique s'il n'a pas été capable de charger.
        /// Si on passe faux, cela enlève la métadonnée.
        /// </summary>
        /// <param name="tdi">Le terminal à indiquer si fautif.</param>
        /// <param name="faulty">TRUE par défaut. Mettre à TRUE si fautif.</param>
        /// <returns>Le nombre de lignes modifiées.</returns>
        public int SetTerminalAsFaulty([NotNull] TerminalInfo tdi, bool faulty = true)
        {
            return SetMetadataSingleValue($"term_{tdi.TerminalId}", "faulty", !faulty); // Si faux, on enlève la ligne.
        }

        /// <summary>
        /// Réinitialise les métadonnées d'un terminal et de ses éléments connexes.
        /// À utiliser lors d'une réinitialisation (téléchargement) d'un terminal, mais pas d'une mise à jour (pour ne pas
        /// enlever le flag de projet par défaut).
        /// </summary>
        /// <param name="tdi">Le terminal à réinitialiser.</param>
        /// <returns>Le nombre de lignes modifiées.</returns>
        public int ResetTerminalMetadata(TerminalInfo tdi)
        {
            return SetTerminalAsFaulty(tdi, false);
        }

        /// <summary>
        /// Indique si un terminal à la mention d'être fautif dans la base de données.
        /// </summary>
        public bool IsTerminalFaulty([NotNull] TerminalInfo tdi)
        {
            // Il y avait une métadonnée de faute associée au questionnaire.
            return GetMetadataSingleValue($"term_{tdi.TerminalId}")?.Equals("faulty") ?? false;
        }
        
        /// <summary>
        /// Met à jour la référence BD où on détermine si un questionnaire est plein écran ou non.
        /// </summary>
        /// <param name="qi">Le questionnaire à modifier.</param>
        /// <param name="isFullScreen">TRUE si le questionnaire est formellement plein écran.</param>
        /// <returns>Le nom de lignes modifiées (1 si questionnaire existe).</returns>
        public int UpdateQuestionnaireFullSCreen([NotNull] QuestionnaireInfo qi, bool isFullScreen)
        {
            var query = $"Update questionnaire Set PleinEcran = {(isFullScreen ? 1 : 0)} Where QuestionnaireID = {qi.Id}";
            return ExecuteNonQuery(query);
        }
        
        /// <summary>
        /// Retourne les terminaux qui ont des répondants non-synchronisés.
        /// </summary>
        public IList<TerminalInfo> GetTerminalsWithRespondentsToSync()
        {
            const string query = "select t.*, count(r.RepondantID) as nbrToSync from repondant r, terminal t where r.TerminalID=t.TerminalID and r.Synchronise=0";

            using (var reader = ExecuteReader(query))
            {
                var list = new List<TerminalInfo>();

                while (reader.Read())
                {
                    // Dans le cas où il n'y a aucun répondant à synchroniser, le SQL retourne [null, null, null, null, null, null, null, 0] sur une ligne.
                    // Il faut valider ce cas de figure. SQLITE ne fait pas de outer join...
                    // Le nombre total est toujours le dernier champ, alors on peut y aller avec une lecture du nombre de champs.

                    var nbr = reader.GetInt32(reader.FieldCount-1);
                    if (nbr == 0)
                    {
                        continue;
                    }

                    AddTerminalFromReaderToList(reader, list);
                }

                return list;
            }
        }

        private Project AddProjectFromReaderToList([NotNull] IDataRecord reader, ICollection<Project> list = null)
        {
            //CREATE TABLE projet (ProjetID int PRIMARY KEY, Nom nvarchar(255), Supprime bit DEFAULT 0;

            var projectId = reader.GetInt32(0);
            var name = reader.GetString(1);

            var project = new Project(projectId, name);

            // Optionel de le mettre dans une liste.
            list?.Add(project);

            AddQuestionnaireInfos(project);
            return project;
        }

        public List<Project> GetProjects()
        {
            DbDataReader reader = null;

            var projectList = new List<Project>();

            var query = "Select * From projet Where Supprime = 0";

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    AddProjectFromReaderToList(reader, projectList);
                }

                return projectList;
            }
            finally
            {
                reader?.Close();
            }
        }

        public Project GetProject(int projectId)
        {
            return GetProject(projectId.ToString());
        }

        public Project GetProject(string projectId)
        {
            var query = "Select * From projet Where ProjetID= " + projectId + " And Supprime = 0";

            using (var reader = ExecuteReader(query, null))
            {
                return reader.Read() ? AddProjectFromReaderToList(reader) : null;
            }
        }
        
        [Obsolete]
        public Synchronization GetSynchro(string ppc, string user, string projectId)
        {
            //CREATE TABLE synchronisation (SynchroID VARCHAR(100) NOT NULL PRIMARY KEY, Date nvarchar(100), PPC nvarchar(105), Step int, No INTEGER NOT NULL, ProjetID int NOT NULL,
            //FOREIGN KEY( ProjetID ) REFERENCES Projet( ProjetID ) ON DELETE CASCADE)

            Synchronization synchro;
            var query = "SELECT * FROM synchronisation Where ProjetID=" + projectId;

            using (var reader = ExecuteReader(query, null))
            {
                if (reader.Read())
                {
                    var synchroId = reader.GetString(0);
                    var number = reader.GetInt32(4);

                    synchro = new Synchronization(synchroId, ppc, number)
                    {
                        RespondentsToSync = GetRespondentsBySyncStatus(projectId, true)
                    };

                }
                else
                {
                    AddSynchro(ppc, projectId);
                    return GetSynchro(ppc, user, projectId);
                }
            }

            return synchro;
        }

        public void SetRespondentSynchronized(int repId, long synchroNo)
        {
            var query = "Update Repondant Set Synchronise = 1, SynchroNo =" + synchroNo + " where RepondantID = " + repId;
            ExecuteNonQuery(query);
        }

        public void SetRespondentReadyToSync(int repId)
        {
            var query = "Update Repondant Set ReadyToSync = 1 Where RepondantID = " + repId;
            ExecuteNonQuery(query);
        }

        private void AddSynchro(string ppc, string projectId)
        {
            //CREATE TABLE synchronisation (SynchroID VARCHAR(100) NOT NULL PRIMARY KEY, Date nvarchar(100), PPC nvarchar(105), Step int, No INTEGER NOT NULL, ProjetID int NOT NULL,
            //FOREIGN KEY( ProjetID ) REFERENCES Projet( ProjetID ) ON DELETE CASCADE)

            const string sql = "INSERT INTO Synchronisation (SynchroID, Date, PPC, No, ProjetID) VALUES ('{0}', '{1}', '{2}', {3}, {4})";

            var sql_mod = string.Format(sql, GenerateUniqueId.GetId(), DateTime.Now, ppc, 0, projectId);

            ExecuteNonQuery(sql_mod);
        }
        
        public List<ResourceFileToDownload> GetResourceFiles(int tId)
        {
            //ExecuteNonQuery("CREATE TABLE fichier_resource (fileName TEXT NOT NULL, size long, lastModifiedDate long, TerminalID int NOT NULL,
            //PRIMARY KEY(fileName, TerminalID), FOREIGN KEY( TerminalID ) REFERENCES Terminal( TerminalID ) ON DELETE CASCADE)");

            var resourceList = new List<ResourceFileToDownload>();

            var query = "Select * From fichier_resource Where TerminalID = '"+ tId + "'";

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var srcVal = reader.GetString(0);
                    var fs = reader.GetInt64(1);
                    var t = reader.GetInt64(2);

                    var resourceFileToDownload = new ResourceFileToDownload
                    {
                        ServerModifiedTime = t,
                        Src = srcVal,
                        FileSize = fs
                    };
                    resourceList.Add(resourceFileToDownload);

                }

                return resourceList;
            }
        }
        
        public int UpdateResourceFileData(int terminalId, ResourceFileToDownload fic)
        {
            const string query = "Update fichier_resource Set size = {0}, lastModifiedDate = {1} where fileName = '{2}' and TerminalID = {3}";
            var formatted = string.Format(query, fic.FileSize, fic.ServerModifiedTime, fic.Src, terminalId);
            return ExecuteNonQuery(formatted);
        }

        public int DeleteResourceFileData(int terminalId, ResourceFileToDownload fic)
        {
            const string query = "delete from fichier_resource where fileName = '{0}' and TerminalID = {1}";
            var formatted = string.Format(query, fic.Src, terminalId);
            return ExecuteNonQuery(formatted);
        }

        public int AddResourceFileData(int terminalId, ResourceFileToDownload fic)
        {
            const string sql = "INSERT INTO fichier_resource (fileName, size, lastModifiedDate, TerminalID) VALUES (@fileName, @size, @lastModified, @tId)";
            var parameters = new DbParameter[4];

            var param = CreateParameter("@fileName", fic.Src);
            parameters[0] = param;

            param = CreateParameter("@size", fic.FileSize);
            parameters[1] = param;

            param = CreateParameter("@lastModified", fic.ServerModifiedTime);
            parameters[2] = param;

            param = CreateParameter("@tId", terminalId);
            parameters[3] = param;

            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Cette méthode va mettre à jour la base de données pour refléter les mises-à-jour possibles
        /// d'une liste de fichiers à télécharger. Soit MAj ou ajouter un fichier, ou l'enlever de la base de données
        /// si ce fichier ne fait plus partie des fichiers à suivre.
        /// </summary>
        /// <param name="terminalId">L'identifiant du questionnaire associé à cette liste de fichiers.</param>
        /// <param name="ressourceList">La liste de fichiers du serveur à mettre à jour localement.</param>
        /// <returns>Le nombre de lignes modifiées.</returns>
        public int ManageRessourceFiles(int terminalId, IEnumerable<ResourceFileToDownload> ressourceList)
        {
            var nbrLine = 0;

            foreach (var fic in ressourceList)
            {
                switch (fic.Action)
                {
                    case RessourceFileAction.Add:
                    case RessourceFileAction.Update:
                        var upLine = UpdateResourceFileData(terminalId, fic);
                        log.Trace("UPDATE request for ressource file ({0}), lines modified: {1}", fic, upLine);
                        if (upLine > 0)
                        {
                            nbrLine += upLine;
                        }
                        else
                        {
                            // Un update va insérer si l'élément n'existe pas.
                            nbrLine += AddResourceFileData(terminalId, fic);
                            log.Trace("ADD request for ressource file ({0}), lines modified: {1}", fic, nbrLine);
                        }
                        break;
                    case RessourceFileAction.Remove:
                        nbrLine += DeleteResourceFileData(terminalId, fic);
                        log.Trace("DELETE request for ressource file ({0}), lines modified: {1}", fic, nbrLine);
                        break;
                } // Ignore == Default
            }

            return nbrLine;
        }
        
        public int InsertPost(string token,int qId,string imgUrl,string localDest)
        {
            if (IsPostAlreadyInserted(token)) return 0;

            const string sql = "INSERT INTO posts (RespondentToken,QuestionnaireId,ImgUrl,ImgLocalDest) VALUES (@RespondentToken,@QId,@ImgUrl,@ImgLocalFileDest)";

            var parameters = new DbParameter[4];

            var param = CreateParameter("@RespondentToken", token);
            parameters[0] = param;
                
            param = CreateParameter("@Qid", qId);
            parameters[1] = param;

            param = CreateParameter("@ImgUrl", imgUrl);
            parameters[2] = param;

            param = CreateParameter("@ImgLocalFileDest", localDest);
            parameters[3] = param;

            return ExecuteNonQuery(sql, parameters);
        }

        public List<GalleryUnitElement> GetPosts(int qId, int startIndex,int endIndex)
        {
            var sql = "SELECT * from posts WHERE QuestionnaireID = '"+qId+"' limit "+startIndex+","+endIndex;

            DbDataReader reader = null;

            try
            {
                var list = new List<GalleryUnitElement>();
                reader = ExecuteReader(sql, null);
                while (reader.Read())
                {
                    var galleryElem = new GalleryUnitElement
                    {
                        Token = reader.GetString(0),
                        ImgUrl = reader.GetString(3)
                    };
                    list.Add(galleryElem);

                }

                return list;

            }
            finally
            {
                reader?.Close();
            }

        }
        
        [Obsolete]
        public int GetTotalNumberOfPosts(int qId)
        {
            var sql = "SELECT * from posts where QuestionnaireID = '"+qId+"'";

            var total = 0;

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(sql, null);
                while (reader.Read())
                {
                    total++;

                }

                return total;

            }
            finally
            {
                reader?.Close();
            }
        }


        private bool IsPostAlreadyInserted(string respToken)
        {
            DbDataReader reader = null;

            var query = "SELECT * " +
                        "FROM posts Where  RespondentToken ='" + respToken+"'";
            try
            {
                reader = ExecuteReader(query, null);

                return reader.Read();
            }
            finally
            {
                reader?.Close();
            }
        }

        public bool IsTerminalAlreadyInserted(int tId)
        {
            var query = $"SELECT * FROM terminal Where  TerminalID ='{tId}'";
            
            using (var reader = ExecuteReader(query, null))
            {
                return reader.Read();
            }
        }
        
        public void DeleteSynchro(string synchroId)
        {
            var query = "Delete from Synchronisation where SynchroID = @SynchroID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@SynchroID", synchroId);
            parameters[0] = param;

            ExecuteNonQuery(query, parameters);
        }

        public void DeleteProject(Project project)
        {
            var query = "Delete from Projet where ProjetID = " + project.Id;
            var nbrProj = ExecuteNonQuery(query);
            log.Info("Project '{0} - {1}' destroyed. Stats: line destroyed = {2}", project.Id, project.Name, nbrProj);

            /*
            log.Info("Project '{0} - {1}' destroyed. Stats: Syncs={2}, Answers={3}, Respondant={4}, Events={5}, Assets={6}, Questionnaires={7}, Projects={8}",
                project.Id, project.Name, nbrSync, nbrResponse, nbrRespondant, nbrEvent, nbrRessource, nbrQuest, nbrProj);
            */
        }
        
        public List<int> GetRespondentsId()
        {

            var respondentList = new List<int>();

            const string query = "Select * From repondant";

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var repId = reader.GetInt32(0);

                    respondentList.Add(repId);
                }

                return respondentList;

            }
            finally
            {
                reader?.Close();
            }
        }
        
        public Respondent GetRespondent(int respId)
        {
            /*
                TODO: Comme Le répondant contient un modèle dépassé (il fait référence à des identifiant externe comme son projet et son questionnaire)
                On doit utiliser une requête large pour chercher ces informations. Trop de code obsololète se fie à ce que le répondant contienne ces informations
                pour l'enlever.
            */

            var query = $"Select R.*, T.QuestionnaireID, T.ProjetID From repondant R, terminal T where T.TerminalID=R.TerminalID and R.RepondantID = {respId}";

            using (var reader = ExecuteReader(query, null))
            {
                return !reader.Read() ? null : AddRespondentFromReaderToList(reader);
            }
        }

        /// <summary>
        /// Lecteur de répondant pour créer une seule instance, et si on l'ajoute dans une liste ou non.
        /// Principale méthode pour créer une instance à partir d'un "reader" de BD.
        /// </summary>
        /// <param name="reader">Le lecteur BD sur une position X. On ne lit qu'une seule entrée.</param>
        /// <param name="list">La liste, optionnelle, où mettre l'instance.</param>
        /// <returns>On retourne également l'instance lue en référence.</returns>
        private static Respondent AddRespondentFromReaderToList([NotNull] IDataRecord reader, ICollection<Respondent> list = null)
        {
            /*  IMPORTANT
                TODO: Comme Le répondant contient un modèle dépassé (il fait référence à des identifiant externe comme son projet et son questionnaire)
                On doit utiliser une requête large pour chercher ces informations. Trop de code obsololète se fie à ce que le répondant contienne ces informations
                pour l'enlever. On assume que l'appellant à utiliser une requête de jointure pour obtenir les champs de questionnaireID et projetID à la fin, dans cet ordre.
            */

            /* Modèle des tables.
                repondant (
                    RepondantID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                    TerminalID int NOT NULL, 
                    DateInsertion nvarchar(100), 
                    FuseauHoraire nvarchar(255), 
                    Agent nvarchar(255), 
                    Langue nvarchar(10), 
                    SynchroNo int, 
                    Synchronise bit DEFAULT 0, 
                    ReadyToSync bit DEFAULT 1
                terminal (
                    TerminalID int NOT NULL PRIMARY KEY, 
                    QuestionnaireID int NOT NULL, 
                    ProjetID int NOT NULL, 
                    TerminalDesc nvarchar(255), 
                    QuestionnaireDesc nvarchar(255), 
                    ProjetDesc nvarchar(255), 
                    MainFile nvarchar(255)
            */

            var index = -1;
            var repId = reader.GetInt32(++index);
            var terId = reader.GetInt32(++index);
            var date = reader[++index] as string;
            var fuseau = reader[++index] as string;
            var agent = reader[++index] as string;
            var langue = reader[++index] as string;

            var synchroNo = 0;
            var rec = reader[++index]; // Peut être null
            if (rec is int sync)
            {
                synchroNo = sync;
            }

            var isSync = reader.GetBoolean(++index);
            var isReadyToSync = reader.GetBoolean(++index);

            // Extra pour les identifiants questionnaire et projet
            var qId = reader.GetInt32(++index);
            var pId = reader.GetInt32(++index);

            if (DateTime.TryParse(date, out var dt))
            {
                var respondent = new Respondent
                {
                    Id = repId,
                    TerminalId = terId,
                    ProjectId = pId,
                    QuestionnaireId = qId,
                    DateLocal = dt,
                    TimeZone = fuseau,
                    Representative = agent,
                    Language = langue,
                    SynchroId = synchroNo.ToString(),
                    IsSynchronized = isSync,
                    IsReadyToSync = isReadyToSync
                };

                list?.Add(respondent);

                return respondent;
            }
            
            log.Error("Could not parse date for respondent");
            return null;
        }

        public string GetRespondentIdFromResponse(string response)
        {
            string respondentId = null;
            DbDataReader reader = null;

            var sql = "Select * From reponse where Reponse = @Reponse";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@Reponse", response);
            parameters[0] = param;

            try
            {
                reader = ExecuteReader(sql, parameters);

                if (reader.Read())
                {
                    var repId = (int)reader["RepondantID"];
                    respondentId = repId.ToString();
                }

                return respondentId;

            }
            finally
            {
                reader?.Close();
            }
        }

        private Answer AddAnswerFromReaderToList([NotNull] IDataRecord reader, ICollection<Answer> list = null)
        {
            /*
                reponse (
                    RepondantID int NOT NULL, 
                    QuestionNo int NOT NULL, 
                    ChoixNo int NOT NULL, 
                    Reponse ntext, 
                    Type int, 
                    PRIMARY KEY( RepondantID, QuestionNo, ChoixNo), 
                    FOREIGN KEY( RepondantID ) REFERENCES repondant( RepondantID ) ON DELETE CASCADE)
            */

            var index = 0; // On saute l'identifiant de répondant à l'index zéro
            var questionNo = reader.GetInt32(++index);
            var choiceNo = reader.GetInt32(++index);

            var reps = reader[++index] as string;
            if (string.IsNullOrWhiteSpace(reps)) reps = null;

            var type = (QuestionTypeEnum) reader.GetInt32(++index);

            var a = new Answer
            {
                Response = reps,
                QuestionNo = questionNo,
                ChoiceNo = choiceNo,
                QuestionType = type
            };

            list?.Add(a);

            return a;
        }

        /// <summary>
        /// Obtient une liste des réponses présentes pour un répondant.
        /// </summary>
        /// <param name="respId">L'identifiant numérique du répondant.</param>
        /// <returns>Une liste non-nulle, mais possiblement vide, des réponses. Ordonnée par question.</returns>
        public List<Answer> GetAnswers(int respId)
        {
            var answers = new List<Answer>();

            var query = "Select * From reponse where RepondantID =" + respId + " Order by QuestionNo";

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    AddAnswerFromReaderToList(reader, answers);
                }

                return answers;
            }
        }

        /// <summary>
        /// Cette méthode retourne les réponses d'un répondant, dans un dictionnaire indexé par l'identifiant de réponse (QuestionIdentifier)
        /// Cette identifiant est unique par couple QuestionNo + ChoixNo
        /// </summary>
        /// <param name="respId">L'identifiant numérique du répondant.</param>
        public Dictionary<QuestionIdentifier, Answer> GetAnswersMap(int respId)
        {
            var dico = new Dictionary<QuestionIdentifier, Answer>();
            
            var query = "Select * From reponse where RepondantID =" + respId;

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var a = AddAnswerFromReaderToList(reader);
                    var qi = a.QuestionIdentifier;

                    if (!dico.ContainsKey(qi)) dico.Add(qi, a);
                }

                return dico;
            }
        }

        /// <summary>
        /// Obtient les identifiant des répondants non-synchronisés et non prêts à l'envoi pour un projet précis.
        /// Habituellement ces répondants doivent compléter leur SubmitAction pour être valides.
        /// </summary>
        /// <param name="projectId">L'identifiant numérique du projet.</param>
        /// <param name="IsReadyToSync">Indique si on veut chercher les répondants par status de prêt à envoyer (FAUX par défaut)</param>
        /// <returns>Une liste non-null, mais peut-être vide, des identifiants d'usagers à synchroniser.</returns>
        [Obsolete]
        public List<int> GetRespondentsBySyncStatus(string projectId, bool IsReadyToSync = false)
        {

            var respondentList = new List<int>();

            var query = $"Select R.RepondantID From repondant R INNER JOIN terminal T on T.TerminalID = R.TerminalID And T.ProjetID = {projectId} Where R.Synchronise = 0 And R.ReadyToSync = {(IsReadyToSync ? '1' : '0')}";

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var repId = reader.GetInt32(0);

                    respondentList.Add(repId);
                }

                return respondentList;

            }
            finally
            {
                reader?.Close();
            }
        }

        /// <summary>
        /// Obtient les identifiant des répondants non-synchronisés et non prêts à l'envoi pour un projet précis.
        /// Habituellement ces répondants doivent compléter leur SubmitAction pour être valides.
        /// On chercher ces répondants via l'identifiant de questionnaire, car les SubmitActions sont liées au questionnaire.
        /// </summary>
        /// <param name="questionnaireId">L'identifiant numérique du questionnaire.</param>
        /// <param name="IsReadyToSync">Indique si on veut chercher les répondants par status de prêt à envoyer (FAUX par défaut)</param>
        /// <returns>Une liste non-null, mais peut-être vide, des identifiants d'usagers à synchroniser.</returns>
        public List<int> GetRespondentIdsBySyncStatus(int questionnaireId, bool IsReadyToSync = false)
        {
            var respondentList = new List<int>();
            var query = $"Select R.RepondantID From repondant R INNER JOIN terminal T on T.TerminalID = R.TerminalID And T.QuestionnaireID = {questionnaireId} Where R.Synchronise = 0 And R.ReadyToSync = {(IsReadyToSync ? '1' : '0')}";
            
            using (var reader =  ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var repId = reader.GetInt32(0);
                    respondentList.Add(repId);
                }

                return respondentList;
            }
        }

        public bool HasResponseWithAnswer(string answer)
        {
            var query = "Select * From reponse Where Reponse = @Reponse";

            var parameters = new DbParameter[1];

            var param = CreateParameter("@Reponse", answer);
            parameters[0] = param;

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, parameters);

                if (reader.Read())
                {
                    return true;
                }

                return false;

            }
            finally
            {
                reader?.Close();
            }
        }

        public bool HasRespondentsToSync(int projectId)
        {
            var query = "Select * From repondant R INNER JOIN terminal T on T.TerminalID = R.TerminalID And T.ProjetID = " + projectId + " Where R.Synchronise = 0";

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, null);

                return reader.Read();
            }
            finally
            {
                reader?.Close();
            }
        }

        /// <summary>
        /// Obtient dans un tuple, la plus vieille et plus récente date d'insertion de répondant dans un terminal.
        /// S'il n'y en a pas (aucun répondant dans le terminal, ou il n'existe pas), on retourne null.
        /// </summary>
        /// <param name="terminalId">L'identifiant de terminal.</param>
        /// <returns>Null si aucun répondant, sinon un groupe de dates "premier, dernier".</returns>
        [CanBeNull]
        public Tuple<DateTime, DateTime> RespondentCreationDateIntervalFromTerminal(int terminalId)
        {
            var queryLast = $@"Select r.RepondantID, r.DateInsertion From repondant r where r.TerminalID = {terminalId} ORDER BY r.DateInsertion DESC LIMIT 1";
            var queryFirst = $@"Select r.RepondantID, r.DateInsertion From repondant r where r.TerminalID = {terminalId} ORDER BY r.DateInsertion ASC LIMIT 1";

            var dtLast = DateTime.MaxValue;
            var dtFirst = DateTime.MinValue;
            var error = false;

            using (var reader = ExecuteReader(queryLast, null))
            {
                if (reader.Read())
                {
                    var str = reader.GetString(1);
                    dtLast = DateTime.Parse(str);
                }
                else
                {
                    error = true;
                }
            }

            using (var reader = ExecuteReader(queryFirst, null))
            {
                if (reader.Read())
                {
                    var str = reader.GetString(1);
                    dtFirst = DateTime.Parse(str);
                }
                else
                {
                    error = true;
                }
            }

            return error ? null : new Tuple<DateTime, DateTime>(dtFirst, dtLast);
        }

        /// <summary>
        /// Retourne un tuple contenant 1) Le total de répondants associés à ce terminal. 2) Combien d'entre eux ne sont pas encore synchronisés.
        /// </summary>
        /// <param name="terminalId">L'identifiant du terminal.</param>
        /// <returns>Un tuple contenant les information. En cas de problème ou d'absence de données, les valeurs sont de -1.</returns>
        public Tuple<int, int> GetTerminalTotalRespondents(int terminalId)
        {
            var nbrNotSync = -1;
            var nbrTotal = -1;
            var queryNotSynch = $"Select COUNT(RepondantID) From repondant R where R.Synchronise = 0 and R.TerminalID = {terminalId}";
            var queryTotal = $"Select COUNT(RepondantID) From repondant R where R.TerminalID = {terminalId}";

            {
                using (var reader = ExecuteReader(queryNotSynch, null))
                {
                    if (reader.Read())
                    {
                        nbrNotSync = reader.GetInt32(0);
                    }
                }
            }

            {
                using (var reader = ExecuteReader(queryTotal, null))
                {
                    if (reader.Read())
                    {
                        nbrTotal = reader.GetInt32(0);
                    }
                }
            }

            return new Tuple<int, int>(nbrTotal, nbrNotSync);
        }
        
        public int GetTotalRespondentsToSync(int questionnaireId)
        {
            var nbrRespondents = 0;
            // var query = "Select COUNT(RepondantID) From repondant R Where R.Synchronise = 0 And QuestionnaireID in (Select QuestionnaireID from repondant where RepondantID =" + repId+")";
            var query = $"Select COUNT(RepondantID) From repondant R Where R.Synchronise = 0 And QuestionnaireID = {questionnaireId})";

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, null);

                if (reader.Read())
                {
                    nbrRespondents = reader.GetInt32(0);
                }

                return nbrRespondents;

            }
            finally
            {
                reader?.Close();
            }
        }

        private void AddQuestionnaireInfos(Project project)
        {
            if (project == null) return;
            var query = "Select * From questionnaire Where ProjetID=" + project.Id;
            
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.HasRows)
                {
                    if (!reader.Read()) return;
                    var qi = AddQuestionnaireFromReaderToList(reader);

                    project.AddQuestionnaireInfo(qi);
                }
            }
        }
        
        [SuppressMessage("ReSharper", "UnusedVariable")]
        private QuestionnaireInfo AddQuestionnaireFromReaderToList([NotNull] IDataRecord reader, ICollection<QuestionnaireInfo> list = null)
        {
            /*
                questionnaire (
                    QuestionnaireID int PRIMARY KEY, 
                    ProjetID int NOT NULL, 
                    Nom nvarchar(255), 
                    XmlSrc nvarchar(255), 
                    PleinEcran bit DEFAULT 0, 
                    FOREIGN KEY(ProjetID) REFERENCES projet(ProjetID) ON DELETE CASCADE)
            */

            var index = -1;
            var questionnaireId = reader.GetInt32(++index);
            var projectId = reader.GetInt32(++index);
            var name = reader[++index] as string;
            var fileSrc = reader[++index] as string; // TODO N'est plus utilisé. Cette info se trouve dans terminal désormais.
            _ = reader.GetBoolean(++index); // TODO n'est plus utilisé. Est lu dans le chargement de questionnaire JUJU via info de fichier principal.

            var qi = new QuestionnaireInfo(questionnaireId, name, fileSrc, projectId);

#pragma warning disable CS0612 // Type or member is obsolete
            AddEvents(qi);
#pragma warning restore CS0612 // Type or member is obsolete

            list?.Add(qi);

            return qi;
        }

        /// TODO: On n'utilise plus les events comme information valable. Tout est remplacer par des terminaux désormais.
        [Obsolete]
        private void AddEvents(QuestionnaireInfo qi)
        {
            DbDataReader reader = null;

            // ExecuteNonQuery("CREATE TABLE evenement (EvenementID int PRIMARY KEY, QuestionnaireID int, Nom nvarchar(255))");

            var query = "Select * From evenement Where QuestionnaireID=" + qi.Id;

            try
            {
                reader = ExecuteReader(query, null);

                while (reader.Read())
                {
                    var eventId = reader.GetInt32(0);
                    string name = null;
                    if (!reader.IsDBNull(2))
                    {
                        name = reader.GetString(2);
                    }
                    var questionnaireId = reader.GetInt32(1);

                    qi.AddEvent(new IbrandEventInfo(eventId, name, questionnaireId));
                }
            }
            finally
            {
                reader?.Close();
            }
        }
        
        public int SaveProject(Project project)
        {
            var nbrLine = 0;
            if (IsProjectAlreadyInserted(project))
                nbrLine += UpdateProject(project);
            else
                nbrLine += InsertProject(project);

            var questionnaireInfos = project.QuestionnaireInfos;

            foreach (var t in questionnaireInfos)
            {
                if (IsQuestionnaireAlreadyInserted(t))
                    nbrLine += UpdateQuestionnaire(t);
                else
                    nbrLine += InsertQuestionnaire(t);
            }

            return nbrLine;
        }

        private int InsertProject(Project project)
        {
            const string sql = "INSERT INTO projet (ProjetID, Nom) VALUES (@ProjetID, @Nom)";
            var parameters = new DbParameter[2];

            var param = CreateParameter("@ProjetID", project.Id);
            parameters[0] = param;

            param = CreateParameter("@Nom", project.Name);
            parameters[1] = param;

            return ExecuteNonQuery(sql, parameters);
        }

        public int InsertRespondent(Respondent respondent, bool readyToSync, bool signalRespondent = true)
        {
            /*
            repondant (
                RepondantID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
                TerminalID int NOT NULL, 
                DateInsertion nvarchar(100), 
                FuseauHoraire nvarchar(255), 
                Agent nvarchar(255), 
                Langue nvarchar(10), 
                SynchroNo int, 
                Synchronise bit DEFAULT 0, 
                ReadyToSync bit DEFAULT 1, 
            FOREIGN KEY( TerminalID ) REFERENCES terminal ( TerminalID ) ON DELETE CASCADE)
            */

            const string sql = "INSERT INTO repondant (TerminalID, DateInsertion, FuseauHoraire, Agent, Langue, ReadyToSync) VALUES (@TerminalID, @DateInsertion, @FuseauHoraire, @Agent, @Langue, @ReadyToSync)";
            
            var dbParams = new List<DbParameter>
            {
                CreateParameter("@TerminalID", respondent.TerminalId),
                CreateParameter("@DateInsertion", respondent.DateLocal.ToString(DateUtil.UtcTimezoneFormatString)),
                CreateParameter("@FuseauHoraire", respondent.TimeZone),
                CreateParameter("@Agent", respondent.Representative),
                CreateParameter("@Langue", respondent.Language),
                CreateParameter("@ReadyToSync", readyToSync ? 1 : 0)
            };

            try
            {
                ExecuteNonQuery(sql, dbParams.ToArray());
                var repId = GetLastInsertedRespondentId();
                respondent.Id = repId;

                var answers = respondent.Answers;

                foreach (var answer in answers)
                {
                    InsertAnswer(repId, answer);
                }
                InsertDynamicHeaders(repId, respondent.DynamicHeaders);

                // Permet de signaler plus tard si un lot de répondant a été créé en même temps.
                if (signalRespondent)
                {
                    OnRespondentInserted?.Invoke(this, EventArgs.Empty);
                }
                
                return repId;
            }
            catch (System.Exception e)
            {
                log.Error(e,$"SQL respondent insert failed. SyncToSet={readyToSync}, Values: {respondent.ToStringPublicProperties()}");
                throw;
            }

        }
        
        /// <summary>
        /// Ajoute une réponse au répondant. Si une réponse existe déjà, elle sera mise à jour.
        /// </summary>
        /// <param name="repId">Le'identifiant du répondant</param>
        /// <param name="answer">La réponse à ajouter.</param>
        /// <returns>Le nombre de lignes modifiées. Zéro ijmplique aucun changement.</returns>
        public int InsertAnswer(int repId, Answer answer)
        {
            if (answer == null) return 0;
            // D'abord on valide que la réponse n'existe pas
            var sqlExiste = $"SELECT * FROM reponse WHERE RepondantID = {repId} AND QuestionNo = {answer.QuestionNo} AND ChoixNo = {answer.ChoiceNo}";
            bool hasRow;
            using (var reader = ExecuteReader(sqlExiste))
            {
                hasRow = reader.Read();
            }

            if (hasRow) return UpdateAnswer(repId, answer);

            var pars = new List<DbParameter> 
            {
                CreateParameter("@RepondantID", repId),
                CreateParameter("@QuestionNo", answer.QuestionNo),
                CreateParameter("@ChoixNo", answer.ChoiceNo),
                CreateParameter("@Reponse", answer.Response),
                CreateParameter("@Type", (int)answer.QuestionType)
            }.ToArray();

            const string sql = "INSERT INTO reponse (RepondantID, QuestionNo, ChoixNo, Reponse, Type) VALUES (@RepondantID, @QuestionNo, @ChoixNo, @Reponse, @Type)";
            log.Trace("Asnwer insert for respondent {0}: {1}", repId, answer);
            return ExecuteNonQuery(sql, pars);
        }

        public void DeleteAnswer(int repId, Answer answer)
        {
            var sql = "Delete from reponse Where RepondantID = @RepondantID And QuestionNo = @QuestionNo And ChoixNo = @ChoixNo";
            var parameters = new DbParameter[3];

            var param = CreateParameter("@RepondantID", repId);
            parameters[0] = param;

            param = CreateParameter("@QuestionNo", answer.QuestionNo);
            parameters[1] = param;

            param = CreateParameter("@ChoixNo", answer.ChoiceNo);
            parameters[2] = param;

            ExecuteNonQuery(sql, parameters);

        }

        public void SetRespondentNotSynchronized(int repId)
        {
            var sql = "UPDATE repondant SET ReadyToSync = 0,  Synchronise = 0 WHERE RepondantID = @RepondantID";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@RepondantID", repId);
            parameters[0] = param;

            ExecuteNonQuery(sql, parameters);
        }
      
        private int UpdateAnswer(int repId, Answer answer)
        {
            // On ne modifie pas le type de la réponse. On peut ajouter cette fonction si ça pose problème.
            const string sql = "UPDATE reponse SET Reponse = @Reponse WHERE RepondantID = @RepondantID And QuestionNo = @QuestionNo And ChoixNo=@ChoixNo";
            var parameters = new List<DbParameter>
            {
                CreateParameter("@Reponse", answer.Response),
                CreateParameter("@RepondantID", repId),
                CreateParameter("@QuestionNo", answer.QuestionNo),
                CreateParameter("@ChoixNo", answer.ChoiceNo)
            }.ToArray();
            
            log.Trace("Asnwer updated for respondent {0}: {1}", repId, answer);
            return ExecuteNonQuery(sql, parameters);
        }

        public int GetLastInsertedRespondentId()
        {
            var repId = -1;

            // http://stackoverflow.com/questions/28201570/sqlite-Get-max-value-Set-to-zero-when-row-doesnt-exist-or-is-null
            const string query = "SELECT IFNULL(MAX(RepondantID), -1) FROM Repondant";
            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query, null);

                if (!reader.Read()) return repId;

                repId = reader.GetInt32(0);
            }
            finally
            {
                reader?.Close();
            }

            return repId;
        }

        /// <summary>
        /// Variation pour aller chercher une valeur entière la plus elevée dans une colonne de table. Utile pour les identifiants automatiques.
        /// ASSUME que la colonne est une valeur entière et uniquement positive.
        /// </summary>
        /// <returns>-1 si la valeur est introuvable, sinon la valeur entière.</returns>
        protected int GetHighestColumnValue([NotNull] string colName, [NotNull] string tableName)
        {
            var query = $"SELECT {colName} from {tableName} order by {colName} DESC limit 1";

            using (var reader = ExecuteReader(query, null))
            {
                if (!reader.Read()) return -1;

                return reader.GetInt32(0);
            }
        }

        private int UpdateProject(Project project)
        {
            var sql = "UPDATE projet Set Nom='" + project.Name + "', Supprime = 0 WHERE ProjetID = " + project.Id;

            return ExecuteNonQuery(sql);
        }

        private bool IsProjectAlreadyInserted(Project project)
        {
            return project != null && IsProjectAlreadyInserted(project.Id);
        }

        private bool IsProjectAlreadyInserted(int project)
        {
            var query = "SELECT * FROM projet Where ProjetID =" + project;
            using (var reader = ExecuteReader(query, null))
            {
                return reader.Read();
            }
        }

        private bool IsQuestionnaireAlreadyInserted(QuestionnaireInfo questionnaireInfo)
        {
            return IsQuestionnaireAlreadyInserted(questionnaireInfo.Id);
        }

        private bool IsQuestionnaireAlreadyInserted(int qId)
        {
            var query = "SELECT * FROM questionnaire Where QuestionnaireID =" + qId;
            using (var reader = ExecuteReader(query, null))
            {
                return reader.Read();
            }
        }

        private bool IsEventAlreadyInserted(IbrandEventInfo eventt)
        {
            var query = "SELECT * FROM evenement Where EvenementID =" + eventt.Id;
            using (var reader = ExecuteReader(query, null))
            {
                return reader.Read();
            }
        }

        private int InsertQuestionnaire(QuestionnaireInfo questionnaireInfo)
        {
            const string sql = "INSERT INTO questionnaire (QuestionnaireID, ProjetID, Nom, XmlSrc) VALUES (@QuestionnaireID, @ProjetID, @Nom, @XmlSrc)";
            var parameters = new DbParameter[4];

            var param = CreateParameter("@QuestionnaireID", questionnaireInfo.Id);
            parameters[0] = param;

            param = CreateParameter("@ProjetID", questionnaireInfo.ProjectId);
            parameters[1] = param;

            param = CreateParameter("@Nom", questionnaireInfo.Name);
            parameters[2] = param;

            param = CreateParameter("@XmlSrc", questionnaireInfo.FileSrc);
            parameters[3] = param;

            var nbrLine = ExecuteNonQuery(sql, parameters);

            var events = questionnaireInfo.Events;

            if (events == null) return nbrLine;
            nbrLine += events.Sum(InsertEvent);

            return nbrLine;
        }

        private int InsertEvent(IbrandEventInfo eventt)
        {
            const string sql = "INSERT INTO evenement (EvenementID, QuestionnaireID, Nom) VALUES (@EvenementID, @QuestionnaireID, @Nom)";
            var parameters = new DbParameter[3];

            var param = CreateParameter("@EvenementID", eventt.Id);
            parameters[0] = param;

            param = CreateParameter("@QuestionnaireID", eventt.QuestionnaireId);
            parameters[1] = param;

            param = CreateParameter("@Nom", eventt.Name);
            parameters[2] = param;

            return ExecuteNonQuery(sql, parameters);
        }

        private int UpdateEvent(IbrandEventInfo eventt)
        {
            var sql = "UPDATE evenement Set Nom='" + eventt.Name + "' WHERE EvenementID = " + eventt.Id;
            return ExecuteNonQuery(sql);
        }

        private int UpdateQuestionnaire(QuestionnaireInfo questionnaireInfo)
        {
            var sql = "UPDATE questionnaire Set Nom='" + questionnaireInfo.Name + "', XmlSrc ='" + questionnaireInfo.FileSrc + "' WHERE QuestionnaireID = " + questionnaireInfo.Id;
            var nbrLine = ExecuteNonQuery(sql);

            var events = questionnaireInfo.Events;

            if (events == null) return nbrLine;
            foreach (var t in events)
            {
                if (IsEventAlreadyInserted(t))
                    nbrLine += UpdateEvent(t);
                else
                    nbrLine += InsertEvent(t);
            }

            return nbrLine;
        }

        /// <summary>
        /// Obtient les adresses IP (sans port) qui ont reçu avec succès les fichiers venant de ce répondant.
        /// On utilise le résultat pour ne pas envoyer deux fois le résultat d'un répondant vers la même addresse IP.
        /// </summary>
        /// <param name="respId">L'identifiant unique du répondant.</param>
        /// <returns>La liste, non-null mais peut-être vide, des addresses IP sans port qui ont déjà eu du succès dans le passé.</returns>
        public List<string> GetSuccessfulPrintstationFileSending(int respId)
        {
            var addressList = new List<string>();

            var query = $"Select ipAddress From printstation_success Where respID = {respId}";

            DbDataReader reader = null;

            try
            {
                reader = ExecuteReader(query);

                while (reader.Read())
                {
                    var ip = reader.GetString(0);
                    addressList.Add(ip);
                }
            }
            finally
            {
                reader?.Close();
            }

            return addressList;
        }

        /// <summary>
        /// Met à jour la liste d'adresses de succès Printstations pour un répondant. Une adresse existante ne sera pas ajoutée deux fois, et
        /// les adresses qui ne sont pas existantes seront ajoutées.
        /// </summary>
        /// <param name="respId">L'identifiant du répondant.</param>
        /// <param name="newAddresses">La liste de résultats.</param>
        public void UpdateSuccessfulPrintstationFileSending(int respId, List<string> newAddresses)
        {
            if (respId < 1) return;

            var currentList = GetSuccessfulPrintstationFileSending(respId);

            var toAdd = newAddresses.Except(currentList);
            var toRemove = currentList.Except(newAddresses);

            foreach (var ip in toRemove)
            {
                var sql = $"Delete from printstation_success Where respID = {respId} And ipAddress = '{ip}'";
                if (1 != ExecuteNonQuery(sql))
                {
                    log.Warn("Could not remove ip address: resp={0}, ip={1}", respId, ip);
                }
            }

            foreach (var ip in toAdd)
            {
                var sql = $"INSERT INTO printstation_success VALUES ({respId}, '{ip}')";
                if (1 != ExecuteNonQuery(sql))
                {
                    log.Warn("Could not add ip address: resp={0}, ip={1}", respId, ip);
                }
            }

            // Les addresses qui sont contenues dans les deux listes sont conservées.
        }

        /// <summary>
        /// Insère les valeurs lue d'une requête vers le serveur pour collecter les données d'un questionnaire.
        /// Ne fait aucune vérification de conformité de données pour accélerer le processus; on ne fait que stocker les données.
        /// </summary>
        /// <returns>Le nombre de tuples ajoutés.</returns>
        public int InsertCondensedAnswersData(int questionnaireId, int serverRespId, DateTime respLocalTime, int qNo, int cNo, string textResponse)
        {
            //"CREATE TABLE condensed_answers (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255), Inactif bit DEFAULT 0, LocalFile nvarchar(255),
            //FOREIGN KEY( QuestionnaireID ) REFERENCES questionnaire( QuestionnaireID ) ON DELETE CASCADE)");

            const string sql = "INSERT INTO condensed_answers (QuestionnaireID, Numero, DateTime, QuestionNo, ChoixNo, Reponse) VALUES (@QuestionnaireID, @Numero, @DateTime, @QuestionNo, @ChoixNo, @Reponse)";

            var parameters = new[]
            {
                CreateParameter("@QuestionnaireID", questionnaireId),
                CreateParameter("@Numero", serverRespId),
                CreateParameter("@DateTime", respLocalTime.Ticks),
                CreateParameter("@QuestionNo", qNo),
                CreateParameter("@ChoixNo", cNo),
                CreateParameter("@Reponse", textResponse)
            };

            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Met à jour le fichier local à associer avec une réponse condensée faisant référence à un fichier local.
        /// Souvent ce fichier doit être téléchargé d'une autre source (référencée par la valeur de la réponse souvent), et donc doit
        /// presque toujours être mise à jour après l'insertion initiale.
        /// NOTE: Il est possible de donner des valeurs Jeton ici, comme "NOT FOUND" ou "UNKNOWN" pour indiquer que la réponse est invalide
        /// car il est impossible de localiser le fichier distant.
        /// </summary>
        /// <param name="serverRespId">L'identifiant serveur du répondant.</param>
        /// <param name="qi">L'identifiant de la question.</param>
        /// <param name="localFilePath">Le chemin vers le fichier.</param>
        /// <returns>Le nombre de lignes modifiées (normalement 1 si la ligne existait).</returns>
        public int UpdateCondensedAnswerLocalFile(int serverRespId, QuestionIdentifier qi, string localFilePath)
        {
            //"CREATE TABLE condensed_answers (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255), Inactif bit DEFAULT 0, LocalFile nvarchar(255),
            //FOREIGN KEY( QuestionnaireID ) REFERENCES questionnaire( QuestionnaireID ) ON DELETE CASCADE)");

            const string sql = "UPDATE condensed_answers SET LocalFile = @localFilePath WHERE Numero = @serverID and QuestionNo = @qNo and ChoixNo = @cNo";

            var parameters = new[]
            {
                CreateParameter("@localFilePath", localFilePath),
                CreateParameter("@serverID", serverRespId),
                CreateParameter("@qNo", qi.QuestionNo),
                CreateParameter("@cNo", qi.ChoiceNo)
            };

            return ExecuteNonQuery(sql, parameters);
        }

        /// <summary>
        /// Insère les valeurs lue d'une requête vers le serveur pour collecter les données d'un questionnaire.
        /// Ne fait aucune vérification de conformité de données pour accélerer le processus; on ne fait que stocker les données.
        /// </summary>
        /// <param name="questionnaireId">Identifiant du questionnaire interrogé.</param>
        /// <param name="cqaList">La liste de réponses condensées.</param>
        /// <returns>Le nombre de tuples ajoutés.</returns>
        [Obsolete]
        public int InsertCondensedAnswersData(int questionnaireId, IEnumerable<IBrandCondensedQuestionnaireAnswer> cqaList)
        {
            var total = 0;
            foreach (var cqa in cqaList)
            {
                var success = DateTime.TryParseExact(cqa.DateString + " " + cqa.HeureString, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                if (!success)
                {
                    log.Warn($"Invalid date received from server: '{cqa.DateString} {cqa.HeureString}'");
                    continue;
                }

                total += InsertCondensedAnswersData(questionnaireId, cqa.Numero, date, cqa.QuestionNo, cqa.ChoixNo, cqa.Reponse);
            }

            return total;
        }

        /// <summary>
        /// Obtient le numéro de répondant le plus élevé pour un questionnaire donné. Si aucune réponse n'existe, zéro.
        /// </summary>
        /// <param name="questionnaireId">Le questionnaire associé.</param>
        /// <returns>0, ou le dernier numéro de répondant.</returns>
        public int GetLastRespondentIdFromCondensedAnswers(int questionnaireId)
        {
            var query = "SELECT IFNULL(MAX(Numero), 0) FROM condensed_answers Where QuestionnaireID = " + questionnaireId;

            var repId = 0;

            using (var reader = ExecuteReader(query, null))
            {
                if (!reader.Read()) return repId;
                repId = reader.GetInt32(0);
            }

            return repId;
        }

        /// <summary>
        /// Obtient la date de création la plus récente pour un questionnaire donné. Si aucune réponse n'existe, cette valeur sera DateTime.Min
        /// </summary>
        /// <param name="questionnaireId">Le questionnaire associé.</param>
        /// <param name="lowest">Inverse la logique et tente d'obtenir la plus petite date connue.</param>
        /// <returns>Date minimale, ou la plus récente valeur</returns>
        public DateTimeOffset GetHighestDateFromCondensedAnswers(int questionnaireId, bool lowest = false)
        {
            var query = $"SELECT IFNULL({(lowest ? "MIN" : "MAX")}(DateTime), 0) FROM condensed_answers Where QuestionnaireID = {questionnaireId}";

            using (var reader = ExecuteReader(query, null))
            {
                if (!reader.Read()) return DateTimeOffset.MinValue;
                var ticks = reader.GetInt64(0);

                return new DateTime(ticks, DateTimeKind.Utc);
            }
        }
        
        /// <summary>
        /// Retourne une liste simplifiée de référence aux numéros de répondants qui ont au moins une réponse dans la table de réponse condensée.
        /// Cette liste est voulue comme un "mapping" dans un combobox qui affiche les choix possibles, car ils sont ordonnés et indexés.
        /// Une fois le choix fait, un utilise l'index du combobox pour trouver l'élément dans la liste retournée, ce faisant trouvant le
        /// numéro de répondant unique associé aux réponses obtenues du serveur principal (et stocké localement).
        /// </summary>
        /// <param name="questionnaireId">L'identifiant de questionnaire présent.</param>
        /// <param name="stringIdentifiers">Optionel. Pour afficher dans le combo-box, on peut concaténer des réponses à des numéros de questions. Comme par exemple "Prénom Nom". Si null,
        /// on va utiliser le numéro et la date de création</param>
        /// <param name="minDate">La date minimale pour faire la recherche. Si nul, il n'y a pas de minimum et on retourne toutes les valeurs.</param>
        /// <returns>Une liste non-nulle, mais possiblement vide, des numéros de répondants potentiels par rapport au questionnaire et à la date.</returns>
        public List<CondensedRespondentReference> GetCondensedRespondentReferences(int questionnaireId, int[] stringIdentifiers = null, DateTime? minDate = null)
        {
            // (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255)
            var answers = new List<CondensedRespondentReference>();

            var query = "Select Numero, DateTime, QuestionNo, Reponse From condensed_answers where Inactif = 0 and QuestionnaireID =" + questionnaireId;

            if (minDate.HasValue)
            {
                query += " and DateTime >= " + minDate.Value.Ticks;
            }

            query += " order by Numero DESC";
            
            using (var reader = ExecuteReader(query, null))
            {
                var lastNumber = int.MaxValue;
                var builder = new StringBuilder();
                var index = 0;
                var dt = DateTime.MinValue;

                while (reader.Read())
                {
                    var num = reader.GetInt32(0);
                    dt = new DateTime(reader.GetInt64(1));
                    var qNo = reader.GetInt32(2);
                    var reponse = reader[3] as string;

                    // Si on change de numéro de répondant, on finalise l'identifiant.
                    if (lastNumber > num)
                    {
                        if (lastNumber != int.MaxValue) // Vérification du cas zéro (initial)
                        {
                            // SI aucun identifiant text n'a été défini, on utilise les variables par défaut (Numero - Date)
                            if (stringIdentifiers == null || builder.Length == 0)
                            {
                                builder.Append(lastNumber);
                                builder.Append(" - ");
                                builder.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                            }

                            // On ajoute le nouveau condensé.
                            answers.Add(new CondensedRespondentReference
                            {
                                Index = index,
                                Numero = lastNumber,
                                Identifier = builder.ToString()
                            });
                            index++;
                        }

                        // On prépare un nouveau condensé.
                        builder.Clear();
                        lastNumber = num;
                    }

                    // On construit l'identifiant actuel à partir de numéro de question si approprié.
                    if (stringIdentifiers == null || !stringIdentifiers.Contains(qNo)) continue;
                    if (builder.Length != 0) builder.Append(' ');
                    builder.Append(reponse);
                }

                if (lastNumber == int.MaxValue) return answers;

                // SI aucun identifiant text n'a été défini, on utilise les variables par défaut (Numero - Date)
                if (stringIdentifiers == null || builder.Length == 0)
                {
                    builder.Append(lastNumber);
                    builder.Append(" - ");
                    builder.Append(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                answers.Add(new CondensedRespondentReference
                {
                    Index = index,
                    Numero = lastNumber,
                    Identifier = builder.ToString()
                });

                return answers;
            }
        }

        /// <summary>
        /// Obtient toutes les réponses associées à un identifiant de répondant serveur. À ne pas confondre avec un identifiant local.
        /// ne contient pas toutes les informations possible d'un format consensé; juste en format d'instance de Answer.
        /// </summary>
        /// <param name="respNumber">L'identifiant serveur.</param>
        /// <returns>Une liste non-nulle (mais p-e vide) de réponses associées à cet identifiant.</returns>
        public List<Answer> GetAnswersFromCondensedTable(int respNumber)
        {
            // "CREATE TABLE condensed_answers (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255), Inactif bit DEFAULT 0, LocalFile nvarchar(255),
            // FOREIGN KEY( QuestionnaireID ) REFERENCES questionnaire( QuestionnaireID ) ON DELETE CASCADE)"
            var answers = new List<Answer>();

            var query = "Select QuestionNo, ChoixNo, Reponse From condensed_answers where Inactif = 0 and Numero = " + respNumber;

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var index = -1;
                    var qNo = reader.GetInt32(++index);
                    var cNo = reader.GetInt32(++index);
                    var reponse = reader[++index] as string;

                    answers.Add(new Answer
                    {
                        QuestionNo = qNo,
                        ChoiceNo = cNo,
                        Response = reponse
                    });
                }
            }

            return answers;
        }

        public IList<CondensedAnswer> GetCondensedAnswersFromRespondent(int serverRespondentId)
        {
            var query = $"Select * From condensed_answers where Numero = {serverRespondentId}";
            return MarshallCondensedAnswers(query);
        }

        private IList<CondensedAnswer> MarshallCondensedAnswers(string query)
        {
            // "CREATE TABLE condensed_answers (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255), Inactif bit DEFAULT 0, LocalFile nvarchar(255),
            // FOREIGN KEY( QuestionnaireID ) REFERENCES questionnaire( QuestionnaireID ) ON DELETE CASCADE)"

            var answers = new List<CondensedAnswer>();

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var index = -1;
                    var questId = reader.GetInt32(++index);
                    var respNo = reader.GetInt32(++index);
                    var dateTicks = reader.GetInt64(++index);
                    var qNo = reader.GetInt32(++index);
                    var cNo = reader.GetInt32(++index);
                    var reponse = reader[++index] as string;
                    var inactif = reader.GetBoolean(++index);
                    var localFile = reader[++index] as string;

                    answers.Add(new CondensedAnswer
                    {
                        QuestionnaireId = questId,
                        RespondentNumber = respNo,
                        LocalInsertedTime = new DateTime(dateTicks, DateTimeKind.Local),
                        QuestionNo = qNo,
                        ChoiceNo = cNo,
                        Response = reponse,
                        Inactive = inactif,
                        LocalFile = localFile
                    });
                }
            }

            return answers;
        }

        public IList<CondensedAnswer> GetCondensedAnswers(int questionnaireId, DateTimeOffset minCutOff, QuestionIdentifier qi = null, bool? inactive = null, bool? hasLocalFile = null, uint? limit = null)
        {
            // "CREATE TABLE condensed_answers (QuestionnaireID int, Numero int, DateTime long, QuestionNo int, ChoixNo int, Reponse nvarchar(255), Inactif bit DEFAULT 0, LocalFile nvarchar(255),
            // FOREIGN KEY( QuestionnaireID ) REFERENCES questionnaire( QuestionnaireID ) ON DELETE CASCADE)"
            
            var query = $"Select * From condensed_answers where QuestionnaireID = {questionnaireId} and DateTime > {minCutOff.Ticks}";

            if (qi != null)
            {
                query += $" and QuestionNo = {qi.QuestionNo} and ChoixNo = {qi.ChoiceNo}";
            }

            if (inactive.HasValue)
            {
                query += $" and Inactif = {(inactive.Value ? "1" : "0")}";
            }
            if (hasLocalFile.HasValue)
            {
                query += $" and LocalFile IS {(hasLocalFile.Value ? "NOT" : "")}NULL";
            }
            query += " ORDER BY DateTime DESC";
            if (limit.HasValue)
            {
                query += $" LIMIT {limit.Value}";
            }

            return MarshallCondensedAnswers(query);
        }

        /// Met inactif les réponses condensées d'un identifiant de répondant. Utilisé dans le cas où on doit faire une utilisation unique d'une séquence de réponses condensée.
        /// Retourne le nombre de tuples modifié. Zéro implique un No-Op.
        public int DeleteCondensedAnswers(int numeroRepondant)
        {
            const string sql = "Update condensed_answers Set Inactif = 1 Where Numero = @Numero";
            var parameters = new DbParameter[1];

            var param = CreateParameter("@Numero", numeroRepondant);
            parameters[0] = param;

            return ExecuteNonQuery(sql, parameters);
        }

        public EshotsDataConfiguration GetLastEshotsConfiguration()
        {
            var currentConfig = new EshotsDataConfiguration();

            const string query = "Select * From eshots_login_data";

            using (var reader = ExecuteReader(query, null))
            {
                if (!reader.Read()) return currentConfig;
                var dateInt = reader.GetInt64(0);
                if (dateInt != 0) currentConfig.ConfigurationDate = new DateTime(dateInt);
                currentConfig.LastUserName = reader.GetString(1);
                currentConfig.AccessToken = reader.GetString(2);
                currentConfig.RefreshToken = reader.GetString(3);
                currentConfig.DontForceRefresh = reader.GetBoolean(4);
                currentConfig.AllowAllDays = reader.GetBoolean(5);
                currentConfig.AllowPastData = reader.GetBoolean(6);

                // On charge les jours maintenant.
                const string query2 = "Select * From eshots_eventdays_data";

                using (var reader2 = ExecuteReader(query2, null))
                {
                    var list = new List<EshotsEventDay>();
                    while (reader2.Read())
                    {
                        var dayId = reader2.GetInt32(0);
                        var dayCode = reader2.GetString(1);

                        list.Add(new EshotsEventDay
                        {
                            EventDayId = dayId,
                            EventDayCode = dayCode
                        });
                    }
                    currentConfig.EventDaysCached = list;
                }
            }

            log.Debug("Database eshots data read from disk: " + currentConfig.ToStringPublicProperties(1));

            return currentConfig;
        }

        public bool RegisterCurrentEshotsConfiguration(EshotsDataConfiguration data)
        {
            // Vérification de l'intégrité des données: S'il y a eu un login, on doit également avoir des jetons. Sinon, on invalide les données.
            if (data != null && (data.LastUserName != null || data.ConfigurationDate != null) && (data.AccessToken == null || data.RefreshToken == null))
            {
                log.Error("Invalid eShots config: you cannot have a login without an access token or refresh token. The following config will NOT be written to disk. Previous data destroyed.:\n{0}",
                    data.ToStringPublicProperties());
                data = null;
            }

            // On détruit les données avant.
            const string sql = "Delete from eshots_login_data";
            const string sql2 = "Delete from eshots_eventdays_data";
            ExecuteNonQuery(sql);
            ExecuteNonQuery(sql2);
            log.Trace("Database eshots data cleared.");

            if (data == null) return false;
            
            // ReSharper disable once UseStringInterpolation
            var sql3 = string.Format("INSERT INTO eshots_login_data VALUES ({0}, '{1}', '{2}', '{3}', {4}, {5}, {6})",
                data.ConfigurationDate?.Ticks ?? 0,
                data.LastUserName,
                data.AccessToken,
                data.RefreshToken,
                data.DontForceRefresh ? 1 : 0,
                data.AllowAllDays ? 1 : 0,
                data.AllowPastData ? 1 : 0);
            if (1 != ExecuteNonQuery(sql3))
            {
                log.Error("Could not write Eshots configuration to database: ", data);
                return false;
            }

            foreach (var day in data.EventDaysCached ?? new List<EshotsEventDay>())
            {
                var sql4 = $"INSERT INTO eshots_eventdays_data VALUES ({day.EventDayId}, '{day.EventDayCode}')";
                if (1 != ExecuteNonQuery(sql4))
                {
                    log.Error("Could not add Eshots event day: id={0}, code={1}", day.EventDayId, day.EventDayCode);
                }
            }
            log.Debug("Database eshots data written: " + data.ToStringPublicProperties(1));

            return true;
        }

        /// <summary>
        /// Détruit l'ensemble des réponses d'un répondant dans la base de données.
        /// </summary>
        /// <param name="repId">l'identifiant du répondant.</param>
        /// <returns>Le nombre de lignes affectées.</returns>
        public int DeleteAllAnswers(int repId)
        {
            var sql = $"Delete from reponse Where RepondantID = {repId}";
            return ExecuteNonQuery(sql);
        }
        
        public int DeleteRespondant(int respId)
        {
            var total = DeleteAllAnswers(respId);
            var sql = $"Delete from repondant Where RepondantID = {respId}";
            total += ExecuteNonQuery(sql);
            return total;
        }

        /// <summary>
        /// Obtient la liste de répondants d'un terminal, ordonnés par leur identifiant numérique.
        /// </summary>
        /// <param name="terminal">Le terminal.</param>
        /// <param name="includeNotSynched">Indique s'il faut inclure les répondants qui ne sont pas synchronisés. Faux par défaut.</param>
        /// <returns>Une liste non-nulles de répondants correspondant à la requête.</returns>
        public IList<Respondent> GetRespondentsByTerminal(TerminalInfo terminal, bool includeNotSynched = false)
        {
            
            var result = new List<Respondent>();
            var querySync = " and rep.Synchronise = 0";
            var query = $"Select R.*, T.QuestionnaireID, T.ProjetID From repondant R, terminal T where T.TerminalID=R.TerminalID and R.TerminalID={terminal.TerminalId}{(!includeNotSynched ? "" : querySync)} order by R.RepondantID";

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    AddRespondentFromReaderToList(reader, result);
                }
                return result;
            }
        }

        /// <summary>
        /// Obtient la liste des réponses d'un terminal, indexées via identifiant de répondants.
        /// </summary>
        /// <param name="terminal">Le terminal.</param>
        /// <param name="includeNotSynched">Si vrai, cherche également les répondants qui ne sont pas synchronisés. Faux par défaut.</param>
        /// <returns>La liste indexée complète.</returns>
        public SortedList<int, IList<Answer>> GetAnswersByTerminal(TerminalInfo terminal, bool includeNotSynched = false)
        {
            var result = new SortedList<int, IList<Answer>>();
            var querySync = " and rep.Synchronise = 0";
            var query = $"Select r.*, rep.RepondantID from reponse r, repondant rep where r.RepondantID = rep.RepondantID and rep.TerminalID = {terminal.TerminalId}{(!includeNotSynched ? "" : querySync)} order by r.RepondantID, r.QuestionNo, r.ChoixNo";

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var a = AddAnswerFromReaderToList(reader);

                    // il reste le ID du répondant à la fin
                    var repIndex = reader.FieldCount - 1;
                    var repId = reader.GetInt32(repIndex);

                    if (!result.TryGetValue(repId, out var list))
                    {
                        list = new List<Answer>();
                        result.Add(repId, list);
                    }

                    list.Add(a);
                }

                return result;
            }
        }
        
        #region CRUD Terminal

        /// <summary>
        /// Cette méthode insère un terminal, ainsi que son questionnaire et projet si ces derniers n'existent pas déjà.
        /// </summary>
        /// <param name="tdi">Le terminal a ajouter.</param>
        /// <returns>0 Si le terminal n'a pas été ajouté (existe déjà). Sinon, 1. Si argument invalide, -1.</returns>
        public int InsertTerminal(TerminalInfo tdi)
        {
            if (tdi == null) return -1;
            if (IsTerminalAlreadyInserted(tdi.TerminalId)) return 0;

            var project = new Project(tdi.ProjectId, tdi.ProjectName);
            var qSrc = $"/projects/{tdi.ProjectId}/{tdi.QuestionnaireId}/{tdi.LoaderFile}";
            var questionnaireInfo = new QuestionnaireInfo(tdi.QuestionnaireId, tdi.ProjectName, qSrc,tdi.ProjectId);

            // TODO: This is to be removed later upon confirmation that event are no more useful when interacting with JUJU
            var rand = new Random();
            var evenement = new IbrandEventInfo(Convert.ToInt32(Math.Floor(26 * rand.NextDouble() + 65)), tdi.QuestionnaireName, tdi.QuestionnaireId);
            questionnaireInfo.AddEvent(evenement);

            project.AddQuestionnaireInfo(questionnaireInfo);
            SaveProject(project); // N'insère pas un projet déjà existant.
            
            const string sql = "INSERT INTO terminal (TerminalID,QuestionnaireID,ProjetID,TerminalDesc,QuestionnaireDesc,ProjetDesc,MainFile) VALUES (@TerminalID,@QuestionnaireID,@ProjetID,@TerminalDesc,@QuestionnaireDesc,@ProjetDesc,@MainFile)";

            var paramList = new List<DbParameter>
            {
                CreateParameter("@TerminalID", tdi.TerminalId),
                CreateParameter("@QuestionnaireID", tdi.QuestionnaireId),
                CreateParameter("@ProjetID", tdi.ProjectId),
                CreateParameter("@TerminalDesc", tdi.TerminalName),
                CreateParameter("@QuestionnaireDesc", tdi.QuestionnaireName),
                CreateParameter("@ProjetDesc", tdi.ProjectName),
                CreateParameter("@MainFile", tdi.LoaderFile)
            };

            return ExecuteNonQuery(sql, paramList.ToArray());
        }

        /// <summary>
        /// Lecteur de Terminal pour créer une seule instance, et si on l'ajoute dans une liste ou non.
        /// Principale méthode pour créer une instance à partir d'un "reader" de BD.
        /// </summary>
        /// <param name="reader">Le lecteur BD sur une position X. On ne lit qu'une seule entrée.</param>
        /// <param name="list">La liste, optionnelle, où mettre l'instance.</param>
        /// <returns>On retourne également l'instance lue en référence.</returns>
        private static TerminalInfo AddTerminalFromReaderToList([NotNull] IDataRecord reader, ICollection<TerminalInfo> list = null)
        {
            /*
            CREATE TABLE terminal ( 
                    TerminalID int NOT NULL PRIMARY KEY, 
                    QuestionnaireID int NOT NULL, 
                    ProjetID int NOT NULL, 
                    TerminalDesc nvarchar(255), 
                    QuestionnaireDesc nvarchar(255), 
                    ProjetDesc nvarchar(255), 
                    MainFile nvarchar(255), 
                FOREIGN KEY( QuestionnaireID ) REFERENCES Questionnaire( QuestionnaireID ) ON DELETE CASCADE, 
                FOREIGN KEY( ProjetID ) REFERENCES projet ( ProjetID ))
            */

            var tId = reader.GetInt32(0);
            var qId = reader.GetInt32(1);
            var pId = reader.GetInt32(2);
            var tDesc = reader[3] as string;
            var qDesc = reader[4] as string;
            var pDesc = reader[5] as string;
            var mFile = reader[6] as string;

            var t = new TerminalInfo
            {
                TerminalId = tId,
                QuestionnaireId = qId,
                ProjectId = pId,
                TerminalName = tDesc,
                QuestionnaireName = qDesc,
                ProjectName = pDesc,
                LoaderFile = mFile
            };

            // Optionel de le mettre dans une liste.
            list?.Add(t);

            return t;
        }

        /// <summary>
        /// Obtient les informations critique du terminal directement de la base de données.
        /// </summary>
        /// <param name="terminalId">L'identifiant unique du terminal.</param>
        /// <returns>NULL si le terminal n'est pas trouvé, sinon l'instance du terminal proprement dit.</returns>
        public TerminalInfo GetTerminal(int terminalId)
        {
            var query = "Select * from terminal where terminal.TerminalID=" + terminalId;

            using (var reader = ExecuteReader(query, null))
            {
                return reader.Read() ? AddTerminalFromReaderToList(reader) : null;
            }
        }

        /// <summary>
        /// Retourne tous les terminaux présent dans la BD.
        /// </summary>
        /// <returns>La liste non-null, mais p-e vide.</returns>
        public List<TerminalInfo> GetTerminals()
        {
            const string query = "Select * from terminal";

            var list = new List<TerminalInfo>();

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    AddTerminalFromReaderToList(reader, list);
                }
            }

            return list;
        }

        #endregion

        public IList<QuestionnaireInfo> GetQuestionnaireIdsWithActionsToExecute()
        {
            const string query = "SELECT q.*, t.TerminalID from questionnaire q, terminal t, repondant r WHERE r.TerminalID=t.TerminalID and T.QuestionnaireID=q.QuestionnaireID and r.ReadyToSync=0";

            var list = new List<QuestionnaireInfo>();

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var qi = AddQuestionnaireFromReaderToList(reader, list);

                    // On fait ici une étape rare, c'est à dire bonifier le QuestionnaireInfo avec son identifiant de terminal via un champ d'extra qui vient d'une autre table
                    // TODO Avec un meilleur système BD (et de classes modèles) pour correctement représenter la hiérarchie de projet/questionnaire/terminal, ce ne serait pas nécessaire.
                    var tIndex = reader.FieldCount - 1;
                    qi.TerminalID = reader.GetInt32(tIndex);
                }
            }

            return list;
        }

        #region Dynamic Headers

        /*
            groupes_dynamiques:
                HeaderId int PRIMARY KEY NOT NULL AUTOINCREMENT, 
                RepondantID int NOT NULL,
                DateCreated nvarchar(40), 
                DataKey nText,
                FOREIGN KEY( RepondantID ) REFERENCES repondant ( RepondantID ) ON DELETE CASCADE
            valeurs_dynamiques: 
                HeaderId int, 
                Value nText, 
                FOREIGN KEY( HeaderId ) REFERENCES groupes_dynamiques ( HeaderId ) ON DELETE CASCADE)
        */

        public int InsertDynamicHeaders(int repId, List<DynamicHeader> listDh)
        {
            if (listDh == null || listDh.Any() == false) return 0;

            var nbrLines = 0;
            foreach (var dh in listDh)
            {
                // On ajoute toujours des données dynamiques: il n'y a pas de concept de doublon sur le client.
                var pars = new List<DbParameter>
                {
                    CreateParameter("@RepondantID", repId),
                    CreateParameter("@DataKey", dh.DataKey),
                    CreateParameter("@DateCreated", dh.DateCreatedStr)
                }.ToArray();

                const string sql = "INSERT INTO groupes_dynamiques (RepondantID, DateCreated, DataKey) VALUES (@RepondantID, @DateCreated, @DataKey)";
                nbrLines += ExecuteNonQuery(sql, pars);

                var id = GetHighestColumnValue("HeaderID", "groupes_dynamiques");
                
                // On mets les valeurs maintenant.
                foreach (var dhv in dh.DataValues)
                {
                    var parsValues = new List<DbParameter>
                    {
                        CreateParameter("@HeaderId", id),
                        CreateParameter("@Value", dhv.ToString())
                    }.ToArray();

                    const string sqlValue = "INSERT INTO valeurs_dynamiques (HeaderId, Value) VALUES (@HeaderId, @Value)";    
                    nbrLines += ExecuteNonQuery(sqlValue, parsValues);
                }

                if (log.IsTraceEnabled) log.Trace("Dynami Header inserted for respondent {0}: {1}", repId, dh.ToString());
            }

            return nbrLines;
        }

        public int DeleteDynamicHeaders(int repId)
        {
            const string sql = "Delete from groupes_dynamiques Where RepondantID = @RepondantID";
            var pars = new List<DbParameter>
            {
                CreateParameter("@RepondantID", repId)
            }.ToArray();

            return ExecuteNonQuery(sql, pars);

        }

        public List<DynamicHeader> GetDynamicHeaders(int respId)
        {
            var headers = new List<DynamicHeader>();

            var query = $"Select g.HeaderId, g.DateCreated, g.DataKey, d.Value From valeurs_dynamiques d, groupes_dynamiques g where g.RepondantID={respId} and d.HeaderId=g.HeaderId";

            using (var reader = ExecuteReader(query, null))
            {
                AddDynamicHeadersFromReaderToList(reader, headers);

                return headers;
            }
        }
        
        private void AddDynamicHeadersFromReaderToList([NotNull] IDataReader reader, [NotNull] ICollection<DynamicHeader> list)
        {
            // g.HeaderId, g.DateCreated, g.DataKey, d.Value sont les 4 valeurs de chaque rangé de Header dynamique. Il faut créer une nouvelle instance de header
            // à chaque fois que le premier champ (I.E. HeaderId) change.

            DynamicHeader currentHeader = null;
            var headerDbId = -1;

            while (reader.Read())
            {
                var index = -1; // On commence à zéro avec incrémenteur pré-lecture.
                var hId = reader.GetInt32(++index);
                var hCreated = reader.GetString(++index);
                var hDataKey = reader.GetString(++index);
                ++index;
                string hValue = null;
                if (!reader.IsDBNull(index)) hValue = reader.GetString(index);

                if (headerDbId != hId)
                {
                    headerDbId = hId;
                    // Si on change de header, on stocke l'ancien.
                    if (currentHeader != null)
                    {
                        list.Add(currentHeader);
                    }

                    currentHeader = new DynamicHeader
                    {
                        DateCreatedStr = hCreated,
                        DataKey = hDataKey
                    };
                }

                currentHeader?.DataValues.Add(new DataValue(hValue));
            }

            // Ajout du dernier header.
            if (currentHeader != null)
            {
                list.Add(currentHeader);
            }
        }

        #endregion
    }
}
