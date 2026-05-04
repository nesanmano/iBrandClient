using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

using NomadLogic.iBrand.Services;
using NomadLogic.iBrand.Synchro;
using NomadLogic.iBrand.Transformation.Video;
using NomadLogic.iBrand.Util.Vimeo;
using NomadLogic.iBrand.Util;
using NomadLogic.iBrand.Ui.Question;
using NomadLogic.iBrand.Ui.Synchro;

namespace NomadLogic.iBrand.Util.SubmitAction
{
    class VeloVolantVideo_old : AbstractSubmitAction
    {
        /*
        private static readonly string CONSUMER_KEY = "8fc0e14767cf127688240b58fd4c6acd9cee07d2";
        private static readonly string CONSUMER_SECRET = "18fe9b701b7cb864e57940cfcb36ba13eb4f2854";
        private static readonly string TOKEN = "392290e45b515c9ede53233c325df5df";
        private static readonly string TOKEN_SECRET = "b862157fccc9c81745d27220f061246bcd9f3749";
        */
         
        internal static readonly int QUESTION_VIDEO = 1;
        internal static readonly int QUESTION_VIDEO_TEMPS = 2;
        internal static readonly int QUESTION_VIMEO_ID = 4;
        internal static readonly int QUESTION_THUMBNAIL = 3;
        internal static readonly string FICHIER_PREFIX = "";
        internal static readonly string FICHIER_INTRO_VIDEO = FICHIER_PREFIX + "intro.avi";
        internal static readonly string FICHIER_SONORE = FICHIER_PREFIX + "sound_f.wav";
        internal static readonly string FICHIER_SONORE_INTRO = FICHIER_PREFIX + "intro_f.wav";
        internal static readonly string FICHIER_SONORE_OUTRO = FICHIER_PREFIX + "outro_f.wav";
        internal static readonly string FICHIER_OUTRO_VIDEO = FICHIER_PREFIX + "outro.avi";
        internal static readonly string FICHIER_OVERLAY = FICHIER_PREFIX + "overlay.png";
        internal static readonly string FICHIER_LOGO = FICHIER_PREFIX + "velovolant_end.jpg";
        internal static readonly string FICHIER_COMPRESS = FICHIER_PREFIX + "compressOptions";

        private static System.Object _lock = null;
        private VeloVolant_WorkerPool _pool = null;

        private bool _start_vlc = false;

        private int thumbnail_time = 1;
        private string gallery_dir = "";
        private string save_dir = "";
        private string archive_dir = null;
        private bool saveFrame = false;
        private bool log = false;

        static VeloVolantVideo_old()
        {
            _lock = new System.Object();
        }

        public VeloVolantVideo_old()
        {
            _pool = new VeloVolant_WorkerPool();
            Thread t = new Thread(_pool.Run);
            t.IsBackground = true; // Important, sinon l'application ne ferme pas.
            t.Name = "VeloVolant Worker Pool";
            t.Priority = ThreadPriority.Lowest;
            t.Start();
        }

        public override void StopProcessing()
        {
            base.StopProcessing(); // super call
            _pool.Stop();
        }

        public override bool IsStopped()
        {
            return this._stopped && _pool.IsStopFinished();
        }

