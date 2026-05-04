using System;
using System.Drawing;
using System.Windows.Forms;
using NomadLogic.Common;
using NomadLogic.Common.Respondent;

namespace NomadLogic.iBrandDesktop.Ui.Choice
{
    /// <summary>
    /// Choix pour CameraRing, sans égard pour le type d'API de caméra.
    /// </summary>
    public abstract class ChoiceCameraRing : Choice
    {
        /// <summary>
        /// Champ de photo pour camera ring.
        /// // TODO Decrait être générique. Présentement lié à l'API Canon.
        /// </summary>
        protected CameraRingCanonTextBox _photo_field;

        /// <summary>
        /// Nombre de fichiers d'images nécessaires pour que le choix soit valide.
        ///   </summary>
        public int MinPictures { get; set; } = 1;

        /// <summary>
        /// Nombre de caméra nécessaire pour que le choix soit valide.
        /// </summary>
        public int MinCameras { get; set; } = 1;
        
        /// <inheritdoc/>
        public override bool Enabled
        {
            set
            {
                _photo_field.Enabled = value;
            }

            internal get { return _photo_field.Enabled; }
        }


        /// <summary>
        /// Assigne une action à entreprendre quand un fichier photo est prêt.
        /// </summary>
        /// <param name="listener">L'action à entreprendre</param>
        public void SetActionOnPictureFileReady(Action listener)
        {
            _photo_field.PictureFileReady += listener;
        }

        /// <inheritdoc/>
        public override void SetFocusable(bool focusable)
        {
            if (_photo_field.InvokeRequired)
            {
                _photo_field.Invoke(new Action(() => { SetFocusable(focusable); }));
                return;
            }
            _photo_field.Enabled = focusable;
        }

        /// <inheritdoc/>
        public override int AddToUserControl(Panel questionPanel, int x, int y, Graphics g, int availableWidth)
        {
            _photo_field.Location = new Point(x + Constants.CHOICE_MARGIN, y);

            _photo_field.Name = "C_" + No;

            if (availableWidth > Constants.MAX_WIDTH)
                availableWidth = Constants.MAX_WIDTH;

            _photo_field.SetSize(new Size(availableWidth, _photo_field.Size.Height));

            questionPanel.Controls.Add(_photo_field);

            return _photo_field.Size.Height + Constants.CHOICE_PAD;
        }

        /// <inheritdoc/>
        public override int GetHeight(int availableWidth, Graphics g)
        {
            return _photo_field.Size.Height + Constants.CHOICE_PAD;
        }

        /// <inheritdoc/>
        public override bool IsAnswered()
        {
            if (_photo_field.Text?.Length <= 0) return false;

            if (MinPictures < 1 && MinCameras < 1) return false;

            var vals = new[]
            {
                _photo_field.CurrentCameraCount,
                MinCameras,
                _photo_field.CurrentPicturesCount,
                MinPictures
            };

            return vals[0] >= vals[1] && vals[2] >= vals[3];
        }

        /// <inheritdoc/>
        public override string GetAnswerText()
        {
            return _photo_field.Text;
        }

        /// <inheritdoc/>
        public override void ResetAnswer()
        {
            _photo_field.Reset();
            CheckAndUpdateDefaultAnswer();
        }

        /// <inheritdoc/>
        public override void SetFocus()
        {
            _photo_field.Focus();
        }

        /// <inheritdoc/>
        public override bool HasFocus()
        {
            return _photo_field.Focused;
        }

        /// <inheritdoc/>
        public override void SetText()
        {
            //do nothing
        }

        /// <inheritdoc/>
        public override void SetAnswer(Answer answer)
        {
            // N'est pas utilisé.
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _photo_field.Dispose();
        }
    }
}
