using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Cette classe affiche un message et le temps restant pour un décompte via des images PNG dans le répertoire par défaut.
    /// Chaque nombre est créé dynamiquement, mais pas l'image "recording.png", qui doit exister sur disque dans les assets via
    /// la méthode "GetImageFileMethod".
    /// La méthode "GetImageFileNoneLanguageMethod" n'est pas utilisée ici.
    /// </summary>
    public class CountdownDecoratorBasicImages : ICountdownDecorator
    {
        private static readonly Font DEFAULT_FONT = new Font(FontFamily.GenericMonospace , 40);
        private static readonly Size DEFAULT_SIZE = new Size(60, 60);

        private Bitmap _recordingImage;
        private List<Bitmap> RecordCountDownList { get; set; }
        private Point? _recordImagePoint;
        private Point? _recordingPoint;
        private bool _isInitialized;
        
        /// <inheritdoc/>
        public GetImageFileDelegate GetImageFileMethod { get; set; }

        /// <inheritdoc/>
        public GetImageFileDelegate GetImageFileNoneLanguageMethod { get; set; }
        
        /// <inheritdoc/>
        public Point? UpperCenterPointNumber { get; set; }
        
        /// <inheritdoc/>
        public Point? UpperCenterPointMessage { get; set; }

        /// <inheritdoc/>
        public int MaxTimeCountdown { get; set; } = 5;

        public Size ImageSize { get; set; } = DEFAULT_SIZE;

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void Init(BitmapModificationFilter? bmf)
        {
            _isInitialized = false;
            if (GetImageFileMethod == null) {
                throw new System.Exception("Cannot initialize. Not all Properties were set correctly.");
            }

            if (UpperCenterPointMessage.HasValue)
            {
                _recordingImage?.Dispose();
                _recordingImage = ImageUtil.ModifyPicture(GetImageFileMethod("recording.png"),
                    ColorSubstitutionFilterGroup.Icon, bmf);
                _recordingPoint = new Point(UpperCenterPointMessage.Value.X - (_recordingImage.Width / 2),
                    UpperCenterPointMessage.Value.Y);
            }

            if (RecordCountDownList != null)
            {
                foreach (var image in RecordCountDownList)
                {
                    image?.Dispose();
                }
                RecordCountDownList.Clear();
            }

            RecordCountDownList = new List<Bitmap>(MaxTimeCountdown);
            if (MaxTimeCountdown > 0 && UpperCenterPointNumber.HasValue)
            {
                for (var i = 1; i <= MaxTimeCountdown; i++)
                {
                    RecordCountDownList.Add(ImageUtil.CreateSimpleTextImage(i.ToString(), Color.Transparent,
                        Color.White, DEFAULT_FONT, ImageSize));
                }
                _recordImagePoint = new Point(UpperCenterPointNumber.Value.X - (RecordCountDownList[0].Width),
                    UpperCenterPointNumber.Value.Y);
            }
            _isInitialized = true;
        }

        public bool DoNotShowWhenRecording { get; set; }

        public void Draw(Graphics g, int timeBeforeRecord, int timeToRecord, bool isRecording)
        {
            if (!_isInitialized) return;

            if (DoNotShowWhenRecording && isRecording) return;

            var indexTimeBeforeRecord = timeBeforeRecord - 1;
            if (_recordImagePoint.HasValue && RecordCountDownList.Count > indexTimeBeforeRecord)
            {
                if (timeBeforeRecord != 0)
                {
                    var img = RecordCountDownList[indexTimeBeforeRecord];
                    g.DrawImage(img, new Rectangle(_recordImagePoint.Value, img.Size), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel);
                }
            }

            if (timeToRecord == 0 || !isRecording || !_recordingPoint.HasValue || _recordingImage == null) return;

            g.DrawImage(_recordingImage, new Rectangle(_recordingPoint.Value, _recordingImage.Size), 0, 0, _recordingImage.Width, _recordingImage.Height, GraphicsUnit.Pixel);
        }
    }
}