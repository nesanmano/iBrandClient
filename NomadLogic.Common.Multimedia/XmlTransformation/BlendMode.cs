using JetBrains.Annotations;

namespace NomadLogic.Multimedia.XmlTransformation
{
    /// Représente un algorithme de combinaison de valeur de pixel.
    /// Ces algorithmes sont souvent représenté dans des logiciels de manipulation d'image par rastérisation (Photoshop, GIMP).
    /// Grosso-modo, ils indiquent comment on calcule un pixel lorsque celui-ci est le composite d'un étage semi-transparent par dessus un autre
    /// étage.
    /// L'algorithme varie selon le format du pixel, et donc l'implémentation selon certains formats sera laissé à d'autres classes. Cette énumaration
    /// ne fait que donner les mode les plus communs.
    /// Dasn tous les examples d'algorithmes, 'a' est l'étage du bas et 'b' est l'étage du dessus.
    /// Voir ici pour plus d'explications: https://en.wikipedia.org/wiki/Blend_modes
    /// Ici est un trésor d'information: https://stackoverflow.com/questions/5919663/how-does-photoshop-blend-two-images-together
    [PublicAPI]
    public enum BlendMode
    {
        /// Applique seulement le pixel de l'étage du dessus, laisse apparaître l'étage d'en-dessous seulement avec un alpha.
        /// Algorithme: f(a, b)=alpha(b, a)
        Normal,
        /// Additionne les deux valeurs, clamp à leur maximum si pas de rotation.
        Addition,
        /// Soustrait les deux valeurs, clamp à leur minimum si pas de rotation.
        Substract,
        /// Donne le 'delta' absolu (différence) entre les deux valeurs. Avec la valeur maximale, aucune modification. Avec minimum, inverse.
        Difference,
        /// Multiplie les valeurs ensemble, donnant un effet de noircissement. Plus les valeurs sont foncées, plus le résultat sera noir.
        /// Algorithme: f(a, b) = ab 
        Multiply,
        /// Inverse, multiplie, pujis inverse à nouveau les valeurs ensemble, donnant un effet d'éclaicissement.
        /// Plus les valeurs sont foncées, plus le résultat sera clair. C'est l'inverse du mode multiplicatif.
        /// Algorithme: f(a, b) = 1 - (1 - a)(1 - b)
        Screen,
        /// Une combinaison de soustraction et de multiplcation. Selon si les partie de l'étage du bas sont foncé ou clair, les parties de l'étage supérieur
        /// seront respectivement plus clair ou plus foncé. En somme, accentue l'effet de contraste des couleurs de b basé sur a.
        /// f(a, b) = { si a clair => 2ab; sinon => 1 - 2(1 - a)(1 - b) }
        Overlay,
        Ligthen,
        Darken,
        Average,
        Negation,
        Exclusion,
        SoftLight,
        ColorDodge,
        ColorBurn,
        Reflect,
        Hue,
        Saturation,
        Color,
        Luminosity
        
    }
}