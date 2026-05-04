using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace NomadLogic.Common.Ui.Localization
{
    /// <summary>
    /// Classe de base pour supporter un changement de texte automatique en cas de changement de langue. Basé sur la propriété "TAG" d'un contrôle,
    /// on va chercher son équivalent localisé dans un RessourceManager explicitement géré dans les classes dériviées.
    /// Ne pas invoquer "InitializeComponent()" dans le constructeur de la classe dérivée. En fait, on ne devrait pas utiliser de constructeur point, mais
    /// uniquement faire l'initialisation dans la méthode "OnLoad" surchargée.
    /// Il faut également que dans le designer de la classe dérivée, on change la signature de "private void InitializeComponent()" pour
    /// "protected override InitializeComponent()". Idéalement, il faut le faire une fois que l'on a terminé de faire le design visuel car
    /// Visual Studio est incapable de visualiser une Forme si elle est héritée ou modifiée d'un quelconque façon. SI on veut de nouveau avoir accès
    /// à l'éditeur visuel, il faut remettre l'héritage à Form et la signature du InitializeComponent à la forme originale. On peut éditer les valeurs
    /// même s'il y a des problèmes de compilation.
    /// </summary>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    public abstract partial class LocalizedForm : Form
    {
        /// <summary>
        /// Constructeur principal.
        /// </summary>
        protected LocalizedForm()
        {
            // make sure we Get notification of any culture change
            InitializeComponent();
            InitializeControlTags();
        }

        /// <summary>
        /// Culture de base pour le français.
        /// </summary>
        [PublicAPI]
        public static CultureInfo FrCulture { get; } = new CultureInfo("fr-CA");
        /// <summary>
        /// Culture de base pour l'anglais.
        /// </summary>
        [PublicAPI]
        public static CultureInfo EnCulture { get; } = new CultureInfo("en");

        /// <summary>
        /// Mettez dans cette méthode les assignations de la propriété TAG aux contrôles que vous voulez automatiquement
        /// traduire selon le changement de langue.
        /// </summary>
        protected abstract void InitializeControlTags();

        /// <summary>Updates the forms resources - the text of the controls, images and the menu item texts.</summary>
        [PublicAPI]
        public void UpdateResources()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(UpdateResources));
                return;
            }

            // On fait le titre de la forme elle-même si on lui a assigné un Tag.
            if (!string.IsNullOrWhiteSpace(Tag?.ToString()))
            {
                Text = GetLocalizedTextFromManager(Tag.ToString());
            }

            UpdateControls(this);
            if (Menu != null)
                UpdateMenus(Menu.MenuItems);
            UpdateCustomControls();
        }

        /// <summary>
        /// Si on a besoin de mettre à jour d'autres choses que les contrôles de base, on peut étendre cette méthode.
        /// </summary>
        protected virtual void UpdateCustomControls()
        {
            // Rien par défaut.
        }

        /* TODO Ne fonctionne pas. Il faut un override pour traiter les items manuellement */
        private void UpdateMenus(Menu.MenuItemCollection mc)
        {
            foreach (ToolStripMenuItem m in mc)
            {                
                try
                {
                    m.Text = GetLocalizedTextFromManager(m.Tag.ToString());
                }
                catch (Exception)
                {
                    // log the error if desired
                }
            }
        }

        protected abstract string GetLocalizedTextFromManager(string tag);
        
        private void UpdateControls(Control control)
        {
            // update the controls that we can do automatically
            if ((control is Label ||
                control is Button ||
                control is CheckBox ||
                control is TabPage ||
                control is RadioButton) && control.Tag != null)
            {
                try
                {
                    control.Text = GetLocalizedTextFromManager(control.Tag.ToString());
                }
                catch 
                { 
                    // log the error if desired
                }
            }

            foreach (Control c in control.Controls)
            {
                // Get any nested controls
                UpdateControls(c);
            }
        }

        /// <summary>
        /// À appeller à chaque fois que la langue change.
        /// </summary>
        /// <param name="sender">Pas utilisé.</param>
        /// <param name="e">Pas utilisé.</param>
        protected void OnChangeLanguage(object sender, EventArgs e)
        {
            UpdateResources();
        }
    }
}