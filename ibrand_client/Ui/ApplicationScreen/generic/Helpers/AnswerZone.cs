using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NomadLogic.Common.Respondent;
using NomadLogic.Common.Util;
using NomadLogic.Common.Util.Math;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Structure pour définir une zone où l'on assigne des réponses à insérer quand il est choisit, et optionnellement
    /// l'endroit où dessiner une image associée quand il est sélectionné.
    /// Également, des options de changements d'écran peuvent être associées aux zones.
    /// </summary>
    [PublicAPI]
    public class AnswerZone : IDisposable, IComparable<AnswerZone>, IEquatable<AnswerZone>
    {
        /// Le chemin vers le fichier de d'image. Ceci est un chemin relatif à partir du répertoire de base d'un questionnaire ou un chemin absolu.
        public string BackgroundImageFilePath { get; set; }

        /// Référence vers l'image chargée en mémoire pour le fond de l'option.
        [JsonIgnore]
        public Bitmap BackgroundImageInstance { get; set; }

        /// Zone pour activer la sélection
        public Rectangle Zone { get; set; }

        /// Référence vers une image qui doit être affichée lorsque la sélection est faite.
        [JsonIgnore]
        public Bitmap SelectionActivatedImage { get; set; }
        
        /// Optionnel. Si on affiche une image lorsque sélectionnée, on l'affiche à cette location. Si nul, on utilise la location de
        /// la zone d'activation.
        public Point? SelectionActivatedImageLocation { get; set; }

        /// Réponses à entrer dans le questionnaire quand on choisit l'option.
        public IList<Answer> AnswersUponSelection { get; set; }

        /// <summary>
        /// Options de changement de page avec conditions. Si non vide, on exécute l'option de changement d'écran lors de la sélection.
        /// </summary>
        public ScreenChangeOptions PageChangeOptions { get; set; }
        
        /// Le chemin vers le fichier de d'image pour la sélection. Ceci est un chemin relatif à partir du répertoire de base d'un questionnaire ou un chemin absolu.
        public string SelectionActivatedImagePath { get; set; }
        
        public override string ToString()
        {
            return ClassUtil.ToStringPublicProperties(this);
        }

        public void Dispose()
        {
            BackgroundImageInstance?.Dispose();
            SelectionActivatedImage?.Dispose();
        }

        #region Equality
        
        public int CompareTo(AnswerZone other)
        {
            if (other == null) return 1;
            
            // Comparaition du rectangle
            var zoneComp = new RectangleComparer();
            var res = zoneComp.Compare(Zone, other.Zone);
            if (res != 0) return res;

            // Comparaison des réponses
            res = AnswersUponSelection.CompareEnumerables(other.AnswersUponSelection);
            if (res != 0) return res;

            // Comparaison des changements de page.
            res = PageChangeOptions.CompareTo(other.PageChangeOptions);
            if (res != 0) return res;

            // Comparaison des images de background
            res = ClassUtil.CompareObjects(BackgroundImageFilePath, other.BackgroundImageFilePath);
            if (res != 0) return res;
            
            // Comparaison des images de background
            res = ClassUtil.CompareObjects(SelectionActivatedImagePath, other.SelectionActivatedImagePath);
            return res;
        }

        public bool Equals(AnswerZone other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Zone.Equals(other.Zone) 
                   // ReSharper disable once InvokeAsExtensionMethod
                   && Enumerable.SequenceEqual(AnswersUponSelection, other.AnswersUponSelection) 
                   && Equals(PageChangeOptions, other.PageChangeOptions);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AnswerZone other)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Zone.GetHashCode();
                hashCode = (hashCode * 397) ^ (AnswersUponSelection != null ? AnswersUponSelection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PageChangeOptions != null ? PageChangeOptions.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(AnswerZone left, AnswerZone right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AnswerZone left, AnswerZone right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}