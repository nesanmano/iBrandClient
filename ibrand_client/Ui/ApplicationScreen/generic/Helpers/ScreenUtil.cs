using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Ui;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;
using NomadLogic.Common.Util.Win32;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Classe contenant des méthode pour gérer des object destinés à l'affichage avec WinForms.
    /// </summary>
    public static class ScreenUtil
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public static readonly Pen DebugBluePen = new Pen(Color.Blue, 4);
        public static readonly Pen DebugLightBluePen = new Pen(Color.LightBlue, 4);
        public static readonly Pen DebugRedPen = new Pen(Color.Red, 4);
        public static readonly Pen DebugBlackPen = new Pen(Color.Black, 4);

        /// <summary>
        /// Créer et initialise une boîte de texte de façon assez générale mais complète.
        /// Si on veut un "look & feel" un peu différent, on peut changer les propriétés à modifier après avoir invoquer cette méthode.
        /// Il ne faut pas passer deux fois la même instance dans cette méthode, cela créer une erreur à cause de la parenté entres les
        /// composantes des formes Windows.
        /// </summary>
        /// <param name="screen">L'écran appelant.</param>
        /// <param name="box">La boîte de texte à initialiser.</param>
        /// <param name="position">La position et taille de la boîte. La hauteur est en fait gérée par le Font.</param>
        /// <param name="font">Le Font de texte utilisée. Optionnel. Gère la taille par la hauteur du texte. Si null, on utilise un font par défaut.</param>
        /// <param name="tabOrder">Optionnel. L'ordre de tabulation. Rien par défaut.</param>
        /// <param name="qNo">Optionnel. Numero de la question dont on veux afficher un reponse par defaut dans le UI</param>
        /// <returns>La même référence que le premier paramètre (pour le chaînage).</returns>
        public static bool InitTextBox(this AbstractScreen screen, out TextBox box, Rectangle position, Font font, int? tabOrder = null, int? qNo = null)
        {
            box = new TextBox();
            var tmp = InitTextBox(screen, box, position, font, tabOrder, qNo);
            return ReferenceEquals(tmp, box);
        }
        
        /// <summary>
        /// Initialise une boîte de texte de façon assez générale mais complète.
        /// Si on veut un "look & feel" un peu différent, on peut changer les propriétés à modifier après avoir invoquer cette méthode.
        /// Il ne faut pas passer deux fois la même instance dans cette méthode, cela créer une erreur à cause de la parenté entres les
        /// composantes des formes Windows.
        /// </summary>
        /// <param name="screen">L'écran appelant.</param>
        /// <param name="box">La boîte de texte à initialiser.</param>
        /// <param name="position">La position et taille de la boîte. La hauteur est en fait gérée par le Font.</param>
        /// <param name="font">Le Font de texte utilisée. Optionnel. Gère la taille par la hauteur du texte. Si null, on utilise un font par défaut.</param>
        /// <param name="tabOrder">Optionnel. L'ordre de tabulation. Rien par défaut.</param>
        /// <param name="qNo">Optionnel. Numero de la question dont on veux afficher un reponse par defaut dans le UI</param>
        /// <returns>La même référence que le premier paramètre (pour le chaînage).</returns>
        [Obsolete]
        public static TextBox InitTextBox(this AbstractScreen screen, TextBox box, Rectangle position, Font font, int? tabOrder = null, int? qNo=null)
        {
            if (box == null) return null;

            box.BorderStyle = BorderStyle.FixedSingle;
            box.Location = position.Location;
            box.Size = position.Size;
            box.TabIndex = tabOrder ?? 1;
            box.Font = font ?? AbstractScreen.DEFAULT_TEXT_BOX_FONT;
            box.TextChanged += screen.OnTextChanged;
            screen.Controls.Add(box);
            box.BringToFront();

            if (qNo == null) return box;

            var quest = screen.GetCurrentQuestionnaire();
            var qIndex = qNo.GetValueOrDefault();
            var question = quest.GetQuestion(qIndex);
            if (question != null && question.HasDefaultAnswer())
                box.Text = question.GetDefaultAnswer(1).Response;

            return box;
        }

        /// <summary>
        /// Initialise une boîte de sélection déjà instanciée de façon assez générale mais complète.
        /// Si on veut un "look & feel" un peu différent, on peut changer les propriétés à modifier après avoir invoquer cette méthode.
        /// Il ne faut pas passer deux fois la même instance dans cette méthode, cela créer une erreur à cause de la parenté entres les
        /// composantes des formes Windows.
        /// </summary>
        /// <param name="screen">L'écran appelant.</param>
        /// <param name="box">La boîte de sélection à initialiser.</param>
        /// <param name="position">La position et taille de la boîte. La hauteur est en fait gérée par le Font.</param>
        /// <param name="font">Le Font de texte utilisée. Optionnel. Gère la taille par la hauteur du texte. Si null, on utilise un font par défaut.</param>
        /// <param name="tabOrder">Optionnel. L'ordre de tabulation. Rien par défaut.</param>
        /// <param name="list">La liste d'éléments pour garnir la sélection.</param>
        [Obsolete]
        public static ComboBox InitComboBox(this AbstractScreen screen, ComboBox box, Rectangle position, Font font, int? tabOrder = null, object[] list = null)
        {
            if (box == null) return null;

            box.DropDownStyle = ComboBoxStyle.DropDownList;
            box.Location = position.Location;
            box.Size = position.Size;
            box.TabIndex = tabOrder ?? 1;
            box.Font = font ?? AbstractScreen.DEFAULT_TEXT_BOX_FONT;
            
            box.SelectedIndex = -1;
            if (list?.Any() ?? false)
            {
                box.Items.AddRange(list);
            }

            box.TextChanged += screen.OnTextChanged;
            screen.Controls.Add(box);
            box.BringToFront();

            return box;
        }

        /// <summary>
        /// Met à jour une liste défilante en reconstruisant sa liste d'éléments.
        /// S'arrange pour ne pas lancer d'événements de changement d'état avant d'avoir reconstruit la liste.
        /// </summary>
        /// <param name="screen">L'écran qui contient la boîte.</param>
        /// <param name="box">La boîte de sélection.</param>
        /// <param name="list">La nouvelle liste d'éléments.</param>
        public static void UpdateComboBox(this AbstractScreen screen, ComboBox box, object[] list)
        {
            if (box == null || screen == null) return;
            
            box.TextChanged -= screen.OnTextChanged;
            box.SelectedIndex = -1;
            box.Items.Clear();
            if (list?.Any() ?? false)
            {
                box.Items.AddRange(list);
            }

            box.TextChanged += screen.OnTextChanged;
        }

        /// <summary>
        /// Créer et initialise une boîte de sélection de façon assez générale mais complète.
        /// Si on veut un "look & feel" un peu différent, on peut changer les propriétés à modifier après avoir invoquer cette méthode.
        /// Il ne faut pas passer deux fois la même instance dans cette méthode, cela créer une erreur à cause de la parenté entres les
        /// composantes des formes Windows.
        /// </summary>
        /// <param name="screen">L'écran appelant.</param>
        /// <param name="box">La boîte de sélection à initialiser.</param>
        /// <param name="position">La position et taille de la boîte. La hauteur est en fait gérée par le Font.</param>
        /// <param name="font">Le Font de texte utilisée. Optionnel. Gère la taille par la hauteur du texte. Si null, on utilise un font par défaut.</param>
        /// <param name="tabOrder">Optionnel. L'ordre de tabulation. Rien par défaut.</param>
        /// <param name="list">La liste d'éléments pour garnir la sélection.</param>
        public static bool InitComboBox(this AbstractScreen screen, out ComboBox box, Rectangle position, Font font, int? tabOrder = null, object[] list = null)
        {
            box = new ComboBox();
            var tmp = InitComboBox(screen, box, position, font, tabOrder, list);
            return ReferenceEquals(tmp, box);
        }

        
        /// <summary>
        /// Analyse et exécute les interactions des changements de pages et de validation.
        /// </summary>
        /// <param name="screen">L'écran appelant.</param>
        /// <param name="x">Position en X du clic</param>
        /// <param name="y">Position en Y du clic</param>
        /// <param name="zoneList">La liste de zones de changements de page (si null, la méthode ne fait rien).</param>
        /// <param name="checkOptions">Optionel. La liste des options de check. N'est évaluée que pour savoir si tous les checks obligatoires sont activés.</param>
        /// <param name="validationMethod">Optionel. La méthode de validation de la classe écran appellante. Assume que si des message doivent être affichés, cette méthode le fera.</param>
        /// <param name="debugLogEnabled">Indique si on doit journaliser au niveau Debug (surpasse le niveau de log de AbstractSCreen).</param>
        /// <returns>True si et seulement si aucune zone n'a été activée, ce qui permet de continuer à tester pour des effets lié au clic.</returns>
        [Obsolete]
        public static bool EvaluateZonesAndValidation(this AbstractScreen screen, int x, int y, IList<SectionSkipScreenData> zoneList, IList<ScreenCheckOption> checkOptions = null, 
            [CanBeNull] Func<bool> validationMethod = null, bool debugLogEnabled = false)
        {
            var hit = zoneList?.FirstOrDefault(data => InArea(x, y, data.Zone));
            if (hit == null) return true;
            log.Debug("SkipScreenData zone hit: {0}", hit.Zone);

            var action = hit.CustomActionOnActivation;
            if (action != null)
            {
                screen.SafeCall(() =>
                {
                    action.Invoke();
                });
            }
            var sco = hit.PageChangeOptions.Evaluate(screen.GetCurrentRespondentWithAnswers());
            if (sco == null)
            {
                log.Warn("Hit was detected but no screen change option was selected. Verify. Options=\n{0}", StringUtil.FormatEnumerable(hit.PageChangeOptions.Options, true, 1));
                return false;
            }
            return screen.ExecuteScreenChangeOption(sco, !hit.SkipValidation, checkOptions, validationMethod, debugLogEnabled);
        }

        /// <summary>
        /// Exécute un changement de page, avec options de vérification. Ne fait rien si le premier paramètre est null.
        /// Si le clic ne provoque aucun événement, cette méthode retourne TRUE, ce qui indique que l'on peut continuer à chercher
        /// pour des événements liés aux clicx.
        /// </summary>
        /// <param name="screen">L'écran appelant.</param>
        /// <param name="sco">Le paramètre de changement d'écrans. Optionel.</param>
        /// <param name="validate">Indiqaue s'il faut valider avant de changer d'écran.</param>
        /// <param name="checkOptions">Les options de CheckBox à vérifier pour options obligatoires.</param>
        /// <param name="validationMethod">La méthode personnalisées de validation de questionnaire associés à l'appellant.</param>
        /// <param name="debugLogEnabled">Indique si on veut forcer la journalisation de niveau DEBUG.</param>
        /// <returns>TRUE si et seulement si aucun événement n'a été provoqué.</returns>
        [Obsolete]
        public static bool ExecuteScreenChangeOption(this AbstractScreen screen, ScreenChangeOption sco, bool validate = false, IList<ScreenCheckOption> checkOptions = null, 
            [CanBeNull] Func<bool> validationMethod = null, bool debugLogEnabled = false)
        {
            if (sco == null) return true;

            if (validate)
            {
                if (!sco.Cancel && !AllMandatoryChecks(checkOptions))
                {
                    if (debugLogEnabled) log.Debug("Screen change mandatory check failed. Checks:\n\t{0}", StringUtil.FormatEnumerable(checkOptions, true, 1));
                    screen.SafeCall(() => { 
                        var mBox = new MessageDialog();
                        mBox.ShowMessage(screen.ActiveLanguage.Equals("en", StringComparison.InvariantCultureIgnoreCase) ?
                            "Not all Mandatory checks are enabled." :
                            "Les options obligatoire par crochet ne sont pas toutes activées.");
                    });
                    return false;
                }

                if (validationMethod != null)
                {
                    if (debugLogEnabled) log.Debug("Execution of screen change execution method.");
                    if (!validationMethod.Invoke()) // Si la méthode n'est pas nulle et échoue
                    {
                        // On assume que la méthode va elle-même provoquer les événements UI (boîte de messages) appropriés pour
                        // sa validation si faussée.
                        return false;
                    }
                }
            }

            if (sco.Cancel)
            {
                if (debugLogEnabled) log.Debug("Screen change cancel execution.");
                screen.FirstScreen();
                return false;
            }

            if (debugLogEnabled) log.Debug("Screen change step execution ({0}).", sco.Step);
            screen.Next(sco.Step);
            return false;
        }

        
        /// <summary>
        /// Cette méthode retourne "false" si et seulement si un des éléments est un "opt-in" obligatoire
        /// et n'est pas coché au moment de la vérification. Donc, une liste vide ou nulle retourne true.
        /// </summary>
        /// <param name="zones">La liste de zones à tester.</param>
        /// <returns>FALSE si et seulement si au moins un check obligatoire n'est pas coché.</returns>
        [Obsolete]
        public static bool AllMandatoryChecks(IEnumerable<ScreenCheckOption> zones)
        {
            if (zones == null) return true;
            return !zones.Any(z => z.Mandatory && !z.AnswerState);
        }

        /// Utiliser la version de MathUtil directement.
        [Obsolete]
        public static bool InArea(int x, int y, Rectangle area)
        {
            return MathUtil.InArea(x, y, area);
        }

        /// Utiliser la version de MathUtil directement.
        [Obsolete]
        public static bool InArea(int x, int y, Rectangle? area)
        {
            return area != null && InArea(x, y, area.Value);
        }

        /// <summary>
        /// Si l'argument contient des messages, on va afficher une boîte de message contenant tous les message destinés à l'usager, d'un coup, dans un
        /// seul écran modal.
        /// </summary>
        /// <param name="resUserMessages">La liste, possiblement null ou vide, des messages possibles à fficher.</param>
        public static void CheckAndShowUserMessages(IList<string> resUserMessages)
        {
            if (resUserMessages == null || !resUserMessages.Any()) return;

            var text = StringUtil.ConcatStringsMultiline(resUserMessages);

            MessageBox.Show(text, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        public static void CheckAndShowUserMessages(params string[] resUserMessages)
        {
            if (resUserMessages == null || !resUserMessages.Any()) return;

            var text = StringUtil.ConcatStringsMultiline(resUserMessages);

            MessageBox.Show(text, @"iBrand", MessageBoxButtons.OK, MessageBoxIcon.None);
        }

        /// <summary>
        /// Englobe les bons appels de fonction GDI+ pour dessiner une image de façon indépendante de sa résolution.
        /// Fait un null-check sur l'image.
        /// </summary>
        /// <param name="g">l'objet Graphics.</param>
        /// <param name="img">L'image à dessiner</param>
        /// <param name="zone">La zone reliée à l'image.</param>
        public static void DrawImage([NotNull] Graphics g, Image img, Rectangle zone)
        {
            if (img == null) return;
            g.DrawImage(img, zone, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Englobe les bons appels de fonction GDI+ pour dessiner une image de façon indépendante de sa résolution.
        /// Fait un null-check sur l'image.
        /// </summary>
        /// <param name="g">l'objet Graphics.</param>
        /// <param name="img">L'image à dessiner</param>
        /// <param name="location">Le point de départ reliée à l'image.</param>
        public static void DrawImage([NotNull] Graphics g, Image img, Point location)
        {
            if (img == null) return;
            g.DrawImage(img, new Rectangle(location, img.Size), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Englobe les bons appels de fonction GDI+ pour dessiner une image de façon indépendante de sa résolution.
        /// Fait un null-check sur l'image.
        /// </summary>
        /// <param name="g">l'objet Graphics.</param>
        /// <param name="img">L'image à dessiner</param>
        /// <param name="zone">La zone reliée à l'image.</param>
        public static void DrawImage([NotNull] Graphics g, Image img, Rectangle? zone)
        {
            if (img == null || zone == null) return;
            g.DrawImage(img, zone.Value, 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Englobe les bons appels de fonction GDI+ pour dessiner une image de façon indépendante de sa résolution.
        /// Fait un null-check sur l'image.
        /// </summary>
        /// <param name="g">l'objet Graphics.</param>
        /// <param name="img">L'image à dessiner</param>
        /// <param name="location">Le point de départ reliée à l'image.</param>
        public static void DrawImage([NotNull] Graphics g, Image img, Point? location)
        {
            if (img == null || location == null) return;
            g.DrawImage(img, new Rectangle(location.Value, img.Size), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
        }

        /// <summary>
        /// Recycle toutes les propriétés qui implémente l'étape de cycle IRecyclable d'une instance d'écran.
        /// </summary>
        /// <param name="screen">L'instance de l'écran.</param>
        public static void RecycleAllProperties(this AbstractScreen screen)
        {
            if (screen == null) return;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IRecyclable));

            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IRecyclable propObject)) continue; // Si null, pas de problème, on saute.
                propObject.Recycle(screen);
            }
        }

        /// <summary>
        /// Dessine toutes les propriétés qui peuvent être dessinées via un graphique GDI+ de l'instance d'écran.
        /// </summary>
        /// <param name="screen">L'instance de l'écran.</param>
        /// <param name="g">L'objet graphique de GDI+</param>
        /// <param name="debug">Indique s'il faut afficher les informations de débogage.</param>
        public static void PaintAllProperties(this AbstractScreen screen, Graphics g, bool debug)
        {
            if (screen == null || g == null) return;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IPaint));

            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IPaint propObject)) continue; // Si null, pas de problème, on saute.
                propObject.Paint(g, debug);
            }
        }

        /// <summary>
        /// Modifie la langue des propriétés qui implémentent l'interface pour réagir à des changements de langue.
        /// </summary>
        /// <param name="screen">L'instance de l'écran.</param>
        /// <param name="languageCode">La code de langue vers lequel changer.</param>
        public static void ChangeLanguageAllProperties(this AbstractScreen screen, string languageCode)
        {
            if (screen == null || string.IsNullOrWhiteSpace(languageCode)) return;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IChangeLanguage));

            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IChangeLanguage propObject)) continue; // Si null, pas de problème, on saute.
                propObject.ChangeLanguage(screen, languageCode);
            }
        }

        /// <summary>
        /// Tente d'appeller l'initialisation de base de toutes les propriétés de l'instance en paramètre.
        /// </summary>
        /// <param name="screen">L'instance de l'écran.</param>
        public static bool InitAllProperties(this AbstractScreen screen)
        {
            if (screen == null) return false;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IInitialisable));

            var allSuccess = true;
            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IInitialisable propObject)) continue; // Si null, pas de problème, on saute.
                allSuccess &= propObject.Init();
            }

            return allSuccess;
        }

        /// <summary>
        /// Évalue les résultats de toutes les propriétés sur un clic de souris.
        /// Ce n'est pas la tâche de cette méthode d'aggrégé les résultats.
        /// </summary>
        /// <param name="screen">l'instance d'écran qui possède les propriétés à analyser.</param>
        /// <param name="x">La position X du clic.</param>
        /// <param name="y">La position Y du clic.</param>
        /// <param name="validationMethod">S'il y a lieu, la méthode de validation du questionnaire.</param>
        /// <returns>Une liste de résultat non-nulle. Peut être vide.</returns>
        public static IList<ScreenEvaluationResult> EvaluateResultsAllProperties(this AbstractScreen screen, int x, int y, Func<ScreenEvaluationResult> validationMethod)
        {
            if (screen == null) return new List<ScreenEvaluationResult>();

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IEvaluateScreenZones));

            var res = new List<ScreenEvaluationResult>();

            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IEvaluateScreenZones propObject)) continue; // Si null, pas de problème, on saute.
                res.Add(propObject.EvaluateZones(screen, x, y, validationMethod));
            }

            return res;
        }

        /// <summary>
        /// Exécute l'activation de toutes les propriétés activable d'une instance d'écran.
        /// </summary>
        /// <param name="screen">l'instance d'écran qui s'active.</param>
        /// <param name="resp">Le répondant dans sa forme présente, réponses inclues.</param>
        public static void ActivateAllProperties(this AbstractScreen screen, IRespondent resp)
        {
            if (screen == null || resp == null) return;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IActivateScreen));
            
            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IActivateScreen propObject)) continue; // Si null, pas de problème, on saute.
                propObject.Activate(screen, resp);
            }
        }

        /// <summary>
        /// Exécute la destruction de toutes les propriétés "Disposable" d'une instance d'écran.
        /// </summary>
        /// <param name="screen">l'instance d'écran qui s'active.</param>
        public static void DisposeAllProperties(this AbstractScreen screen)
        {
            if (screen == null) return;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IDisposable));
            
            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IDisposable propObject)) continue; // Si null, pas de problème, on saute.
                propObject.Dispose();
            }
        }

        /// <summary>
        /// Initialise toutes les propriétés comprenant des images. L'écran fournit la langue sur laquelle il faut faire l'initialisation.
        /// </summary>
        /// <param name="screen">L'instance d'écran qui doit initialiser ses images.</param>
        /// <param name="baseDir">Le répertoire de base de recherche pour images avec chemin relatif. Peut être nul.</param>
        /// <returns>TRUE si et seulement si toutes les tentatives d'initialisation de tous les objets se sont bien complétés.</returns>
        public static bool InitializeImagesAllProperties(this AbstractScreen screen, string baseDir)
        {
            if (screen == null) return false;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IImageInitialisable));
            var lang = screen.ActiveLanguage;
            var aggResult = true;
            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IImageInitialisable propObject)) continue; // Si null, pas de problème, on saute.
                aggResult &= propObject.InitImage(baseDir, lang);
            }

            return aggResult;
        }

        /// <summary>
        /// Sauvegarde dans le questionnaire toutes les valeurs désirables des propriétés avec la bonne interface lors d'un changement de page.
        /// </summary>
        /// <param name="screen">L'écran sur le point de changer qui doit mettre des réponse dans le questionnaire.</param>
        /// <returns>TRUE si toutes les opérations se sont bien exécutées.</returns>
        public static bool SaveAllProperties(this AbstractScreen screen)
        {
            if (screen == null) return false;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(ISaveToQuestionnaire));
            var aggResult = true;
            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is ISaveToQuestionnaire propObject)) continue; // Si null, pas de problème, on saute.
                aggResult &= propObject.Save(screen, screen.ActiveLanguage);
            }

            return aggResult;
        }

        /// <summary>
        /// Sauvegarde dans le questionnaire toutes les valeurs désirables des propriétés avec la bonne interface lors d'un changement de page.
        /// </summary>
        /// <param name="screen">L'écran sur le point de changer qui doit mettre des réponse dans le questionnaire.</param>
        /// <returns>TRUE si toutes les opérations se sont bien exécutées.</returns>
        public static bool OnLoadAllProperties(this AbstractScreen screen)
        {
            if (screen == null) return false;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IOnLoad));
            var aggResult = true;
            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IOnLoad propObject)) continue; // Si null, pas de problème, on saute.
                aggResult &= propObject.OnLoad(screen);
            }

            return aggResult;
        }

        /// <summary>
        /// Vérifie l'état global des propriétés avec état de cet écran.
        /// Certaines propriétés ont un état qu'il doit être possible de vérifier (comme par example, des boîtes de textes sont obligatoires
        /// et doivent être remplies). Des états invalides, et leur messages d'erreurs appropriés pour y remédier, sont tous appellés et
        /// colligés dans cette méthode.
        /// Les résultats ne sont pas aggrégés: ceci est la tâche de l'appellant.
        /// </summary>
        /// <param name="screen">L'écran qui doit valider l'état de ses propriétés.</param>
        /// <returns>La liste des résultats de cette vérification. Non nul, mais peut être vide.</returns>
        public static IList<ScreenEvaluationResult> EvaluateStateAllProperties(this AbstractScreen screen)
        {
            if (screen == null) return new List<ScreenEvaluationResult>();

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IEvaluateDefinitionsState));

            var res = new List<ScreenEvaluationResult>();

            foreach (var p in props)
            {
                if (!(p.GetMethod.Invoke(screen, null) is IEvaluateDefinitionsState propObject)) continue; // Si null, pas de problème, on saute.
                res.Add(propObject.EvaluateDefinitionsState(screen.ActiveLanguage));
            }

            return res;
        }

        /// <summary>
        /// Vérifie et active toutes les propriétés qui doivent réagir sur l'événement du timer d'inactivité d'un écran.
        /// </summary>
        /// <param name="screen">L'écran qui a lancé l'événement et dont on doit vérifier les propriétés.</param>
        /// <param name="onTimerEventDefault">S'il n'y a aucune propriété qui supercharge l'événement d'inctivité, on invoque cette méthode.</param>
        /// <returns>TRUE si toutes les propriétés se sont exécutées correctement. N'a pas d'effet sur l'invocation de la méthode par défaut.</returns>
        public static bool OnIntervalTimerEvent(this AbstractScreen screen, Action onTimerEventDefault)
        {
            if (screen == null) return false;

            // On identifie les propriétés implémentant les interfaces désirées.
            var props = ClassUtil.GetPropertiesFromTypes(screen, typeof(IInactiveTimer)).ToList();
            var aggResult = true;
            
            var invokedProps = 0;
            foreach (var p in props)
            {
                var invokRes = p.GetMethod.Invoke(screen, null);
                if (!(invokRes is IInactiveTimer propObject)) continue; // Si null, pas de problème, on saute.
                invokedProps++;
                log.Trace("Calling {1} timer overrides properties. Screen Tag => {0}", screen.Tag, props.Count);
                aggResult &= propObject.OnInactiveTimerEvent(screen);
            }

            // Si aucune propriété de modification de Timer n'a été invoquée (I.E. Elle sont toutes nulles), on invoke l'exécution de base de l'écran.
            if (invokedProps > 0) return aggResult;
            log.Trace("Found no properties in screen that overrides Timer events. Executing basic timer call. Screen Tag => {0}", screen.Tag);
            onTimerEventDefault?.Invoke();
            return true;

        }

        public static void DrawDebugRectangle(Graphics g, Pen pen, Point? point, Size? size)
        {
            if (point == null || size == null) return;
            var x = point.Value.X;
            var y = point.Value.Y;
            var w = size.Value.Width;
            var h = size.Value.Height;
            g.DrawRectangle(pen, x, y,w,h);
        }

        public static void DrawDebugRectangle(Graphics g, Pen pen, Rectangle? zone)
        {
            if (zone == null) return;
            g.DrawRectangle(pen, zone.Value);
        }

        public static void DrawDebugInactiveRectangle(Graphics g, Pen pen, Point? point, Size? size)
        {
            if (point == null || size == null) return;
            var p1 = point.Value;
            var s1 = size.Value;
            g.DrawRectangle(pen, p1.X, p1.Y, s1.Width, s1.Height);

            var p = new []
            {
                new Point(p1.X, p1.Y),
                new Point(p1.X+s1.Width, p1.Y+s1.Height),
                new Point(p1.X+s1.Width, p1.Y), 
                new Point(p1.X, p1.Y+s1.Height) 
            };
            g.DrawLines(pen, p);
        }

        public static void DrawDebugInactiveRectangle(Graphics g, Pen pen, Rectangle? inactiveRectangle)
        {
            if (inactiveRectangle == null) return;
            DrawDebugRectangle(g, pen, inactiveRectangle.Value.Location, inactiveRectangle.Value.Size);
        }

        public static void DrawDebugInactivePoint(Graphics g, Pen pen, Point? p)
        {
            if (p == null) return;
            var point = p.Value;
            
            var pc = new []
            {
                new Point(point.X-10, point.Y+10),
                new Point(point.X+10, point.Y-10),
                new Point(point.X-10, point.Y-10),
                new Point(point.X+10, point.Y+10)
            };
            g.DrawLines(pen, pc);
        }

        public static void DrawDebugLines(Graphics g, Pen pen, Point[] coords)
        {
            if (coords == null || coords.Length < 2) return;
            g.DrawLines(pen, coords);
        }
    }
}