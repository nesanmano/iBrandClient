using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using static NomadLogic.Common.Ui.Keyboard.KeyboardKeyValueType;

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Représente une clé de clavier pour la forme UI d'un clavier "touch" dans nos écrans. Contient un peu de logique pour calculer sa valeur et s'il faut
    /// la représenter comme image ou comme chaîne de caractère.
    /// </summary>
    public class KeyboardKey
    {
        private readonly Dictionary<KeyboardKeyValueType, KeyboardKeyValue> _mapValues = new Dictionary<KeyboardKeyValueType, KeyboardKeyValue>();

        /// <summary>
        /// Constructeur pour des clés invariables, qui ne contiennent qu'un type de valeur sans options de modification. Applicable généralement
        /// uniquement à certaines touches de contrôles, comme la tabulation, le Shift, entrée et autres.
        /// Cette clé aura automatiquement un largeur de 1.
        /// </summary>
        /// <param name="key">L'équivalent énumératif de la "clé" clavier.</param>
        /// <param name="keyType">Le type de clé. Typiquement, détermine si c'est un clé de caractère ou une clé de contrôle (comme SHIFT)</param>
        /// <param name="normalShowValue">Valeur affichable de base pour la clé.</param>
        /// <param name="normalSendValue">Valeur d'envoi système pour la clé.</param>
        public KeyboardKey(Key key, KeyType keyType, [NotNull] string normalShowValue, string normalSendValue = null) : this(key, keyType, 1, normalShowValue, normalSendValue) { }

        /// <summary>
        /// Constructeur avec une valeur de clé normale obligatoire, suivi optionnellement d'une série d'autres clés de modifications.
        /// Cette clé aura automatiquement un largeur de 1.
        /// </summary>
        /// <param name="key">L'équivalent énumératif de la "clé" clavier.</param>
        /// <param name="keyType">Le type de clé. Typiquement, détermine si c'est un clé de caractère ou une clé de contrôle (comme SHIFT)</param>
        /// <param name="standard">Structure pour le code normal. La type référencé n'est pas important: cette structure sera stocké comme modificateur normal.</param>
        /// <param name="others">Séquence de Structures de modifications additionnelles. Toute autre modificateur "Normal" sera ignoré, tout comme les doublons à l'interne.</param>
        public KeyboardKey(Key key, KeyType keyType, KeyboardKeyValue standard, params KeyboardKeyValue[] others) : this(key, keyType, 1, standard, others) { }

        /// <summary>
        /// Constructeur pour des clés invariables, qui ne contiennent qu'un type de valeur sans options de modification. Applicable généralement
        /// uniquement à certaines touches de contrôles, comme la tabulation, le Shift, entrée et autres.
        /// </summary>
        /// <param name="width">Indique le "nombre" d'espacement normal de cette clé.
        /// Par exemple, si cette valeur est 2, elle occupe autant d'espace que deux clés plus leur espacement.</param>
        /// <param name="key">L'équivalent énumératif de la "clé" clavier.</param>
        /// <param name="keyType">Le type de clé. Typiquement, détermine si c'est un clé de caractère ou une clé de contrôle (comme SHIFT)</param>
        /// <param name="normalShowValue">Valeur affichable de base pour la clé.</param>
        /// <param name="normalSendValue">Valeur d'envoi système pour la clé.</param>
        public KeyboardKey(Key key, KeyType keyType, int width,[NotNull] string normalShowValue, string normalSendValue = null) : this(key, keyType, width) {
            _mapValues.Add(Normal, new KeyboardKeyValue(Normal, normalShowValue, normalSendValue));
        }

        /// <summary>
        /// Constructeur avec une valeur de clé normale obligatoire, suivi optionnellement d'une série d'autres clés de modifications.
        /// </summary>
        /// <param name="width">Indique le "nombre" d'espacement normal de cette clé.
        /// Par exemple, si cette valeur est 2, elle occupe autant d'espace que deux clés plus leur espacement.</param>
        /// <param name="key">L'équivalent énumératif de la "clé" clavier.</param>
        /// <param name="keyType">Le type de clé. Typiquement, détermine si c'est un clé de caractère ou une clé de contrôle (comme SHIFT)</param>
        /// <param name="standard">Structure pour le code normal. La type référencé n'est pas important: cette structure sera stocké comme modificateur normal.</param>
        /// <param name="others">Séquence de Structures de modifications additionnelles. Toute autre modificateur "Normal" sera ignoré, tout comme les doublons à l'interne.</param>
        public KeyboardKey(Key key, KeyType keyType, int width, KeyboardKeyValue standard, params KeyboardKeyValue[] others) : this(key, keyType, width)
        {
            _mapValues.Add(Normal, new KeyboardKeyValue(Normal, standard.ShowValue, standard.SendValue));
            
            if (others == null) return;
            foreach (var kkv in others) {
                if (!_mapValues.ContainsKey(kkv.ValueType)) _mapValues.Add(kkv.ValueType, kkv);
            }
        }

        /// <summary>
        /// Constrcuteur de base, caché.
        /// </summary>
        /// <param name="key">La clé clavier.</param>
        /// <param name="keyType">Le type de clé.</param>
        /// <param name="width">Largeur, en nombre de touches.</param>
        protected KeyboardKey(Key key, KeyType keyType, int width) {
            Width = width;
            Key = key;
            KeyType = keyType;
        }

        /// Identifie le type de clé.
        public KeyType KeyType { get; }

        /// Valeur énumérative.
        public Key Key { get; }

        /// Largeur de la clé, en nombre de boutons (pas en pixels).
        public int Width { get; }

        /// <summary>
        /// Obtient la valeur de la clé à envoyer au système. Cette méthode est une façon rapide de déterminer quelle est la valeur à afficher
        /// selon l'état actuel du clavier, à savoir les trois valeurs de contrôles.
        /// Si une clé à une valeur système définie, ce code aura précédence sur tout le reste. Cette valeur n'est pas influencée par les variable
        /// booléennes du clavier. Sinon, on envoi la valeur affichable.
        /// </summary>
        /// <param name="isCapital">Est-ce que la clavier est présentement capitalisé?</param>
        /// <param name="isCapLock">Est-ce que le bouton LEFT_SHIFT est enfoncé?</param>
        /// <param name="isAccentuated">Est-ce que l'on doit utiliser les accents?</param>
        /// <returns>Le code de la clé en string.</returns>
        public string GetValue(bool isCapital, bool isCapLock, bool isAccentuated) {

            var val2 = GetValueFromModifiers(isCapital, isCapLock, isAccentuated);
            return val2.SendValue ?? val2.ShowValue;
        }

        /// <summary>
        /// Obtient le code affichable de la clé, selon l'état du clavier.
        /// </summary>
        /// <param name="isCapital">Est-ce que la clavier est présentement capitalisé?</param>
        /// <param name="isCapLock">Est-ce que le bouton LEFT_SHIFT est enfoncé?</param>
        /// <param name="isAccentuated">Est-ce que l'on doit utiliser les accents?</param>
        /// <returns>La code à afficher sur le Label de la clé..</returns>
        public string GetVisual(bool isCapital, bool isCapLock, bool isAccentuated)
        {
            return GetValueFromModifiers(isCapital, isCapLock, isAccentuated).ShowValue;
        }

        private KeyboardKeyValue GetValueFromModifiers(bool isCapital, bool isCapLock, bool isAccentuated) {
            if (isCapLock)
            {
                var val = GetValueFromKeyboardCapslockModifiers(isCapital, isAccentuated, out _);
                return val;
            }

            var mods = Normal;
            if (isCapital) mods = Shifted;
            if (isAccentuated) mods |= Accentuated;

            return _mapValues.ContainsKey(mods)
                      ? _mapValues[mods]
                      : _mapValues[Normal];
        }

        private KeyboardKeyValue GetValueFromKeyboardCapslockModifiers(bool isCapital, bool isAccentuated, out KeyboardKeyValueType mods)
        {
            mods = KeyType == KeyType.Letters
                                       ? Shifted & Accentuated
                                       : Accentuated;
            if (isCapital && isAccentuated)
            {
                mods = KeyType == KeyType.Letters
                           ? Accentuated
                           : Shifted;
            }
            else if (isCapital)
            {
                mods = KeyType == KeyType.Letters
                           ? Normal
                           : Shifted;
            }
            if (!isAccentuated)
            {
                mods = KeyType == KeyType.Letters
                           ? Shifted
                           : Normal;
            }

            return _mapValues.ContainsKey(mods)
                  ? _mapValues[mods]
                  : _mapValues[Normal];
        }
    }

    /// <summary>
    /// Structure pour contenir les valeur de clé selon les modificateurs systèmes.
    /// </summary>
    public struct KeyboardKeyValue : IEquatable<KeyboardKeyValue>
    {
        /// <summary>
        /// Constructeur principal. Pour les codes d'envoi système, voir ici:
        /// https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.send(v=vs.110).aspx
        /// </summary>
        /// <param name="type">Les modificateurs associée aux valeurs.</param>
        /// <param name="showValue">Valeur d'affichage, à montrer sur le clavier.</param>
        /// <param name="sendValue">Valeur d'envoi système.</param>
        public KeyboardKeyValue(KeyboardKeyValueType type, string showValue, string sendValue = null)
        {
            ValueType = type;
            ShowValue = showValue;
            SendValue = sendValue;
        }

        /// Valeur d'envoi système. Si nulle, est égal à ShowValue
        public string SendValue { get; }

        /// Valeur d'affichage
        public string ShowValue { get; }

        /// Type de modificateur.
        public KeyboardKeyValueType ValueType { get; }

        #region Implementation of IEquatable<KeyboardKeyValue>

        /// <inheritdoc />
        public bool Equals(KeyboardKeyValue other)
        {
            return ValueType == other.ValueType;
        }

        #endregion

        #region Equality members

        /// <inheritdoc />
        public override bool Equals(object other)
        {
            if (other is KeyboardKeyValue)
                return Equals((KeyboardKeyValue)other);
            return false;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)ValueType;
        }

        #endregion
    }

    /// <summary>
    /// Type de valeur données à une touche. Cette énumération est basé sur le concept de "flags", ce qui fait que vous pouvez tester
    /// dela façon suivante "val == Accentuated|Shifted" pour l'équivalent de "val == Accentuated || val == Shifted".
    /// </summary>
    [Flags]
    public enum KeyboardKeyValueType
    {
        /// Valeur sans modifieurs.
        Normal = 0,
        /// Modificateur pour les accents.
        Accentuated = 1,
        /// Modificateur pour le "shift/majuscule"
        Shifted = 2,
    }
}
