using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Services.Database;
using NomadLogic.Common.Ui.NomadScreen;

namespace NomadLogic.Common.Services.ActivityLogger
{
    public enum ActivityLoggerActionType
    {
        Debug = 0,
        Print =1,
        Canceled = 2,
        DirectSave = 3,
        RespondentFinalized = 4,
        TimeSpent = 5,
        FtpFileSelected = 6
    }

    [PublicAPI]
    public sealed class ActivityLoggerDatabase : SqliteDatabase, IInitialisable
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static readonly (int, string, string)[] INIT_DATA =
        {
            ((int)ActivityLoggerActionType.Debug, "Debug", "Test data."),
            ((int)ActivityLoggerActionType.Print, "Print", "Print action made on external printer."),
            ((int)ActivityLoggerActionType.Canceled, "Canceled", "Questionnaire sequence aborted midway."),
            ((int)ActivityLoggerActionType.DirectSave, "Direct Save to server", "Information send to server by a screen."),
            ((int)ActivityLoggerActionType.RespondentFinalized, "Respondent finalized", "Respondant questionnaire sequence finished."),
            ((int)ActivityLoggerActionType.TimeSpent, "Time spent", "Value in seconds that a respondent used from second screen to finalization."),
            ((int)ActivityLoggerActionType.FtpFileSelected, "Ftp file search", "Attempt of a selection of a distant image file.")
        };

        public ActivityLoggerDatabase(string fileName) : base(fileName)
        {
            ExecuteNonQuery("PRAGMA foreign_keys = ON;");
        }

        /// <inheritdoc />
        protected override void CreateTables()
        {
            // Table des types d'activitées journalisées.
            ExecuteNonQuery("CREATE TABLE action_type (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name nvarchar(45) NOT NULL, Description nvarchar(100))");
            // Ajout des valeurs de cette table
            foreach (var (item1, item2, item3) in INIT_DATA)
            {
                ExecuteNonQuery($"INSERT INTO action_type (Id, Name, Description) VALUES({item1}, '{item2}', '{item3}')");
            }
            
            // Table journal principale
            ExecuteNonQuery("CREATE TABLE activity (Id INTEGER PRIMARY KEY AUTOINCREMENT, DateInserted long, ActionType int, TerminalId int, Details nvarchar(50), FOREIGN KEY( ActionType ) REFERENCES action_type( Id ) ON DELETE CASCADE)");
            ExecuteNonQuery("CREATE INDEX activity_dateinserted_index ON activity ( DateInserted )");
            ExecuteNonQuery("CREATE INDEX activity_terminalid_index ON activity ( TerminalId )");
        }

        public bool Init()
        {
            return true;
        }
        
        public int AddActivityReport(Activity act)
        {
            if (act == null) return 0;

            var now = DateTime.Now.Ticks;

            var pars = new []
            {
                CreateParameter("@DateInserted", now),
                CreateParameter("@ActionType", (int)act.ActionType),
                CreateParameter("@TerminalId", act.TerminalId),
                CreateParameter("@Details", act.Details)
            };

            const string sql = "INSERT INTO activity (DateInserted, ActionType, TerminalId, Details) VALUES (@DateInserted, @ActionType, @TerminalId, @Details)";
            return ExecuteNonQuery(sql, pars);
        }

        /// <summary>
        /// Obtient les journeaux de la BD selon les informations données. Retourne une liste des événements.
        /// </summary>
        /// <param name="terminalId">L'identifiant du terminal demandé. Requis.</param>
        /// <param name="actionType">Le type d'action que l'on veut voir. Optionnel.</param>
        /// <param name="minDate">La date minimum de tri. Inclusive. Optionnelle.</param>
        /// <param name="maxDate">La date maximum de tri. Exclusive. Optionnelle.</param>
        /// <returns></returns>
        public IList<ActivityReportItem> GetActivityReport(int terminalId, ActivityLoggerActionType? actionType = null, DateTime? minDate = null, DateTime? maxDate = null)
        {
            var query = $"SELECT * FROM activity act WHERE act.TerminalId ={terminalId}";

            if (actionType.HasValue)
            {
                query += $" AND act.ActionType = {(int)actionType.Value}";
            }

            if (minDate.HasValue)
            {
                query += $" AND act.DateInserted >= {minDate.Value.Ticks}";
            }

            if (maxDate.HasValue)
            {
                query += $" AND act.DateInserted < {maxDate.Value.Ticks}";
            }

            query += " ORDER BY DateInserted ASC";

            var list = new List<ActivityReportItem>();

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    /*
                        TABLE activity (
                            Id int PRIMARY KEY, 
                            DateInserted long,
                            ActionType int,
                            TerminalId int,
                            Details nvarchar(50)
                        )
                    */

                    var index = -1;
                    var id = reader.GetInt32(++index);
                    var dateInsertedLong = reader.GetInt64(++index);
                    var actionTypeRead = reader.GetInt32(++index);
                    var termIdRead = reader.GetInt32(++index);
                    var details = reader[++index] as string;
                    
                    var ar = new ActivityReportItem(id, new DateTime(dateInsertedLong), (ActivityLoggerActionType)actionTypeRead, termIdRead, details);
                    
                    list.Add(ar);
                }
            }

            return list;
        }
    }
}