        /// <summary>
        /// Insère le répondant à la fin du traitement.
        /// </summary>
        /// <param name="respondent"></param>
        /// <returns></returns>
        public override bool SubmitRespondent(Respondent respondent)
        {
            try
            {
                IBrandDatabase db = (IBrandDatabase)ServicesContainer.GetService(ServicesNames.SQL_DB);
                int resp_id = db.InsertRespondent(respondent, false);

                bool ok = ProcessRespondent(respondent, resp_id);
                
                return ok;
            }
            catch (System.Exception ex)
            {
                LogUtil.GetLogger().Fatal(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Indique si VLC doit être démarré lors de la fin du traitement.
        /// </summary>
        /// <returns></returns>
        public bool HasToStartVLC()
        {
            return _start_vlc;
        }

        /// <summary>
        /// Exécute le traitement du vidéo.
        /// </summary>
        /// <param name="resp">Le répondant à traiter.</param>
        /// <param name="resp_id">L'identifiant de ce répondant.</param>
        public bool ProcessRespondent(Respondent resp, int resp_id)
        {
            GetTransformationProperties();

            Answer answer_video = resp.Answers[0];//Question video
            String video_name = answer_video.Response;

            Configuration configuration = (Configuration)ServicesContainer.GetService(ServicesNames.CONFIGURATION);

            String file_name = StringUtil.GetFileName(video_name);

            String video_moved_name = configuration.VideosDir + file_name;
            String video_original = configuration.VideosCanonDir + file_name;

            if (!File.Exists(video_original) && !File.Exists(video_moved_name))
            {
                throw new FileNotFoundException("Video file not found: " + video_original);
            }

            if (!File.Exists(video_moved_name) && File.Exists(video_original))
            {
                File.Move(video_original, video_moved_name);
            }

            VeloVolantVideo_Worker_old temp = new VeloVolantVideo_Worker_old(this, resp, resp_id, thumbnail_time, gallery_dir, saveFrame, save_dir, log);

            return _pool.AddWorker(temp);
        }

        /// <summary>
        /// Parsing rapide d'un fichier de propriété
        /// Référence: http://stackoverflow.com/questions/485659/can-net-load-and-parse-a-properties-file-equivalent-to-java-properties-class
        /// </summary>
        private void GetTransformationProperties()
        {
            string path = _project.ProjectDir + "config.txt";
            if (File.Exists(path))
            {
                var data = new Dictionary<string, string>();
                foreach (var row in File.ReadAllLines(path))
                {
                    string[] tab = row.Split('=');
                    string rest = string.Concat(tab.Skip(1).ToArray());
                    data.Add(tab[0], rest);
                }

                string temp = data["thumbnail_time"];
                if (temp != null)
                {
                    try
                    {
                        thumbnail_time = int.Parse(temp);
                    }
                    finally
                    {
                        //die nice
                    }
                }

                temp = data["gallery_dir"];
                if (temp != null)
                {
                    gallery_dir = temp;
                }

                temp = data["save_dir"];
                if (temp != null)
                {
                    save_dir = temp;
                }

                temp = data["save_frame"];
                if (temp != null)
                {
                    try
                    {
                        saveFrame = bool.Parse(temp);
                    }
                    finally
                    {
                        //die nice
                    }
                }

                temp = data["log_all"];
                if (temp != null)
                {
                    try
                    {
                        log = bool.Parse(temp);
                        _pool.EnableLog(log);
                    }
                    finally
                    {
                        //die nice
                    }
                }

                temp = data["archive_dir"];
                if (temp != null && Directory.Exists(temp))
                {
                    try
                    {
                        archive_dir = temp;
                    }
                    finally
                    {
                        //die nice
                    }
                }
            }
        }

        public override void TransformNotReadyToSyncRespondent()
        {
            _start_vlc = false;
            
            IBrandDatabase db = (IBrandDatabase)ServicesContainer.GetService(ServicesNames.SQL_DB);
            List<int> respondent_list = db.GetRespondentsIDNotReadyToSync(Convert.ToString(_project.ID));

            Project project = db.GetProject(Convert.ToString(_project.ID));

            _ui_synchro.SetProgressBarMinMax(0, respondent_list.Count);

            for (int i = 0; i < respondent_list.Count; i++)
            {
                int resp_id = respondent_list[i];
                Respondent respondent = db.GetRespondent(resp_id);
                respondent.Answers = db.GetAnswers(resp_id);
                
                GetTransformationProperties();
                VeloVolantVideo_Worker_old temp = new VeloVolantVideo_Worker_old(this, respondent, resp_id, thumbnail_time, gallery_dir, saveFrame, save_dir, log);
                temp.DoWork();

                _ui_synchro.AddProgressStep(1);
            }
        }

        /// <summary>
        /// Indique que l'on a une transformation à effectuer. Dans ce cas-ci, un vidéo.
        /// </summary>
        /// <returns></returns>
        public override bool HasTransformation()
        {
            return true;
        }

        /// <summary>
        /// Classe de travail sur fil d'exécution pour créer le vidéo.
        /// </summary>
        private class VeloVolantVideo_Worker_old
        {
            VeloVolantVideo_old _action = null;
            Respondent _respondent_t = null;
            int _resp_id_t;
            int thumbnail_time = 1;
            string thumbnail_dir = "";
            string save_dir = "";
            bool saveFrames = false;
            bool logDebug = false;

            /// <summary>
            /// Constructeur principal
            /// </summary>
            /// <param name="action">L'initiateur</param>
            /// <param name="resp">Le répondant à créer</param>
            /// <param name="respID">L'identifiant.</param>
            /// <param name="temps">Le temps du vidéo</param>
            /// <param name="dir">Le répertoire de la gallerie</param>
            /// <param name="saveFrames">Indique s'il faut sauvegarder les trames produites à fin de vérification manuelle.</param>
            /// <param name="save_dir">Le répertoire à utiliser pour sauvegarder les trames.</param>
            /// <param name="log">Indique s'il faut logger toute les étapes.</param>
            public VeloVolantVideo_Worker_old(VeloVolantVideo_old action, Respondent resp, int respID, int temps, string dir, bool saveFrames, string save_dir, bool log)
            {
                _action = action;
                _respondent_t = resp;
                _resp_id_t = respID;
                thumbnail_time = temps;
                thumbnail_dir = dir;
                logDebug = log;
                this.saveFrames = saveFrames;
                this.save_dir = save_dir;
            }

            /// <summary>
            /// Obtient l,ID du répondant à traiter.
            /// </summary>
            /// <returns>L'ID du répondant.</returns>
            public int GetRespId()
            {
                return _resp_id_t;
            }

            /// <summary>
            /// Retourne l'instance du submit action lié à cette instance.
            /// </summary>
            /// <returns>La référence de l'action Submit lié à ce travailleur.</returns>
            public VeloVolantVideo_old GetSubmitAction()
            {
                return _action;
            }

           /// <summary>
           /// Cette méthode contient le gros du travail à faire sur le fil d'exécution.
           /// </summary>
            public void DoWork()
            {
                try
                {
                    Answer answer_video = _respondent_t.Answers[0];//Question video
                    String video_name = answer_video.Response;

                    Configuration configuration = (Configuration)ServicesContainer.GetService(ServicesNames.CONFIGURATION);

                    String file_name = StringUtil.GetFileName(video_name);

                    String video_moved_name = configuration.VideosDir + file_name;

                    if (!File.Exists(video_moved_name))
                    {
                        throw new FileNotFoundException("Video file not found: " + video_moved_name);
                    }

                    lock (_lock)
                    {
                            IBrandDatabase db = (IBrandDatabase)ServicesContainer.GetService(ServicesNames.SQL_DB);

                            String id = file_name.Substring(0, file_name.LastIndexOf("."));

                            String video_final_name = configuration.VideosDir + id + ".f4v";

                            String video_avi_name = configuration.VideosDir + file_name + ".avi";

                            if (_action._ui_synchro != null)
                            {
                                _action._ui_synchro.AddUiMessage("Transforming video " + file_name + " ...");
                            }

                            float timeVideo = -1;

                            if (!File.Exists(video_final_name))
                            {

                                if (File.Exists(video_avi_name))
                                {
                                    File.Delete(video_avi_name);
                                }


                                MovToAvi.TransformToAvi(video_moved_name, video_avi_name, configuration.FfmpegLocation, 1280, 720);


                                string intro = _action._project.ProjectDir + FICHIER_INTRO_VIDEO;
                                string sound = _action._project.ProjectDir + FICHIER_SONORE;
                                string sound_intro = _action._project.ProjectDir + FICHIER_SONORE_INTRO;
                                string sound_outro = _action._project.ProjectDir + FICHIER_SONORE_OUTRO;
                                string outro = _action._project.ProjectDir + FICHIER_OUTRO_VIDEO;
                                string overlay_image = _action._project.ProjectDir + FICHIER_OVERLAY;
                                string outro_image = _action._project.ProjectDir + FICHIER_LOGO;

                                string compressOptions = _action._project.ProjectDir + FICHIER_COMPRESS;

                                TransformationAvi transformation = new TransformationAvi();

                                timeVideo = transformation.transformation_VeloVolant(
                                    intro,
                                    outro,
                                    sound,
                                    sound_intro,
                                    sound_outro,
                                    video_avi_name,
                                    overlay_image,
                                    outro_image,
                                    video_final_name,
                                    saveFrames,
                                    save_dir,
                                    compressOptions,
                                    false,
                                    logDebug);

                                if (timeVideo > 0)
                                {
                                    //Add video id answer
                                    Answer video_id_answer = new Answer(QuestionText.TYPE, QUESTION_VIDEO_TEMPS, 1, timeVideo.ToString());
                                    db.DeleteAnswer(_resp_id_t, video_id_answer);
                                    db.InsertAnswer(_resp_id_t, video_id_answer);
                                }
                            }

                            if (File.Exists(video_avi_name))
                            {
                                File.Delete(video_avi_name);
                            }

                            String video_name_f4v = video_name.Substring(0, video_name.Length - 4) + ".f4v";
                            String video_final_name_mov = configuration.VideosDir + video_name;
                            String video_final_name_f4v = configuration.VideosDir + video_name_f4v;

                            /* Création du fichier Thumbnail */
                            String timestamp = DateUtil.GetCurrentTimeMilli();

                            String thumbnail_file_name = timestamp + "_" + video_name.Substring(0, video_name.Length - 4) + ".jpg";
                            String thumbnail_gallery_file = configuration.ThumbnailsDir + thumbnail_file_name;

                            int time = thumbnail_time;

                            if (time < 0 || time > timeVideo)
                            {
                                time = (int)Math.Floor(timeVideo / 2);
                            }

                            new ThumbnailsGenerator().GenerateThumbnail(video_final_name_f4v, thumbnail_gallery_file, time);

                            Bitmap _image = new Bitmap(thumbnail_gallery_file);

                            Bitmap final_image = new Bitmap(_image.Width, _image.Height, PixelFormat.Format32bppArgb);
                            Graphics g = Graphics.FromImage(final_image);
                            g.DrawImage(_image, new Point(0, 0));
                            Image _play = Image.FromFile(this._action._project.ProjectDir + "play.png");
                            g.DrawImage(_play, new Point(0, 0));

                            g.Dispose();

                            _play.Dispose();
                            _image.Dispose();

                            final_image.Save(thumbnail_gallery_file, ImageFormat.Jpeg);
                            final_image.Dispose();

                            // On copie ensuite le vidéo à l'endroit de la gallerie d'image pour Android.
                            if (!Directory.Exists(_action.gallery_dir))
                                Directory.CreateDirectory(_action.gallery_dir);

                            File.Copy(thumbnail_gallery_file, _action.gallery_dir + thumbnail_file_name, true);
                            File.Delete(thumbnail_gallery_file);

                            // On remplit finalement la question thumbnail
                            Answer thumbnail_merge = new Answer(QuestionText.TYPE, QUESTION_THUMBNAIL, 1, thumbnail_file_name);
                            db.DeleteAnswer(_resp_id_t, thumbnail_merge);
                            db.InsertAnswer(_resp_id_t, thumbnail_merge);

                            if (_action._ui_synchro != null)
                                _action._ui_synchro.AddUiMessage("Video transformed, thumbnail created.");

                            db.SetRespondentReadyToSync(_resp_id_t);

                            if (this._action._sync_events != null)
                                this._action._sync_events.NewItemEvent.Set();

                            if (File.Exists(video_final_name_mov))
                            {
                                File.Delete(video_final_name_mov);
                            }

                            // On déplace le fichier F4V une fois tout fait.
                            if (_action.archive_dir != null)
                            {
                                try
                                {
                                    String moveToName = _action.archive_dir + video_name_f4v;
                                    File.Move(video_final_name_f4v, moveToName);
                                }
                                catch (System.IO.IOException e)
                                {
                                    LogUtil.GetLogger().Error("VeloVolant: could not move final file to archive directory (" + e.Message + ")\n\t" + _action.archive_dir);
                                }
                            }
                    }
                }
                catch (System.Exception ex)
                {
                    LogUtil.GetLogger().Fatal("Error while VeloVolantVideo#DoWork():\n" + ex.StackTrace);

                    if (_action._ui_synchro != null)
                    {
                        _action._ui_synchro.AddUiMessage("Error while VeloVolantVideo#DoWork()");
                    }
                }
            }
        }

        /// <summary>
        /// Classe qui exécute les "workers" de VeloVolant un à la fois. Agit comme un gestionnaire de fils d'exécution.
        /// </summary>
        private class VeloVolant_WorkerPool
        {
            private Queue<VeloVolantVideo_Worker_old> queue = new Queue<VeloVolantVideo_Worker_old>();
            private volatile bool running = false; // Sémaphore d'arrêt principale.
            private System.Threading.Tasks.Task tache = null;
            private bool stopFinished = false;
            private bool log = false;

            /// <summary>
            /// Indique que le processus de fermeture s'est bien terminé.
            /// </summary>
            /// <returns></returns>
            public bool IsStopFinished()
            {
                return stopFinished;
            }

            /// <summary>
            /// N'ajoute pas de "worker" si le pool n'est pas en train de fonctionner.
            /// </summary>
            /// <param name="worker"></param>
            /// <returns></returns>
            public bool AddWorker(VeloVolantVideo_Worker_old worker)
            {
                if (IsRunning())
                {
                    lock (queue)
                    {
                        queue.Enqueue(worker);
                        return true;
                    }
                }
                return false;
            }

            public int GetQueueSize()
            {
                int nbr;

                lock (queue)
                {
                    nbr = queue.Count;
                }

                return nbr;
            }

            public void Stop()
            {
                running = false;
                if (log) LogUtil.GetLogger().Debug("(Pool) stop requested.");
            }

            public bool IsRunning()
            {
                return running;
            }

            /// <summary>
            /// Cette méthode ASSUME qu'elle est lancée sur un fil d'exécution séparé.
            /// Si un processus d'arrêt à été complété (IsStopFinished retourne vrai), cette méthode ne fera rien.
            /// </summary>
            public void Run()
            {
                if (!stopFinished)
                {
                    Stopwatch watch = new Stopwatch();
                    running = true;
                
                    // Boucle principale
                    while(running) {

                        lock (queue)
                        {
                            // Même si on a arrêté et repartit le pool, tache est toujours référencé et peut être correctement évalué.
                            if (tache != null && tache.IsCompleted) {
                                VeloVolantVideo_Worker_old worker = queue.Dequeue();
                                worker.GetSubmitAction().OnWorkFinished(worker.GetRespId());
                                tache = null;
                                watch.Stop();
                                //if (log)
                                    LogUtil.GetLogger().Debug("(Pool) Worker finished for respondent " + worker.GetRespId() + ". (" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds+ " m.)");
                            }
                    
                            if (tache == null)
                            {
                                if (queue.Count > 0)
                                {
                                    VeloVolantVideo_Worker_old worker = queue.Peek();
                                    tache = new System.Threading.Tasks.Task(worker.DoWork);
                                    if (log) LogUtil.GetLogger().Debug("(Pool) New worker launched (ID=" + worker.GetRespId() + "). Queue size is currently " + queue.Count);
                                    tache.Start();
                                    watch.Restart();
                                }
                            }
                        }

                        System.Threading.Thread.Sleep(500);
                    }

                    /* Quand on sort de cette boucle, il faut vérifier les cas suivants
                     * - La tâche actuelle est-elle complétée?
                     * - Si non, on attend qu'elle le soit.
                     * - Entretemps, on vide la queue en mentionnant qu'ils ne font plus partie d'une queue à l'action (OnWorkFinished).
                     * - Une fois la tâche actuelle finie, on termine la tâche et on sort de cette méthode.
                     */

                    VeloVolantVideo_Worker_old lastworker = null;

                    lock (queue)
                    {
                        if (log) LogUtil.GetLogger().Debug("(Pool) Stop initiated. Queue size is " + queue.Count);

                        if (tache != null)
                        {
                            // Une tâche non-nulle est toujours associée au premier élément de la queue.
                            lastworker = queue.Dequeue();
                        }

                        // On détruit la reste de la queue en mentionnant que le travail est terminé.
                        StringBuilder sb = new StringBuilder(queue.Count * 7);
                        int nbr = 0;
                        bool first = true;
                        while (queue.Count > 0)
                        {
                            nbr++;
                            VeloVolantVideo_Worker_old temp = queue.Dequeue();
                            temp.GetSubmitAction().OnWorkFinished(temp.GetRespId());
                            if (log)
                            {
                                sb.Append(temp.GetRespId());
                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    if (nbr % 20 == 0) {
                                        sb.Append(", ");
                                    } else {
                                        sb.Append("\n");
                                    }
                                }
                            }
                        }

                        if (log) LogUtil.GetLogger().Debug("(Pool) Workers skipped due to stop process :\n" + sb.ToString());
                    }

                    // On attend maintenant que la tâche en cours termine.
                    if (tache != null) {
                        while (!tache.IsCompleted)
                        {
                            System.Threading.Thread.Sleep(500);
                        }

                        stopFinished = true;
                        lastworker.GetSubmitAction().OnWorkFinished(lastworker.GetRespId());
                        watch.Stop();
                        tache = null;
                        if (log) LogUtil.GetLogger().Debug("(Pool) Worker finished for trailing respondent " + lastworker.GetRespId() + ". (" + watch.Elapsed.Minutes + ":" + watch.Elapsed.Seconds + " m.)");
                    }

                    stopFinished = true;
                }
            }

            internal void EnableLog(bool log)
            {
                this.log = log;
            }
        }
    }
}
