using System;
using System.Drawing;
using System.Drawing.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Condition;
using NomadLogic.Common.Questionnaire;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers.ImageTransforms
{
    /// Options pour la transformation d'une image de présentation.
    /// Souvent à des fins de courriel ou d'affichage sur le Printstation.
    [PublicAPI]
    public class ImageListTransformOptionsFirstPicture
    {
        /// Au lieu d'utiliser l'image prise dans l'écran, on utilise comme source une image dans un numéro de question.
        public int? InputFromQuestion { get; set; }
        // Condition pour exécuter la transformation
        public ICondition Condition { get; set; }
        
        /// Façon de sauvegarder la réponse. Si cette valeur est nulle (elle l'est par défaut),
        /// cela implique que la transformation voulue n'aura pas lieu et que l'invocation soit considérée comme "no-op".
        public DataModelBase SaveDataModel { get; set; }

        /// Qualité de sauvegarde des fichiers, ce qui baisse leur taille en octets. On peut baisser à 80 environ si on veut sauver sur l'espace disque.
        public int JpegSaveQuality { get; set; } = 90;
        /// Référence vers le groupe de sélecteur pour l'image de superposition.
        public IImageSelector OverlaySelector { get; set; }
        /// Taille du canevas pour la création de l'image. Si null, utilise NewSize, puis la taille de l'image si encore null.
        public Size? CanvasSize { get; set; }
        /// Couleur de base pour le remplir le canevas. Est transparent par défaut.
        public Color? CanvasBaseColor { get; set; }
        /// Redimension de l'image à mettre sur le canevas.
        public Size? NewSize { get; set; }
        /// Position où dessiner l'image redimensionnée sur le canevas. Si null, utilise 0,0
        public Point? NewPos { get; set; }
        /// Position où dessiner l'image de surperposition sur le canevas. Si null, utilise 0,0
        public Point? OverlayPos { get; set; }
        /// Définition du Font à utiliser pour dessiner un texte.
        public FontDefinitionInfo FontDefinition { get; set; }
        /// Le numéro de question d'où chercher le texte voulu. TODO Implémenter un ISelector en bonne et dûe forme, comme pour les images.
        public int? QuestionNoToFetchText { get; set; }
        /// Point où commencer à écrire le texte dans le canevas.
        public Point? TextPos { get; set; }
        /// Angle de rotation en degré à donner au texte. 0 degré (ou null) signifit aucune rotation.
        public float? TextRotation { get; set; }
        /// Indique si on centre le texte (verticalement et horizontalement) sur le point de position du texte. Faux par défaut.
        public bool TextCentered { get; set; }
        /// La largeur maximale en pixels pour rendre le texte. S'il dépasse, il sera divisé par ligne, entre les mots. Si null ou zéro, aucune
        /// division ne sera faite.
        public int? TextMaxPixelPerLine { get; set; }
        /// La distance entre chaque ligne de texte, si séparé. Zéro par défaut, mais ne donne pas de bons résultats.
        public float? TextLineOffset { get; set; }
        /// Collection de l'instance de Font personnalisés, lus directement de fichiers.
        [JsonIgnore]
        public PrivateFontCollection CustomFontCollection { get; set; } = new PrivateFontCollection();
        /// Indique si on veit une modification de la case. 1 veut dire UPPER, 2 veut dire LOWER, toute autre valeur est ignorée.
        /// TODO: Énumération
        public int TextCase { get; set; }
        /// Indique le type de Rotation / Inversion à effectuer sur l'image. Null par défaut.
        public RotateFlipType? NewRotateFlip { get; set; }
        /// Indique un fichier d'image précis à utiliser comme background. Sera dessiné après la couleur de fond, mais avant l'image prise.
        public string BackgroundImage { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }
    }
}