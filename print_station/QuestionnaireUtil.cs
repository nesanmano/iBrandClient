using NLog;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Services;
using NomadLogic.Common.Ui.Forms;
using NomadLogic.Common.Ui.Keyboard;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Json;
using NomadLogic.iBrandDesktop;
using NomadLogic.iBrandDesktop.Services;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers;
using NomadLogic.iBrandDesktop.Ui.Question;
using NomadLogic.iBrandDesktop.Util;
using NomadLogic.iBrandDesktop.Util.Social.Facebook;
using NomadLogic.iBrandDesktop.Util.Social.Twitter;
using NomadLogic.PrintStation.Json;
using NomadLogic.PrintStation.Ui;
using NomadLogic.PrintStation.Ui.custom.Agropur;
using NomadLogic.PrintStation.Ui.custom.BNC;
using NomadLogic.PrintStation.Ui.custom.Brewers;
using NomadLogic.PrintStation.Ui.custom.Carte_Hockey;
using NomadLogic.PrintStation.Ui.custom.Clorox;
using NomadLogic.PrintStation.Ui.custom.Hospital_Foundation;
using NomadLogic.PrintStation.Ui.custom.Maizex;
using NomadLogic.PrintStation.Ui.custom.Mercedez2019;
using NomadLogic.PrintStation.Ui.custom.Mosaique;
using NomadLogic.PrintStation.Ui.custom.OneThirdLab;
using NomadLogic.PrintStation.Ui.custom.OshKosh;
using NomadLogic.PrintStation.Ui.custom.RbcSomeday2015;
using NomadLogic.PrintStation.Ui.custom.Sofina;
using NomadLogic.PrintStation.Ui.custom.SOP;
using NomadLogic.PrintStation.Ui.custom.Sportium;
using NomadLogic.PrintStation.Ui.custom.TastyBites2016;
using NomadLogic.PrintStation.Ui.generic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Questionnaire;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms;
using NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ScreenDefinitions;
using NomadLogic.PrintStation.Ui.custom.Tangerine;
using GenericScreen = NomadLogic.PrintStation.Ui.generic.GenericScreen;
using HomeScreen = NomadLogic.PrintStation.Ui.generic.HomeScreen;
using MailScreen = NomadLogic.PrintStation.Ui.generic.MailScreen;
using QuestionnaireScreen = NomadLogic.PrintStation.Ui.custom.Flook.QuestionnaireScreen;
using ShareScreen = NomadLogic.PrintStation.Ui.generic.ShareScreen;
using SmsScreen = NomadLogic.PrintStation.Ui.generic.SmsScreen;
using ThanksEndScreen = NomadLogic.PrintStation.Ui.generic.ThanksEndScreen;

// ReSharper disable UnusedVariable
// ReSharper disable UnusedParameter.Local
#pragma warning disable 612

#pragma warning disable 618

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleInvalidOperationException

namespace NomadLogic.PrintStation
{
    /// <summary>
    /// Classe d'initialisation de questionnaires pour Printstations. Essentiellement un classe "Factory". Mauvais design, mais à moins d'une refactorisation complète,
    /// on est pogné avec.
    /// </summary>
    public class QuestionnaireUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        readonly List<IAbstractPrintStationScreen> _screens = new List<IAbstractPrintStationScreen>();

        /// <summary>
        /// Constructeur principal.
        /// </summary>
        /// <param name="screenPanel">Le panneau principal.</param>
        /// <param name="user">L'usager (d'un compte HashTag) courant du Printstation.</param>
        // ReSharper disable once UnusedParameter.Local
        public QuestionnaireUtil(Panel screenPanel, PrintStationUser user)
        {
            TIH_2020_01_PS(screenPanel, user);
        }

        /* Code de base pour la classe */
        #region Générique

        /// <summary>
        /// L'image sélectionnée par l'usager. Fait référence à plus qu'un seul fichier sur disque.
        /// </summary>
        public PicturePostInfo SelectedPicture { get; set; }

        /// <summary>
        /// Configuration d'écrans pour questionnaire de Printstation.
        /// </summary>
        public PrintStationScreenConfig Config { get; set; }

        /// <summary>
        /// Référence vers le questionnaire qui sert à saisir les données.
        /// </summary>
        public Questionnaire Questionnaire { get; private set; }

        /// <summary>
        /// Le contrôleur d'affichage pour les formes UI du questionnaire.
        /// </summary>
        public QuestionnaireController QuestionnaireController { get; private set; }

        /// <summary>
        /// Ajout un écran à la fin de ceux déjà fournit.
        /// </summary>
        public void AddScreens()
        {
            try
            {
                foreach (var screen in _screens)
                {
                    if (screen.CurrentRespondentSource != QuestionnaireController)
                    {
                        screen.CurrentRespondentSource = QuestionnaireController;
                    }
                    QuestionnaireController.AddScreen(screen);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        /// <summary>
        /// Termine l'usage du questionnaire, ce qui provoque la fermeture du logiciel Printstation.
        /// </summary>
        public void CloseQuestionnaire()
        {
            QuestionnaireController.CloseQuestionnaire();
        }

        /// <summary>
        /// Initialise le questionnaire quand on clique sur une image de la gallerie.
        /// </summary>
        /// <param name="picture">L'image sélectionnée.</param>
        public void ShowQuestionnaire(PicturePostInfo picture)
        {
            try
            {
                if (PostQuestionnaireMapping == null)
                {
                    if (picture.Src.Contains("_en")) { QuestionnaireController.OnLanguageSelection("en"); }
                    else if (picture.Src.Contains("_fr")) { QuestionnaireController.OnLanguageSelection("fr"); }
                }
                else
                {
                    // Si on a des infos de mapping, on tente de lire les métadonnées du Post sur le disque (si existantes),
                    // puis les associer aux questions. Si une des questions associées est la question de langue, on tente de
                    // lancer l'événement de changement de langue.

                    // 1- Lecture des métadonnées
                    var commonId = StringUtil.RemoveExtension(StringUtil.GetFileName(picture.Src));
                    var baseDir = DirectoryUtil.GetLocalPrintstationMediaDir(picture.UserId,
                        DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.file_bundles) + commonId;

                    if (Directory.Exists(baseDir) && Questionnaire != null)
                    {
                        // 2- On lit maintenant le fichier JSON contenant les réponses du précédent questionnaire.
                        var files = Directory.GetFiles(baseDir, "*.json");

                        if (files.Length >= 1)
                        {
                            var jsonFile = files[0];
                            var answers = JsonUtil.ReadJsonFile<Answers>(new FileInfo(jsonFile));

                            foreach (var answer in answers.Values)
                            {
                                var qNo = answer.QuestionNo;
                                if (qNo != 0 && !PostQuestionnaireMapping.ContainsKey(qNo)) continue;

                                var questionToSet = PostQuestionnaireMapping[qNo];
                                var q = Questionnaire.GetQuestion(questionToSet);
                                var newAnswer = new Answer
                                {
                                    QuestionType = answer.QuestionType,
                                    ChoiceNo = answer.ChoiceNo,
                                    Response = answer.Response,
                                    QuestionNo = questionToSet // Seul changement par rapport à la réponse lue.
                                };

                                // test pour voir si la réponse de la question fait allusion à un fichier multimédia existant dans le
                                // répertoire de base du Post, ou a un fichier ZIP (bundle d'images).
                                if (newAnswer.Response.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) ||
                                    newAnswer.Response.EndsWith(".png", StringComparison.InvariantCultureIgnoreCase) ||
                                    newAnswer.Response.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase) ||
                                    newAnswer.Response.EndsWith(".mov", StringComparison.InvariantCultureIgnoreCase) ||
                                    newAnswer.Response.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase) ||
                                    newAnswer.Response.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var newPath = baseDir + Path.DirectorySeparatorChar + StringUtil.GetFileName(newAnswer.Response);
                                    if (!File.Exists(newPath)) log.Error("File referenced in Post JSON answers does not reference file sent in bundle. Post should be cleared.");
                                    newAnswer.Response = newPath;
                                }

                                q.SetAnswer(newAnswer);
                            }
                        }

                        /* 3- Une fois les assignations faites, on regarde le code de langue du répondant. S'il est différent de celui du questionnaire
                         * par défaut, on lance l'événement de changement de langue.
                        */
                        var lang = Questionnaire.GetRespondentLanguage();
                        if (!Questionnaire.LanguageCurrent.Equals(lang))
                        {
                            Questionnaire.LanguageCurrent = lang;
                            QuestionnaireController.OnLanguageSelection(lang);
                        }
                    }
                }

                SelectedPicture = picture;
                QuestionnaireController.ShowFirstScreen(false);

                if (Questionnaire != null && PostQuestionnaireRespondantNoQuestion.HasValue)
                {
                    var qNo = (int)PostQuestionnaireRespondantNoQuestion.Value;
                    var question = Questionnaire.GetQuestion(qNo);
                    //this.RespondentID = picture.RepondantNo;
                    question?.SetAnswer(new Answer{QuestionType = QuestionTypeEnum.Text, QuestionNo = qNo, Response = picture.RepondantNo.ToString()});
                }

                if (Questionnaire != null && SaveSelectedPictureToQuestionNo.HasValue)
                {
                    var qNo = (int)SaveSelectedPictureToQuestionNo.Value;
                    var question = Questionnaire.GetQuestion(qNo);
                    //this.RespondentID = picture.RepondantNo;
                    question?.SetAnswer(new Answer{QuestionType = QuestionTypeEnum.PhotoText, QuestionNo = qNo, Response = picture.Src});
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex);
            }
        }

        /// <summary>
        /// Quand non-null, cette propriété contient les références pour mettre les réponses contenues dans les métadonnées d'un Post
        /// (contenues dans un fichier JSON, dans le répertoire associé au Post, seulement quand le Post est créé à partir d'un ZIP), et
        /// les associer à d'autres questions. Le "mapping" des réponses est nécessaire car les numéros de questions ne sont peut-être
        /// pas les mêmes, car il s'agit de questionnaires différents. Par exemple, la question langue est p-e la numéro 1 dans le questionnaire
        /// d'origine, mais il s'agit de la question 4 dans le questionnaire du printstation. Dans ce cas, il faut "mapper" la question 1 à la question 4.
        /// Ce qui se traduit ici par une entrée "1,4" dans le dictionnaire.
        /// Même si les métadonnées contiennent plus de données que nécessaire, aucun mapping ne sera fait si ce dictionnaire ne contient rien.
        /// Pour un exemple, voir le projet Sofina 2017.
        /// </summary>
        public Dictionary<int, int> PostQuestionnaireMapping;

        /// <summary>
        /// Indique quel est le numéro de question du questionnaire correspondant à la question du numéro de répondant (à ne pas confondre avec son identifiant).
        /// Cette information viens du serveur quand un projet iBrand est utilisé comme source de données, et fait partie des métadonnées du Post.
        /// </summary>
        public uint? PostQuestionnaireRespondantNoQuestion { get; set; }

        public uint? SaveSelectedPictureToQuestionNo { get; set; }

        /// <summary>
        /// Initialise un questionnaire prinstation avec ses informations de base.
        /// On s'assure de sa création et mise à jour dans la BD locale, on créé et initialise le questionnaire et son controlleur,
        /// et pour finir on retourne le répertoire local où son stocké les fichiers du UI (photobooth_ui).
        /// </summary>
        /// <param name="screenPanel">La forme parente.</param>
        /// <param name="sc">L'objet de configuration des écrans.</param>
        /// <param name="questionnaireFile">Le fichier du questionnaire XML.</param>
        /// <param name="version">Le numéro de version du questionnaire.</param>
        /// <returns></returns>
        private string InitPrintStationQuestionnaire(Panel screenPanel, PrintStationScreenConfig sc, string questionnaireFile, string version)
        {
            var qInfo = sc.GenerateQuestionnaireInfo(questionnaireFile, version);
            var project = sc.GenerateProject();
            var eveent = sc.GenerateEvent();

            project.AddQuestionnaireInfo(qInfo);

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.SaveProject(project);

            // Prevoir un questionnaire loader pour le prinstation
            var questionnaireLoader = new IBrandQuestionnaireLoader(qInfo, project);

            Questionnaire = questionnaireLoader.Load(true);

            QuestionnaireController = new QuestionnaireController(project, eveent, "PC", Questionnaire, screenPanel);

            var dir = DirectoryUtil.GetLocalIbrandTerminalDataDir(sc.ProjectId, sc.QuestionnaireId, sc.TerminalId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);
            return dir;
        }

        #endregion Générique

