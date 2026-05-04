using System;
using System.Collections.Generic;
using System.Drawing;
using JetBrains.Annotations;
using NLog;
using NomadLogic.Common.Util;
using NomadLogic.Multimedia;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Ce système de décompte affiche des fichiers d'images spécifiques à un pallier de temps.
    /// Au fil et à mesure du décompte, dès que l'on passe un pallier, on affiche une image spécifique à un pallier.
    /// Chaque pallier est identifié avec un nombre de secondes et une image. L'algorithme choisi le pallier le plus élevé,
    /// où la valeur de temps est inférieure ou égale au temps actuel. Si le temps est supérieur, on prend toujours la valeur la plus élevée.
    /// Exemple: Si on a les palliers suivants: { {"a.jpg", 100}, {"b.jpg", 50}, {"c.jpg", 5}, {"d.jpg", 1} }
    /// Avec la valeur de temps 51 et plus, l'image A sera choisie. Entre 50 et 6, l'image B sera choisie. Entre 5 et 2, C sera choisi. La valeur 1
    /// choisira l'image D. À 0 ou moins, l'affichage ne se produit plus, selon le contrat de la méthode Draw.
    /// </summary>
    public class CountdownDecoratorImageSteps : ICountdownDecorator
    {
        // ReSharper disable once UnusedMember.Local
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Structure pour un pallier d'images.
        /// </summary>
        protected struct ImageStepData : IComparable<ImageStepData>
        {
            /// <summary>
            /// L'image.
            /// </summary>
            public Bitmap Bmp { get; set; }
            /// <summary>
            /// Le temps limite supérieur de ce pallier.
            /// </summary>
            public uint Step { get; set; }
            /// <summary>
            /// Point supérieur gauche pour afficher l'image.
            /// </summary>
            public Point DrawPoint { get; set; }

            /// <inheritdoc/>
            public int CompareTo(ImageStepData other)
            {
                return (int)(other.Step - Step); // L'inverse de d'habitude. Va ordonner de façon décroissante.
            }

            /// <inheritdoc/>
            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType()) return false;
                var arg = (ImageStepData)obj;
                return Step != arg.Step;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return Step.GetHashCode();
            }
        }

        private bool _isInitialized;
        private readonly SortedSet<ImageStepData> _stepSet = new SortedSet<ImageStepData>();
        private Bitmap _recordingImage;
        private Point? _recordingPoint;

        /// <summary>
        /// Décrit chaque pallier d'affichage. Indique le fichier à aller chercher (en mode relatif ou non), le temps du pallier, et s'il faut chercher l'image
        /// dans le sous-répertoire de langue (mode relatif seulement)
        /// </summary>
        [PublicAPI]
        public List<ImageStepInfo> Steps { get; set; }

        /// <summary>
        /// Indique le temps maximal, en secondes, que l'on doit afficher un pallier une fois franchit. Par défaut zéro, et toute valeur inférieure à 1
        /// indique qu'il faut afficher le pallier continuellement.
        /// </summary>
        [PublicAPI]
        public int MaxStepShowTime { get; set; }

        /// <summary>
        /// Si présent, cette classe contient les informations pour générer des images à partir de texte et le contenir dans une image pré-rendue.
        /// Remplacement la lecture pure et simple de l'information de Steps comme image.
        /// Les images générée par cette option ne sont pas modifié par les filtres bitmap de la méthode d'Initialisation.
        /// </summary>
        [PublicAPI]
        public ProceduralImageTextInfo ImageGenerationInfo { get; set; }

        public GetImageFileDelegate GetImageFileMethod { get; set; }

        public GetImageFileDelegate GetImageFileNoneLanguageMethod { get; set; }

        public Point? UpperCenterPointNumber { get; set; }

        public Point? UpperCenterPointMessage { get; set; }

        public int MaxTimeCountdown { get; set; } = 5;

        public void Init(BitmapModificationFilter? bmf)
        {
            _isInitialized = false;
            if (MaxTimeCountdown < 1 || GetImageFileMethod == null || UpperCenterPointNumber == null)
            {
                throw new System.Exception("Cannot initialize. Not all Properties were set correctly.");
            }

            _recordingImage?.Dispose();
            _recordingImage = ImageUtil.ModifyPicture(GetImageFileMethod("recording.png"), ColorSubstitutionFilterGroup.Icon, bmf);
            _recordingPoint = null;
            if (UpperCenterPointMessage != null) { _recordingPoint = new Point(UpperCenterPointMessage.Value.X - (_recordingImage.Width / 2), UpperCenterPointMessage.Value.Y); }

            foreach (var step in _stepSet)
            {
                step.Bmp?.Dispose();
            }
            _stepSet.Clear();

            foreach (var info in Steps)
            {
                Bitmap result = null;

                if (ImageGenerationInfo == null)
                {
                    // Chargement en tant que fichier.
                    // Chargement d'image relatif au répertoire data (ou local), ou un chargement absolu?
                    
                    if (!string.IsNullOrWhiteSpace(info.Data))
                    {
                        Bitmap bmp;
                        if (!info.Data.Contains(":"))
                        {
                            bmp = !info.IsLanguageDependent
                                ? GetImageFileNoneLanguageMethod(info.Data)
                                : GetImageFileMethod(info.Data);
                        }
                        else
                        {
                            bmp = FileUtil.LoadBitmap(info.Data);
                        }

                        result = ImageUtil.ModifyPicture(bmp, ColorSubstitutionFilterGroup.Countdown, bmf);
                    }
                }
                else
                {
                    result = ImageUtil.CreateProceduralTextBitmap(ImageGenerationInfo, info.Data, null, ImageGenerationInfo.BitmapSize.Width);
                }

                var step = new ImageStepData
                {
                    Bmp = result,
                    Step = info.TimeSec
                };

                // Vérification
                if (step.Bmp == null && !string.IsNullOrWhiteSpace(info.Data))
                {
                    string message;
                    if (ImageGenerationInfo != null) { message = "Invalid Text image generation data."; }
                    else { message = "Image file does not exists: " + info.Data; }
                    throw new System.Exception(message);
                }

                if (UpperCenterPointNumber != null)
                    step.DrawPoint = new Point(
                        UpperCenterPointNumber.Value.X - (step.Bmp?.Width ?? 0) / 2,
                        UpperCenterPointNumber.Value.Y);

                _stepSet.Add(step);
            }

            _isInitialized = true;
        }

        public bool DoNotShowWhenRecording { get; set; }

        public void Draw(Graphics g, int timeBeforeRecord, int timeToRecord, bool isRecording)
        {
            if (DoNotShowWhenRecording && isRecording) return;

            if (!_isInitialized) return;
            ImageStepData? step;
            if (timeBeforeRecord != 0)
            {
                step = GetStepPicture(timeBeforeRecord);
                if (step?.Bmp != null)
                {
                    g.DrawImage(step.Value.Bmp, new Rectangle(step.Value.DrawPoint, step.Value.Bmp.Size), 0, 0, step.Value.Bmp.Width, step.Value.Bmp.Height, GraphicsUnit.Pixel);
                }
            }

            if (timeToRecord == 0 || !isRecording) return;

            if (_recordingPoint != null)
            {
                g.DrawImage(_recordingImage, new Rectangle(_recordingPoint.Value, _recordingImage.Size), 0, 0, _recordingImage.Width, _recordingImage.Height, GraphicsUnit.Pixel);
            }
            step = GetStepPicture(timeToRecord);
            if (step?.Bmp != null)
            {
                g.DrawImage(step.Value.Bmp, new Rectangle(step.Value.DrawPoint, step.Value.Bmp.Size), 0, 0, step.Value.Bmp.Width, step.Value.Bmp.Height, GraphicsUnit.Pixel);
            }
        }

        private ImageStepData? GetStepPicture(int timeInSeconds)
        {
            using (var ite = _stepSet.GetEnumerator())
            {

                if (!ite.MoveNext()) { return null; }

                var result = ite.Current;

                while (ite.MoveNext())
                {
                    var tmp = ite.Current;
                    if (tmp.Step < timeInSeconds)
                    {
                        break; // On a déjà le plus haut pallier valide.
                    }

                    // On prend ce pallier-là, mais on vérifie les prochains.
                    result = tmp;
                }

                // On fait maintenant une vérification avec le temps maximal d'affichage. Si cela fait plus que X secondes que l'on
                // est dans le même pallier, on cesse d'afficher le pallier. Par défait -1, ce qui signifit que l'on affiche le pallier
                // constamment.
                if (MaxStepShowTime > 0 && (result.Step - timeInSeconds) > MaxStepShowTime) { return null; }

                return result;
            }
        }
    }
}