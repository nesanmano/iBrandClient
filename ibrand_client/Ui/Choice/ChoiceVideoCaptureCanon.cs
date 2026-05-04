using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util.Trigger.LaserBeam;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    /// <summary>
    /// Cette classe englobe les différentes façon de saisir un fichier vidéo d'une caméra à partir de l'interface usager d'un choix.
    /// <br/>Spécifiquement, ce choix interagit avec une caméra de type Canon exclusivement.
    /// </summary>
    public class ChoiceVideoCaptureCanon : Choice
    {
        /// <summary>
        /// L'interface usager du choix.
        /// </summary>
        private readonly VideoCaptureCanonTextBox2 _videoField;

        public ChoiceVideoCaptureCanon()
        {
            _videoField = new VideoCaptureCanonTextBox2();
        }

        /// <summary>
        /// Assigne une action à entreprendre quand un fichier vidéo est prêt.
        /// </summary>
        /// <param name="listener">L'action à entreprendre</param>
        public void SetActionOnVideoFileReady(Action listener)
        {
            _videoField.VideoFileReady += listener;
        }

        /// <summary>
        /// Assigne (ou enlève dans le cas d'un paramètre nul) un gestionnaire de faisceau lumineux au choix.
        /// Quand on assigne un tel gestionnaire, l'option de partir manuellement la capture vidéo est déactivée.
        /// </summary>
        /// <param name="lbm">La référence du gestionnaire.</param>
        public void SetLaserBeamManager(LaserBeamManager lbm)
        {
            _videoField.SetLaserBeamManager(lbm);
        }

        /// <summary>
        /// Donne le temps d'enregistrement pour la capture vidéo quand le choix est associé à un gestionnaire de laser.
        /// Voir la méthode "SetLaserBeamManager(LaserBeamManager)".
        /// </summary>
        /// <param name="secs"></param>
        public void SetRecordTime(float secs)
        {
            _videoField.VideoCaptureTime = secs;
        }

        /// <summary>
        /// Indique si on doit afficher les boutons de démarrage et arrêt de la prise vidéo. Vrai par défaut.
        /// </summary>
        /// <param name="value"></param>
        public void ShowStartStopButtons(bool value)
        {
            _videoField.ShowStartStopButtons = value;
        }

        /// <summary>
        /// Indique si l'interface usager du choix est utilisable par l'usager.
        /// </summary>
        public override bool Enabled
        {
            set => _videoField.Enabled = value;
            internal get => _videoField.Enabled;
        }

        public void SetCountDown(string countDown)
        {
            _videoField.SetCountDown(int.Parse(countDown));
        }

        public override void SetFocusable(bool focusable)
        {
            _videoField.Enabled = focusable;
        }

        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _videoField.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _videoField.Name = "C_" + No;

            if (availableWidth > Constants.MAX_WIDTH)
                availableWidth = Constants.MAX_WIDTH;

            var videoFieldWidth = availableWidth;

            _videoField.SetSize(new Size(videoFieldWidth - (2 * Constants.CHOICE_MARGIN), _videoField.Size.Height));

            questionPanel.Controls.Add(_videoField);

            return _videoField.Size.Height + Constants.CHOICE_PAD;
        }

        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _videoField.Size.Height + Constants.CHOICE_PAD;
        }

        /// <summary>
        /// Indique si une réponse est contenue dans le choix.
        /// </summary>
        /// <returns>TRUE si du texte, quel qu'il soit, est présent dans la réponse.</returns>
        public override bool IsAnswered()
        {
            
            return _videoField.Text.Length > 0;
        }

        public override string GetAnswerText()
        {
            return _videoField.Text;
        }

        /// <summary>
        /// Réinitialise le choix au complet.
        /// </summary>
        public override void ResetAnswer()
        {
            _videoField.Reset();
        }

        public override void SetFocus()
        {
            _videoField.Focus();
        }

        public override bool HasFocus()
        {
            return _videoField.Focused;
        }

        public override void SetText()
        {
            //do nothing
        }

        public override void SetAnswer(Answer answer)
        {
            throw new NotImplementedException();
        }

        public override void Dispose()
        {
            _videoField.DisconnectCamera();
            _videoField.SetLaserBeamManager(null);
        }
    }
}