        /* Code spécifique à chaque projet PrintStation avec questionnaire. */
/*

        #region Saputo Dairy Land

        // Projet FFA-2016-02b
        private void InitSaputoDairyland(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(771, 743, user)
            {
                ProjectNo = "n96dq4qqh1",
                EventId = 708,
                ForceFileCheck = true,
                ProjectDescription = "Saputo Dairyland",
                QuestionFirstPictureNo = 3,
                QuestionSmsNo = 1,
                QuestionEmailNo = 2,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "ffa-2016-02b-desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            var share = new ShareScreen(dir + "sharing.jpg", null, sc, facebookShare, twitterShare,
                "Share your photo on the Dairyland Facebook page and get the chance to win free milk for a year!", null,
                null, // print
                new Rectangle(896, 310, 101, 101), // mail
                new Rectangle(896, 167, 101, 101), // sms
                new Rectangle(276, 167, 101, 101), // facebook
                new Rectangle(276, 310, 101, 101), // twitter
                new Rectangle(559, 538, 161, 069), // end/cancel
                150, new Size(480, 480))
            {
                Parent = screenPanel
            };
            _screens.Add(share);

            var mailScreen = new MailScreen(dir + "email.jpg", null, sc, 1,
                new Rectangle(133, 160, 1107, 48), // Textbox
                new Rectangle(581, 262, 116, 47), // previous zone
                new KeyboardControl(-1, 324, 1157, 309))
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "sms.jpg", null, sc, 2,
                new Rectangle(133, 160, 1107, 48), // Textbox
                new Rectangle(581, 262, 116, 47), // previous zone
                new KeyboardControl(-1, 324, 1157, 309))
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);
        }
        #endregion Saputo Dairy Land

        #region Musee McCord
        private void InitMuseeMcCord(PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            Config = new PrintStationScreenConfig(899, 859, user)
            {
                ProjectNo = "j3fosk0ku1",
                EventId = 821,
                ForceFileCheck = true,
                ProjectDescription = "Musee McCord",
                QuestionFirstPictureNo = 1
            };

            InitPrintStationQuestionnaire(null, Config, "musee_mccord_questionnaire.xml", "1.0");
        }

        #endregion Musee McCord

        #region Manina Lancement
        private void InitManinaLancement(PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            Config = new PrintStationScreenConfig(911, 868, user)
            {
                ProjectNo = "ptxnq25kd1",
                EventId = 830,
                ForceFileCheck = true,
                ProjectDescription = "Musee McCord",
                QuestionFirstPictureNo = 1
            };

            InitPrintStationQuestionnaire(null, Config, "manina_questionnaire.xml", "1.0");
        }

        #endregion Manina Lancement

        #region Manina Decarie Motors
        private void InitManinaDecarieMotors(PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            Config = new PrintStationScreenConfig(935, 891, user)
            {
                ProjectNo = "wd9z5cd381",
                EventId = 851,
                ForceFileCheck = true,
                ProjectDescription = "wd9z5cd381",
                QuestionFirstPictureNo = 1
            };

            InitPrintStationQuestionnaire(null, Config, "manina_questionnaire.xml", "1.0");
        }

        #endregion Manina Decarie Motors

        #region Clorox
        private void InitClorox(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(695, 675, user)
            {
                ProjectNo = "1l5lq54fn1",
                EventId = 641,
                ForceFileCheck = true,
                ProjectDescription = "GreenWorks"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "col_2015_1-desktop.xml", "1.0");

            _screens.Add(new Ui.custom.Clorox.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        private void InitClorox2(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(712, 692, user)
            {
                ProjectNo = "rhvppqil11",
                EventId = 657,
                ForceFileCheck = true,
                ProjectDescription = "Clorox"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new QuestionnaireScreen2(dir + "questionnaire.jpg", sc));
        }

        private void InitClorox3(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(719, 699, user)
            {
                ProjectNo = "n2y95wb6t1",
                EventId = 664,
                ForceFileCheck = true,
                ProjectDescription = "Clorox"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new QuestionnaireScreenXmas(dir + "questionnaire.jpg", sc));
        }
        #endregion Clorox

        #region KijijiBoomerang

        private void InitKijijiBoomerang(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(728, 708, user)
            {
                ProjectNo = "t0w6bf2801",
                EventId = 673,
                ForceFileCheck = true,
                ProjectDescription = "Kijiji Boomerang"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new Ui.custom.Kijiji.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        #endregion

        #region Flook
        private void InitFlook(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(673, 654, user)
            {
                ProjectNo = "gw22gu2q31",
                EventId = 620,
                ForceFileCheck = true,
                QuestionEmailNo = 3,
                ProjectDescription = "Flook"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }
        #endregion Flook

        #region Corgi
        private void InitCorgi(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(682, 663, user)
            {
                ProjectNo = "ckzzt2j411",
                EventId = 629,
                ForceFileCheck = true,
                QuestionEmailNo = 3,
                ProjectDescription = "Corgi"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new Ui.custom.Corgi.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }
        #endregion Corgi

        #region OshKosh

        private void InitOshKosh(Panel screenPanel, PrintStationUser user)
        {

            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(681, 662, user)
            {
                ProjectNo = "axyximwrs1",
                EventId = 628,
                ForceFileCheck = true,
                ProjectDescription = "Oshkosh"
            };
            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            var webScreen = new WebBrowserScreenPrintStation(sc, new Size(1280, 800), new Size(1100, 300))
            {
                TagToIntercept = "success",
            };

            var lss = new LanguageSelectionScreen(dir + "Langues.jpg", sc, webScreen);
            lss.OnLanguageSelection += QuestionnaireController.OnLanguageSelection;

            _screens.Add(lss);
            _screens.Add(webScreen);
        }

        #endregion

        #region HbcCanadaDay

        private void InitHbcCanadaDay(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(672, 653, user)
            {
                ProjectNo = "dmbtoh8ff1",
                EventId = 619,
                ForceFileCheck = true,
                QuestionEmailNo = 3,
                ProjectDescription = "HBC Canada Day"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");
            _screens.Add(new Ui.custom.HBC_Canada_Day.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        #endregion

        #region HbcCalgary

        private void InitHbcCalgary(Panel screenPanel, PrintStationUser user)
        {
            var qInfo = new QuestionnaireInfo(655, "HBC Canada Day",
                "projects\\674\\655\\questionnaire_desktop.xml", 674);

            var project = new Project(674, "HBC Canada Day", "da0yuitel1");

            var eveent = new IbrandEventInfo(621, "HBC Canada Day", 655);

            project.AddQuestionnaireInfo(qInfo);

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.SaveProject(project);

            var questionnaireLoader = new IBrandQuestionnaireLoader(qInfo, project);

            Questionnaire = questionnaireLoader.Load(true);

            QuestionnaireController = new QuestionnaireController(project, eveent, "PC", Questionnaire, screenPanel);

            var sc = new PrintStationScreenConfig(qInfo.ProjectId, qInfo.Id, user)
            {
                ProjectNo = project.No,
                EventId = eveent.Id,
                ForceFileCheck = true,
                QuestionFirstPictureNo = 6,
                QuestionEmailNo = 3
            };

            var dir = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(sc.ProjectId, sc.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            _screens.Add(new Ui.custom.HBC_Canada_Day.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        #endregion

        #region HbcOttawa

        private void InitHbcOttawa(Panel screenPanel, PrintStationUser user)
        {
            var qInfo = new QuestionnaireInfo(657, "HBC Canada Day",
                "projects\\676\\657\\questionnaire_desktop.xml", 676);

            var project = new Project(676, "HBC Canada Day", "skt608y3j1");

            var eveent = new IbrandEventInfo(623, "HBC Canada Day", 657);

            project.AddQuestionnaireInfo(qInfo);

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.SaveProject(project);

            var questionnaireLoader = new IBrandQuestionnaireLoader(qInfo, project);

            Questionnaire = questionnaireLoader.Load(true);

            QuestionnaireController = new QuestionnaireController(project, eveent, "PC", Questionnaire, screenPanel);

            var sc = new PrintStationScreenConfig(qInfo.ProjectId, qInfo.Id, user)
            {
                ProjectNo = project.No,
                EventId = eveent.Id,
                ForceFileCheck = true,
                QuestionFirstPictureNo = 6,
                QuestionEmailNo = 3
            };

            var dir = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(sc.ProjectId, sc.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            _screens.Add(new Ui.custom.HBC_Canada_Day.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        #endregion

        #region HbcVancouver

        private void InitHbcVancouver(Panel screenPanel, PrintStationUser user)
        {
            var qInfo = new QuestionnaireInfo(658, "HBC Canada Day",
                "projects\\677\\658\\questionnaire_desktop.xml", 677);

            var project = new Project(677, "HBC Canada Day", "ycf5rq0b01");

            var eveent = new IbrandEventInfo(624, "HBC Canada Day", 658);

            project.AddQuestionnaireInfo(qInfo);

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.SaveProject(project);

            var questionnaireLoader = new IBrandQuestionnaireLoader(qInfo, project);

            Questionnaire = questionnaireLoader.Load(true);

            QuestionnaireController = new QuestionnaireController(project, eveent, "PC", Questionnaire, screenPanel);

            var sc = new PrintStationScreenConfig(qInfo.ProjectId, qInfo.Id, user)
            {
                ProjectNo = project.No,
                EventId = eveent.Id,
                ForceFileCheck = true,
                QuestionFirstPictureNo = 6,
                QuestionEmailNo = 3
            };

            var dir = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(sc.ProjectId, sc.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            _screens.Add(new Ui.custom.HBC_Canada_Day.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        #endregion

        #region JohnFrieda

        private void InitJohnFrieda(Panel screenPanel, PrintStationUser user)
        {
            var qInfo = new QuestionnaireInfo(623, "John Frieda",
                "projects\\636\\623\\questionnaire_desktop.xml", 636);

            var project = new Project(636, "John Frieda", "3rz05fkik1");

            var eveent = new IbrandEventInfo(589, "John Frieda", 623);

            project.AddQuestionnaireInfo(qInfo);

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.SaveProject(project);

            var questionnaireLoader = new IBrandQuestionnaireLoader(qInfo, project);

            Questionnaire = questionnaireLoader.Load(true);

            QuestionnaireController = new QuestionnaireController(project, eveent, "PC", Questionnaire, screenPanel);

            var sc = new PrintStationScreenConfig(qInfo.ProjectId, qInfo.Id, user)
            {
                ProjectNo = project.No,
                EventId = eveent.Id,
                ForceFileCheck = true,
                QuestionFirstPictureNo = 7,
                QuestionEmailNo = 3
            };

            var dir = DirectoryUtil.GetLocalIbrandQuestionnaireDataDir(sc.ProjectId, sc.QuestionnaireId,
                DirectoryUtil.ProjectDataDirectoriesEnum.photobooth_ui, DirectoryUtil.ProjectDataDirectoriesModEnum.none);

            _screens.Add(new Ui.custom.John_Frieda.QuestionnaireScreen(dir + "questionnaire_en.png",
                dir + "questionnaire_fr.jpg", sc));
        }

        #endregion

        #region Nestea

        /*private void InitNestea(Panel screenPanel, PrintStationUser user)
        {
            var qInfo = new QuestionnaireInfo(644, "Nestea", "projects\\662\\644\\questionnaire.xml", 601);

            var project = new Project(662, "Nestea Tablet", "eu45c2naw1");

            var eveent = new Event(610, "Nestea", 644);

            project.AddQuestionnaireInfo(qInfo);

            var db = ServicesContainer.GetService<IbrandDatabase>();
            db.SaveProject(project);

            log.Debug("2");

            var questionnaireLoader = new QuestionnaireLoader(qInfo, project);

            Questionnaire = questionnaireLoader.Load(true);

            QuestionnaireController = new QuestionnaireController(project, eveent, "PC", Questionnaire, screenPanel);

            var sc = new PrintStationScreenConfig(662, 644, user)
            {
                ProjectNo = "eu45c2naw1",
                EventId = 610,
                ForceFileCheck = true,
                QuestionGifNo = 3,
                QuestionFirstPictureNo = 4,
                QuestionEmailNo = 1,
                QuestionSmsNo = 2,
                QuestionFacebookNo = 5
            };

            var dir = DirectoryUtil.GetLocalPrintstationMediaDir(user.Id,
                DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.ui);
            _screens.Add(new ShareScreen(dir + "share_page.jpg", dir + "share_page_fr.jpg", sc));
        }#1#

        #endregion

        #region Pampers

        private void InitPampers(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(690, 670, user)
            {
                ProjectNo = "wwk0sy75x1",
                EventId = 636,
                ForceFileCheck = true,
                ProjectDescription = "Pampers 2015"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "cit_2015_1b-desktop.xml", "1.0");

            _screens.Add(new Ui.custom.Pampers.QuestionnaireScreen(dir + "questionnaire.jpg", sc));
        }

        #endregion

        #region RbcSomeday

        private void InitRbcSomeday(Panel screenPanel, PrintStationUser user)
        {

            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(693, 673, user)
            {
                ProjectNo = "ycih8z05r1",
                EventId = 639,
                ForceFileCheck = true,
                SendMediaToTwitter = true,
                ProjectDescription = "Rbc Someday"
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "mat_2015_1b-desktop.xml", "1.0");

            var webScreen = new WebBrowserScreenPrintStation(sc, new Size(1280, 800), new Size(1160, 310))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, "web");

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, "web");
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                "web")
            {
                CustomText =
                    "Check out my #Someday screenplay cover and photo shoot from the RBC Someday Studio at #TIFF15."
            };

            _screens.Add(new Rbc02(dir + "2.jpg", sc)); // First Name + Last Name
            _screens.Add(new Rbc03(dir + "3.jpg", sc)); // Script someday
            _screens.Add(new Rbc04(dir + "4.jpg", sc)); // Preview
            //_screens.Add(new Rbc05(dir + "5.jpg", sc, facebookShare, twitterShare)); // Facebook + Twitter
            _screens.Add(new Rbc06(dir + "6.jpg", sc)); // Email + SMS
            _screens.Add(new Rbc06Mail(dir + "mail.jpg", sc)); // Email sub-page
            _screens.Add(new Rbc06Sms(dir + "sms.jpg", sc)); // SMS sub-page
            _screens.Add(new Rbc07(dir + "7.jpg", sc)); // Print
            _screens.Add(new Rbc08(dir + "8.jpg", sc)); // Consent
            _screens.Add(new Rbc08Legal(dir + "legal.jpg", sc)); // Consent sub-page
            _screens.Add(new Rbc09(dir + "9.jpg", sc)); // Finish
        }

        #endregion

        #region Sephora

        private void InitSephora(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(738, 716, user)
            {
                ProjectNo = "2zfpbmqec1",
                EventId = 681,
                ForceFileCheck = true,
                ProjectDescription = "Sephora",
                QuestionFirstPictureNo = 3,
                QuestionGifNo = 4,
                QuestionSmsNo = 1,
                QuestionEmailNo = 2,
                QuestionFacebookNo = 5,
                QuestionTwitterNo = 6,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                webSCreenTag);

            //_screens.Add(new Ui.custom.Sephora.QuestionnaireScreen(dir + "sharing.png", sc, facebookShare, twitterShare, "Having a blast with Sephora at NHSCC! #SEPHORASPIRIT"));

            //var mailScreen = new Ui.custom.Sephora.Mail(dir + "mail.jpg", sc, 1) { Parent = screenPanel }; _screens.Add(mailScreen);

            //var smsScreen = new Ui.custom.Sephora.Sms(dir + "sms.jpg", sc, 2) { Parent = screenPanel }; _screens.Add(smsScreen);
        }

        #endregion

        #region FoodNetwork

        private void InitFoodNetwork(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(748, 724, user)
            {
                ProjectNo = "yiua3y9031",
                EventId = 689,
                ForceFileCheck = true,
                ProjectDescription = "Food Network",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "aim-2016-01-b-desktop-printstation.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                webSCreenTag);

            //_screens.Add(new Ui.custom.Food_Network.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare, twitterShare, "I'll be cheering for my #AllStarAcademy team Sundays at 9/8c on @FoodNetwork"));

            //var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(424, 31)) { Parent = screenPanel }; _screens.Add(mailScreen);

            //var smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(424, 31)) { Parent = screenPanel }; _screens.Add(smsScreen);
        }

        #endregion

        #region BaconFest

        private void InitFoodNetworkBaconFest(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(777, 749, user)
            {
                ProjectNo = "orleezjsr1",
                EventId = 714,
                ForceFileCheck = true,
                ProjectDescription = "Food Network",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_printstation.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                webSCreenTag);

            //_screens.Add(new Ui.custom.Food_Network.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare, twitterShare, "Check out my photo from the @cookingchannel Baconfest Chicago lounge!"));

            //var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(424, 31)) { Parent = screenPanel };  _screens.Add(mailScreen);

            //var smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(531, 31)) { Parent = screenPanel }; _screens.Add(smsScreen);
        }

        #endregion

        #region CookingChannel

        private void InitCookingChanel(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(745, 722, user)
            {
                ProjectNo = "btvvl88j41",
                EventId = 687,
                ForceFileCheck = true,
                ProjectDescription = "Cooking Chanel",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "aim-2016-01-a-desktop-printstation.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                webSCreenTag);

            //_screens.Add(new Ui.custom.Food_Network.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare, twitterShare, "I'll be watching #DinneratTiffanis on @CookingChannel Wednesdays at 8p EST/5p PST!"));

            //var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(424, 31)) { Parent = screenPanel }; // _screens.Add(mailScreen);

            //ar smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(524, 31)) { Parent = screenPanel }; _screens.Add(smsScreen);
        }

        #endregion

        #region TravelChannel

        private void InitTravelChanel(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(751, 726, user)
            {
                ProjectNo = "jmcdqy9991",
                EventId = 691,
                ForceFileCheck = true,
                ProjectDescription = "Travel Chanel",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "aim-2016-01-c-desktop-printstation.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                webSCreenTag);

            //_screens.Add(new Ui.custom.Food_Network.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare, twitterShare, "I'll be watching #BizarreFoods Delicious Destinations on @TravelChannel Tuesdays at 9p/8c!"));

            //var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(424, 31)) { Parent = screenPanel }; _screens.Add(mailScreen);

            //var smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(424, 31)) { Parent = screenPanel }; _screens.Add(smsScreen);
        }

        #endregion

        #region T1DFC
        private void InitT1DFC(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(761, 736, user)
            {
                ProjectNo = "3sw9l8jj41",
                EventId = 701,
                ForceFileCheck = true,
                ProjectDescription = "T1 DFC TABLET",
                QuestionFirstPictureNo = 1,
                QuestionVideoNo = 2,
                QuestionSmsNo = 3,
                QuestionVimeoId = 4,
                QuestionEmailNo = 5,
                QuestionFacebookNo = 6,
                QuestionTwitterNo = 7,
                VideoLink = "http://www.championher.com/",
                SendMediaToTwitter = false
            };


            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            // _screens.Add(new Ui.custom.DFC_T1.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare, twitterShare, "Not every girl is encouraged to do this. But I believe they should be. %23ChampionHer %40WomenChampions"));

            //var mailScreen = new Ui.custom.DFC_T1.Mail(dir + "mail.jpg", sc, 1, new Point(308, 131)) { Parent = screenPanel }; _screens.Add(mailScreen);

            //var smsScreen = new Ui.custom.DFC_T1.Sms(dir + "sms.jpg", sc, 2, new Point(308, 131)) { Parent = screenPanel }; _screens.Add(smsScreen);
        }
        #endregion T1DFC

        #region MarathonDFC
        private void InitMarathonDFC(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(780, 752, user)
            {
                ProjectNo = "nm7r5w10c1",
                EventId = 717,
                ForceFileCheck = true,
                ProjectDescription = "DFC marathon TABLET",
                QuestionFirstPictureNo = 1,
                QuestionVideoNo = 2,
                QuestionSmsNo = 3,
                QuestionVimeoId = 4,
                QuestionEmailNo = 5,
                QuestionFacebookNo = 6,
                QuestionTwitterNo = 7,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            // _screens.Add(new Ui.custom.DFC_Marathon.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare, twitterShare, "Proud to %23BeatEverySunrise %26 Recover with chocolate milk every time! %40RechargeMilk"));

            // var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(327, 40)) { Parent = screenPanel };  _screens.Add(mailScreen);

            //var smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(531, 40)) { Parent = screenPanel };  _screens.Add(smsScreen);
        }
        #endregion MarathonDFC

        #region BMW
        private void Init7CommBMW(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(791, 763, user)
            {
                ProjectNo = "bl1gfnd1o1",
                EventId = 728,
                ForceFileCheck = true,
                ProjectDescription = "7 Comm BMW",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionNumberOfPrintNo = 4,
                SendMediaToTwitter = true
            };


            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new Ui.custom.BMW.ShareScreen(dir + "share.jpg", sc));

            //var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(327, 40)) { Parent = screenPanel }; _screens.Add(mailScreen);

            //var smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(531, 40)) { Parent = screenPanel }; _screens.Add(smsScreen);
        }

        private void Init7CommBMWWest(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(800, 771, user)
            {
                ProjectNo = "rhyioc9u71",
                EventId = 736,
                ForceFileCheck = true,
                ProjectDescription = "7 Comm BMW West",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionNumberOfPrintNo = 4,
                SendMediaToTwitter = true
            };


            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            _screens.Add(new Ui.custom.BMW.ShareScreen(dir + "share.jpg", sc));

            //var mailScreen = new Mail(dir + "mail.jpg", sc, 1, new Point(327, 40)) { Parent = screenPanel }; _screens.Add(mailScreen);

            // var smsScreen = new Sms(dir + "sms.jpg", sc, 2, new Point(531, 40)) { Parent = screenPanel };_screens.Add(smsScreen);
        }

        private void Init7CommBMWBilingual(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(795, 767, user)
            {
                ProjectNo = "78rl4tvcv1",
                EventId = 732,
                ForceFileCheck = true,
                ProjectDescription = "7 Comm BMW Bilingue",
                QuestionFirstPictureNo = 2,
                QuestionSmsNo = 3,
                QuestionEmailNo = 4,
                QuestionNumberOfPrintNo = 5
            };


            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            var homeScreen = new HomeScreen(dir + "language_selection.jpg", sc, new Rectangle(678, 105, 530, 580), new Rectangle(59, 105, 565, 580), new Rectangle(1194, 14, 70, 70)) { Parent = screenPanel, QuestionLanguageNo = 1 };
            homeScreen.OnLanguageSelection += QuestionnaireController.OnLanguageSelection;
            _screens.Add(homeScreen);

            _screens.Add(new Ui.custom.BMW.ShareScreenBilingual(dir + "share.jpg", sc));

            var mailScreen = new MailScreen(dir + "mail.jpg", dir + "mail_fr.jpg", sc, 1, new Rectangle(322, 42, 700, 48), // Textbox
                new Rectangle(538, 171, 200, 71), // previous zone
                new KeyboardControl(80, 270, 1157, 309))
            { Parent = screenPanel };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "sms.jpg", dir + "sms_fr.jpg", sc, 2, new Rectangle(535, 42, 500, 48), // Textbox
                new Rectangle(538, 171, 200, 71), // previous zone
                new KeyboardControl(80, 270, 470, 360))
            { Parent = screenPanel };
            _screens.Add(smsScreen);
        }

        private void Init7Comm2017_01(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(950, 906, user)
            {
                ProjectNo = "6dk8hh5je1",
                EventId = 866,
                ForceFileCheck = true,
                ProjectDescription = "7 Comm BMW 2017-01",
                QuestionFirstPictureNo = 2,
                QuestionSmsNo = 3,
                QuestionEmailNo = 4,
                QuestionNumberOfPrintNo = 5
            };


            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            var clavier = new KeyboardControl(-1, 300, 1140, 350);
            var clavierNumerique = new NumbersKeyboardControl(-1, 300, 500, 350);

            var homeScreen = new HomeScreen(dir + "2-Language.jpg", sc, new Rectangle(20, 20, 587, 604), new Rectangle(663, 70, 587, 604), new Rectangle(1194, 14, 70, 70)) { Parent = screenPanel, QuestionLanguageNo = 1 };
            homeScreen.OnLanguageSelection += QuestionnaireController.OnLanguageSelection;
            _screens.Add(homeScreen);

            _screens.Add(new Ui.custom.BMW.ShareScreenBilingual(dir + "3-Sharing_option.jpg", sc));

            var mailScreen = new MailScreen(dir + "4-E-mail_ENG.jpg", dir + "4-E-mail_FR.jpg", sc, 1, new Rectangle(484, 63, 700, 48), // Textbox
                new Rectangle(538, 171, 200, 71), // previous zone
                clavier)
            { Parent = screenPanel };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "5-SMS_ENG.jpg", dir + "5-SMS_FR.jpg", sc, 2, new Rectangle(483, 63, 500, 48), // Textbox
                new Rectangle(538, 171, 200, 71), // previous zone
                clavierNumerique)
            { Parent = screenPanel };
            _screens.Add(smsScreen);
        }
        #endregion BMW

        #region FoodNetwork MasterCard

        private void InitFoodNetworkMasterCard(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(863, 829, user)
            {
                ProjectNo = "f749xizb81",
                EventId = 791,
                ForceFileCheck = true,
                ProjectDescription = "Food Network MasterCard",
                QuestionFirstPictureNo = 1,
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6,
                QuestionPictureToPrintNo = 1,
                SendMediaToTwitter = true
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire-printstation.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen,
                webSCreenTag);

            _screens.Add(new ShareScreen(dir + "share.jpg", null, sc, facebookShare, twitterShare,
                "Great night with Chef @amandafreitag at the @MasterCard #PricelessTable in NYC!", null
                , new Rectangle(902, 079, 156, 158) // Print
                , new Rectangle(902, 251, 156, 158) // mail
                , new Rectangle(222, 079, 156, 158) // sms
                , new Rectangle(222, 251, 156, 158) // facebook
                , new Rectangle(222, 424, 156, 158) // twitter
                , new Rectangle(529, 599, 224, 75) // end
                , 90, new Size(480, 480)
                )
            {
                Parent = screenPanel,
                ApplyTransformationToSelectedImage = false
            });
            /*_screens.Add(new Ui.custom.Food_Network.QuestionnaireScreen(dir + "share.jpg", sc, facebookShare,
               twitterShare, "Having a great time at the #PricelessTable hosted by @MasterCard!"));#1#

            var mailScreen = new MailScreen(dir + "email.jpg", null, sc, 1
                , new Rectangle(430, 27, 626, 48)
                , new Rectangle(531, 166, 218, 77)
                , new KeyboardControl(67, 307, 1138, 246))
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "sms.jpg", null, sc, 2
                , new Rectangle(538, 31, 664, 53)
                , new Rectangle(531, 166, 218, 77)
                , new KeyboardControl(67, 307, 1138, 246))
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);
        }

        #endregion

        #region Dublin's Pub 2016

        private void InitDublinPub2016(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(806, 777, user)
            {
                ProjectNo = "i55rackj21",
                EventId = 742,
                ForceFileCheck = true,
                ProjectDescription = "Dublin Pub",
                QuestionPictureToPrintNo = 2
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "pro-2016-01b-desktop.xml", "1.0");
            var dirBase = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            var selecEcran = new BackgroundSelectionAndTransformationScreen(dirUi + "choix_cadre.jpg", sc,
                null, new Rectangle(1211, 17, 56, 56), /*new[]
                {
                    new TransformationSelectionOption(new BackgroundSelectionOption(new Rectangle(025, 181, 292, 428), 1, 1, 1), dirBase + "v04-1.jpg"),
                    new TransformationSelectionOption(new BackgroundSelectionOption(new Rectangle(962, 181, 292, 438), 1, 1, 2), dirBase + "v04-2.jpg"),
                    new TransformationSelectionOption(new BackgroundSelectionOption(new Rectangle(650, 181, 292, 438), 1, 1, 3), dirBase + "v04-3.jpg"),
                    new TransformationSelectionOption(new BackgroundSelectionOption(new Rectangle(337, 181, 292, 428), 1, 1, 4), dirBase + "v04-4.jpg"),
                }#1#null)
            {
                Parent = screenPanel
            };
            _screens.Add(selecEcran);
        }

        #endregion

        #region Stampede PrintStation 2016

        private void InitStampedePrint2016(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(819, 790, user)
            {
                ProjectNo = "gjliqs6ho1",
                EventId = 755,
                ForceFileCheck = true,
                ProjectDescription = "Stampede Proshot Print 2016",
                QuestionFirstPictureNo = 2,
                QuestionEmailNo = 3,
                QuestionSmsNo = 4
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "mac-2016-02c-desktop.xml", "1.0");

            var shareScreen = new ShareScreen(dirUi + "sharing.jpg", null, sc, null, null, null, null, null,
                new Rectangle(968, 181, 175, 175), // Mail
                new Rectangle(190, 181, 175, 175), // sms
                null, null,
                new Rectangle(525, 500, 230, 60), //end
                27, new Size(450, 450))
            {
                Parent = screenPanel,
                MailJump = 1,
                SmsJump = 2,
                AdditionalProcessForFileBundle =
                    (imageFile => // Action supplémentaire pour gérer les fichiers du Bundle
                    {
                        var questionnaire = MainForm.GetInstance().Questionnaire;
                        var commonId = StringUtil.RemoveExtension(StringUtil.GetFileName(imageFile));
                        var bundleDir = DirectoryUtil.GetLocalPrintstationMediaDir(sc.PrintStationUser.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.file_bundles) + commonId;

                        if (!Directory.Exists(bundleDir)) throw new Exception("Following bundle directory does not exists: " + bundleDir);

                        var gifFile = Directory.EnumerateFiles(bundleDir).First(a => a.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase));

                        if (!File.Exists(gifFile)) throw new Exception("Following gif file directory does not exists: " + gifFile);

                        // On sauvegarde les fichiers
                        var question = questionnaire.GetQuestion(2);
                        var tmp = question.GetChoice(1);
                        var answer = new Answer(QuestionTypeEnum.PhotoText, 2, 1) { Response = gifFile };
                        tmp.SetAnswer(answer);

                        question = questionnaire.GetQuestion(1);
                        tmp = question.GetChoice(1);
                        answer = new Answer(QuestionTypeEnum.PhotoText, 1, 1) { Response = imageFile };
                        tmp.SetAnswer(answer);
                    })
            };
            _screens.Add(shareScreen);

            var mail = new MailScreen(dirUi + "email.jpg", dirUi + "email.jpg", sc, 1,
                new Rectangle(512, 111, 708, 42), // Textbox
                new Rectangle(569, 240, 141, 60), // Previous
                new KeyboardControl(56, 321, 1167, 432)) // Keyboard
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            var sms = new SmsScreen(dirUi + "sms.jpg", dirUi + "sms.jpg", sc, 2,
                new Rectangle(512, 111, 708, 42), // Textbox
                new Rectangle(569, 240, 141, 60), // Previous
                new KeyboardControl(56, 321, 1167, 432)) // Keyboard
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region FFAM Beer Fest 2016

        private void InitBeerFest2016(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(824, 795, user)
            {
                ProjectNo = "cqebwbw911",
                EventId = 760,
                ForceFileCheck = true,
                ProjectDescription = "FFAM Beer Fest 2016",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "ffa-2016-04b-desktop.xml", "1.0");

            var shareScreen = new ShareScreen(dirUi + "sharing_options.jpg", null, sc, null, null, null, null, null,
                new Rectangle(994, 327, 107, 107), // Mail
                new Rectangle(996, 184, 107, 107), // sms
                null, null,
                new Rectangle(560, 582, 180, 47), //end
                170, new Size(480, 270))
            {
                Parent = screenPanel,
                MailJump = 1,
                SmsJump = 2,
                CancelArea = new Rectangle(1203, 18, 48, 51),
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(shareScreen);

            var mail = new MailScreen(dirUi + "envoie_email.png", dirUi + "envoie_email.png", sc, 1,
                new Rectangle(327, 92, 834, 39), // Textbox
                new Rectangle(519, 191, 180, 49), // Previous
                new KeyboardControl(60, 275, 1160, 462)) // Keyboard
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            var sms = new SmsScreen(dirUi + "envoi_sms.png", dirUi + "envoi_sms.png", sc, 2,
                new Rectangle(251, 92, 844, 34), // Textbox
                new Rectangle(519, 191, 180, 49), // Previous
                new KeyboardControl(60, 275, 1160, 462)) // Keyboard
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Nordstrom Camera Ring

        private void InitNordstromCamreraRing2016(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(879, 843, user)
            {
                ProjectNo = "iciklk1d91",
                EventId = 805,
                ForceFileCheck = true,
                ProjectDescription = "Nordstrom camera ring 2016",
                QuestionFirstPictureNo = 1,
                QuestionEmailNo = 5,
                QuestionSmsNo = 3,
                QuestionFacebookNo = 7,
                VideoLink = "http://www.nomadlogicapp.com/nordstrom.jsp",
                QuestionTwitterNo = 8
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire-printstation.xml", "1.0");

            var preview = new Ui.generic.PreviewScreen(dirUi + "preview.jpg", sc,
                new Rectangle(656, 484, 265, 76), // Cancel
                new Rectangle(360, 484, 265, 76), // Next
                30,
                new Size(525, 350)) // picture size
            {
                Parent = screenPanel
            };
            _screens.Add(preview);

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            var shareScreen = new ShareScreen(dirUi + "share.jpg", null, sc, facebookShare, twitterShare, "We hope you had an awesome time at the Nordstrom Yorkdale Gala! #NordstromTO", null,
                null,
                new Rectangle(1052, 276, 140, 140), // Mail
                new Rectangle(1052, 82, 140, 140), // sms
                new Rectangle(96, 276, 140, 140), // Facebook
                new Rectangle(96, 82, 140, 140), // Twitter
                new Rectangle(506, 479, 273, 80), //end
                40, new Size(600, 400))
            {
                Parent = screenPanel,
                MailJump = 1,
                SmsJump = 2,
                AdditionalProcessForFileBundle =
                    (imageFile => // Action supplémentaire pour gérer les fichiers du Bundle
                    {
                        var questionnaire = MainForm.GetInstance().Questionnaire;
                        var commonId = StringUtil.RemoveExtension(StringUtil.GetFileName(imageFile));
                        var bundleDir = DirectoryUtil.GetLocalPrintstationMediaDir(sc.PrintStationUser.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.file_bundles) + commonId;

                        if (!Directory.Exists(bundleDir)) throw new Exception("Following bundle directory does not exists: " + bundleDir);

                        var zipFile = Directory.EnumerateFiles(bundleDir).First(a => a.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase));

                        if (!File.Exists(zipFile)) throw new Exception("Following zip File file directory does not exists: " + zipFile);

                        var mp4File = Directory.EnumerateFiles(bundleDir).First(a => a.EndsWith(".mp4", StringComparison.InvariantCultureIgnoreCase));

                        if (!File.Exists(mp4File)) throw new Exception("Following mp4 File file directory does not exists: " + mp4File);

                        // On sauvegarde les fichiers
                        var question = questionnaire.GetQuestion(2);
                        var tmp = question.GetChoice(1);
                        var answer = new Answer(QuestionTypeEnum.PhotoBundle, 2, 1) { Response = zipFile };
                        tmp.SetAnswer(answer);

                        question = questionnaire.GetQuestion(1);
                        tmp = question.GetChoice(1);
                        answer = new Answer(QuestionTypeEnum.PhotoText, 1, 1) { Response = imageFile };
                        tmp.SetAnswer(answer);

                        question = questionnaire.GetQuestion(9);
                        tmp = question.GetChoice(1);
                        answer = new Answer(QuestionTypeEnum.Video, 9, 1) { Response = mp4File };
                        tmp.SetAnswer(answer);
                    })
            };
            _screens.Add(shareScreen);

            var mail = new MailScreen(dirUi + "email.jpg", null, sc, 1,
                new Rectangle(423, 53, 708, 42), // Textbox
                new Rectangle(504, 240, 276, 70), // Previous
                new KeyboardControl(56, 321, 1167, 432), //keyboard
                new[]
                {
                    new ScreenCheckOption(new Rectangle(70, 110, 302, 105), 6, 1, true, new Point(104, 128))
                },
                new Rectangle(470, 181, 115, 40)) // Privacy
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            var sms = new SmsScreen(dirUi + "sms.jpg", null, sc, 2,
                new Rectangle(427, 53, 708, 42), // Textbox
                new Rectangle(504, 240, 276, 70), // Previous
                new KeyboardControl(56, 321, 1167, 432),
                new[]
                {
                    new ScreenCheckOption(new Rectangle(70, 110, 302, 105), 4, 1, true, new Point(104, 128))
                },
                new Rectangle(470, 181, 115, 40)) // Privacy
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Tasty Bites 2016 TAS-2016-01

        private void InitTastyBites2016(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(828, 799, user)
            {
                ProjectNo = "gmarjmqzm1",
                EventId = 764,
                ForceFileCheck = true,
                ProjectDescription = "Tasty Bites 2016",
                QuestionEmailNo = 3,
                QuestionFirstPictureNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "TAS-2016-1-A-android.xml", "1.0");

            var mail = new QuestionnaireTastyBites2016(dirUi + "questionnaire.jpg", dirUi + "questionnaire.jpg", sc, 1,
                new Rectangle(247, 108, 994, 36), // Textbox
                new Rectangle(460, 235, 160, 50), // Previous
                new KeyboardControl(41, 317, 1197, 339), // Keyboard
                new[]
                {
                    new ScreenCheckOption(new Rectangle(41, 170, 1200, 57), 2, 1, false, new Point(46, 176))
                },
                new Rectangle(658, 235, 160, 50)) // Cancel 
            {
                Parent = screenPanel,
                MoveForward = true
            };
            _screens.Add(mail);

        }

        #endregion

        #region Dynamite DYN-2016-01

        private void InitDynamite2016(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(842, 812, user)
            {
                ProjectNo = "mo0xw1pbc1",
                EventId = 775,
                ForceFileCheck = true,
                ProjectDescription = "Dynamite 2016",
                QuestionEmailNo = 3,
                QuestionFirstPictureNo = 6,
                QuestionPictureToPrintNo = 7,
                QuestionNumberOfPrintNo = 8
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "dyn-2016-01-desktop.xml", "1.0");

            var mail = new Ui.custom.Dynamite.QuestionnaireScreen(dirUi + "questionnaire.jpg", dirUi + "questionnaire.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

        }

        #endregion

        #region Sofina Star Wars SOF-2016-03

        private void InitSofinaStarWars(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(850, 819, user)
            {
                ProjectNo = "tt3es4vs91",
                EventId = 781,
                ForceFileCheck = true,
                ProjectDescription = "Sofina Star Wars",
                QuestionEmailNo = 3,
                QuestionSmsNo = 2,
                QuestionFacebookNo = 6,
                QuestionTwitterNo = 7,
                QuestionPictureTransformedNo = 1,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            var quest = new Ui.custom.Sofina.QuestionnaireScreen(dirUi + "2- 1_questionnaire.jpg", null, sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            var share = new ShareScreen(dirUi + "2- 2_sharing_options.jpg", null, sc, facebookShare, twitterShare,
                "Here is your photo from the Janes booth at FanExpo. We hope you share it with the rest of the galaxy!", null,
                null, //print
                null, //mail
                null, //sms
                new Rectangle(81, 186, 101, 101), //facebook
                new Rectangle(81, 329, 101, 101), // twitter
                new Rectangle(581, 583, 115, 44), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                WaiverDialogZones = new[]
                {
                    new WaiverDialogClickZone(new FileInfo(dirUi + "reglementsSofina.html"), new Rectangle(525, 659, 60, 32))
                },
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(share);
        }

        #endregion

        #region QDA Statue QDA-2016-01

        private void InitQdaQuebecStatue(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(882, 846, user)
            {
                ProjectNo = "9it5lw0331",
                EventId = 808,
                ForceFileCheck = true,
                ProjectDescription = "QDA Québec Statue"
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "qda-questionnaire.xml", "1.0");

            var home = new HomeScreen(dirUi + "choix_langue.jpg", sc,
                new Rectangle(254, 419, 293, 107), // Français
                new Rectangle(732, 419, 293, 107), // Anglais
                new Rectangle(1222, 13, 45, 45)) // Cancel
            {
                Parent = screenPanel,
                QuestionLanguageNo = 2
            };
            home.OnLanguageSelection += QuestionnaireController.OnLanguageSelection;
            _screens.Add(home);

            /*            var filtersHourglass = new[]
                        {
                            new ColorSubstitutionFilter(10, Color.Black, Color.White, ColorSubstitutionFilterGroup.All)
                        };
                        var bmfHourglass = new BitmapModificationFilter(filtersHourglass, null);
            #1#
            var mailSms = new Ui.custom.QDA.MailSmsScreen(dirUi + "page_1_en.jpg", dirUi + "page_1_fr.jpg", sc, QuestionnaireController)
            {
                Parent = screenPanel,
                //                ProceduralHourglassImageDefinition = new ProceduralExtendedImageDefinition(
                //                    218, 327, ImageUtil.ModifyPicture(ImageUtil.LoadBitmapFromFile(DirectoryUtil.GetCurrentApplicationInstallAssetsDir() + @"photobooth_ui\hourglass_black.png"),
                //                            ColorSubstitutionFilterGroup.All, bmfHourglass), Color.Black)
            };
            _screens.Add(mailSms);

            var quest = new Ui.custom.QDA.QuestionnaireScreen(dirUi + "page_2_en.jpg", dirUi + "page_2_fr.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);
        }

        #endregion

        #region DIA-2016-03-PS

        private void InitDiaPS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(875, 839, user)
            {
                ProjectNo = "xze7y0nxo1",
                EventId = 801,
                ForceFileCheck = true,
                ProjectDescription = "TD Share your Story PS 2016",
                QuestionEmailNo = 3,
                QuestionFacebookNo = 5,
                QuestionTwitterNo = 6,
                QuestionPictureTransformedNo = 1,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            /*
            // first screen contain only confirmation radio button
            var diaConfirmationScreen = new DiaConfirmationScreen(
                dirUi + "2-disclaimer.jpg",
                sc // nombre de step en arriere
                )
            {
                Parent = screenPanel,

            };
            _screens.Add(diaConfirmationScreen);
            #1#

            // second screen contain email + confirmation + facebook and twitter share
            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1920, 1200), new Size(1600, 600))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            /*
            var diaEmailAndShareScreen = new DiaEmailAndShareScreen(

            dirUi + "3-sharing_options.jpg",
            sc,
            facebookShare, // url to facebook
            twitterShare,
            "Celebrating the joy of reading at The Word On The Street Toronto! #TDReads", // twitter text
            null, // print
            null, // Mail
            null, // sms
            new Rectangle(1026, 375, 382, 216), // facebook
            new Rectangle(1474, 375, 382, 216), // twitter
            new Rectangle(1556, 756, 301, 72), //end
            1, // nbrPageToGoBack
            new Rectangle(132, 319, 800, 94), // Textbox
            new Rectangle(1013, 765, 135, 53), // Previous
            new KeyboardControl(50, 600, 913, 300), // Keyboard
          null
           )
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(44, 1106, 90, 47),
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(diaEmailAndShareScreen);

            // third screen cancel + finish

            var diaCancelAndFinishScreen = new DiaCancelAndFinishScreen(dirUi + "4-thank_you_screen.jpg", dirUi + "4-thank_you_screen.jpg",
                sc)
            {
                Parent = screenPanel

            };
            _screens.Add(diaCancelAndFinishScreen);
            #1#
        }

        #endregion

        #region Sofina 2017

        private void InitSofina2017(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(893, 854, user)
            {
                ProjectNo = "qvnwsx7e31",
                EventId = 816,
                ForceFileCheck = true,
                ProjectDescription = "Sofina Star Wars 2017",
                QuestionEmailNo = 4,
                QuestionSmsNo = 3,
                QuestionFacebookNo = 7,
                QuestionTwitterNo = 8,
                QuestionPictureTransformedNo = 2,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 4, 1 } };

            var quest = new QuestionnaireScreenSofina2017(dirUi + "2- 1_questionnaire_ENG.jpg", dirUi + "2- 1_questionnaire_FR.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            var share = new ShareScreen(dirUi + "2- 2_sharing_options.jpg", dirUi + "2- 2_options de partage.jpg", sc, facebookShare, twitterShare,
                "Being a Star Wars fan just got a whole lot tastier with Janes Star Wars Chicken Nuggets. #peeweequebec",
                "Vous apprécierez encore plus la Guerre des étoiles avec les nouvelles pépites de poulet Star Wars de Janes.  #peeweequebec",
                null, //print
                null, //mail
                null, //sms
                new Rectangle(81, 186, 101, 101), //facebook
                new Rectangle(81, 331, 101, 101), // twitter
                new Rectangle(551, 583, 179, 44), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 2
            };
            _screens.Add(share);
        }

        #endregion

        #region Sofina 2017-04
        private void InitSofina2017_04(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(945, 901, user)
            {
                ProjectNo = "b7c9gjbbr1",
                EventId = 861,
                ForceFileCheck = true,
                ProjectDescription = "Sofina Star Wars 2017",
                QuestionEmailNo = 4,
                QuestionSmsNo = 3,
                QuestionFacebookNo = 7,
                QuestionTwitterNo = 8,
                QuestionPictureTransformedNo = 2,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 4, 1 } };

            var quest = new QuestionnaireScreenSofina2017(dirUi + "2- 1_questionnaire_ENG.jpg", dirUi + "2- 1_questionnaire_FR.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            var share = new ShareScreen(dirUi + "2- 2_sharing_options.jpg", dirUi + "2- 2_options de partage.jpg", sc, facebookShare, twitterShare,
                "Being a Star Wars fan just got a whole lot tastier with Janes Star Wars Chicken Nuggets. #StarWarsNuggets",
                "Vous apprécierez encore plus la Guerre des étoiles avec les nouvelles pépites de poulet Star Wars de Janes.  #StarWarsPépites",
                null, //print
                null, //mail
                null, //sms
                new Rectangle(81, 186, 101, 101), //facebook
                new Rectangle(81, 331, 101, 101), // twitter
                new Rectangle(551, 583, 179, 44), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 2
            };
            _screens.Add(share);
        }

        #endregion

        #region Sofina 2017 PS New Version

        private void InitSofina2017NewVersion(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(24, 25, user)
            {
                ForceFileCheck = true,
                ProjectDescription = "Sofina Star Wars 2017 PS",
                QuestionEmailNo = 4,
                QuestionSmsNo = 3,
                QuestionFacebookNo = 7,
                QuestionTwitterNo = 8,
                QuestionPictureTransformedNo = 2,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire2.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 4, 1 } };

            var quest = new QuestionnaireScreenSofina2017(dirUi + "2- 1_questionnaire_ENG.jpg", dirUi + "2- 1_questionnaire_FR.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            var share = new ShareScreen(dirUi + "2- 2_sharing_options.jpg", dirUi + "2- 2_options de partage.jpg", sc, facebookShare, twitterShare,
                "Being a Star Wars fan just got a whole lot tastier with Janes Star Wars Chicken Nuggets. #peeweequebec",
                "Vous apprécierez encore plus la Guerre des étoiles avec les nouvelles pépites de poulet Star Wars de Janes.  #peeweequebec",
                null, //print
                null, //mail
                null, //sms
                new Rectangle(81, 186, 101, 101), //facebook
                new Rectangle(81, 331, 101, 101), // twitter
                new Rectangle(551, 583, 179, 44), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 2
            };
            _screens.Add(share);
        }

        #endregion Sofina 2017 PS New Version

        #region Sofina 2017 PS New Version Server

        private void InitSofina2017NewVersionServer(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(23, 23, user)
            {
                ForceFileCheck = true,
                ProjectDescription = "Sofina Star Wars 2017 PS",
                QuestionEmailNo = 4,
                QuestionSmsNo = 3,
                QuestionFacebookNo = 7,
                QuestionTwitterNo = 8,
                QuestionPictureTransformedNo = 2,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire2.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 4, 1 } };

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag)
            {
                IsServerJuJu = true
            };


            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);



            var shareScreen = new ShareScreen(dirUi + "share.jpg", null, sc, null, null, null, null, null,
             new Rectangle(994, 327, 107, 107), // Mail
             new Rectangle(996, 184, 107, 107), // sms
             null, null,
             new Rectangle(560, 582, 180, 47), //end
             170, new Size(480, 270))
            {
                Parent = screenPanel,
                MailJump = 1,
                SmsJump = 2,
                CancelArea = new Rectangle(1203, 18, 48, 51),
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(shareScreen);



            var share = new ShareScreen(dirUi + "2- 2_sharing_options.jpg", dirUi + "2- 2_options de partage.jpg", sc, facebookShare, twitterShare,
                "Being a Star Wars fan just got a whole lot tastier with Janes Star Wars Chicken Nuggets. #peeweequebec",
                "Vous apprécierez encore plus la Guerre des étoiles avec les nouvelles pépites de poulet Star Wars de Janes.  #peeweequebec",
                null, //print
                null, //mail
                null, //sms
                new Rectangle(81, 186, 101, 101), //facebook
                new Rectangle(81, 331, 101, 101), // twitter
                new Rectangle(551, 583, 179, 44), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 2,
                SingleFileForFacebook = true

            };
            _screens.Add(share);
        }

        #endregion Sofina 2017 PS New Version Server

        #region MarathonDFC New Version
        private void InitMarathonDFC_NewVersion(Panel screenPanel, PrintStationUser user)
        {
            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 3, 2 }, { 2, 1 }, { 4, 4 } };

            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(22, 21, user)
            {
                //QuestionFirstPictureNo = 1,
                // QuestionVideoNo = 2,
                ForceFileCheck = true,
                ProjectDescription = "DFC marathon",
                QuestionSmsNo = 3,
                QuestionVimeoId = 4,
                QuestionEmailNo = 5,
                QuestionFacebookNo = 6,
                QuestionTwitterNo = 7,
                SendMediaToTwitter = true
            };



            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire2.xml", "1.0");



            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag)
            {
                IsServerJuJu = true
            };


            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            var share = new ShareScreen(dirUi + "share.jpg", null, sc, facebookShare, twitterShare,
                "Proud to BeatEverySunrise  Recover with chocolate milk every time! RechargeMilk",
                null,
                null, //print
                new Rectangle(953, 369, 101, 101), //mail
                new Rectangle(953, 226, 101, 101), //sms
                new Rectangle(211, 226, 101, 101), //facebook
                new Rectangle(211, 369, 101, 101), // twitter
                new Rectangle(544, 571, 178, 45), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                SingleFileForFacebook = true,
                ForceTwitterToUseQuestionNo = 1
            };
            _screens.Add(share);



            var mail = new MailScreen(dirUi + "mail.jpg", null, sc, 1,
                new Rectangle(342, 38, 579, 52), // Textbox
                new Rectangle(548, 167, 183, 73),  // Previous
                new KeyboardControl(156, 275, 972, 368)  // Keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            var sms = new SmsScreen(dirUi + "sms.jpg", null, sc, 2,
                new Rectangle(527, 34, 391, 59), // Textbox
                new Rectangle(551, 167, 179, 74), // Previous
                new KeyboardControl(156, 275, 972, 368)) // Keyboard
            {
                Parent = screenPanel
            };
            _screens.Add(sms);

        }
        #endregion MarathonDFC  New Version

        #region Cowboy (Printstation) COW-2017-1

        private void InitCOW_2017_1(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(941, 897, user)
            {
                ProjectNo = "kx67w0rlo1",
                EventId = 857,
                ForceFileCheck = true,
                ProjectDescription = "Cowboy (Printstation)",
                QuestionEmailNo = 5,
                QuestionSmsNo = 4,
                QuestionFirstPictureNo = 2 // C'est le gif, mais ca sert pour l'écran de sharing.
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 1, 1 }, { 3, 2 } };

            // Claviers
            var clavier = new KeyboardControl(-1, 293, 1190, 363);

            var clavierNumerique = new NumbersKeyboardControl(164, 365, 976, 260);


            // 1- Écran Share
            var share = new ShareScreen(dirUi + "share.jpg", dirUi + "share.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(192, 189, 126, 128), //mail
                new Rectangle(967, 189, 126, 128), //sms
                null, //facebook
                null, // twitter
                new Rectangle(540, 570, 207, 67), //end
                130, new Size(500, 333))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1223, 5, 50, 50)
            };
            _screens.Add(share);

            // 2A- Mail
            var mail = new MailScreen(dirUi + "mail.jpg", dirUi + "mail.jpg", sc, 1,
                new Rectangle(366, 90, 789, 39), // Textbox
                new Rectangle(541, 184, 201, 61), // Previous
                clavier, // keyboard
                new List<ScreenCheckOption> { new ScreenCheckOption(new Rectangle(294, 142, 828, 43), 3, 1, false, new Point(299, 147)) }
                )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            // 2B- SMS
            var sms = new SmsScreen(dirUi + "sms.jpg", dirUi + "sms.jpg", sc, 2,
                new Rectangle(357, 89, 789, 39), // Textbox
                new Rectangle(546, 185, 201, 61), // Previous
                clavierNumerique // keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Make Up Forever (Printstation) MAK-2017-01B

        private void InitMAK_2017_01_B(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(898, 858, user)
            {
                ProjectNo = "gw2izo52q1",
                EventId = 820,
                ForceFileCheck = true,
                ProjectDescription = "Make Up Forever 2017 (Printstation)",
                QuestionEmailNo = 7,
                QuestionSmsNo = 6,
                QuestionFirstPictureNo = 9 // C'est le gif, mais ca sert pour l'écran de sharing.
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "mak-2017-01b-desktop.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 1, 1 }, { 3, 9 } };

            // Claviers
            var clavierUs = new UsaKeyboardControl
            {
                Location = new Point(164, 365),
                Size = new Size(976, 260)
            };

            var clavierNumerique = new NumbersKeyboardControl
            {
                Location = new Point(164, 365),
                Size = new Size(976, 260)
            };

            // 1- Écran questionnaire
            var quest = new Ui.custom.MakeUpForever.QuestionnaireScreenMak_2017_01(dirUi + "questionnaire.jpg", dirUi + "questionnaire.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            // 2- Écran Share
            var share = new ShareScreen(dirUi + "share.jpg", dirUi + "share.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(940, 221, 126, 128), //mail
                new Rectangle(231, 226, 119, 122), //sms
                null, //facebook
                null, // twitter
                new Rectangle(542, 525, 199, 51), //end
                130, new Size(500, 333))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1223, 5, 50, 50)
            };
            _screens.Add(share);

            // 2A- Mail
            var mail = new MailScreen(dirUi + "mail.jpg", dirUi + "mail.jpg", sc, 1,
                new Rectangle(459, 162, 789, 39), // Textbox
                new Rectangle(541, 297, 201, 51), // Previous
                clavierUs // keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            // 2B- SMS
            var sms = new SmsScreen(dirUi + "sms.jpg", dirUi + "sms.jpg", sc, 2,
                new Rectangle(459, 162, 789, 39), // Textbox
                new Rectangle(541, 297, 201, 51), // Previous
                clavierNumerique // keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Make Up Forever (Printstation) MAK-2017-02B

        private void InitMAK_2017_02_B(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(956, 910, user)
            {
                ProjectNo = "1zhz0emwp1",
                EventId = 870,
                ForceFileCheck = true,
                ProjectDescription = "Make Up Forever 2017 (Printstation)",
                QuestionEmailNo = 2,
                QuestionSmsNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 3;


            // Claviers
            /*var clavierUs = new UsaKeyboardControl
            {
                Location = new Point(28, 365),
                Size = new Size(976, 260)
            };

            var clavierNumerique = new NumbersKeyboardControl
            {
                Location = new Point(427, 365),
                Size = new Size(976, 260)
            };#1#

            var clavierUs = new UsaKeyboardControl(-1, 315, 1190, 353);

            var clavierNumerique = new NumbersKeyboardControl(164, 365, 976, 260);

            // 1- Écran questionnaire
            var quest = new Ui.custom.MakeUpForever.QuestionnaireScreenMak_2017_01(dirUi + "questionnaire.jpg", dirUi + "questionnaire.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            // 2- Écran Share
            var share = new ShareScreen(dirUi + "share.jpg", dirUi + "share.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(940, 221, 126, 128), //mail
                new Rectangle(231, 226, 119, 122), //sms
                null, //facebook
                null, // twitter
                new Rectangle(542, 525, 199, 51), //end
                130, new Size(500, 333))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1223, 5, 50, 50),
                SaveSelectedPictureToQuestionNo = 4
            };
            _screens.Add(share);

            // 2A- Mail
            var mail = new MailScreen(dirUi + "mail.jpg", dirUi + "mail.jpg", sc, 1,
                new Rectangle(459, 162, 757, 39), // Textbox
                new Rectangle(541, 297, 201, 51), // Previous
                clavierUs // keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            // 2B- SMS
            var sms = new SmsScreen(dirUi + "sms.jpg", dirUi + "sms.jpg", sc, 2,
                new Rectangle(459, 162, 789, 39), // Textbox
                new Rectangle(541, 297, 201, 51), // Previous
                clavierNumerique // keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Food Network Gif (Printstation) 2017

        private void InitFoodNetworkGif2017_1(Panel screenPanel, PrintStationUser user)
        {
            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            //PostQuestionnaireMapping = new Dictionary<int, int> { {1, 1}, { 2, 6 } };

            /* Tous les "magic numbers" du projet ici. #1#
            /* var sc = new PrintStationScreenConfig(910, 867, user)
             {
                 ProjectNo = "cddc0pibo1",
                 EventId = 829,
                 ForceFileCheck = true,
                 ProjectDescription = "Food Network Gif  2017 (Printstation)",
                 QuestionEmailNo = 3,
                 QuestionSmsNo = 2,
                 QuestionFirstPictureNo = 1,
                 QuestionPictureToPrintNo = 8,
                 QuestionFacebookNo = 4,
                 QuestionGifNo = 6,
                 QuestionTwitterNo = 5,
                 SendMediaToTwitter = true
             };#1#

            //var dir = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire_desktop.xml", "1.0");

            //var bkgColor = Color.FromArgb(239,65, 54);

            /* Écran web #1#
            //const string webSCreenTag = "web";
            /* var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new KeyboardControl(0,0, 1100, 300))
             {
                 Parent = screenPanel
             };#1#
            //QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            /*var facebookShare = new ShareDirectUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag)
            {
                QuotesMap = new Dictionary<string, string>
                {
                    {"en", "Check out my photo from the @cookingchannel Baconfest Chicago lounge!" }
                }
            };#1#

            //var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            /*var selecEcran = new BackgroundSelectionAndTransformationScreen(dir + "printout selection.jpg", sc,
            null, new Rectangle(1223, 17, 44, 44), new[]
            {
                            new TransformationSelectionOption(new BackgroundSelectionOption()),
                            new TransformationSelectionOption(new BackgroundSelectionOption(/*new Rectangle(772, 178, 258, 385), 7, 1, 2), dir + "printout_man fire food.jpg"#1#//)
                                                                                                                                                                               //),
                                                                                                                                                                               /*})
                                                                                                                                                                               {
                                                                                                                                                                                   Parent = screenPanel,
                                                                                                                                                                                   NoPrint = true,
                                                                                                                                                                                   CustomPrintTranformation = (sourceFile, pictureInfo, saveFilePath, chosenFrameFile) =>
                                                                                                                                                                                   {
                                                                                                                                                                                       var commonId = StringUtil.RemoveExtension(StringUtil.GetFileName(sourceFile));
                                                                                                                                                                                       var baseDir = DirectoryUtil.GetLocalPrintstationMediaDir(pictureInfo.UserId,
                                                                                                                                                                                           DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.file_bundles) + commonId;

                                                                                                                                                                                       var explodedDir = baseDir + Path.DirectorySeparatorChar + "zip" + Path.DirectorySeparatorChar;

                                                                                                                                                                                       // N'extrait les fichiers qu'une fois, si ce n'est pas fait.
                                                                                                                                                                                       if (!Directory.Exists(explodedDir))
                                                                                                                                                                                       {
                                                                                                                                                                                           // On assume qu'il n'y a qu'un seul fichier ZIP.
                                                                                                                                                                                           var bundleFile = Directory.EnumerateFiles(baseDir, "*.zip").ToList().FirstOrDefault();
                                                                                                                                                                                           if (!File.Exists(bundleFile))
                                                                                                                                                                                           {
                                                                                                                                                                                               log.Error("Could not find necessary bundle zip file in following directory {0}", baseDir);
                                                                                                                                                                                               return;
                                                                                                                                                                                           }

                                                                                                                                                                                           var dirInfo = Directory.CreateDirectory(explodedDir);
                                                                                                                                                                                           if (!dirInfo.Exists)
                                                                                                                                                                                           {
                                                                                                                                                                                               log.Error("Could not create exploded bundleDir {0}", explodedDir);
                                                                                                                                                                                               return;
                                                                                                                                                                                           }

                                                                                                                                                                                           try
                                                                                                                                                                                           {
                                                                                                                                                                                               var zip = new ZipFile(bundleFile);
                                                                                                                                                                                               foreach (var e in zip.Entries)
                                                                                                                                                                                               {
                                                                                                                                                                                                   e.Extract(explodedDir);
                                                                                                                                                                                               }
                                                                                                                                                                                           }
                                                                                                                                                                                           catch (Exception e)
                                                                                                                                                                                           {
                                                                                                                                                                                               log.Error(e, $"Could not extract zip file to directory. Zip={bundleFile}, Dir={explodedDir}");
                                                                                                                                                                                               return;
                                                                                                                                                                                           }
                                                                                                                                                                                       }

                                                                                                                                                                                       // Maintenant que le répertoire du Post est extrait, on traite l'image.
                                                                                                                                                                                       var files = Directory.GetFiles(explodedDir).ToList();
                                                                                                                                                                                       PrintStationUtil.CreateFramedPhoto(files, pictureInfo, saveFilePath, chosenFrameFile);
                                                                                                                                                                                   }
                                                                                                                                                                               };
                                                                                                                                                                               _screens.Add(selecEcran);#1#

            /*_screens.Add(new Ui.generic.ShareScreen(dir + "Sharing options.jpg", null, sc, facebookShare, twitterShare, 
                "Check out my photo from the @cookingchannel Baconfest Chicago lounge!", null
                , new Rectangle(897, 65, 133, 133) // Print
                , new Rectangle(897, 229, 133, 133) // mail
                , new Rectangle(250, 65, 133, 133) // sms
                , new Rectangle(250, 229, 133, 133) // facebook
                , new Rectangle(250, 393, 133, 133) // twitter
                , new Rectangle(547, 566, 187, 62) // end
                , 50, new Size(480, 480)
                )
            {
                Parent = screenPanel,
                SingleFileForFacebook = false,
                ApplyTransformationToSelectedImage = false,
                IgnorePictureToPrintQuestion = true,
                AdditionalProcessForFileBundle =
                    (imageFile => // Action supplémentaire pour gérer les fichiers du Bundle
                    {
                        var questionnaire = MainForm.GetInstance().Questionnaire;
                        var commonId = StringUtil.RemoveExtension(StringUtil.GetFileName(imageFile));
                        var bundleDir = DirectoryUtil.GetLocalPrintstationMediaDir(sc.PrintStationUser.Id,
                            DirectoryUtil.NomadLogicPrintstationDirectoriesEnum.file_bundles) + commonId;

                        if (!Directory.Exists(bundleDir)) throw new Exception("Following bundle directory does not exists: " + bundleDir);

                        var gifFile = Directory.EnumerateFiles(bundleDir).First(a => a.EndsWith(".gif", StringComparison.InvariantCultureIgnoreCase));

                        if (!File.Exists(gifFile)) throw new Exception("Following gif file directory does not exists: " + gifFile);

                        // On sauvegarde les fichiers
                        var question = questionnaire.GetQuestion(6);
                        var tmp = question.GetChoice(1);
                        var answer = new Answer(QuestionPhotoText.TYPE, 6, 1) { Response = gifFile };
                        tmp.SetAnswer(answer);

                      
                    })
            });#1#

            /*var mailScreen = new MailScreen(dir + "email_sharing.jpg", null, sc, 1
                , new Rectangle(380, 76, 687, 66)
                , new Rectangle(536, 169, 208, 70)
                , new KeyboardControl(0, 270, 1000, 360) { BackColor = bkgColor})
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "sms_sharing.jpg", null, sc, 2
                , new Rectangle(534, 76, 548, 70)
                , new Rectangle(536, 167, 210, 72)
                , new NumbersKeyboardControl(0, 270, 600, 360) { BackColor = bkgColor})
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);#1#
        }





        #endregion Food Network Gif (Printstation) 2017

        #region Food Network 2017 Mastercard Priceless Table 18 mai

        private void InitFoodNetworkGif2017_1_C_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(918, 874, user)
            {
                ProjectNo = "p80d4bx771",
                EventId = 834,
                ForceFileCheck = true,
                SendMediaToTwitter = true,
                ProjectDescription = "Food Network 2017 Mastercard Priceless Table 18 mai PS",
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "foo-2017-01-c-ps-desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new KeyboardControl(0, 0, 1100, 300))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            _screens.Add(new ShareScreen(dir + "sharing-options.jpg", null, sc, facebookShare, twitterShare,
                "Great night at the @Mastercard #PricelessTable in @FoodNetwork Kitchen in NYC!", null
                , new Rectangle(886, 52, 156, 158) // Print
                , new Rectangle(886, 216, 156, 158) // mail
                , new Rectangle(237, 52, 156, 158) // sms
                , new Rectangle(237, 216, 156, 158) // facebook
                , new Rectangle(237, 381, 156, 158) // twitter
                , new Rectangle(546, 564, 187, 66) // end
                , 53, new Size(480, 480)
                )
            {
                Parent = screenPanel,
                ApplyTransformationToSelectedImage = true,
                SaveSelectedPictureToQuestionNo = 1,
                SingleFileForFacebook = false,
                ForceTwitterToUseQuestionNo = 1

            });

            var mailScreen = new MailScreen(dir + "enter-email.jpg", null, sc, 1
                , new Rectangle(418, 33, 619, 52)
                , new Rectangle(533, 167, 213, 74)
                , new KeyboardControl(-1, 261, 1000, 347))
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "sms-sharing.jpg", null, sc, 2
                , new Rectangle(497, 34, 728, 47)
                , new Rectangle(533, 167, 213, 74)
                , new NumbersKeyboardControl(-1, 256, 371, 343))
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);
        }

        #endregion

        #region Food Network 2017 Mastercard Priceless Table 17 mai

        private void InitFoodNetworkGif2017_1_B_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(919, 875, user)
            {
                ProjectNo = "n5oe57scq1",
                EventId = 835,
                ForceFileCheck = true,
                SendMediaToTwitter = true,
                ProjectDescription = "Food Network 2017 Mastercard Priceless Table 17 mai PS",
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionNumberOfPrintNo = 6
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "foo-2017-01-b-ps-desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new KeyboardControl(0, 0, 1100, 300))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            _screens.Add(new ShareScreen(dir + "sharing-options.jpg", null, sc, facebookShare, twitterShare,
                "Great night with @AmericanAir, @Mastercard & @Citi at a #PricelessTable in @FoodNetwork kitchen in NYC!", null
                , new Rectangle(886, 52, 156, 158) // Print
                , new Rectangle(886, 216, 156, 158) // mail
                , new Rectangle(237, 52, 156, 158) // sms
                , new Rectangle(237, 216, 156, 158) // facebook
                , new Rectangle(237, 381, 156, 158) // twitter
                , new Rectangle(546, 564, 187, 66) // end
                , 53, new Size(480, 480)
                )
            {
                Parent = screenPanel,
                ApplyTransformationToSelectedImage = true,
                SaveSelectedPictureToQuestionNo = 1,
                SingleFileForFacebook = false,
                ForceTwitterToUseQuestionNo = 1
            });

            var mailScreen = new MailScreen(dir + "enter-email.jpg", null, sc, 1
                , new Rectangle(418, 33, 619, 52)
                , new Rectangle(533, 167, 213, 74)
                , new KeyboardControl(-1, 261, 1000, 347))
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "enter-sms.jpg", null, sc, 2
                , new Rectangle(497, 34, 728, 47)
                , new Rectangle(533, 167, 213, 74)
                , new NumbersKeyboardControl(-1, 256, 371, 343))
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);
        }

        #endregion

        #region Île-du-Prince-Édouard - PS

        private void Init_IGN201701_PS(Panel screenPanel, PrintStationUser user)
        {
            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 1, 1 }, { 3, 6 } };

            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(928, 883, user)
            {
                ProjectNo = "cuigsuaji1",
                EventId = 843,
                ForceFileCheck = true,
                SendMediaToTwitter = true,
                ProjectDescription = "Île-du-Prince-Édouard - PS",
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionFirstPictureNo = 1
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "ign-2017-01-ps-desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new KeyboardControl(0, 0, 1100, 300))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            _screens.Add(new ShareScreen(dir + "2- 1_sharing_options.jpg", dir + "2- 1_options de partage.jpg", sc, facebookShare, twitterShare
                , "Come find your island!"
                , "Votre Ile vous attend!"
                , null // Print
                , new Rectangle(1093, 330, 102, 102) // mail
                , new Rectangle(1095, 187, 102, 102) // sms
                , new Rectangle(81, 187, 102, 102) // facebook
                , new Rectangle(81, 330, 102, 102) // twitter
                , new Rectangle(551, 583, 179, 43) // end
                , 50, new Size(750, 500)
            )
            {
                Parent = screenPanel,
                ApplyTransformationToSelectedImage = false,
                SingleFileForFacebook = false,
                ForceTwitterToUseQuestionNo = 1,
                CancelArea = new Rectangle(1210, 0, 70, 66)
            });

            var mailScreen = new MailScreen(dir + "2- 3_e-mail_ENG.jpg", dir + "2- 3_e-mail_FR.jpg", sc, 1
                , new Rectangle(358, 89, 857, 43)
                , new Rectangle(551, 190, 180, 44)
                , new KeyboardControl(-1, 265, 1185, 389),
                new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(293, 140, 648, 44), 7, 1, false, new Point(297, 137))
                })
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "2- 2_SMS_ENG.jpg", dir + "2- 2_SMS_FR.jpg", sc, 2
                , new Rectangle(358, 89, 857, 43) // text
                , new Rectangle(551, 190, 180, 44) // previous
                , new NumbersKeyboardControl(-1, 265, 510, 494))
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);
        }

        #endregion

        #region Île-du-Prince-Édouard - PS - B

        private void Init_IGN201701B_PS(Panel screenPanel, PrintStationUser user)
        {
            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 1, 1 }, { 3, 6 } };

            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(930, 885, user)
            {
                ProjectNo = "gon53f5zh1",
                EventId = 845,
                ForceFileCheck = true,
                SendMediaToTwitter = true,
                ProjectDescription = "Île-du-Prince-Édouard - PS - B",
                QuestionSmsNo = 2,
                QuestionEmailNo = 3,
                QuestionFacebookNo = 4,
                QuestionTwitterNo = 5,
                QuestionFirstPictureNo = 1
            };

            var dir = InitPrintStationQuestionnaire(screenPanel, sc, "ign-2017-01b-ps-desktop.xml", "1.0");

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new KeyboardControl(0, 0, 1100, 300))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            _screens.Add(new ShareScreen(dir + "2- 1_sharing_options.jpg", dir + "2- 1_options de partage.jpg", sc, facebookShare, twitterShare
                , "Come find your island!"
                , "Votre Ile vous attend!"
                , null // Print
                , new Rectangle(1093, 330, 102, 102) // mail
                , new Rectangle(1095, 187, 102, 102) // sms
                , new Rectangle(81, 187, 102, 102) // facebook
                , new Rectangle(81, 330, 102, 102) // twitter
                , new Rectangle(551, 583, 179, 43) // end
                , 50, new Size(750, 500)
            )
            {
                Parent = screenPanel,
                ApplyTransformationToSelectedImage = false,
                SingleFileForFacebook = false,
                ForceTwitterToUseQuestionNo = 1,
                CancelArea = new Rectangle(1210, 0, 70, 66)
            });

            var mailScreen = new MailScreen(dir + "2- 3_e-mail_ENG.jpg", dir + "2- 3_e-mail_FR.jpg", sc, 1
                , new Rectangle(358, 89, 857, 43)
                , new Rectangle(551, 190, 180, 44)
                , new KeyboardControl(-1, 265, 1185, 389),
                new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(293, 140, 648, 44), 7, 1, false, new Point(297, 137))
                })
            {
                Parent = screenPanel
            };
            _screens.Add(mailScreen);

            var smsScreen = new SmsScreen(dir + "2- 2_SMS_ENG.jpg", dir + "2- 2_SMS_FR.jpg", sc, 2
                , new Rectangle(358, 89, 857, 43) // text
                , new Rectangle(551, 190, 180, 44) // previous
                , new NumbersKeyboardControl(-1, 265, 510, 494))
            {
                Parent = screenPanel
            };
            _screens.Add(smsScreen);
        }

        #endregion

        #region GMC Chuchwagon Calgary Stamepede 2017 (MOM-2017-01-PS)

        private void InitMom_2017_01_ps(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(939, 895, user)
            {
                ProjectNo = "u66iyog4t1",
                EventId = 855,
                ForceFileCheck = true,
                ProjectDescription = "GMC Chuchwagon Calgary Stamepede 2017",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4,
                QuestionFirstPictureNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "mom-2017-01-ps-desktop.xml", "1.0");

            // Mapping questionnaire à partir des réponses du questionnaire original, selon sa configuration d'envoi Printstation.
            PostQuestionnaireMapping = new Dictionary<int, int> { { 6, 1 }, { 3, 2 } };

            // Claviers
            var clavier = new KeyboardControl(-1, 315, 1140, 350);
            var clavierNumerique = new NumbersKeyboardControl(-1, 315, 500, 350);

            // 1- Écran de partage
            var share = new ShareScreen(dirUi + "sharing_options.jpg", dirUi + "sharing_options.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(1023, 163, 103, 102), //mail
                new Rectangle(1023, 321, 103, 102), //sms
                null, //facebook
                null, // twitter
                new Rectangle(537, 531, 206, 90), //end
                97, new Size(696, 392))
            {
                Parent = screenPanel
            };
            _screens.Add(share);

            // 1A- Mail
            var mail = new MailScreen(dirUi + "Enter-email_CONFIRM OPT-IN.jpg", dirUi + "Enter-email_CONFIRM OPT-IN.jpg", sc, 1,
                new Rectangle(346, 99, 853, 34), // Textbox
                new Rectangle(537, 205, 207, 92), // Previous
                clavier, // keyboard
                new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(96, 142, 1090, 47), 5, 1, false, new Point(100, 140))
                }
            )
            {
                Parent = screenPanel

            };
            _screens.Add(mail);

            // 1B- SMS
            var sms = new SmsScreen(dirUi + "enter-mobile-number.jpg", dirUi + "enter-mobile-number.jpg", sc, 2,
                new Rectangle(346, 99, 853, 34), // Textbox
                new Rectangle(537, 205, 207, 92), // Previous
                clavierNumerique // keyboard
            )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Bet Wedding BET 2017-01
        private void InitBET_2017_1(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(949, 905, user)
            {
                ProjectNo = "ov0uwp8ez1",
                EventId = 865,
                ForceFileCheck = true,
                ProjectDescription = "Wedding (Printstation)",
                QuestionEmailNo = 2,
                QuestionSmsNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "questionnaire.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 3;

            // Claviers
            var clavier = new KeyboardControl(-1, 293, 1190, 363);

            var clavierNumerique = new NumbersKeyboardControl(164, 365, 976, 260);


            // 1- Écran Share
            var share = new ShareScreen(dirUi + "share.jpg", dirUi + "share.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(192, 189, 126, 128), //mail
                new Rectangle(967, 189, 126, 128), //sms
                null, //facebook
                null, // twitter
                new Rectangle(540, 570, 207, 67), //end
                130, new Size(500, 333))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1223, 5, 50, 50),
                SaveSelectedPictureToQuestionNo = 4
            };
            _screens.Add(share);

            // 2A- Mail
            var mail = new MailScreen(dirUi + "mail.jpg", dirUi + "mail.jpg", sc, 1,
                new Rectangle(366, 90, 789, 39), // Textbox
                new Rectangle(541, 184, 201, 61), // Previous
                clavier, // keyboard
                new List<ScreenCheckOption> { new ScreenCheckOption(new Rectangle(294, 142, 828, 43), 3, 1, false, new Point(299, 147)) }
                )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            // 2B- SMS
            var sms = new SmsScreen(dirUi + "sms.jpg", dirUi + "sms.jpg", sc, 2,
                new Rectangle(357, 89, 789, 39), // Textbox
                new Rectangle(546, 185, 201, 61), // Previous
                clavierNumerique // keyboard
                )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }
        #endregion

        #region DIA-2017-02 (PS)

        private void InitDiaPS2017(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(961, 914, user)
            {
                ProjectNo = "z5mu4zior1",
                EventId = 873,
                ForceFileCheck = true,
                ProjectDescription = "TD Share your Story PS 2017",
                QuestionEmailNo = 3,
                QuestionFacebookNo = 5,
                QuestionTwitterNo = 6,
                QuestionPictureTransformedNo = 1,
                QuestionPictureToPrintNo = 1,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "dia-2017-02b-desktop.xml", "1.0");
            /*
            // first screen contain only confirmation radio button
            var diaConfirmationScreen = new DiaConfirmationScreen(
                dirUi + "2- parental disclaimer.jpg", sc
            )
            {
                Parent = screenPanel
            };
            _screens.Add(diaConfirmationScreen);
            #1#
            // second screen contain email + confirmation + facebook and twitter share
            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1000, 350))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);
            /*
            var diaEmailAndShareScreen = new DiaEmailAndShareScreen(
                dirUi + "3- sharing options.jpg",
                sc,
                facebookShare, // url to facebook
                twitterShare,
                "Celebrating the joy of reading at The Word On The Street Toronto! #TDReads", // twitter text
                null, // print
                null, // Mail
                null, // sms
                new Rectangle(683, 251, 255, 144), // facebook
                new Rectangle(982, 251, 255, 144), // twitter
                new Rectangle(1036, 504, 201, 46), //end
                1, // nbrPageToGoBack
                new Rectangle(88, 251, 480, 34), // Textbox
                new Rectangle(683, 513, 84, 29), // Previous
                new KeyboardControl(8, 410, 668, 384), // Keyboard
                null
            )
            {
                Parent = screenPanel,
                //CancelArea = new Rectangle(44, 1106, 90, 47),
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(diaEmailAndShareScreen);
            #1#
            // third screen cancel + finish

            var thxScreen = new ThanksEndScreen(dirUi + "4- thank you screen.jpg", sc)
            {
                Parent = screenPanel,
                ForcePrintOnActivation = true
            };
            _screens.Add(thxScreen);
        }

        #endregion

        #region Make Up Forever (Printstation) MAK-2017-01C

        private void InitMAK_2017_01_C(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(967, 920, user)
            {
                ProjectNo = "bf1s7ut6f1",
                EventId = 880,
                ForceFileCheck = true,
                ProjectDescription = "Make Up Forever 2017 (Printstation) V3",
                QuestionEmailNo = 2,
                QuestionSmsNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "mak-2017-01c-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 3;

            var clavierUs = new UsaKeyboardControl(-1, 315, 1190, 353);

            var clavierNumerique = new NumbersKeyboardControl(164, 365, 976, 260);

            // 1- Écran questionnaire
            var quest = new Ui.custom.MakeUpForever.QuestionnaireScreenMak_2017_01_C(dirUi + "questionnaire_page1.jpg", dirUi + "questionnaire_page1.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            // 2- Écran Share
            var share = new ShareScreen(dirUi + "questionnaire_page2.jpg", dirUi + "questionnaire_page2.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(940, 221, 126, 128), //mail
                new Rectangle(231, 226, 119, 122), //sms
                null, //facebook
                null, // twitter
                new Rectangle(542, 525, 199, 51), //end
                130, new Size(500, 333))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1223, 5, 50, 50),
                SaveSelectedPictureToQuestionNo = 4
            };
            _screens.Add(share);

            // 2A- Mail
            var mail = new MailScreen(dirUi + "questionnaire_page3.jpg", dirUi + "questionnaire_page3.jpg", sc, 1,
                new Rectangle(459, 162, 757, 39), // Textbox
                new Rectangle(541, 297, 201, 51), // Previous
                clavierUs // keyboard
            )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            // 2B- SMS
            var sms = new SmsScreen(dirUi + "questionnaire_page4.jpg", dirUi + "questionnaire_page4.jpg", sc, 2,
                new Rectangle(459, 162, 789, 39), // Textbox
                new Rectangle(541, 297, 201, 51), // Previous
                clavierNumerique // keyboard
            )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Top Shot DIA-2017-03B

        private void InitDIA_2017_03B(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(970, 923, user)
            {
                ProjectNo = "ackqk8zko1",
                EventId = 883,
                ForceFileCheck = true,
                ProjectDescription = "Top Shot (Printstation)",
                QuestionEmailNo = 2,
                QuestionNumberOfPrintNo = 3,
                QuestionPictureToPrintNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "dia-2017-03b-desktop.xml", "1.0");

            // 1- Écran Share
            var share = new ShareScreen(dirUi + "1-E-mail_print.jpg", dirUi + "1-E-mail_print.jpg", sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                new Rectangle(897, 236, 114, 114), //print
                new Rectangle(273, 236, 114, 114), //mail
                null, //sms
                null, //facebook
                null, // twitter
                new Rectangle(531, 535, 209, 76), //end
                80, new Size(400, 400))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1223, 5, 50, 50),
                ApplyTransformationToSelectedImage = true,
                DontUseInactiveImages = true
            };
            _screens.Add(share);

            var optins = new List<ScreenCheckOption>
            {
                new ScreenCheckOption(new Rectangle(152, 115, 645, 45), 4, 1, false, new Point(154, 111))
            };

            // 1A- Mail
            var clavier = new KeyboardControl(-1, 295, 1190, 385);
            var mail = new MailScreen(dirUi + "2-Enter_E-mail.jpg", dirUi + "2-Enter_E-mail.jpg", sc, 1,
                new Rectangle(348, 73, 815, 33), // Textbox
                new Rectangle(540, 203, 199, 70), // Previous
                clavier, // keyboard
                optins // Opt-ins
            )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);
        }

        #endregion

        #region Hospital_Fundation_Oakville STE-2017-02

        private void InitHospital_STE_2017_02(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(980, 927, user)
            {
                ProjectNo = "nlrob3lvw1",
                EventId = 887,
                ForceFileCheck = true,
                ProjectDescription = "Hospital Foundation (Printstation)",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "ste-2017-02b-desktop.xml", "1.0");

            #region Share screen
            // 1- Écran Share
            var share = new ShareScreen(dirUi + "2- Sharing option.jpg", null, sc,
                null, // Twitter Share Object
                null, // Facebook Share Object
                null, // message Twitter EN
                null, // message Twitter FR
                null, //print
                new Rectangle(221, 191, 102, 102), //mail
                new Rectangle(955, 191, 102, 102), //sms
                null, //facebook
                null, // twitter
                new Rectangle(542, 502, 190, 61), //end
                90, new Size(500, 333))
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1199, 1, 79, 60),
                DontUseInactiveImages = true,
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(share);
            #endregion

            #region Mail
            var optins = new List<ScreenCheckOption>
            {
                new ScreenCheckOption(new Rectangle(146, 122, 992, 38), 4, 1, false, new Point(157, 113))
            };
            var clavier = new KeyboardControl(-1, 288, 1176, 332);

            var mail = new MailScreen(dirUi + "3- e-mail.jpg", null, sc, 1,
                new Rectangle(334, 69, 810, 43), // Textbox
                new Rectangle(544, 208, 189, 58), // Previous
                clavier, // keyboard
                optins // Opt-ins
            )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);
            #endregion

            #region Sms

            var clavier2 = new NumbersKeyboardControl(-1, 287, 432, 330);
            var smsOpt = new SmsScreen(dirUi + "4-SMS.jpg", null, sc, 2,
                new Rectangle(341, 68, 844, 42),
                new Rectangle(545, 209, 189, 59),
                clavier2)
            {
                Parent = screenPanel
            };
            _screens.Add(smsOpt);

            #endregion
        }

        #endregion

        #region Coca-Cola Caravan (ESH-2017-04, 6 projets différents)

        private void Init_ESH_2017_04_t1(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 981;
            const string projNo = "l109ttkn61";
            const int questId = 928;
            const int eventId = 888;
            const int teamNo = 1;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        private void Init_ESH_2017_04_t2(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 982;
            const string projNo = "jkchcxc3n1";
            const int questId = 929;
            const int eventId = 889;
            const int teamNo = 2;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        private void Init_ESH_2017_04_t3(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 983;
            const string projNo = "kyp9h8c3u1";
            const int questId = 930;
            const int eventId = 890;
            const int teamNo = 3;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        private void Init_ESH_2017_04_t4(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 984;
            const string projNo = "ak13ypc451";
            const int questId = 931;
            const int eventId = 891;
            const int teamNo = 4;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        private void Init_ESH_2017_04_t5(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 985;
            const string projNo = "hitvf3c4f1";
            const int questId = 932;
            const int eventId = 892;
            const int teamNo = 5;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        private void Init_ESH_2017_04_t6(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 986;
            const string projNo = "oahtz3c4n1";
            const int questId = 933;
            const int eventId = 893;
            const int teamNo = 6;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        private void Init_ESH_2017_04_t7(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            const int projId = 993;
            const string projNo = "fnkgkr3z31";
            const int questId = 940;
            const int eventId = 900;
            const int teamNo = 7;

            InitEsh201701Team(screenPanel, user, projId, projNo, questId, eventId, teamNo);
        }

        /// Constructeur "générique" pour le projet ESH-2017-04, puisqu'il y a très peu de variation antre les projets (juste les magic numbers en somme).
        private void InitEsh201701Team(Panel screenPanel, PrintStationUser user, int projId, string projNo, int questId, int eventId, int teamNo)
        {
            var sc = new PrintStationScreenConfig(projId, questId, user)
            {
                ProjectNo = projNo,
                EventId = eventId,
                ForceFileCheck = true,
                ProjectDescription = $"Coca-Cola Caravan (Team {teamNo})",
                QuestionPictureToPrintNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, $"esh-2017-04_t{teamNo}-desktop.xml", "1.0");

            /* var zones = new List<TransformationSelectionOption>
             {
                 new TransformationSelectionOption(new BackgroundSelectionOption(new Rectangle(294, 213, 243, 243), 2, 1, 1), dirUi + "PRINTOUT-ENGLISH.jpg"),
                 new TransformationSelectionOption(new BackgroundSelectionOption(new Rectangle(742, 213, 243, 243), 2, 1, 2), dirUi + "PRINTOUT-SPANISH.jpg")
             };#1#

            /*var selScreen = new BackgroundSelectionAndTransformationScreen(dirUi + "language-selection.jpg", sc, null, new Rectangle(1210, 2, 67, 65))
            {
                Parent = screenPanel
            };
            _screens.Add(selScreen);#1#
        }

        #endregion

        #region Sofina Fusion (SOF-2017-04f Printstation)

        private void Init_SOF_2017_04_fusion(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(988, 935, user)
            {
                ProjectNo = "4ikylo4771",
                EventId = 895,
                ForceFileCheck = true,
                ProjectDescription = "Sofina Star Wars 2017 Fusion PS",
                QuestionEmailNo = 4,
                QuestionSmsNo = 3,
                QuestionFacebookNo = 7,
                QuestionTwitterNo = 8,
                QuestionPictureTransformedNo = 2,
                SendMediaToTwitter = true
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "sof-2017-04f-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 9;

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);
            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);
            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag);

            var langScreen = new HomeScreen(dirUi + "2- 1_ language selection.jpg", sc,
                new Rectangle(111, 467, 418, 459), // fr
                new Rectangle(751, 494, 418, 161), // en
                null)
            {
                Parent = screenPanel,
                QuestionLanguageNo = 1
            };
            _screens.Add(langScreen);

            var quest = new QuestionnaireScreenSofina2017(dirUi + "2- 2_questionnaire_ENG.jpg", dirUi + "2- 2_questionnaire_FR.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);

            var share = new ShareScreen(dirUi + "2- 3_sharing_options.jpg", dirUi + "2- 3_options de partage.jpg", sc, facebookShare, twitterShare,
                "Being a Star Wars fan just got a whole lot tastier with Janes Star Wars Chicken & Fish Nuggets. #StarWarsNuggets",
                "Vous apprecierez encore plus la Guerre des etoiles avec les nouvelles pepites de poisson Star Wars de Janes.  #StarWarsPepites",
                null, //print
                null, //mail
                null, //sms
                new Rectangle(81, 186, 101, 101), //facebook
                new Rectangle(81, 331, 101, 101), // twitter
                new Rectangle(551, 583, 179, 44), //end
                40, new Size(717, 508))
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 2
            };
            _screens.Add(share);
        }

        #endregion

        #region Diamond MBNA DIA_2017-04 (PS)

        private void Init_DIA_2017_04_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(990, 937, user)
            {
                ProjectNo = "3grh9uq0z1",
                EventId = 897,
                ForceFileCheck = true,
                ProjectDescription = "MBNA Diamond 2017 PS"
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "dia-2017-04-ps-desktop.xml", "1.0");

            var langScreen = new HomeScreen(dirUi + "2-choix de langue.jpg", sc,
                new Rectangle(918, 516, 290, 84), // fr
                new Rectangle(471, 518, 282, 79), // en
                null)
            {
                Parent = screenPanel,
                QuestionLanguageNo = 2
            };
            _screens.Add(langScreen);

            var legal = new Ui.generic.SectionSkipScreen(dirUi + "3- legal.jpg", dirUi + "3- legal FRENCH.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZones = new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(326, 484, 69, 64), 3, 1, true)
                },
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1349, 926, 280, 78),
                        PageSkip = 2
                    },
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(677, 559, 678, 64),
                        PageSkip = 1,
                        SkipValidation = true
                    }
                }
            };
            _screens.Add(legal);

            var terms = new Ui.generic.SectionSkipScreen(dirUi + "3a - Terms and conditions ENG.jpg", dirUi + "3a - Terms and conditions FRENCH.jpg", sc)
            {
                Parent = screenPanel,
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1443, 924, 85, 85),
                        PageSkip = -1
                    }
                }
            };
            _screens.Add(terms);
            /*
            var quest = new QuestionnaireScreenDia201704(dirUi + "4 - questionnaire sharing ENG.jpg", dirUi + "4 - questionnaire sharing FRENCH.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);
            #1#
            var unsub = new Ui.generic.SectionSkipScreen(dirUi + "4a - unsubscribe MBNA ENG.jpg", dirUi + "4a - unsubscribe MBNA FRENCH.jpg", sc)
            {
                Parent = screenPanel,
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1443, 924, 85, 85),
                        PageSkip = -1
                    }
                }
            };
            _screens.Add(unsub);

            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "5 - thank you ENG.jpg", sc, dirUi + "5 - thank you FRENCH.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 5000
            };
            _screens.Add(thxScreen);
        }

        #endregion

        #region Diamond MBNA DIA_2017-04 (PS + Fusion)

        private void Init_DIA_2017_04F_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(995, 942, user)
            {
                ProjectNo = "pwkaxe2zs1",
                EventId = 902,
                ForceFileCheck = true,
                ProjectDescription = "MBNA Diamond 2017 PS (Fusion)"
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "dia-2017-04f-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 10;

            var langScreen = new HomeScreen(dirUi + "2-choix de langue.jpg", sc,
                new Rectangle(918, 516, 290, 84), // fr
                new Rectangle(471, 518, 282, 79), // en
                null)
            {
                Parent = screenPanel,
                QuestionLanguageNo = 2
            };
            _screens.Add(langScreen);

            var legal = new Ui.generic.SectionSkipScreen(dirUi + "3- legal.jpg", dirUi + "3- legal FRENCH.jpg", sc, new Rectangle(1208, 924, 109, 89))
            {
                Parent = screenPanel,
                CheckBoxZones = new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(326, 484, 69, 64), 3, 1, true)
                },
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1349, 926, 280, 78),
                        PageSkip = 2
                    },
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(677, 559, 678, 64),
                        PageSkip = 1,
                        SkipValidation = true
                    }
                }
            };
            _screens.Add(legal);

            var terms = new Ui.generic.SectionSkipScreen(dirUi + "3a - Terms and conditions ENG.jpg", dirUi + "3a - Terms and conditions FRENCH.jpg", sc)
            {
                Parent = screenPanel,
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1443, 924, 85, 85),
                        PageSkip = -1
                    }
                }
            };
            _screens.Add(terms);
            /*
            var quest = new QuestionnaireScreenDia201704(dirUi + "4 - questionnaire sharing ENG.jpg", dirUi + "4 - questionnaire sharing FRENCH.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(quest);
            #1#
            var unsub = new Ui.generic.SectionSkipScreen(dirUi + "4a - unsubscribe MBNA ENG.jpg", dirUi + "4a - unsubscribe MBNA FRENCH.jpg", sc)
            {
                Parent = screenPanel,
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1443, 924, 85, 85),
                        PageSkip = -1
                    }
                }
            };
            _screens.Add(unsub);

            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "5 - thank you ENG.jpg", sc, dirUi + "5 - thank you FRENCH.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 5000
            };
            _screens.Add(thxScreen);
        }

        #endregion

        #region Facebook STA_2018-01 (PS + Fusion)

        private void Init_STA_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1015, 958, user)
            {
                ProjectNo = "wq9a4s32f1",
                EventId = 918,
                ForceFileCheck = true,
                ProjectDescription = "Facebook 2018 PS (Fusion)",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "sta-2018-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 5;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2-Language.jpg", sc,
                new Rectangle(182, 303, 257, 81), // fr
                new Rectangle(841, 299, 253, 85), // en
                null)
            {
                Parent = screenPanel,
                QuestionLanguageNo = 7
            };
            _screens.Add(langScreen);
            

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);

            // Écran 4 - Sharing
            var share = new ShareScreen(dirUi + "3-Sharing_option.jpg", null, sc,
                facebookShare, null, null, null, null,
                new Rectangle(262, 175, 113, 109), // Mail
                new Rectangle(265, 360, 113, 113), // SMS
                new Rectangle(906, 280, 106, 106), // facebook
                null,
                new Rectangle(545, 558, 187, 60), // End
                90, new Size(450, 450)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 7, 90, 68)
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4-E-mail_ENG.jpg", dirUi + "4-E-mail_FR.jpg", sc, 1,
                new Rectangle(400, 65, 657, 56), //textbox
                new Rectangle(546, 168, 189, 63), // ok button
                new KeyboardControl(44, 250, 1202, 378)
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "5-SMS_ENG.jpg", dirUi + "5-SMS_FR.jpg", sc, 2,
                new Rectangle(550, 65, 565, 71),
                new Rectangle(543, 165, 197, 68),
                new KeyboardControl(44, 250, 1202, 378)
            );
            _screens.Add(sms);
        }

        #endregion

        #region Hive-2018-Grolsch (PS + Fusion)

        private void Init_HIVE_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1017, 960, user)
            {
                ProjectNo = "y5zn7fmb71",
                EventId = 920,
                ForceFileCheck = true,
                ProjectDescription = "Hive 2018 PS (Fusion)",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4,
                QuestionFirstPictureNo = 2
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "hiv-2018-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 8;

            // Écran 3 - questionnaire screen
            var questionnaire = new Ui.custom.Groslch.QuestionnaireScreen(dirUi + "2-questionnaire.jpg", sc) // cancel
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(questionnaire);


            // Écran 4 - Sharing
            var share = new ShareScreen(dirUi + "3-Sharing_screen.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(1077, 324, 128, 117), // Mail
                new Rectangle(1079, 179, 127, 119), // SMS
                null, // facebook
                null,
                new Rectangle(546, 510, 187, 52), // End
                50, new Size(307, 450)) // Preview
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1187, 7, 90, 68)
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4b-email_sharing.jpg", null, sc, 1,
                new Rectangle(691, 92, 483, 49), //textbox
                new Rectangle(565, 174, 151, 53), // ok button
                new KeyboardControl(-1, 232, 1248, 340)
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "4a-sms_sharing.jpg", null, sc, 2,
                new Rectangle(770, 92, 386, 43),
                new Rectangle(564, 174, 151, 52),
                new NumbersKeyboardControl(-1, 232, 374, 340)
            );
            _screens.Add(sms);
        }

        #endregion

        #region SquareEnix Tomb Raider AirCam KEN-2018-01-MTL (PS + Fusion)

        private void Init_KEN_2018_01_MTL_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1019, 962, user)
            {
                ProjectNo = "nt1xh5b2w1",
                EventId = 922,
                ForceFileCheck = true,
                ProjectDescription = "Kenwood 2018-01 PS (MTL)",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4,
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "ken-2018-01-mtl-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 5;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2-Language.jpg", sc,
                new Rectangle(182, 303, 257, 81), // fr
                new Rectangle(841, 299, 253, 85), // en
                null)
            {
                Parent = screenPanel,
                QuestionLanguageNo = 2
            };
            _screens.Add(langScreen);

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "3-Sharing_option.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(214, 191, 110, 110), // Mail
                new Rectangle(955, 190, 110, 110), // SMS
                null, // facebook
                null, // twitter
                new Rectangle(544, 515, 190, 61), // End
                80, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 7, 90, 68)
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4-E-mail_EN.jpg", dirUi + "4-E-mail_FR.jpg", sc, 1,
                new Rectangle(400, 65, 777, 38), //textbox
                new Rectangle(544, 171, 192, 59), // ok button
                new KeyboardControl(44, 250, 1202, 378)
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "5-SMS_EN.jpg", dirUi + "5-SMS_FR.jpg", sc, 2,
                new Rectangle(540, 60, 623, 52),
                new Rectangle(538, 160, 208, 77),
                new NumbersKeyboardControl(44, 250, 1202, 378)
            );
            _screens.Add(sms);
        }

        #endregion

        #region SquareEnix Tomb Raider AirCam KEN-2018-01-LA (PS + Fusion)

        private void Init_KEN_2018_01_LA_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1021, 964, user)
            {
                ProjectNo = "gat48lsnp1",
                EventId = 924,
                ForceFileCheck = true,
                ProjectDescription = "Kenwood 2018-01 PS (LA)",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4,
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "ken-2018-01-la-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 5;

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "3-Sharing_option.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(214, 191, 110, 110), // Mail
                new Rectangle(955, 190, 110, 110), // SMS
                null, // facebook
                null, // twitter
                    new Rectangle(544, 515, 190, 61), // End
                    80, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 7, 90, 68)
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4-E-mail_EN.jpg", null, sc, 1,
                new Rectangle(400, 65, 777, 38), //textbox
                new Rectangle(544, 171, 192, 59), // ok button
                new KeyboardControl(44, 250, 1202, 378),
                new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(152, 116, 24, 24), 6, 1, false,new Point(151,115))

                }
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "5-SMS_EN.jpg", null, sc, 2,
                new Rectangle(540, 60, 623, 52),
                new Rectangle(538, 160, 208, 77),
                new NumbersKeyboardControl(44, 250, 1202, 378)
            );
            _screens.Add(sms);
        }

        #endregion

        #region Cayman Jack's(PS + Fusion)

        private void Init_Cayman_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1025, 967, user)
            {
                ProjectNo = "akjgbtnwj1",
                EventId = 927,
                ForceFileCheck = true,
                ProjectDescription = "Cayman Jack 2018 PS (Fusion)",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "g7-2018-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 5;

            // Écran 4 - Sharing
            var share = new ShareScreen(dirUi + "2-sharing.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(241, 193, 110, 110), // Mail
                new Rectangle(927, 192, 110, 110), // SMS
                null, // facebook
                null,
                new Rectangle(522, 537, 234, 67), // End
                85, new Size(408, 408)) // Preview
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1187, 7, 90, 68),
                SaveSelectedPictureToQuestionNo = 1
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "3-e-mail.jpg", null, sc, 1,
                new Rectangle(427, 70, 787, 50), //textbox
                new Rectangle(523, 205, 232, 68), // ok button
                new KeyboardControl(-1, 275, 1248, 340),
                new List<ScreenCheckOption>
                {
                   new ScreenCheckOption(new Rectangle(136, 127, 28, 28), 4, 1, false, new Point(135, 125))
                }

                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "4-sms.jpg", null, sc, 2,
                new Rectangle(585, 68, 621, 53),
                new Rectangle(523, 208, 234, 67),
                new NumbersKeyboardControl(-1, 275, 374, 340)
            );
            _screens.Add(sms);
        }

        #endregion

        #region Sensix AirCam SEN-2018-01 (PS + Fusion)

        private void Init_SEN_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1053, 990, user)
            {
                ProjectNo = "gfvna7jea1",
                EventId = 938,
                ForceFileCheck = true,
                ProjectDescription = "Sensix 2018-01 PS",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "sen-2018-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 5;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2-language.jpg", sc,
                new Rectangle(182, 303, 257, 81), // fr
                new Rectangle(841, 299, 253, 85), // en
                null)
            {
                Parent = screenPanel,
                QuestionLanguageNo = 2
            };
            _screens.Add(langScreen);

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "3-sharing options.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(214, 191, 110, 110), // Mail
                new Rectangle(955, 190, 110, 110), // SMS
                null, // facebook
                null, // twitter
                new Rectangle(544, 515, 190, 61), // End
                80, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 7, 90, 68)
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4-email_EN.jpg", dirUi + "4-email_FR.jpg", sc, 1,
                new Rectangle(400, 65, 777, 38), //textbox
                new Rectangle(544, 171, 192, 59), // ok button
                new KeyboardControl(44, 250, 1202, 378)
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "5-SMS_EN.jpg", dirUi + "5-SMS_FR.jpg", sc, 2,
                new Rectangle(540, 60, 623, 52),
                new Rectangle(538, 160, 208, 77),
                new NumbersKeyboardControl(44, 250, 1202, 378)
            );
            _screens.Add(sms);
        }

        #endregion
        
        #region One Third Lab 2018 01

        private void One_Third_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1071, 1001, user)
            {
                ProjectNo = "01z8sxryh1",
                EventId = 947,
                ForceFileCheck = true,
                ProjectDescription = "One Third Lab  2018",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "one-2018-01b-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2. Language selection.jpg", sc,
                new Rectangle(230, 342, 345, 144), // fr
                new Rectangle(706, 345, 340, 110), // en
                new Rectangle(1186,0,94,75))
            {
                Parent = screenPanel,
                QuestionLanguageNo = 7
            };
            _screens.Add(langScreen);

            PostQuestionnaireRespondantNoQuestion = 5;
            
            // Écran 3 - Disclaimer
            var terms = new Ui.generic.SectionSkipScreen(dirUi + "3. Disclaimer EN.jpg", dirUi + "3. Disclaimer FR.jpg", sc,new Rectangle(1197,0,83,65)) // cancel
                {
                    Parent = screenPanel,
                    CurrentRespondentSource = QuestionnaireController,
                    Zones = new[]
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(1164, 3, 118, 92),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(470, 517, 342, 109),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                        }
                    }
                };
            _screens.Add(terms);

            var questionnaire = new OneThirdLab(dirUi + "4. Questionnaire EN.jpg", dirUi + "4. Questionnaire FR.jpg", sc) // cancel
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(questionnaire);

            var questionnaireSuivant = new OneThirdLabCheckBoxScreen(dirUi + "5. Opt-ins EN.jpg", dirUi + "5. Opt-ins FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                CheckBoxZone = new []
                {
                    new ScreenCheckOption(new Rectangle(60, 151, 23, 26), 5, 1, false, new Point(60, 150)),
                    new ScreenCheckOption(new Rectangle(60, 210, 23, 24), 8, 1, false, new Point(60, 210)),
                    new ScreenCheckOption(new Rectangle(60, 269, 23, 24), 6, 1, false, new Point(60, 268))
                },
                ZoneList = new List<SectionSkipScreenData>
                {
                    new SectionSkipScreenData()
                    {
                        SkipValidation = true,
                        Zone = new Rectangle(366, 269, 255, 30),
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions()
                    },
                    new SectionSkipScreenData()
                    {
                        SkipValidation = true,
                        Zone = new Rectangle(470, 517, 341, 110),
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(1000)
                    }
                }
            };
            _screens.Add(questionnaireSuivant);
            
            var termsAndConditions = new Ui.generic.SectionSkipScreen(dirUi + "5a.  Contest rules and regs.jpg", dirUi + "5a.  Contest rules and regs.jpg", sc,null,new Rectangle(205,79,840,541))
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                Zones =  new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(1182, 2, 92, 82),
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()

                    }
                },
                RuleAndRegsFileEn = baseDir + "concours EN diner en blanc REGS.txt",
                RuleAndRegsFilefr = baseDir + "concours FR diner en blanc REGS.txt"

            };
            _screens.Add(termsAndConditions);

        }

        #endregion

        #region YHP 2018 01

        private void YHP_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1080, 1008, user)
            {
                ProjectNo = "8147kixrj1",
                EventId = 954,
                ForceFileCheck = true,
                ProjectDescription = "Maizex Elite 2018 01",
                QuestionEmailNo = 7,
                QuestionSmsNo = 6
                
            };

            

           
            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "yhp-2018-01b-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);


            PostQuestionnaireRespondantNoQuestion = 8;

           

            var questionnaireSuivant = new MaizexEliteQuestionnaire(dirUi + "2-QUESTIONNAIRE.jpg", dirUi + "2-QUESTIONNAIRE.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(questionnaireSuivant);

            var termsAndConditions = new Ui.generic.SectionSkipScreen(dirUi + "3-OPT-IN CONCOURS.jpg", dirUi + "3-OPT-IN CONCOURS.jpg", sc, null, new Rectangle(146, 33, 973, 466))
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                Zones = new[]
                {
                    new SectionSkipScreenData
                    {
                        Zone = new Rectangle(508, 581, 264, 58),
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicNextOptions(1000)

                    }
                },
                CheckBoxZones = new[]
                {
                    new ScreenCheckOption(new Rectangle(184, 529, 24, 26), 9, 1, false, new Point(185, 530))
                },
                RuleAndRegsFilefr = baseDir + "règlements concours.txt",
                RuleAndRegsFileEn = baseDir + "règlements concours.txt"

            };
            _screens.Add(termsAndConditions);

        }

        #endregion

        #region ESH 2018 01 Mercedez-Benz (PS + Fusion)

        private void ESH_2018_03_PS(Panel screenPanel, PrintStationUser user)  
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1085, 1013, user)
            {
                ProjectNo = "r5rca1ngl1",
                EventId = 959,
                ForceFileCheck = true,
                ProjectDescription = "Mercedez-Benz 2018 03",
                QuestionEmailNo = 1,
                QuestionSmsNo = 3
               ,
                
                QuestionFirstPictureNo = 8

            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "Mercedez-esh-2018-03b-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);


            PostQuestionnaireRespondantNoQuestion = 4;


            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);


            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);

            /*
            // Écran 3 - Sharing
            var share = new MercedezShareScreen(dirUi + "2. Sharing.jpg", null, sc,
                    facebookShare, null, null, null, null,
                    new Rectangle(283, 203, 114, 107), // Mail
                    new Rectangle(890, 202, 110, 107), // SMS
                    new Rectangle(-1,-1,0,0), // facebook
                    new Rectangle(-1,-1,0,0), // twitter
                    new Rectangle(542, 562, 193, 63), // End
                    54, new Size(332, 490)) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1171, 4, 101, 59),
                    SaveSelectedPictureToQuestionNo = 7,
                    CurrentRespondentSource = QuestionnaireController
                };
            _screens.Add(share);
            #1#

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "3. Email.jpg", null, sc, 1,
                new Rectangle(342, 64, 615, 48), //textbox
                new Rectangle(542, 169, 195, 61), // ok button
                new KeyboardControl(-1, 260, 1202, 300)
            );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "4. SMS.jpg", null, sc, 2,
                new Rectangle(442, 74, 399, 43),
                new Rectangle(545, 168, 190, 63),
                new NumbersKeyboardControl(-1, 260, 399, 352)
            );
            _screens.Add(sms);

        }

        #endregion

        #region T1 2018 01 Lotto-Max (PS + Fusion)

        private void T1_2018_03_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1095, 1020, user)
            {
                ProjectNo = "94jg3ele51",
                EventId = 966,
                ForceFileCheck = true,
                ProjectDescription = "LOTTO-MAX 2018 03",
                QuestionEmailNo = 1,
                QuestionSmsNo = 2,
                QuestionNumberOfPrintNo = 6,
                QuestionPictureToPrintNo = 3,
                QuestionFirstPictureNo = 4
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "LottoMax-t1-2018-03b-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            PostQuestionnaireRespondantNoQuestion = 5;

            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);


            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);

            /*
            // Écran 1 - Sharing
            var share = new LottoMaxShareScreen(dirUi + "2-sharing.jpg", null, sc,
                    facebookShare, null, null, null,
                    new Rectangle(1142,353,145,147), // print
                    new Rectangle(378, 356, 159, 144), // Mail
                    null, // SMS
                    new Rectangle(-1, -1, 0, 0), // facebook
                    new Rectangle(-1, -1, 0, 0), // twitter
                    new Rectangle(680, 791, 318, 73), // End
                    281, new Size(483, 483)) // Preview
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(-1, -1, -1, -1),
                SaveSelectedPictureToQuestionNo = 3,
                CurrentRespondentSource = QuestionnaireController,
                ApplyTransformationToSelectedImage = true,
                DontUseInactiveImages = true   // ignore images par defaut
            };
            _screens.Add(share);
            #1#

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4-e-mail.jpg", null, sc, 1,
                new Rectangle(405, 245, 876, 32), //textbox
                new Rectangle(1527, 3, 150, 111), // ok button
                new KeyboardControl(138, 375, 1400, 395)
            );
            _screens.Add(mail);
        }

        #endregion

        #region Hospital_Fundation_Oakville STE-2018-01

        private void InitHospital_STE_2018_01(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1096, 1021, user)
            {
                ProjectNo = "nlrob3lvw1",
                EventId = 967,
                ForceFileCheck = true,
                ProjectDescription = "Hospital Foundation (Printstation)",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3,
                QuestionFirstPictureNo = 6,
                QuestionPictureToPrintNo = 1,
                QuestionNumberOfPrintNo = 7,
                
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "ste-2018-01b-desktop.xml", "1.0");

            #region Share screen
            // 1- Écran Share
            var share = new HospitalFoundationShareScreen(dirUi + "2-sharing.jpg", null, sc,
                    null, null, null, null,
                    new Rectangle(1185, 196, 150, 132), // print
                    new Rectangle(344, 196, 150, 132), // Mail
                    null, // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(680, 732, 324, 80), // End
                    81, new Size(622, 570)) // Preview
                {
               
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1199, 1, 79, 60),
                    CurrentRespondentSource = QuestionnaireController,
                    DontUseInactiveImages = true,
                    ApplyTransformationToSelectedImage = true,   // necessaire pour appliquer le Frame pour l'impression
                    SaveSelectedPictureToQuestionNo = 1
                };
            _screens.Add(share);
            #endregion

            #region Mail
            var optins = new List<ScreenCheckOption>
            {
                new ScreenCheckOption(new Rectangle(146, 122, 992, 38), 4, 1, false, new Point(157, 113))
            };
            var clavier = new KeyboardControl(-1, 357, 1176, 332);

            var mail = new MailScreen(dirUi + "4-email.jpg", null, sc, 1,
                new Rectangle(264, 137, 1155, 79), // Textbox
                new Rectangle(681, 246, 320, 80), // Previous
                clavier, // keyboard
                optins // Opt-ins
            )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);
            #endregion

            #region Sms

            var clavier2 = new NumbersKeyboardControl(-1, 287, 432, 330);
            var smsOpt = new SmsScreen(dirUi + "3-sms.jpg", null, sc, 2,
                new Rectangle(264, 136, 1118, 82),
                new Rectangle(1533, 0, 146, 116),
                clavier2)
            {
                Parent = screenPanel
            };
            _screens.Add(smsOpt);

            #endregion
        }

        #endregion
        
        #region Hunter Disney Mary Poppins HUN-2018-01b (PS + Fusion)

        private void Init_HUN_2018_01b(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1100, 1025, user)
            {
                ProjectNo = "onfhkkqq71",
                EventId = 970,
                ForceFileCheck = true,
                ProjectDescription = "Hunter Disney Marie Poppins PS",
                QuestionEmailNo = 1,
                QuestionSmsNo = 3,
                QuestionNumberOfPrintNo = 5,
                QuestionPictureToPrintNo = 7
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "hun-2018-01b-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 4;

            // Écran 2 - Sharing
            var share = new ShareScreen(dirUi + "2. Sharing and printing.jpg", null, sc,
                null, null, null, null,
                new Rectangle(932,237,104,104), // Print
                new Rectangle(244,173,104,104), // Mail
                new Rectangle(244,299,104,104), // SMS
                null, // facebook
                null, // twitter
                new Rectangle(561,534,165,65), // End
                88, new Size(400,400)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 6,
                CancelArea = new Rectangle(1220, 5, 50, 50),
                DontUseInactiveImages = true,
                ApplyTransformationToSelectedImage = true,
                MailJump = 1,
                SmsJump = 2
            };
            _screens.Add(share);

            // Écran 3 - Mail
            var mail = new MailScreen(dirUi + "3. Email.jpg", null, sc, 1,
                new Rectangle(402,80,477,35), //textbox
                new Rectangle(561,534,165,65), // ok button
                new UsaKeyboardControl(-1,139,1162,360)
                )
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

            // Écran 4 - SMS
            var sms = new SmsScreen(dirUi + "4. SMS.jpg", null, sc, 2,
                new Rectangle(402, 80, 477, 35),
                new Rectangle(561, 534, 165, 65),
                new NumbersKeyboardControl(-1, 139, 360, 360)
                )
            {
                Parent = screenPanel
            };
            _screens.Add(sms);
        }

        #endregion

        #region Ignition 180 AirCam IGN-2018-01 (PS + Fusion)

        private void Init_IGN_2018_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1102, 1027, user)
            {
                ProjectNo = "7lek19les1",
                EventId = 972,
                ForceFileCheck = true,
                ProjectDescription = "Ignition 2018-01 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "ign-2018-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 4;
            
            // Écran 2 - Disclaimer
            var disclaimer = new Ui.generic.SectionSkipScreen(dirUi + "2. Disclaimer.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(disclaimer);

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "3. Sharing.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(214, 191, 110, 110), // Mail
                new Rectangle(955, 190, 110, 110), // SMS
                null, // facebook
                null, // twitter
                    new Rectangle(544, 515, 190, 61), // End
                    80, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 7, 90, 68)
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4a. Email.jpg", null, sc, 1,
                new Rectangle(400, 65, 777, 38), //textbox
                new Rectangle(544, 171, 192, 59), // ok button
                new KeyboardControl(44, 250, 1202, 378),
                new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(152, 116, 24, 24), 6, 1, false,new Point(151,115))

                }
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "4b. SMS.jpg", null, sc, 2,
                new Rectangle(540, 60, 623, 52),
                new Rectangle(538, 160, 208, 77),
                new NumbersKeyboardControl(44, 250, 1202, 378)
            );
            _screens.Add(sms);
        }

        #endregion

        #region 24 Heures Tremblant (Fondation Charles-Bruneau)

        private void Init_FCB2018_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1104, 1029, user)
            {
                ProjectNo = "a6c8qlu3b1",
                EventId = 974,
                ForceFileCheck = true,
                ProjectDescription = "Fondation Charles-Bruneau 2018-01 PS",
                QuestionEmailNo = 3,
                QuestionSmsNo = 2,
                QuestionPictureToPrintNo = 1,
                QuestionNumberOfPrintNo = 5
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "cha-2018-01-ps-desktop.xml", "1.0");
            
            // Écran Sharing
            var share = new ShareScreen(dirUi + "1-sharing.jpg", null, sc, null, null, null, null,
                    new Rectangle(1005, 193, 100, 100), // print
                    new Rectangle(1005, 465, 100, 100), // Mail
                    new Rectangle(1005, 329, 100, 100), // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(542, 582, 194, 61), // End
                    193, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                DontUseInactiveImages = true,
                CancelArea = new Rectangle(17, 9, 354, 175)
            };
            _screens.Add(share);

            // Écran Mail
            var mail = new MailScreen(dirUi + "2-e-mail.jpg", null, sc, 1,
                new Rectangle(365, 200, 794, 36), //textbox
                new Rectangle(547, 319, 185, 56), // ok button
                new KeyboardControl(-1, 391, 1097, 272),
                new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(111, 259, 810, 31), 4, 1, false)
                }
                );
            _screens.Add(mail);

            // Écran SMS
            var sms = new SmsScreen(dirUi + "3-sms.jpg", null, sc, 2,
                new Rectangle(513, 200, 646, 36), //textbox
                new Rectangle(547, 319, 185, 56), // ok button
                new NumbersKeyboardControl(-1, 391, 556, 272)
            );
            _screens.Add(sms);
        }

        #endregion

        #region Agropur

        private void Init_AGR201801_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1108, 1033, user)
            {
                ProjectNo = "c5yle7zqh1",
                EventId = 978,
                ForceFileCheck = true,
                ProjectDescription = "Agropur 2018-01 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3,
                QuestionPictureToPrintNo = 1,
                QuestionNumberOfPrintNo = 8
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "agr-2018-01b-desktop.xml", "1.0");

            // Écran Sharing
            var share = new ShareScreen(dirUi + "2-sharing.jpg", null, sc, null, null, null, null,
                    new Rectangle(951, 159, 78, 78), // print
                    new Rectangle(951, 295, 78, 78), // Mail / Next
                    null,
                    null, // facebook
                    null, // twitter
                    new Rectangle(547, 456, 185, 56), // End
                    42, new Size(575, 383)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                DontUseInactiveImages = true,
                CancelArea = new Rectangle(1210, 10, 60, 60)
            };
            _screens.Add(share);

            // Écran Questionnaire
            var mail = new Agropur2018Questionnaire(dirUi + "3-questions.jpg", null, sc)
            {
                Parent = screenPanel
            };
            _screens.Add(mail);

        }

        #endregion
        
        #region Spotium-2019-01 (PS + Fusion)

        private void Sportium_IGN_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1131, 1046, user)
            {
                ProjectNo = "8ijccvty51",
                EventId = 989,
                ForceFileCheck = true,
                ProjectDescription = "Sportium 2019-01 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "car-2019-01-ps-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            PostQuestionnaireRespondantNoQuestion = 4;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2. choix de langue.jpg", sc,
                new Rectangle(182, 303, 253, 79), // fr
                new Rectangle(841, 303, 253, 82), // en
                new Rectangle(1186,0,93,75))
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                QuestionLanguageNo = 5
            };
            _screens.Add(langScreen);
            
            // Écran 2 - Disclaimer
           /* var disclaimer = new Ui.generic.SectionSkipScreen(dirUi + "2. Disclaimer.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(disclaimer);#1#

            // Écran 3 - Sharing
            /*var share = new Ui.generic.ShareScreen(dirUi + "3. Sharing.jpg", null, sc,
                null, null, null, null, null,
                null,//new Rectangle(214, 191, 110, 110), // Mail
                null,//new Rectangle(955, 190, 110, 110), // SMS
                null, // facebook
                null, // twitter
                    new Rectangle(544, 515, 190, 61), // End
                    80, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 7, 90, 68),
                CurrentRespondentSource = QuestionnaireController,
                PreviewPictureQuestion = 1
            };
            _screens.Add(share);#1#

            // Écran 4a - Mail
            /*var mail = new MailScreen(dirUi + "4a. Email.jpg", null, sc, 1,
                new Rectangle(400, 65, 777, 38), //textbox
                new Rectangle(544, 171, 192, 59), // ok button
                new KeyboardControl(44, 250, 1202, 378),
                new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(152, 116, 24, 24), 6, 1, false,new Point(151,115))

                }
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "4b. SMS.jpg", null, sc, 2,
                new Rectangle(540, 60, 623, 52),
                new Rectangle(538, 160, 208, 77),
                new NumbersKeyboardControl(44, 250, 1202, 378)
            );
            _screens.Add(sms);#1#

            var questionnaire = new SportiumQuestionnaireScreen(dirUi + "3-questionnaire_EN.jpg", dirUi+"3-questionnaire_FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(114, 236, 29, 29), 6, 1, false,new Point(120,235))/*,
                    new ScreenCheckOption(new Rectangle(858, 358, 40, 38), 16, 1, false,new Point(863,361)),
                    new ScreenCheckOption(new Rectangle(863, 405, 39, 36), 17, 1, false,new Point(863,405))#1#
                },
                ZoneList = new List<SectionSkipScreenData>(){
                    new SectionSkipScreenData
                    {
                        // disclaimer
                        PageSkip = 1,
                        Zone = new Rectangle(new Point(837, 232), new Size(277,21)),
                        SkipValidation = false
                    },
                    new SectionSkipScreenData
                    { // next 
                        PageSkip = 2,
                        Zone = new Rectangle(new Point(541, 573), new Size(198,64)),
                        SkipValidation = false
                    }
                    
                 }

            };
            _screens.Add(questionnaire);

            var disclaimer = new Ui.generic.SectionSkipScreen(dirUi + "4. Policy privacy et politque de confidentialité.jpg",dirUi + "4. Policy privacy et politque de confidentialité.jpg", sc,null,new Rectangle(154,112,946,431))
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
              
                Zones = new []{
                    new SectionSkipScreenData
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                        Zone = new Rectangle(new Point(1204, 0), new Size(76,76)),
                        SkipValidation = true
                    }
                },
                RuleAndRegsFileEn = baseDir+"privacy policy sportium.txt",
                RuleAndRegsFilefr = baseDir+"politique de confidentialité sportium.txt"
            };
            _screens.Add(disclaimer);

            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "5. Thank you_EN.jpg", sc, dirUi + "5. Merci_FR.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);


        }

        #endregion

        #region ESH 2019 01 Mercedez-Benz (PS + Fusion)

        private void ESH_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1115, 1038, user)
            {
                ProjectNo = "fulp1po1x1",
                EventId = 983,
                ForceFileCheck = true,
                ProjectDescription = "Mercedez-Benz 2019 01",
                QuestionEmailNo = 1,
                QuestionSmsNo = 3,

                QuestionFirstPictureNo = 8

            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "Mercedez-esh-2019-01b-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);


            PostQuestionnaireRespondantNoQuestion = 4;
            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);

            // Écran 3 - Sharing
            var share = new Mercedez2019ShareScreen(dirUi + "2. Sharing.jpg", null, sc,
                    facebookShare, null, null, null, null,
                    new Rectangle(199, 202, 108, 108), // Mail
                    new Rectangle(973, 202, 110, 105), // SMS
                    new Rectangle(-1, -1, 0, 0), // facebook
                    new Rectangle(-1, -1, 0, 0), // twitter
                    new Rectangle(542, 562, 185, 59), // End
                    91, new Size(536, 403)) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1171, 4, 101, 59),
                    SaveSelectedPictureToQuestionNo = 7,
                    CurrentRespondentSource = QuestionnaireController
                };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "3. Email.jpg", null, sc, 1,
                new Rectangle(342, 64, 615, 48), //textbox
                new Rectangle(542, 169, 195, 61), // ok button
                new KeyboardControl(-1, 260, 1202, 300));
            _screens.Add(mail);

            var sms = new SmsScreen(dirUi + "4. SMS.jpg", null, sc, 2,
                new Rectangle(442, 74, 399, 43),
                new Rectangle(545, 168, 190, 63),
                new NumbersKeyboardControl(-1, 260, 399, 352));
            _screens.Add(sms);

        }
        #endregion

        #region TIH 2019 01 Carte-Hockey (PS + Fusion)

        private void TIH_2019_01_PS(Panel screenPanel, PrintStationUser user)  
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1128, 1043, user)
            {
                ProjectNo = "81gzl19c31",
                EventId = 986,
                ForceFileCheck = true,
                ProjectDescription = "Carte Hockey 2019 01",
                QuestionEmailNo = 6,
                QuestionPictureToPrintNo = 11,
                QuestionSmsNo = 7/*,             
                QuestionFirstPictureNo = 12#1#


            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "TIH-2019-01b-desktop.xml", "1.0");
            
            PostQuestionnaireRespondantNoQuestion = 13;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2. Choix de langue.jpg", sc,
                new Rectangle(182, 303, 255, 81), // fr
                new Rectangle(843, 305, 250, 76), // en
                new Rectangle(1177,3,102,84))
            {
                Parent = screenPanel,
                QuestionLanguageNo = 10
            };
            _screens.Add(langScreen);


            //Ecran opt Ins

            var optInScreen = new OptInScreen(dirUi + "2.1 Opt-in disclaimer EN.jpg",
                dirUi + "2.1 Opt-in disclaimer FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(109, 118, 23, 25), 8, 1, false,new Point(100,103)),
                    new ScreenCheckOption(new Rectangle(109, 174, 23, 24), 9, 1, false,new Point(100,160))
                }
              
            };
            _screens.Add(optInScreen);



            var questionnaire = new CarteHockeyQuestionnaireScreen(dirUi + "3. Personnalisation EN.jpg",dirUi + "3. Personnalisation FR.jpg",sc);
            _screens.Add(questionnaire);
       


            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);


            /* Classe de partages utilisant l'écran web #1#
            var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);

            var transforms = new ImageListTransformOptions
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick> { 
                    // Si quotation personalisée inexistante
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "carte_hokey_two_steps.txt",
                        ResultMapToQuestionnaire = new Dictionary<string, int>
                        {
                            {"impression", 11 },{"recto",12}
                        },
                        Timeout = TimeSpan.FromSeconds(20)
                    }/*,
                    // Si avec quotation personalisée
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "imgMgk_trans_02.txt",
                        ResultMapToQuestionnaire = new Dictionary<string, int>() { {"1", 3 } }
                    }#1#
                }
            };

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "4. Sharing.jpg", dirUi + "4. Sharing.jpg", sc,
                    facebookShare, null, null, null, null,
                    new Rectangle(285, 194, 105, 106), // Mail
                    new Rectangle(891, 194, 105, 105), // SMS
                    new Rectangle(-1,-1,0,0), // facebook
                    new Rectangle(-1,-1,0,0), // twitter
                    new Rectangle(542, 562, 193, 63), // End
                    36, new Size(372, 495),true) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1171, 4, 101, 59),
                    CurrentRespondentSource = QuestionnaireController,
                    ImageListTranformOptions = transforms,
                    PicturesToTransform = new List<int>(){1},
                    SaveOriginalImageToQuestion = 1,
                    AutoPrint = true,
                    PreviewPictureQuestion = 12
                    
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "6. Email EN.jpg", dirUi +"6. Email FR.jpg", sc, 1,
                new Rectangle(375, 76, 536, 41), //textbox
                new Rectangle(542, 166, 195, 68), // ok button

                new KeyboardControl(-1, 260, 1202, 300)
            );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "5.SMS EN.jpg", dirUi +"5.SMS FR.jpg", sc, 2,
                new Rectangle(375, 76, 536, 41),
                new Rectangle(545, 168, 195, 68),

                new NumbersKeyboardControl(-1, 260, 399, 352)
            );
            _screens.Add(sms);

        }

        #endregion
        
        #region Fondation Santé (SOP-2019-01)

        private void Init_SOP201901_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1133, 1048, user)
            {
                ProjectNo = "lfi60uako1",
                EventId = 991,
                ForceFileCheck = true,
                ProjectDescription = "Fontaine Santé 2019-01 PS",
                QuestionEmailNo = 3,
                QuestionSmsNo = 4,
                QuestionFirstPictureNo = 7,
                QuestionPictureToPrintNo = 7,
                QuestionNumberOfPrintNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "sop-2019-01-ps-desktop.xml", "1.0");

            // Écran Sharing
            var share = new ShareScreen(dirUi + "1-sharing.jpg", null, sc, null, null, null, null,
                    new Rectangle(335, 272, 125, 125), // print
                    new Rectangle(1215, 272, 125, 125), // Next
                    null,
                    null, // facebook
                    null, // twitter
                    new Rectangle(707, 712, 262, 61), // End
                    55, new Size(400, 600)) // Preview
                {
                    Parent = screenPanel,
                    SaveSelectedPictureToQuestionNo = 7,
                    DontUseInactiveImages = true,
                    CancelArea = new Rectangle(1210, 10, 60, 60),
                    CurrentRespondentSource = QuestionnaireController,
                    AutoPrint = false,
                    PreviewPictureQuestion = 7
            };
            _screens.Add(share);

            // Écran Scan RFID
            var rfid = new Ui.generic.RfidScannerScreen(dirUi + "2-scan.jpg", null, sc, new Rectangle(585, 180, 504, 50))
            {
                Parent = screenPanel,
                WaitSequence = null,
                WebFacadeConfigFile = "facade_config.json",
                ValidateRfid = new Rectangle(1207, 481, 377, 321),
                ScanQuestionNo = 8,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(rfid);

            // Écran Questionnaire
            var questionnaire = new Questionnaire_SOP201901(dirUi + "3-confirm info.jpg", null, sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(907, 320, 123, 30), 5, 1, false,new Point(1002, 323)), // courriel
                    new ScreenCheckOption(new Rectangle(1036, 320, 102, 30), 6, 1, false,new Point(1111, 323)) // sms
                },
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(questionnaire);
        }

        #endregion

        #region Norwegian Aircam (NOR-2019-01)

        private void Init_NOR201901_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1136, 1050, user)
            {
                ProjectNo = "aquzp96b41",
                EventId = 993,
                ForceFileCheck = true,
                ProjectDescription = "Norwegian 2019-01 PS",
                QuestionEmailNo = 2,
                QuestionFirstPictureNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "nor-2019-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 3;
            
            /* Écran web #1#
            const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);

            var twitterShare = new UploadPictureToTwitterLeNomadPseudomodal(sc, QuestionnaireController, webScreen, webSCreenTag)
            {
                ShareFromRespondentNo = true,
                RespondentNoQuestionNo = 3
            };

            // Écran Sharing
            var share = new ShareScreen(dirUi + "2. sharing.jpg", null, sc, null, twitterShare,
                    "Aboard The Ultimate Joy Ride! #NorwegianJOY 2019 U.S. Inaugural.", // Twitter EN
                    "Aboard The Ultimate Joy Ride! #NorwegianJOY 2019 U.S. Inaugural.", // Twitter FR
                    null, // print
                    new Rectangle(739, 143, 133, 133), // Mail
                    null,
                    null, // facebook
                    new Rectangle(411, 143, 133, 133), // twitter
                    new Rectangle(548, 442, 185, 57) // End
                    )
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1220, 5, 50, 50),
                CurrentRespondentSource = QuestionnaireController,
                AutoPrint = false,
                PreviewPictureQuestion = 1,
                EndJump = 2 // Va au Thanks Screen
            };
            _screens.Add(share);

            // Écran Mail
            var mail = new MailScreen(dirUi + "3. Email.jpg", null, sc, 1,
                new Rectangle(349, 93, 761, 35), //textbox
                new Rectangle(547,572,185,56), // ok button
                new KeyboardControl(-1, 153, 1200, 395)
                {
                    CustomLabelInfo = new CustomPaintLabelInfo()
                    {
                        Font = new Font(FontFamily.GenericSansSerif, 25, FontStyle.Bold)
        }
                }
            )
            {
                Parent = screenPanel,
                MoveForward = true,
                CurrentRespondentSource = QuestionnaireController,
                CancelArea = new Rectangle(1220, 5, 50, 50)
            };
            _screens.Add(mail);

            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "4. Thank you!.jpg", sc)
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);

        }

        #endregion

        #region 6Degrees 2019 01 6Degrees (PS + Fusion)

        private void Degrees6_2019_01_PS(Panel screenPanel, PrintStationUser user)  
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1146, 1059, user)
            {
                ProjectNo = "5iah5bozf1",
                EventId = 1002,
                ForceFileCheck = true,
                ProjectDescription = "6 Degrees 2019 01",
                QuestionEmailNo = 4,
                QuestionPictureToPrintNo = 3


            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "6DE-2019-01-ps-desktop.xml", "1.0");
            
            PostQuestionnaireRespondantNoQuestion = 8;


            //Ecran opt Ins

            /*var optInScreen = new OptInScreen(dirUi + "2.1 Opt-in disclaimer EN.jpg",
                dirUi + "2.1 Opt-in disclaimer FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(109, 118, 23, 25), 8, 1, false,new Point(100,103)),
                    new ScreenCheckOption(new Rectangle(109, 174, 23, 24), 9, 1, false,new Point(100,160))
                }
              
            };
            _screens.Add(optInScreen);#1#

            /*
            var questionnaire = new Degrees6QuestionnaireScreen(dirUi + "3-Questionnaire.jpg", null, sc)
            {
                Parent = screenPanel
            };
            _screens.Add(questionnaire);
            #1#

            /* Écran web #1#
            /*const string webSCreenTag = "web";
            var webScreen = new WebBrowserScreen(sc, new Size(1280, 800), new Size(1100, 400))
            {
                Parent = screenPanel
            };
            QuestionnaireController.AddPseudoModalScreen(webScreen, webSCreenTag);#1#


            /* Classe de partages utilisant l'écran web #1#
            //var facebookShare = new ShareUrlToFacebookPseudomodal(QuestionnaireController, webScreen, webSCreenTag);

            /*var transforms = new ImageListTransformOptions
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick> { 
                    // Si quotation personalisée inexistante
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "carte_hokey_two_steps.txt",
                        ResultMapToQuestionnaire = new Dictionary<string, int>
                        {
                            {"impression", 11 },{"recto",12}
                        },
                        Timeout = TimeSpan.FromSeconds(20)
                    },
                    // Si avec quotation personalisée
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "imgMgk_trans_02.txt",
                        ResultMapToQuestionnaire = new Dictionary<string, int>() { {"1", 3 } }
                    }
                }
            };#1#

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "4-Sharing.jpg",null, sc,
                    null, null, null, null, null,
                    null, // Mail
                    null, // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(641, 719, 378, 97), // End
                    145, new Size(402, 544),true) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1503, 96, 149, 125),
                    CurrentRespondentSource = QuestionnaireController,
                    AutoPrint = true,
                    ApplyTransformationToSelectedImage = false,
                    CropRectangle = new Rectangle(201,251,789,1100),
                    SaveSelectedPictureToQuestionNo = 3,
                    EndJump = 1
                    
                };
                _screens.Add(share);

            // Écran 4a - Mail
           /* var mail = new MailScreen(dirUi + "6. Email EN.jpg", dirUi +"6. Email FR.jpg", sc, 1,
                new Rectangle(375, 76, 536, 41), //textbox
                new Rectangle(542, 166, 195, 68), // ok button

                new KeyboardControl(-1, 260, 1202, 300)
            );
            _screens.Add(mail);#1#

            var thxScreen = new ThanksEndScreen(dirUi + "5-Thankyou.jpg", sc)
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);

          

        }

        #endregion

        #region MAT-2019-01 (PS + Fusion)

        private void MAT_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1155, 1067, user)
            {
                ProjectNo = "co5hhz5em1",
                EventId = 1010,
                ForceFileCheck = true,
                ProjectDescription = "MMs 2019-01 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "mat-2019-01-ps-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            PostQuestionnaireRespondantNoQuestion = 4;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2.Language.jpg", sc,
                new Rectangle(757, 285, 313, 86), // fr
                new Rectangle(209, 286, 310, 85), // en
                new Rectangle(1186,0,93,75))
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                QuestionLanguageNo = 5
            };
            _screens.Add(langScreen);
            
            // Écran 2 - Disclaimer
           /* var disclaimer = new Ui.generic.SectionSkipScreen(dirUi + "2. Disclaimer.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(disclaimer);#1#

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "3.Sharing.jpg", null, sc,
                null, null, null, null, null,
                new Rectangle(955, 245, 110, 110), // Mail
                new Rectangle(210, 248, 110, 110), // SMS
                null, // facebook
                null, // twitter
                    new Rectangle(540, 514, 195, 64), // End
                    110, new Size(379, 380)) // Preview
            {
                Parent = screenPanel,
                SaveSelectedPictureToQuestionNo = 1,
                CancelArea = new Rectangle(1187, 1, 100, 100),
                CurrentRespondentSource = QuestionnaireController,
                PreviewPictureQuestion = 1,
                EndJump =3
                
              
            };
            _screens.Add(share);

            // Écran 4a - Mail
            var mail = new MailScreen(dirUi + "4.Email EN.jpg", dirUi + "4.Email FR.jpg", sc, 1,
                new Rectangle(433, 72, 413, 35), //textbox
                new Rectangle(543, 165, 192, 67), // ok button
                new KeyboardControl(69, 266, 1141, 311)
                {
                    CustomLabelInfo = new CustomPaintLabelInfo()
                    {
                        TextBrush = new SolidBrush(Color.FromArgb(
                            253,206,2)),
                       ColorPalette = {Color.FromArgb(
                           140,91,2),Color.FromArgb(43,25,0)}
                    }
                }
                );
            _screens.Add(mail);

            // Écran 4b - SMS
            var sms = new SmsScreen(dirUi + "5.SMS EN.jpg", dirUi + "5.SMS FR.jpg", sc, 2,
                new Rectangle(433, 72, 413, 35),
                new Rectangle(543, 165, 192, 67),
                new NumbersKeyboardControl(396, 267, 488, 311)
            );
            _screens.Add(sms);

            /*var questionnaire = new SportiumQuestionnaireScreen(dirUi + "3-questionnaire_EN.jpg", dirUi+"3-questionnaire_FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(114, 236, 29, 29), 6, 1, false,new Point(120,235))/*,
                    new ScreenCheckOption(new Rectangle(858, 358, 40, 38), 16, 1, false,new Point(863,361)),
                    new ScreenCheckOption(new Rectangle(863, 405, 39, 36), 17, 1, false,new Point(863,405))#1#
               /* },
                ZoneList = new List<SectionSkipScreenData>(){
                    new SectionSkipScreenData
                    {
                        // disclaimer
                        PageSkip = 1,
                        Zone = new Rectangle(new Point(613, 232), new Size(235,21)),
                        SkipValidation = false
                    },
                    new SectionSkipScreenData
                    { // next 
                        PageSkip = 2,
                        Zone = new Rectangle(new Point(541, 573), new Size(198,64)),
                        SkipValidation = false
                    }
                    
                 }

            };
            _screens.Add(questionnaire);

            var disclaimer = new Ui.generic.SectionSkipScreen(dirUi + "4. Policy privacy et politque de confidentialité.jpg",dirUi + "4. Policy privacy et politque de confidentialité.jpg", sc,null,new Rectangle(154,112,946,431))
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
              
                Zones = new []{
                    new SectionSkipScreenData
                    {
                        PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                        Zone = new Rectangle(new Point(1204, 0), new Size(76,76)),
                        SkipValidation = true
                    }
                },
                RuleAndRegsFileEn = baseDir+"privacy policy sportium.txt",
                RuleAndRegsFilefr = baseDir+"politique de confidentialité sportium.txt"
            };
            _screens.Add(disclaimer);#1#

            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "6.Thank you!.jpg", sc, dirUi + "6.Merci!.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);


        }

        #endregion

        #region ESH 2019 04B Golf

        private void ESH_2019_04B_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1156, 1068, user)
            {
                ProjectNo = "uh6oufwhq1",
                EventId = 1011,
                ForceFileCheck = true,
                ProjectDescription = "Eshots Golf 2019",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3,
                QuestionPictureToPrintNo = 1,
                QuestionNumberOfPrintNo = 4,
                QuestionFirstPictureNo = 1
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "esh-2019-04b-desktop.xml", "1.0");
            
            // Écran 1 - Sharing
            var share = new ShareScreen(dirUi + "2-sharing.jpg", null, sc,
                    null, null, null, null, new Rectangle(1304,395,156,145), 
                    new Rectangle(225, 302, 155, 147), // Mail
                    new Rectangle(228, 501, 153, 149), // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(683, 779, 313, 66), // End
                    231, new Size(786, 483)) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1500, 8, 176, 141),
                    SaveSelectedPictureToQuestionNo = 1,
                    PreviewPictureQuestion = 1,
                    CurrentRespondentSource = QuestionnaireController,
                    EndJump = 3,
                    DontUseInactiveImages = true
                };
            _screens.Add(share);

            // Écran 2 - Mail
            var mail = new MailScreen(dirUi + "4-e-mail.jpg", null, sc, 1,
                new Rectangle(422, 242, 1134, 109), //textbox
                new Rectangle(732, 377, 212, 66), // ok button
                new KeyboardControl(71, 480, 1533, 525)
                {
                    CustomLabelInfo = new CustomPaintLabelInfo()
                    {
                        Font = new Font(FontFamily.GenericSansSerif, 30, FontStyle.Bold)
        }
                })
            {
                Parent = screenPanel
                    
            };
            _screens.Add(mail);

            // écran 3-  SMS
            var sms = new SmsScreen(dirUi + "3-sms.jpg", null, sc, 2,
                new Rectangle(422, 242, 1134, 109),
                new Rectangle(728, 378, 219, 60),
                new NumbersKeyboardControl(441, 480, 785, 543))
            {
                Parent = screenPanel
            };
            _screens.Add(sms);

            var end = new ThanksEndScreen(dirUi + "5-fin.jpg", sc)
            {
                Parent = screenPanel,
                InactivityTimerInterval = 5000
            };
            _screens.Add(end);
        }
        #endregion

        #region ESH 2019 03 Brewer B Golf

        private void ESH_2019_04_Brewer_B_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1159, 1071, user)
            {
                ProjectNo = "eheiwjho71",
                EventId = 1014,
                ForceFileCheck = true,
                ProjectDescription = "Eshots Brewers Baseball",
                QuestionEmailNo = 4,
                QuestionFirstPictureNo = 1,
                QuestionPictureToPrintNo = 11
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "esh-2019-03_bre_b-desktop.xml", "1.0");
            var cancelRect = new Rectangle(1568, 12, 100, 100);

            var eshotIdentifier = new EshotsIdentiferVerificationScreen(dirUi + "2-EmailAddress.jpg",null, sc)
            {
                IdentierType="Email",
                SaveIdentiferToQuestion=4,
                SaveGallerySelectedImageToQuestion=1,
                RequireCorrectCustomerId = false,
                IsCodeEmail = true,
                NameQuestion = 5,
                ContinueEvenWithError = true,
                EventDayIdQuestionNo = 3,
                CustomerIdQuestionNo = 2,
                WebFacadeConfigFile = "facade_config.json",
                CurrentRespondentSource = QuestionnaireController,
                CancelZone = cancelRect
            };
            _screens.Add(eshotIdentifier);

            var questionnaire = new GenericScreen(dirUi + "3-Personalise.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DebugWritePropertiesToFiles = false,
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "03_navi.json",
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(759,894,164,78),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Step = 1
                                    }
                                }
                            }
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(1568, 12, 100, 100),
                            SkipValidation = true,
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Cancel = true
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "03_textbox.json",
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(230, 203, 582, 71),
                            QuestionNo = 5,
                            Mandatory = true,
                            FieldName = "Name",
                            Modifications = TextModificationsEnum.Trim | TextModificationsEnum.ToUpper,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must enter your name." }
                                }}
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(230, 274, 582, 71),
                            QuestionNo = 6,
                            Mandatory = true,
                            FieldName = "HomeTown",
                            Modifications = TextModificationsEnum.Trim | TextModificationsEnum.ToUpper,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must enter your home town." }
                                }}
                            }
                        }
                    }
                },
                DefinitionComboxbox = new ScreenDefinitionComboBoxes
                {
                    ConfigFileName = "03_combo.json",
                    ComboboxDefinitions = new List<ComboboxDefinition>
                    {
                        new ComboboxDefinition
                        {
                            Position = new Rectangle(1040, 203, 582, 71),
                            QuestionNo = 8,
                            Mandatory = true,
                            FieldName = "Position",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must select your position." }
                                }}
                            },
                            DefaultIndex = 7,
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"en", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("FIRST BASEMAN", null),
                                    new Tuple<string, string>("SECOND BASEMAN", null),
                                    new Tuple<string, string>("SHORTSTOP", null),
                                    new Tuple<string, string>("THIRD BASEMAN", null),
                                    new Tuple<string, string>("LEFT FIELDER", null),
                                    new Tuple<string, string>("CENTER FIELDER", null),
                                    new Tuple<string, string>("RIGHT FIELDER", null),
                                    new Tuple<string, string>("PITCHER", null),
                                    new Tuple<string, string>("CATCHER", null)
                                } }
                            }
                        },
                        new ComboboxDefinition
                        {
                            Position = new Rectangle(1040, 274, 582, 71),
                            QuestionNo = 9,
                            Mandatory = true,
                            FieldName = "Bats",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must select your batting side." }
                                }}
                            },
                            DefaultIndex = 1,
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"en", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("LEFT", null),
                                    new Tuple<string, string>("RIGHT", null)
                                } }
                            }
                        },
                        new ComboboxDefinition
                        {
                            Position = new Rectangle(1040, 354, 582, 71),
                            QuestionNo = 10,
                            Mandatory = true,
                            FieldName = "Throws",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must select your pitching side." }
                                }}
                            },
                            DefaultIndex = 1,
                            TextValues = new Dictionary<string, IList<Tuple<string, string>>>
                            {
                                {"en", new List<Tuple<string, string>>
                                {
                                    new Tuple<string, string>("LEFT", null),
                                    new Tuple<string, string>("RIGHT", null)
                                } }
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "03_keyboard.json",
                    Position = new Rectangle(59, 419, 1563, 459)
                }
            };
            _screens.Add(questionnaire);

            var transforms = new ImageListTransformOptions
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick> {
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "brewers_script.txt",
                        ResultMapToQuestionnaire = new Dictionary<string, int>
                        {
                            {"impression", 11 },{"recto_out",12},{"verso_out",13}
                        },
                        Timeout = TimeSpan.FromSeconds(20)
                    }
                }
            };
            var share = new ShareScreen(dirUi + "4-Print.jpg", null, sc,
                    null, null, null, null,
                    null,
                    null, // Mail
                    null, // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(630, 885, 413, 99), // End
                    145, new Size(402, 544), true) // Preview
            {
                Parent = screenPanel,
                SaveOriginalImageToQuestion = 1,
                CancelArea = cancelRect,
                CurrentRespondentSource = QuestionnaireController,
                AutoPrint = true,
                ApplyTransformationToSelectedImage = false,
                ImageListTranformOptions = transforms,
                PreviewPictureQuestion = 12,
                EndJump = 1

            };
            _screens.Add(share);

            var end = new ThanksEndScreen(dirUi + "5-ThankYou.jpg", sc)
            {
                Parent = screenPanel,
                InactivityTimerInterval=5000
            };
            _screens.Add(end);
        }
        #endregion
        
        #region ESH 2019 03 Cardinals B Golf

        private void ESH_2019_04_Cardinals_B_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1161, 1073, user)
            {
                ProjectNo = "74epsshrt1",
                EventId = 1016,
                ForceFileCheck = true,
                ProjectDescription = "Eshots Cardinals Baseball",
                QuestionEmailNo = 4,
                QuestionFirstPictureNo = 1,
                QuestionPictureToPrintNo = 11
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "esh-2019-03_car_b-desktop.xml", "1.0");
            var cancelRect = new Rectangle(1568, 12, 100, 100);

            var eshotIdentifier = new EshotsIdentiferVerificationScreen(dirUi + "2-EmailAddress.jpg",null, sc)
            {
                IdentierType="Email",
                SaveIdentiferToQuestion=4,
                SaveGallerySelectedImageToQuestion=1,
                RequireCorrectCustomerId = false,
                IsCodeEmail = true,
                NameQuestion = 5,
                ContinueEvenWithError = true,
                EventDayIdQuestionNo = 3,
                CustomerIdQuestionNo = 2,
                WebFacadeConfigFile = "facade_config.json",
                CurrentRespondentSource = QuestionnaireController,
                CancelZone = cancelRect
            };
            _screens.Add(eshotIdentifier);

            var transforms = new ImageListTransformOptions
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick> {
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "cardinals_script.txt",
                        ResultMapToQuestionnaire = new Dictionary<string, int>
                        {
                            {"impression", 11 },{"recto_out",12},{"verso_out",13}
                        },
                        Timeout = TimeSpan.FromSeconds(20)
                    }
                }
            };

            var questionnaire = new BrewersQuestionnaireScreen(dirUi + "3-Personalise.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                CancelZone = cancelRect
            };
            _screens.Add(questionnaire);

            var share = new ShareScreen(dirUi + "4-Print.jpg", null, sc,
                    null, null, null, null,
                    null,
                    null, // Mail
                    null, // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(630, 885, 413, 99), // End
                    145, new Size(402, 544), true) // Preview
            {
                Parent = screenPanel,
                SaveOriginalImageToQuestion = 1,
                CancelArea = cancelRect,
                CurrentRespondentSource = QuestionnaireController,
                AutoPrint = true,
                ApplyTransformationToSelectedImage = false,
                ImageListTranformOptions = transforms,
                PreviewPictureQuestion = 12,
                EndJump = 1

            };
            _screens.Add(share);

            var end = new ThanksEndScreen(dirUi + "5-ThankYou.jpg", sc)
            {
                Parent = screenPanel,
                InactivityTimerInterval=5000
            };
            _screens.Add(end);
        }
        #endregion

        #region CAR-2019-02 (PS + Fusion)

        private void CAR_2019_02_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1165, 1076, user)
            {
                ProjectNo = "i5ommk8t61",
                EventId = 1019,
                ForceFileCheck = true,
                ProjectDescription = "Tangerine 2019-01 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "car-2019-02-ps-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            PostQuestionnaireRespondantNoQuestion = 4;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2. Language.jpg", sc,
                new Rectangle(196, 342, 325, 119), // fr
                new Rectangle(752, 342, 324, 119), // en
                new Rectangle(1186, 0, 93, 75))
            {
                Parent = screenPanel,
                QuestionLanguageNo = 5,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(langScreen);

            var questionnaire = new TangerineQuestionnaireScreen(dirUi + "3. Questionnaire EN.jpg", dirUi + "3. Questionnaire FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
         _screens.Add(questionnaire);

            var questionnaire2 = new TangerineQuestionnaireScreen2(dirUi + "4. Opt-in EN.jpg", dirUi + "4. Opt-in FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(97,49,36,36), 11, 1, false,new Point(98,50)),
                    new ScreenCheckOption(new Rectangle(97,139,36,36), 7, 1, false,new Point(98,141)),
                    new ScreenCheckOption(new Rectangle(97,226,36,36), 8, 1, false,new Point(98,227))
                },
                CurrentRespondentSource = QuestionnaireController

            };
            _screens.Add(questionnaire2);

            var questionnaire3 = new TangerineQuestionnaireScreen3(dirUi + "5. Choix de transmission EN.jpg", dirUi + "5. Choix de transmission FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(627,238,448,344), 9, 1, false,new Point(-100,-100)),
                    new ScreenCheckOption(new Rectangle(189,240,356,339), 10, 1, false,new Point(-100,-400))
                    
                },
                CurrentRespondentSource = QuestionnaireController

            };
            _screens.Add(questionnaire3);


            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "6. Thank you.jpg", sc, dirUi + "6. Merci.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);


        }

        #endregion
        
        #region CAR-2019-03 B (PS + Fusion)

        private void CAR_2019_03B(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1168, 1078, user)
            {
                ProjectNo = "ieqk1vs5p1",
                EventId = 1021,
                ForceFileCheck = true,
                ProjectDescription = "Tangerine Slo-Mo 2019-03 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "car-2019-03b-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 4;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2. Language.jpg", sc,
                new Rectangle(196, 342, 325, 119), // fr
                new Rectangle(752, 342, 324, 119), // en
                new Rectangle(1186, 0, 93, 75))
            {
                Parent = screenPanel,
                QuestionLanguageNo = 5,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(langScreen);

            var questionnaire = new TangerineQuestionnaireScreen(dirUi + "3. Questionnaire EN.jpg", dirUi + "3. Questionnaire FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
         _screens.Add(questionnaire);

            var questionnaire2 = new TangerineQuestionnaireScreen2(dirUi + "4. Opt-in EN.jpg", dirUi + "4. Opt-in FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(97,49,36,36), 11, 1, false,new Point(98,50)),
                    new ScreenCheckOption(new Rectangle(97,139,36,36), 7, 1, false,new Point(98,141)),
                    new ScreenCheckOption(new Rectangle(97,226,36,36), 8, 1, false,new Point(98,227))
                },
                CurrentRespondentSource = QuestionnaireController

            };
            _screens.Add(questionnaire2);

            var questionnaire3 = new TangerineQuestionnaireScreen3(dirUi + "5. Choix de transmission EN.jpg", dirUi + "5. Choix de transmission FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(627,238,448,344), 9, 1, false,new Point(-100,-100)),
                    new ScreenCheckOption(new Rectangle(189,240,356,339), 10, 1, false,new Point(-100,-400))
                    
                },
                CurrentRespondentSource = QuestionnaireController

            };
            _screens.Add(questionnaire3);


            // Thank you
            var thxScreen = new ThanksEndScreen(dirUi + "6. Thank you.jpg", sc, dirUi + "6. Merci.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);
        }

        #endregion

        #region STE-2019-01 Durex PS (PS + Fusion)

        private void STE_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(1170, 1080, user)
            {
                ProjectNo = "8ocyw4w1t1",
                EventId = 1023,
                ForceFileCheck = true,
                ProjectDescription = "Durex 2019-01 PS",
                QuestionPictureToPrintNo = 8
            };

            var uiDir = InitPrintStationQuestionnaire(screenPanel, sc, "ste-2019-01-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 9;

            // 1 - Choix de langue
            var langScreen = new Ui.generic.GenericScreen(uiDir + "1-Accueil.jpg", null,  sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DebugWritePropertiesToFiles = false,
                DefinitionLanguageZones = new ScreenDefinitionLanguageZones
                {
                    ConfigFileName = "01_languages.json",
                    SelectionZones = new List<LanguageZone>
                    {
                        new LanguageZone
                        {
                            LanguageCode = "en",
                            Zone = new Rectangle(1008, 456, 400, 140),
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer(QuestionTypeEnum.Combo, 1, 1, 1)
                            },
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Step = 1
                                    }
                                }
                            }
                        },
                        new LanguageZone
                        {
                            LanguageCode = "fr",
                            Zone = new Rectangle(272, 456, 400, 140),
                            AnswersUponSelection = new List<Answer>
                            {
                                new Answer(QuestionTypeEnum.Combo, 1, 1, 2)
                            },
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Step = 1
                                    }
                                }
                            }
                        }
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "01_navigation.json",
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            SkipValidation = true,
                            Zone = new Rectangle(1186, 0, 93, 75),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        ValidationTrigger = false,
                                        Cancel = true
                                    }
                                }
                            }
                        }
                    }
                }
            };
            _screens.Add(langScreen);

            var fightFor = new GenericScreen(uiDir + "2-Infos.jpg", uiDir + "2-Infos_fr.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DebugWritePropertiesToFiles = false,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "02_textboxes.json",
                    DefaultFontDefinition = new FontDefinitionInfo
                    {
                        Size = 18
                    },
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(415, 231, 980, 50),
                            FieldName = "Nom",
                            Mandatory = true,
                            QuestionNo = 3,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must enter your name." },
                                    {"fr", "Vous devez entrez votre nom." }
                                }}
                            },
                            Modifications = TextModificationsEnum.Trim
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(415, 321, 980, 50),
                            FieldName = "Courriel",
                            Mandatory = true,
                            QuestionNo = 4,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must enter your email." },
                                    {"fr", "Vous devez entrez votre courriel." }
                                }}
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            }
                        },
                        new TextboxDefinition
                        {
                            Position = new Rectangle(415, 406, 980, 50),
                            FieldName = "Sms",
                            Mandatory = true,
                            QuestionNo = 5,
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must enter your cell phone number." },
                                    {"fr", "Vous devez entrez votre numéro de portable." }
                                }}
                            },
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "02_keyboard.json",
                    Position = new Rectangle(-1, 472, 1460, 410)
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "02_navigation.json",
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(583, 892, 515, 90),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Step = 1
                                    }
                                }
                            }
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(1325, 33, 336, 112),
                            SkipValidation = true,
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Cancel = true
                                    }
                                }    
                            }
                        }
                    }
                }
            };
            _screens.Add(fightFor);

            //8- Opt-in
            var optIn = new GenericScreen(uiDir + "3-Disclaimer.jpg", uiDir + "3-Disclaimer_fr.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "03_navigation.json",
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(582, 892, 512, 90),
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Step = 1
                                    }
                                }
                            }
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(1325, 33, 336, 112),
                            SkipValidation = true,
                            PageChangeOptions = new ScreenChangeOptions
                            {
                                Options = new List<ScreenChangeOption>
                                {
                                    new ScreenChangeOption
                                    {
                                        Cancel = true
                                    }
                                }    
                            }
                        }
                    }
                },
                DefinitionsCheckBoxes = new ScreenDefinitionCheckBoxes
                {
                    ConfigFileName = "03_checks.json",
                    CheckboxDefinitions = new List<ScreenCheckOption>
                    {
                        new ScreenCheckOption // 18 ans (pas de question associée)
                        {
                            Mandatory = true,
                            Position = new Rectangle(158, 302, 602, 48),
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must state that you are adult." },
                                    {"fr", "Vous devez confirmer que vous êtes majeur." }
                                }}
                            }
                        },
                        new ScreenCheckOption // courriel
                        {
                            Mandatory = true,
                            QuestionNo = 7,
                            Position = new Rectangle(158, 404, 1300, 92),
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must agree to receive your picture by email." },
                                    {"fr", "Vous devez accepter de recevoir votre image par courriel." }
                                }}
                            }
                        },
                        new ScreenCheckOption // Info
                        {
                            Mandatory = true,
                            QuestionNo = 6,
                            Position = new Rectangle(158, 527, 1374, 100),
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                { UserErrorEnum.Mandatory, new Dictionary<string, string>
                                {
                                    {"en", "You must accept to receive information for future events." },
                                    {"fr", "Vous devez accepter de recevoir de l'information sur des événements futur." }
                                }}
                            }
                        }
                    }
                }
            };
            _screens.Add(optIn);

            var sharePrint = new ShareScreen(uiDir+"4-PreviewWhilePrinting.jpg", uiDir+"4-PreviewWhilePrinting_fr.jpg", sc,
                null, null, null, null,
                null, //print 
                null, null, null, null,
                new Rectangle(566, 886, 544, 96) //end
                , 85, new Size(550, 711), true
            )
            {
                Parent = screenPanel,
                AutoPrint = true,
                SaveSelectedPictureToQuestionNo = 2,
                PreviewPictureQuestion = 2,
                EndJump = 1,
                ImageListTranformOptions = new ImageListTransformOptions
                {
                    ImageMagickOptions = new List<ImageListTransformOptionsImageMagick>
                    {
                        new ImageListTransformOptionsImageMagick
                        {
                            CommandFile = "durex_ps_script.txt",
                            ResultMapToQuestionnaire = new Dictionary<string, int>
                            {
                                {"impression", 8}
                            }
                        }
                    }
                },
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(sharePrint);

            // Thank you
            var thxScreen = new ThanksEndScreen(uiDir + "5-Thanks.jpg", sc, uiDir + "5-Thanks_fr.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);
        }

        #endregion
        
        #region Mas-2019-01 Mosaïque

        private void MAS_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(1180, 1088, user)
            {
                ProjectNo = "vi8zm49ii1",
                EventId = 1031,
                ForceFileCheck = true,
                ProjectDescription = "Mosaïque 2019-01 PS",
                QuestionPictureToPrintNo = 8
            };

            var uiDir = InitPrintStationQuestionnaire(screenPanel, sc, "mas-2019-01-ps-desktop.xml", "1.0");
            
            var baseCancel = new Rectangle(1325, 33, 336, 112);

            // 1 - Choix de langue
            var langScreen = new HomeScreen(uiDir + "1-Accueil.jpg", sc,
                new Rectangle(278, 596, 388, 134), //FR
                new Rectangle(1013, 596, 389, 134), // EN
                new Rectangle(1580, 0, 100, 100)) // Cancel
            {
                Parent = screenPanel,
                QuestionLanguageNo = 1,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(langScreen);

            // Questionnaire Part 1
            var fightFor = new QuestionnaireScreen_MAS201901_info(uiDir + "2-Infos.jpg", uiDir + "2-Infos_fr.jpg", sc)
            {
                Parent = screenPanel
            };
            _screens.Add(fightFor);
            
            // Couleurs
            var changeNext = ScreenChangeOptions.GenerateBasicNextOptions();

            var listZones = new List<AnswerZone>();
            var zones = new List<Rectangle>
            {
                new Rectangle(172, 250, 175, 262), 
                new Rectangle(559, 250, 175, 262),
                new Rectangle(945, 250, 175, 262),
                new Rectangle(1333, 250, 175, 262),
                new Rectangle(347, 590, 175, 262),
                new Rectangle(734, 590, 175, 262),
                new Rectangle(1121, 590, 175, 262)
            };

            for (var i = 1; i <= 7; i++)
            {
                if (i == 1 || i == 3) continue; // Blocage de certaines couleurs

                listZones.Add(new AnswerZone
                {
                    PageChangeOptions = changeNext,
                    Zone = zones[i-1],
                    BackgroundImageFilePath = $"{i}.jpg",
                    AnswersUponSelection = new List<Answer>
                    {
                        new Answer(QuestionTypeEnum.Combo, 10, 1, i)
                    }
                });
            }

            var optInScreen = new BackgroundSelection_MAS(uiDir + "3-Couleur.jpg", uiDir + "3-Couleur_fr.jpg", sc, baseCancel)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                Zones = listZones
            };
            _screens.Add(optInScreen);

            // Thanks
            var optionsMagick = new ImageListTransformOptions
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick>()
            };

            for (var i = 1; i <= 7; i++)
            {
                optionsMagick.ImageMagickOptions.Add(new ImageListTransformOptionsImageMagick
                {
                    CommandFile = $"0{i}_script.txt",
                    Condition = new QuestionCondition(10, 1, i),
                    ResultMapToQuestionnaire = new Dictionary<string, int>
                    {
                        {"impression", 8}
                    }
                });
            }
            
            var sharePrint = new ShareScreen(uiDir+"4-Thanks.jpg", uiDir+"4-Thanks_fr.jpg", sc,
                null, null, null, null,
                null, //print 
                null, null, null, null,
                new Rectangle(80, 130, 1512, 828) //end
            )
            {
                Parent = screenPanel,
                AutoPrint = true,
                SaveSelectedPictureToQuestionNo =2,
                ImageListTranformOptions = optionsMagick,
                CurrentRespondentSource = QuestionnaireController,
                InactivityTimerInterval = 15,
                CreatingImageSequence = new AnimatedImageSequence
                {
                    DrawLocation = new Point(550, 800),
                    Loop = true,
                    ImageGenerators = new List<IAnimatedImageSequenceGenerator>()
                    {
                        new AnimatedImageSequenceGeneratorFromText()
                        {
                            ProceduralImageTextInfo = new ProceduralImageTextInfo
                            {
                                Color = Color.Black,
                                FontSize = 50,
                                BitmapSize = new Size(600,200),
                                Center = false
                            },
                            TextsLang = new Dictionary<string, IList<string>>
                            {
                                {"fr", new List<string>
                                    {
                                        "Traitement.",
                                        "Traitement..",
                                        "Traitement..."
                                    }
                                },
                                {"en", new List<string>
                                    {
                                        "Working.",
                                        "Working..",
                                        "Working..."
                                    }
                                }
                            }
                        }
                    }
                }
            };
            _screens.Add(sharePrint);
        }

        #endregion

        #region BNC-360-2019-01 PS (PS + Fusion)

        private void BNC_360_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(1179, 1087, user)
            {
                ProjectNo = "gxawz85xn1",
                EventId = 1030,
                ForceFileCheck = true,
                ProjectDescription = "BNC 2019-01 PS",
                QuestionPictureToPrintNo = 2,
                QuestionSmsNo = 3
            };

            var uiDir = InitPrintStationQuestionnaire(screenPanel, sc, "bnc-360-2019-01-PS-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId,sc.QuestionnaireId);

            PostQuestionnaireRespondantNoQuestion = 4;

            var sms = new BNCQuestionnaireScreen(uiDir + "1-Infos_fr.jpg", uiDir + "1-Infos_fr.jpg",sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController
            };
            _screens.Add(sms);


            // Écran 1 - Sharing
            var share = new ShareScreen(uiDir + "2-PreviewWhilePrinting_fr.jpg", null, sc,
                    null, null, null, null, null,
                    null, // Mail
                    null, // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(654, 888, 367, 107), // End
                    200, new Size(549, 695)) // Preview
            {
                Parent = screenPanel,
                CancelArea = new Rectangle(1533,20,135,129),
                SaveSelectedPictureToQuestionNo = 1,
                PreviewPictureQuestion = 1,
                CurrentRespondentSource = QuestionnaireController,
                EndJump = 1,
                DontUseInactiveImages = true,
                AutoPrint=true,
                ImageListTranformOptions = new ImageListTransformOptions
                {
                    ImageMagickOptions = new List<ImageListTransformOptionsImageMagick>
                    {
                        new ImageListTransformOptionsImageMagick
                        {
                            CommandFile = "bnc_script.txt",
                            ResultMapToQuestionnaire = new Dictionary<string, int>
                            {
                                {"impression", 2}
                            }
                        }
                    }
                },
            };
            _screens.Add(share);


            // Thank you
            var thxScreen = new ThanksEndScreen(uiDir + "3-Thanks_fr.jpg", sc, uiDir + "3-Thanks_fr.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);
        }

        #endregion
        
        #region 360-2019-02 Maestria PS (PS + Fusion)

        private void Maestria_360_2019_02_PS(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(1193, 1095, user)
            {
                ProjectNo = "wmw71erut1",
                EventId = 1035,
                ForceFileCheck = true,
                ProjectDescription = "Maestria 360-2019-02 PS"
            };

            var uiDir = InitPrintStationQuestionnaire(screenPanel, sc, "360-2019-02-ps-desktop.xml", "1.0");

            PostQuestionnaireRespondantNoQuestion = 4;
            SaveSelectedPictureToQuestionNo = 1;

            // 1 - Choix de langue
            var langScreen = new GenericScreen(uiDir + "2. Choix de langue.jpg", null,  sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DebugWritePropertiesToFiles = false,
                DefinitionLanguageZones = new ScreenDefinitionLanguageZones
                {
                    ConfigFileName = "01_languages.json"
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "01_navigation.json"
                }
            };
            _screens.Add(langScreen);

            var fightFor = new GenericScreen(uiDir + "3. questionnaire EN.jpg", uiDir + "3. questionnaire FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DebugWritePropertiesToFiles = false,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "02_textboxes.json"
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "02_keyboard.json"
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "02_navigation.json"
                }
            };
            _screens.Add(fightFor);

            // Thank you
            var thxScreen = new ThanksEndScreen(uiDir + "4. Thank you!.jpg", sc, uiDir + "4. Merci !.jpg")
            {
                Parent = screenPanel,
                InactivityTimerInterval = 3000
            };
            _screens.Add(thxScreen);
        }

        #endregion
        
        #region Mini Invasion 7co-2019-01 PS (PS + Fusion)

        private void Mini_7co_2019_01_PS(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(1195, 1097, user)
            {
                ProjectNo = "fy0plu8cg1",
                EventId = 1037,
                ForceFileCheck = true,
                ProjectDescription = "Mini Invasion 7co-2019-01 PS",
                QuestionPictureToPrintNo = 5,
                QuestionNumberOfPrintNo = 4
            };

            var uiDir = InitPrintStationQuestionnaire(screenPanel, sc, "7co-2019-01b-desktop.xml", "1.0");
            
            // Share screen
            var share = new ShareScreen(uiDir + "2-Sharing_option.png", null, sc,
                    null, null, null, null, 
                    new Rectangle(1045, 91, 100, 100), // Print
                    new Rectangle(1045, 395, 100, 100), // Mail
                    new Rectangle(1045, 243, 100, 100), // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(544, 546, 189, 60), // End
                    91, new Size(400, 400)) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1180,0,100,100),
                    SaveSelectedPictureToQuestionNo = 1,
                    PreviewPictureQuestion = 1,
                    CurrentRespondentSource = QuestionnaireController,
                    DontUseInactiveImages = false,
                    SmsJump = 1,
                    MailJump = 2,
                    ApplyTransformationToSelectedImage = true
                };
            _screens.Add(share);

            // SMS
            var smsScreen = new GenericScreen(uiDir + "3-SMS.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(425, 69, 695, 40),
                            AllowEmpty = true,
                            Mandatory = false,
                            QuestionNo = 3,
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            FieldName = "SMS",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"en", "That does not look like a mobile number..."}
                                }}
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 244, 600, 400),
                    KeyboardType = KeyboardTypeEnum.Number
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            SkipValidation = true,
                            Zone = new Rectangle(1180, 0, 100, 100),
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(542, 170, 192, 62),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                        }
                    }
                }
            };
            _screens.Add(smsScreen);

            // Email
            var emailScreen = new GenericScreen(uiDir + "4-E-mail.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(320, 69, 695, 40),
                            AllowEmpty = true,
                            Mandatory = false,
                            QuestionNo = 2,
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            FieldName = "Email",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"en", "That does not look like an email..."}
                                }}
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 244, 1180, 400),
                    KeyboardType = KeyboardTypeEnum.Email,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 18,
                        FamilyName = "Arial",
                        Style = "Bold"
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            SkipValidation = true,
                            Zone = new Rectangle(1180, 0, 100, 100),
                            PageChangeOptions = ScreenChangeOptions.GenerateCancelOptions()
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(542, 170, 192, 62),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(-2)
                        }
                    }
                }
            };
            _screens.Add(emailScreen);
        }

        #endregion
        
        #region Oakville Hospital foundation - Candlelight (STE-2019-02)

        private void Oakville_ste_2019_02_PS(Panel screenPanel, PrintStationUser user)
        {
            var sc = new PrintStationScreenConfig(1214, 1116, user)
            {
                ProjectNo = "w3wmolk0a1",
                EventId = 1056,
                ForceFileCheck = true,
                ProjectDescription = "Oakville Hospital foundation v2 - Candlelight (STE-2019-02)",
                QuestionPictureToPrintNo = 4,
                QuestionNumberOfPrintNo = 3
            };

            var uiDir = InitPrintStationQuestionnaire(screenPanel, sc, "ste-2019-02bv2-desktop.xml", "1.0");
            
            // Share screen
            var share = new ShareScreen(uiDir + "2-Sharing screen.jpg", null, sc,
                    null, null, null, null, 
                    new Rectangle(1192, 335, 136, 136), // Print
                    new Rectangle(348, 335, 136, 136), // Mail
                    null, // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(686, 737, 308, 68), // End
                    150, new Size(500, 500)) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1605, 0, 75, 75),
                    SaveSelectedPictureToQuestionNo = 1,
                    PreviewPictureQuestion = 1,
                    CurrentRespondentSource = QuestionnaireController,
                    DontUseInactiveImages = true,
                    MailJump = 1,
                    ApplyTransformationToSelectedImage = true
                };
            _screens.Add(share);

            // Email
            var emailScreen = new GenericScreen(uiDir + "3-email.jpg", null, sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(300, 138, 1100, 40),
                            AllowEmpty = true,
                            Mandatory = false,
                            QuestionNo = 2,
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            FieldName = "Email",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"en", "That does not look like an email..."}
                                }}
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 348, 1460, 480),
                    KeyboardType = KeyboardTypeEnum.Email,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 18,
                        FamilyName = "Arial",
                        Style = "Bold"
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(1605, 0, 75, 75),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                        },
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(681, 249, 317, 70),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                        }
                    }
                }
            };
            _screens.Add(emailScreen);
        }

        #endregion
        
        #region Sportium-2019-05 (PS + Fusion)

        private void Sportium_2019_05_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1215, 1117, user)
            {
                ProjectNo = "6y6tueiqd1",
                EventId = 1057,
                ForceFileCheck = true,
                ProjectDescription = "Sportium 2019-05 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 3
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "car-2019-05-ps-desktop.xml", "1.0");
            var baseDir = DirectoryUtil.GetLocalIbrandQuestionnaireDir(sc.ProjectId, sc.QuestionnaireId);

            PostQuestionnaireRespondantNoQuestion = 4;
            SaveSelectedPictureToQuestionNo = 1;

            const bool extractDebug = false;

            // 1 - Choix de langue
            var langScreen = new GenericScreen(dirUi + "2-choix_de_langue.jpg", null, sc)
            {
                DebugWritePropertiesToFiles = extractDebug,
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionLanguageZones = new ScreenDefinitionLanguageZones()
                {
                    ConfigFileName = "02-lang.json"
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "02-nav.json"
                    
                }
            
            };
            _screens.Add(langScreen);


            var sportiumQuestionnaire = new GenericScreen(dirUi + "3-questionnaire_EN.jpg", dirUi + "3-questionnaire_FR.jpg", sc)
            {
                DebugWritePropertiesToFiles = extractDebug,
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    ConfigFileName = "03-sportium.json"
                },
                DefinitionsCheckBoxes = new ScreenDefinitionCheckBoxes()
                {
                    ConfigFileName = "03-check-sportium.json"
                
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    ConfigFileName = "03-keyboard-sportium.json"
                            
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ConfigFileName = "03-nav-sportium.json"
                    
                            
                  }
                  
                };
                _screens.Add(sportiumQuestionnaire);
        
            

                var disclaimer = new Ui.generic.SectionSkipScreen(dirUi + "4-policy.jpg", dirUi + "4-policy.jpg", sc, null, new Rectangle(154, 112, 946, 431))
                {
                    Parent = screenPanel,
                    CurrentRespondentSource = QuestionnaireController,

                    Zones = new[]{
                        new SectionSkipScreenData
                        {
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(),
                            Zone = new Rectangle(new Point(1204, 0), new Size(76,76)),
                            SkipValidation = true
                        }
                    },
                    RuleAndRegsFileEn = baseDir + "privacy policy sportium.txt",
                    RuleAndRegsFilefr = baseDir + "politique de confidentialité sportium.txt"
                };
                _screens.Add(disclaimer);

                // Thank you
                var thxScreen = new ThanksEndScreen(dirUi + "5-Thankyou_EN.jpg", sc, dirUi + "5-Merci_FR.jpg")
                {
                    Parent = screenPanel,
                    CurrentRespondentSource = QuestionnaireController,
                    InactivityTimerInterval = 3000
                };
                _screens.Add(thxScreen);


        }

        #endregion
        
        #region 24 Heures Tremblant (Fondation Charles-Bruneau)

        private void Init_FCB2019_PS(Panel screenPanel, PrintStationUser user)
        {
            /* Tous les "magic numbers" du projet ici. #1#
            var sc = new PrintStationScreenConfig(1222, 1124, user)
            {
                ProjectNo = "gyv082le51",
                EventId = 1064,
                ForceFileCheck = true,
                ProjectDescription = "Fondation Charles-Bruneau 2019-01 PS",
                QuestionEmailNo = 2,
                QuestionSmsNo = 1,
                QuestionPictureToPrintNo = 4
            };
            SaveSelectedPictureToQuestionNo = 4;
            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "cha-2019-01-desktop-ps.xml", "1.0");

            // Écran Sharing
            var share = new ShareScreen(dirUi + "1-sharing.jpg", null, sc, null, null, null, null,
                    new Rectangle(997, 186, 114, 114), // print
                    new Rectangle(999, 457, 111, 116), // Mail
                    new Rectangle(998, 322, 113, 109), // SMS
                    null, // facebook
                    null, // twitter
                    new Rectangle(541, 578, 199, 68), // End
                    193, new Size(575, 350)) // Preview
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                PreviewPictureQuestion = 4,
                DontUseInactiveImages = true,
                CancelArea = new Rectangle(1191,0,89,67)
            };
            _screens.Add(share);

            // Écran Mail
            var mail = new MailScreen(dirUi + "2-e-mail.jpg", dirUi + "2-e-mail.jpg", sc, 1,
                new Rectangle(359, 197, 825, 46), //textbox
                new Rectangle(540, 313, 198, 65), // ok button
                new KeyboardControl(52, 394, 1170, 260),
                new List<ScreenCheckOption>
                {
                    new ScreenCheckOption(new Rectangle(115, 262, 258, 33), 3, 1, false)
                }
            )
            {
                CancelArea = new Rectangle(1203, 0, 76, 76)
            };
            _screens.Add(mail);

            // Écran SMS
            var sms = new SmsScreen(dirUi + "3-sms.jpg", dirUi + "3-sms.jpg", sc, 2,
                new Rectangle(365, 196, 530, 47), //textbox
                new Rectangle(541, 267, 199, 70), // ok button
                new NumbersKeyboardControl(352, 343, 578, 319)
            )
            {
                CancelArea = new Rectangle(1203, 0, 76, 76)
            };
            _screens.Add(sms);
        }

        #endregion
        */

        #region TIH 2020 01 Carte-Hockey (PS)

        private void TIH_2020_01_PS(Panel screenPanel, PrintStationUser user)  
        {
            /* Tous les "magic numbers" du projet ici. */
            var sc = new PrintStationScreenConfig(1229, 1130, -1, user)
            {
                ProjectNo = "ja13kzdfj1",
                EventId = 1070,
                ForceFileCheck = true,
                ProjectDescription = "IGA Pee-Wee 2020 B",
                QuestionEmailNo = 6,
                QuestionPictureToPrintNo = 11,
                QuestionSmsNo = 7,
                QuestionNumberOfPrintNo = 14
            };

            var dirUi = InitPrintStationQuestionnaire(screenPanel, sc, "TIH-2020-01b-desktop.xml", "1.0");
            
            PostQuestionnaireRespondantNoQuestion = 13;

            // Écran 2 - Choix de langue
            var langScreen = new HomeScreen(dirUi + "2. Choix de langue.jpg", sc,
                new Rectangle(182, 303, 255, 81), // fr
                new Rectangle(843, 305, 250, 76), // en
                new Rectangle(1177,3,102,84))
            {
                Parent = screenPanel,
                QuestionLanguageNo = 10
            };
            _screens.Add(langScreen);

            //Ecran opt Ins
            var optInScreen = new OptInScreen(dirUi + "2.1 Opt-in disclaimer EN.jpg",
                dirUi + "2.1 Opt-in disclaimer FR.jpg", sc)
            {
                Parent = screenPanel,
                CheckBoxZone = new List<ScreenCheckOption>()
                {
                    new ScreenCheckOption(new Rectangle(109, 118, 23, 25), 8, 1, false,new Point(100,103)),
                    new ScreenCheckOption(new Rectangle(109, 174, 23, 24), 9, 1, false,new Point(100,160))
                }
              
            };
            _screens.Add(optInScreen);

            var questionnaire = new CarteHockeyQuestionnaireScreen(dirUi + "3. Personnalisation EN.jpg",dirUi + "3. Personnalisation FR.jpg",sc);
            _screens.Add(questionnaire);
       
            var transforms = new ImageListTransformOptions
            {
                ImageMagickOptions = new List<ImageListTransformOptionsImageMagick> { 
                    // Si quotation personalisée inexistante
                    new ImageListTransformOptionsImageMagick() {
                        CommandFile = "carte_hokey_two_steps.txt",
                        ResultMapSaveModels = new Dictionary<string, DataModelBase>
                        {
                            {"impression", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(11)} },
                            {"recto", new DataModelQuestionnaire {Identifier = new QuestionIdentifier(12)} }
                        },
                        Timeout = TimeSpan.FromSeconds(20)
                    }
                }
            };

            // Écran 3 - Sharing
            var share = new ShareScreen(dirUi + "4. Sharing.jpg", dirUi + "4. Sharing.jpg", sc,
                    null, null, null, null, 
                    new Rectangle(889, 242, 104, 104 ), // Print
                    new Rectangle(284, 179, 105, 104), // Mail
                    new Rectangle(284, 306, 105, 104), // SMS
                    new Rectangle(-1,-1,0,0), // facebook
                    new Rectangle(-1,-1,0,0), // twitter
                    new Rectangle(542, 562, 193, 63), // End
                    36, new Size(372, 495),true) // Preview
                {
                    Parent = screenPanel,
                    CancelArea = new Rectangle(1171, 4, 101, 59),
                    CurrentRespondentSource = QuestionnaireController,
                    ImageListTranformOptions = transforms,
                    PicturesToTransform = new List<int>(){1},
                    SaveOriginalImageToQuestion = 1,
                    AutoPrint = false,
                    PreviewPictureQuestion = 12,
                    DontUseInactiveImages = true
                    
            };
            _screens.Add(share);

            var emailScreen = new GenericScreen(dirUi + "6. Email EN.jpg", dirUi +"6. Email FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(375, 76, 536, 41),
                            AllowEmpty = true,
                            Mandatory = false,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(6)
                            }},
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Email
                            },
                            FieldName = "Email",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"en", "The email is not valid."},
                                    {"fr", "Le courriel est invalide."}
                                }}
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 260, 1202, 300),
                    KeyboardType = KeyboardTypeEnum.Email,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 18,
                        FamilyName = "Arial",
                        Style = "Bold"
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(542, 166, 195, 68),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions()
                        }
                    }
                }
            };
            _screens.Add(emailScreen);
            
            var smsScreen = new GenericScreen(dirUi + "5.SMS EN.jpg", dirUi +"5.SMS FR.jpg", sc)
            {
                Parent = screenPanel,
                CurrentRespondentSource = QuestionnaireController,
                DefinitionTextbox = new ScreenDefinitionTextBoxes
                {
                    TextboxDefinitions = new List<TextboxDefinition>
                    {
                        new TextboxDefinition
                        {
                            Position = new Rectangle(375, 76, 536, 41),
                            AllowEmpty = true,
                            Mandatory = false,
                            DataModels = new List<DataModelBase>{
                                new DataModelQuestionnaire
                            {
                                Identifier = new QuestionIdentifier(7)
                            }},
                            Modifications = TextModificationsEnum.Trim,
                            Validation = new TextValidationBasic
                            {
                                Option = TextValidationBasic.TextValidationBasicEnum.Sms
                            },
                            FieldName = "Sms",
                            UserMessages = new Dictionary<UserErrorEnum, IDictionary<string, string>>
                            {
                                {UserErrorEnum.Validation, new Dictionary<string, string>
                                {
                                    {"en", "The mobile number is not valid."},
                                    {"fr", "Le numéro de mobile est invalide."}
                                }}
                            }
                        }
                    }
                },
                DefinitionKeyboard = new ScreenDefinitionKeyboard
                {
                    Position = new Rectangle(-1, 260, 399, 352),
                    KeyboardType = KeyboardTypeEnum.Number,
                    FontDefinition = new FontDefinitionInfo
                    {
                        Size = 18,
                        FamilyName = "Arial",
                        Style = "Bold"
                    }
                },
                DefinitionNavigation = new ScreenDefinitionNavigation
                {
                    ZoneList = new List<SectionSkipScreenData>
                    {
                        new SectionSkipScreenData
                        {
                            Zone = new Rectangle(545, 168, 195, 68),
                            PageChangeOptions = ScreenChangeOptions.GenerateBasicBackOptions(-2)
                        }
                    }
                }
            };
            _screens.Add(smsScreen);

        }

        #endregion
        
    }
}