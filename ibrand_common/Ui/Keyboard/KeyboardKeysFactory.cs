using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using static NomadLogic.Common.Ui.Keyboard.Key;
using static NomadLogic.Common.Ui.Keyboard.KeyboardKeyValueType;
using static NomadLogic.Common.Ui.Keyboard.KeyType;

// ReSharper disable InconsistentNaming

namespace NomadLogic.Common.Ui.Keyboard
{
    /// <summary>
    /// Classe statique pour générer des séquences de clés de claviers pour différentes situations.
    /// Voir ici pour les caractères UTF-8: 
    /// http://csbruce.com/software/utf-8.html
    /// Voiri ici pour la commande SendKey de C# et les codes spéciaux qui y sont rattachés: 
    /// https://msdn.microsoft.com/en-us/library/system.windows.forms.sendkeys.send(v=vs.110).aspx
    /// </summary>
    public static class KeyboardKeysFactory
    {
        /* Pour ne pas faire trop de cochoneries en mémoire, on garde le résultat de chaque méthode une fois créée et on renvoit la même
         * séquence. */
        private static KeyboardKey[] _numericKeys;
        private static KeyboardKey[] _normalKeys;
        private static KeyboardKey[] _usKeys;
        private static KeyboardKey[] _emailKeys;

        /// <summary>
        /// Génère des clés pour un clavier purement Numérique.
        /// </summary>
        public static KeyboardKey[] KeyboardNumbersButtons
        {
            get
            {
                if (_numericKeys != null) return _numericKeys;

                var numbersKeys = new List<KeyboardKey>
                {
                    new KeyboardKey(ONE, Others, "1"),
                    new KeyboardKey(TWO, Others, "2"),
                    new KeyboardKey(THREE, Others, "3"),
                    new KeyboardKey(FOUR, Others, "4"),
                    new KeyboardKey(FIVE, Others, "5"),
                    new KeyboardKey(SIX, Others, "6"),
                    new KeyboardKey(SEVEN, Others, "7"),
                    new KeyboardKey(EIGHT, Others, "8"),
                    new KeyboardKey(NINE, Others, "9"),
                    new KeyboardKey(MINUS, Others, "-"),
                    new KeyboardKey(ZERO, Others, "0"),
                    new KeyboardKey(BACKSPACE, Control, "⌫", "{BACKSPACE}")
                };

                _numericKeys = numbersKeys.ToArray();

                return _numericKeys;
            }
        }

        /// <summary>
        /// Génère des clés pour un clavier bilingue, avec accents et symboles, seulement avec des symboles de Fontes UTF-8
        /// </summary>
        public static KeyboardKey[] KeyboardUtf8
        {
            get
            {
                if (_normalKeys != null) return _normalKeys;

                var keys = new List<KeyboardKey>
                {
                    /* 0 */
                    new KeyboardKey(ONE, Others,
                        new KeyboardKeyValue(Normal, "1"),
                        new KeyboardKeyValue(Shifted, "!")),
                    new KeyboardKey(TWO, Others,
                        new KeyboardKeyValue(Normal, "2"),
                        new KeyboardKeyValue(Shifted, "@")),
                    new KeyboardKey(THREE, Others,
                        new KeyboardKeyValue(Normal, "3"),
                        new KeyboardKeyValue(Shifted, "#")),
                    new KeyboardKey(FOUR, Others,
                        new KeyboardKeyValue(Normal, "4"),
                        new KeyboardKeyValue(Shifted, "$")),
                    new KeyboardKey(FIVE, Others,
                        new KeyboardKeyValue(Normal, "5"),
                        new KeyboardKeyValue(Shifted, "%", "{%}")),
                    new KeyboardKey(SIX, Others,
                        new KeyboardKeyValue(Normal, "6"),
                        new KeyboardKeyValue(Shifted, "?")),
                    new KeyboardKey(SEVEN, Others,
                        new KeyboardKeyValue(Normal, "7"),
                        new KeyboardKeyValue(Shifted, "&")),
                    new KeyboardKey(EIGHT, Others,
                        new KeyboardKeyValue(Normal, "8"),
                        new KeyboardKeyValue(Shifted, "*")),
                    new KeyboardKey(NINE, Others,
                        new KeyboardKeyValue(Normal, "9"),
                        new KeyboardKeyValue(Shifted, "(", "{(}")),
                    new KeyboardKey(ZERO, Others,
                        new KeyboardKeyValue(Normal, "0"),
                        new KeyboardKeyValue(Shifted, ")", "{)}")),
                    new KeyboardKey(BACKSPACE, Control, 2, "⌫", "{BACKSPACE}"),
                    new KeyboardKey(Q, Letters,
                        new KeyboardKeyValue(Normal, "q"),
                        new KeyboardKeyValue(Accentuated, "à"),
                        new KeyboardKeyValue(Shifted, "Q"),
                        new KeyboardKeyValue(Accentuated|Shifted, "À")),
                    
                    /* 12 */
                    new KeyboardKey(W, Letters,
                        new KeyboardKeyValue(Normal, "w"),
                        new KeyboardKeyValue(Accentuated, "á"),
                        new KeyboardKeyValue(Shifted, "W"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Á")),
                    new KeyboardKey(E, Letters,
                        new KeyboardKeyValue(Normal, "e"),
                        new KeyboardKeyValue(Accentuated, "â"),
                        new KeyboardKeyValue(Shifted, "E"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Â")),
                    new KeyboardKey(R, Letters,
                        new KeyboardKeyValue(Normal, "r"),
                        new KeyboardKeyValue(Accentuated, "ä"),
                        new KeyboardKeyValue(Shifted, "R"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ä")),
                    new KeyboardKey(T, Letters,
                        new KeyboardKeyValue(Normal, "t"),
                        new KeyboardKeyValue(Accentuated, "æ"),
                        new KeyboardKeyValue(Shifted, "T"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Æ")),
                    new KeyboardKey(Y, Letters,
                        new KeyboardKeyValue(Normal, "y"),
                        new KeyboardKeyValue(Accentuated, "ç"),
                        new KeyboardKeyValue(Shifted, "Y"),
                        new KeyboardKeyValue(Accentuated|Shifted, "¨C")),
                    new KeyboardKey(U, Letters,
                        new KeyboardKeyValue(Normal, "u"),
                        new KeyboardKeyValue(Accentuated, "è"),
                        new KeyboardKeyValue(Shifted, "U"),
                        new KeyboardKeyValue(Accentuated|Shifted, "È")),
                    new KeyboardKey(I, Letters,
                        new KeyboardKeyValue(Normal, "i"),
                        new KeyboardKeyValue(Accentuated, "é"),
                        new KeyboardKeyValue(Shifted, "I"),
                        new KeyboardKeyValue(Accentuated|Shifted, "É")),
                    new KeyboardKey(O, Letters,
                        new KeyboardKeyValue(Normal, "o"),
                        new KeyboardKeyValue(Accentuated, "ê"),
                        new KeyboardKeyValue(Shifted, "O"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ê")),
                    new KeyboardKey(P, Letters,
                        new KeyboardKeyValue(Normal, "p"),
                        new KeyboardKeyValue(Accentuated, "ë"),
                        new KeyboardKeyValue(Shifted, "P"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ë")),
                    new KeyboardKey(UNDERSCORE, Others,
                        new KeyboardKeyValue(Normal, "_"),
                        new KeyboardKeyValue(Shifted, "+", "{+}")),
                    new KeyboardKey(MINUS, Others,
                        new KeyboardKeyValue(Normal, "-"),
                        new KeyboardKeyValue(Shifted, "_")),
                    new KeyboardKey(AT, Others,
                        new KeyboardKeyValue(Normal, "@"),
                        new KeyboardKeyValue(Shifted, "/")),
                    
                    /* 24 */
                    new KeyboardKey(A, Letters,
                        new KeyboardKeyValue(Normal, "a"),
                        new KeyboardKeyValue(Accentuated, "ì"),
                        new KeyboardKeyValue(Shifted, "A"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ì")),
                    new KeyboardKey(S, Letters,
                        new KeyboardKeyValue(Normal, "s"),
                        new KeyboardKeyValue(Accentuated, "í"),
                        new KeyboardKeyValue(Shifted, "S"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Í")),
                    new KeyboardKey(D, Letters,
                        new KeyboardKeyValue(Normal, "d"),
                        new KeyboardKeyValue(Accentuated, "î"),
                        new KeyboardKeyValue(Shifted, "D"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Î")),
                    new KeyboardKey(F, Letters,
                        new KeyboardKeyValue(Normal, "f"),
                        new KeyboardKeyValue(Accentuated, "ï"),
                        new KeyboardKeyValue(Shifted, "F"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ï")),
                    new KeyboardKey(G, Letters,
                        new KeyboardKeyValue(Normal, "g"),
                        new KeyboardKeyValue(Accentuated, "ò"),
                        new KeyboardKeyValue(Shifted, "G"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ò")),
                    new KeyboardKey(H, Letters,
                        new KeyboardKeyValue(Normal, "h"),
                        new KeyboardKeyValue(Accentuated, "ó"),
                        new KeyboardKeyValue(Shifted, "H"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ó")),
                    new KeyboardKey(J, Letters,
                        new KeyboardKeyValue(Normal, "j"),
                        new KeyboardKeyValue(Accentuated, "ô"),
                        new KeyboardKeyValue(Shifted, "J"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ô")),
                    new KeyboardKey(K, Letters,
                        new KeyboardKeyValue(Normal, "k"),
                        new KeyboardKeyValue(Accentuated, "ö"),
                        new KeyboardKeyValue(Shifted, "K"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ö")),
                    new KeyboardKey(L, Letters,
                        new KeyboardKeyValue(Normal, "l"),
                        new KeyboardKeyValue(Accentuated, "ù"),
                        new KeyboardKeyValue(Shifted, "L"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ù")),
                    new KeyboardKey(ENTER, Control, 2, "⁋", "{ENTER}"),
                    new KeyboardKey(LEFT_SHIFT, Control, 2, "⇑"),
                    new KeyboardKey(Z, Letters,
                        new KeyboardKeyValue(Normal, "z"),
                        new KeyboardKeyValue(Accentuated, "ú"),
                        new KeyboardKeyValue(Shifted, "Z"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ú")),
                    /* 36 */
                    new KeyboardKey(X, Letters,
                        new KeyboardKeyValue(Normal, "x"),
                        new KeyboardKeyValue(Accentuated, "û"),
                        new KeyboardKeyValue(Shifted, "X"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Û")),
                    new KeyboardKey(C, Letters,
                        new KeyboardKeyValue(Normal, "c"),
                        new KeyboardKeyValue(Accentuated, "ü"),
                        new KeyboardKeyValue(Shifted, "C"),
                        new KeyboardKeyValue(Accentuated|Shifted, "Ü")),
                    new KeyboardKey(V, Letters,
                        new KeyboardKeyValue(Normal, "v"),
                        new KeyboardKeyValue(Shifted, "V")),
                    new KeyboardKey(B, Letters,
                        new KeyboardKeyValue(Normal, "b"),
                        new KeyboardKeyValue(Shifted, "B")),
                    new KeyboardKey(N, Letters,
                        new KeyboardKeyValue(Normal, "n"),
                        new KeyboardKeyValue(Shifted, "N")),
                    new KeyboardKey(M, Letters,
                        new KeyboardKeyValue(Normal, "m"),
                        new KeyboardKeyValue(Shifted, "M")),
                    new KeyboardKey(DOT_COM, Control, 2, ".com"),
                    new KeyboardKey(POINT, Others,
                        new KeyboardKeyValue(Normal, "."),
                        new KeyboardKeyValue(Accentuated, "."),
                        new KeyboardKeyValue(Shifted, ","),
                        new KeyboardKeyValue(Accentuated|Shifted, ",")),
                    new KeyboardKey(COLON, Others,
                        new KeyboardKeyValue(Normal, ":"),
                        new KeyboardKeyValue(Accentuated, ":"),
                        new KeyboardKeyValue(Shifted, ";"),
                        new KeyboardKeyValue(Accentuated|Shifted, ";")),
                    new KeyboardKey(APOSTROPHE, Others, "'"),
                    new KeyboardKey(ACCENTS_LETTERS, Others, "àéç"),
                    new KeyboardKey(SPACE, Others,5, " "),
                    /* 48 */
                    new KeyboardKey(ARROW_LEFT, Control, "⏴", "{LEFT}"),
                    new KeyboardKey(ARROW_RIGHT, Control, "⏵", "{RIGHT}"),
                    new KeyboardKey(ARROW_UP, Control, "⏶", "{UP}"),
                    new KeyboardKey(ARROW_DOWN, Control, "⏷", "{DOWN}")
                };
                _normalKeys = keys.ToArray();

                return _normalKeys;
            }
        }

        /// <summary>
        /// Clavier avec accents et position de clés US. N'a pas de clé pour les accents, just le "Shift", "Caps Lock"
        /// https://upload.wikimedia.org/wikipedia/commons/thumb/3/37/ANSI_Keyboard_Layout_Diagram_with_Form_Factor.svg/1313px-ANSI_Keyboard_Layout_Diagram_with_Form_Factor.svg.png
        /// </summary>
        public static KeyboardKey[] KeyboardUtf8Us
        {
            get
            {
                if (_usKeys != null) return _usKeys;

                var keys = new List<KeyboardKey>
                {
                    // Rangées contiennent 14 boutons de large.
                    
                    /* Rangée 1 */
                    new KeyboardKey(SINGLE_QUOTE, Others, 
                        new KeyboardKeyValue(Normal, "`"),
                        new KeyboardKeyValue(Shifted, "~", "{~}")),
                    new KeyboardKey(ONE, Others,
                        new KeyboardKeyValue(Normal, "1"),
                        new KeyboardKeyValue(Shifted, "!")),
                    new KeyboardKey(TWO, Others,
                        new KeyboardKeyValue(Normal, "2"),
                        new KeyboardKeyValue(Shifted, "@")),
                    new KeyboardKey(THREE, Others,
                        new KeyboardKeyValue(Normal, "3"),
                        new KeyboardKeyValue(Shifted, "#")),
                    new KeyboardKey(FOUR, Others, 
                        new KeyboardKeyValue(Normal, "4"),
                        new KeyboardKeyValue(Shifted, "$")),
                    new KeyboardKey(FIVE, Others, 
                        new KeyboardKeyValue(Normal, "5"),
                        new KeyboardKeyValue(Shifted, "%", "{%}")),
                    new KeyboardKey(SIX, Others,
                        new KeyboardKeyValue(Normal, "6"),
                        new KeyboardKeyValue(Shifted, "^", "{^}")),
                    new KeyboardKey(SEVEN, Others,
                        new KeyboardKeyValue(Normal, "7"),
                        new KeyboardKeyValue(Shifted, "&")),
                    new KeyboardKey(EIGHT, Others,
                        new KeyboardKeyValue(Normal, "8"),
                        new KeyboardKeyValue(Shifted, "*")),
                    new KeyboardKey(NINE, Others,
                        new KeyboardKeyValue(Normal, "9"),
                        new KeyboardKeyValue(Shifted, "(", "{(}")),
                    new KeyboardKey(ZERO, Others,
                        new KeyboardKeyValue(Normal, "0"),
                        new KeyboardKeyValue(Shifted, ")", "{)}")),
                    new KeyboardKey(HYPHEN_MINUS, Others,
                        new KeyboardKeyValue(Normal, "-"),
                        new KeyboardKeyValue(Shifted, "_")),
                    new KeyboardKey(BACKSPACE, Control, 2, "Delete", "{BACKSPACE}"),
                    
                    /* Rangée 2 */
                    new KeyboardKey(EQUAL, Others,
                        new KeyboardKeyValue(Normal, "="),
                        new KeyboardKeyValue(Shifted, "+", "{+}")),
                    new KeyboardKey(Q, Letters,
                        new KeyboardKeyValue(Normal, "q"),
                        new KeyboardKeyValue(Shifted, "Q")),
                    new KeyboardKey(W, Letters,
                        new KeyboardKeyValue(Normal, "w"),
                        new KeyboardKeyValue(Shifted, "W")),
                    new KeyboardKey(E, Letters,
                        new KeyboardKeyValue(Normal, "e"),
                        new KeyboardKeyValue(Shifted, "E")),
                    new KeyboardKey(R, Letters,
                        new KeyboardKeyValue(Normal, "r"),
                        new KeyboardKeyValue(Shifted, "R")),
                    new KeyboardKey(T, Letters,
                        new KeyboardKeyValue(Normal, "t"),
                        new KeyboardKeyValue(Shifted, "T")),
                    new KeyboardKey(Y, Letters,
                        new KeyboardKeyValue(Normal, "y"),
                        new KeyboardKeyValue(Shifted, "Y")),
                    new KeyboardKey(U, Letters,
                        new KeyboardKeyValue(Normal, "u"),
                        new KeyboardKeyValue(Shifted, "U")),
                    new KeyboardKey(I, Letters,
                        new KeyboardKeyValue(Normal, "i"),
                        new KeyboardKeyValue(Shifted, "I")),
                    new KeyboardKey(O, Letters,
                        new KeyboardKeyValue(Normal, "o"),
                        new KeyboardKeyValue(Shifted, "O")),
                    new KeyboardKey(P, Letters,
                        new KeyboardKeyValue(Normal, "p"),
                        new KeyboardKeyValue(Shifted, "P")),
                    new KeyboardKey(LEFT_BRACKET, Others,
                        new KeyboardKeyValue(Normal, "[", "{[}"),
                        new KeyboardKeyValue(Shifted, "{", "{{}")),
                    new KeyboardKey(RIGHT_BRACKET, Others,
                        new KeyboardKeyValue(Normal, "]", "{]}"),
                        new KeyboardKeyValue(Shifted, "}", "{}}")),
                    new KeyboardKey(BACK_SLASH, Others,
                        new KeyboardKeyValue(Normal, "\\"),
                        new KeyboardKeyValue(Shifted, "|")),
                    
                    /* Rangée 3 */
                    new KeyboardKey(CAPS_LOCK, Control, 2, "Uppercase", "{CAPSLOCK}"),
                    new KeyboardKey(A, Letters,
                        new KeyboardKeyValue(Normal, "a"),
                        new KeyboardKeyValue(Shifted, "A")),
                    new KeyboardKey(S, Letters,
                        new KeyboardKeyValue(Normal, "s"),
                        new KeyboardKeyValue(Shifted, "S")),
                    new KeyboardKey(D, Letters,
                        new KeyboardKeyValue(Normal, "d"),
                        new KeyboardKeyValue(Shifted, "D")),
                    new KeyboardKey(F, Letters,
                        new KeyboardKeyValue(Normal, "f"),
                        new KeyboardKeyValue(Shifted, "F")),
                    new KeyboardKey(G, Letters,
                        new KeyboardKeyValue(Normal, "g"),
                        new KeyboardKeyValue(Shifted, "G")),
                    new KeyboardKey(H, Letters,
                        new KeyboardKeyValue(Normal, "h"),
                        new KeyboardKeyValue(Shifted, "H")),
                    new KeyboardKey(J, Letters,
                        new KeyboardKeyValue(Normal, "j"),
                        new KeyboardKeyValue(Shifted, "J")),
                    new KeyboardKey(K, Letters,
                        new KeyboardKeyValue(Normal, "k"),
                        new KeyboardKeyValue(Shifted, "K")),
                    new KeyboardKey(L, Letters,
                        new KeyboardKeyValue(Normal, "l"),
                        new KeyboardKeyValue(Shifted, "L")),
                    new KeyboardKey(SEMICOLON, Others,
                        new KeyboardKeyValue(Normal, ";"),
                        new KeyboardKeyValue(Shifted, ":")),
                    new KeyboardKey(APOSTROPHE, Others,
                        new KeyboardKeyValue(Normal, "`"),
                        new KeyboardKeyValue(Shifted, "\"")),
                    new KeyboardKey(AT, Others, "@"),
                    
                    /* Rangée 4 */
                    new KeyboardKey(LEFT_SHIFT, Control, 2, "⇑"),
                    new KeyboardKey(Z, Letters,
                        new KeyboardKeyValue(Normal, "z"),
                        new KeyboardKeyValue(Shifted, "Z")),
                    new KeyboardKey(X, Letters,
                        new KeyboardKeyValue(Normal, "x"),
                        new KeyboardKeyValue(Shifted, "X")),
                    new KeyboardKey(C, Letters,
                        new KeyboardKeyValue(Normal, "c"),
                        new KeyboardKeyValue(Shifted, "C")),
                    new KeyboardKey(V, Letters,
                        new KeyboardKeyValue(Normal, "v"),
                        new KeyboardKeyValue(Shifted, "V")),
                    new KeyboardKey(B, Letters,
                        new KeyboardKeyValue(Normal, "b"),
                        new KeyboardKeyValue(Shifted, "B")),
                    new KeyboardKey(N, Letters,
                        new KeyboardKeyValue(Normal, "n"),
                        new KeyboardKeyValue(Shifted, "N")),
                    new KeyboardKey(M, Letters,
                        new KeyboardKeyValue(Normal, "m"),
                        new KeyboardKeyValue(Shifted, "M")),
                    new KeyboardKey(COMMA, Others,
                        new KeyboardKeyValue(Normal, ","),
                        new KeyboardKeyValue(Shifted, "<")),
                    new KeyboardKey(PERIOD, Others,
                        new KeyboardKeyValue(Normal, "."),
                        new KeyboardKeyValue(Shifted, ">")),
                    new KeyboardKey(SLASH, Others,
                        new KeyboardKeyValue(Normal, "/"),
                        new KeyboardKeyValue(Shifted, "?")),
                    new KeyboardKey(DOT_COM, Others, 2, ".com"),

                    /* Rangée 5 */
                    new KeyboardKey(SPACE, Others, 10, " "),
                    new KeyboardKey(ARROW_LEFT, Control, "⏴", "{LEFT}"),
                    new KeyboardKey(ARROW_RIGHT, Control, "⏵", "{RIGHT}"),
                    new KeyboardKey(ARROW_UP, Control, "⏶", "{UP}"),
                    new KeyboardKey(ARROW_DOWN, Control, "⏷", "{DOWN}")
                };
                _usKeys = keys.ToArray();

                return _usKeys;
            }
        }

        /// <summary>
        /// Ce clavier est un clavier simplifié avec des caractères destinés à un format courriel. Il contient plusieures touches "macro" pour compléter les
        /// noms de domaines fréquents.
        /// </summary>
        public static KeyboardKey[] KeyboardUtf8Email
        {
            get
            {
                if (_emailKeys != null) return _emailKeys;

                var keys = new List<KeyboardKey>
                {
                    // la matrice de clés est 16x4

                    #region Rangée 1
                    new KeyboardKey(GMAIL_COM, Others, 2, "@gmail.com"),
                    new KeyboardKey(YAHOO_COM, Others, 2, "@yahoo.com"),
                    new KeyboardKey(HOTMAIL_COM, Others, 2, "@hotmail.com"),
                    new KeyboardKey(OUTLOOK_COM, Others, 2, "@outlook.com"),
                    new KeyboardKey(ICLOUD_COM, Others, 2, "@icloud.com"),
                    new KeyboardKey(BACKSPACE, Control, 2, "Backspace", "{BACKSPACE}"),
                    new KeyboardKeyNone(), // Colonne 13
                    new KeyboardKey(ONE, Others, "1"),
                    new KeyboardKey(TWO, Others, "2"),
                    new KeyboardKey(THREE, Others, "3"),
                    #endregion

                    #region Rangée 2
                    new KeyboardKey(PLUS, Others, "+", "{+}"),
                    new KeyboardKey(Q, Letters, new KeyboardKeyValue(Normal, "q"), new KeyboardKeyValue(Accentuated, "à")),
                    new KeyboardKey(W, Letters, new KeyboardKeyValue(Normal, "w"), new KeyboardKeyValue(Accentuated, "á")),
                    new KeyboardKey(E, Letters, new KeyboardKeyValue(Normal, "e"), new KeyboardKeyValue(Accentuated, "â")),
                    new KeyboardKey(R, Letters, new KeyboardKeyValue(Normal, "r"), new KeyboardKeyValue(Accentuated, "ã")),
                    new KeyboardKey(T, Letters, new KeyboardKeyValue(Normal, "t"), new KeyboardKeyValue(Accentuated, "ç")),
                    new KeyboardKey(Y, Letters, new KeyboardKeyValue(Normal, "y"), new KeyboardKeyValue(Accentuated, "è")),
                    new KeyboardKey(U, Letters, new KeyboardKeyValue(Normal, "u"), new KeyboardKeyValue(Accentuated, "é")),
                    new KeyboardKey(I, Letters, new KeyboardKeyValue(Normal, "i"), new KeyboardKeyValue(Accentuated, "ê")),
                    new KeyboardKey(O, Letters, new KeyboardKeyValue(Normal, "o"), new KeyboardKeyValue(Accentuated, "ë")),
                    new KeyboardKey(P, Letters, new KeyboardKeyValue(Normal, "p"), new KeyboardKeyValue(Accentuated, "*")),
                    new KeyboardKey(MINUS, Others, "-"),
                    new KeyboardKeyNone(), // Colonne 13
                    new KeyboardKey(FOUR, Others, "4"),
                    new KeyboardKey(FIVE, Others, "5"),
                    new KeyboardKey(SIX, Others, "6"),
                    #endregion
                    
                    #region Rangée 3
                    new KeyboardKeyNone(),
                    new KeyboardKey(A, Letters, new KeyboardKeyValue(Normal, "a"), new KeyboardKeyValue(Accentuated, "î")),
                    new KeyboardKey(S, Letters, new KeyboardKeyValue(Normal, "s"), new KeyboardKeyValue(Accentuated, "ï")),
                    new KeyboardKey(D, Letters, new KeyboardKeyValue(Normal, "d"), new KeyboardKeyValue(Accentuated, "ñ")),
                    new KeyboardKey(F, Letters, new KeyboardKeyValue(Normal, "f"), new KeyboardKeyValue(Accentuated, "ò")),
                    new KeyboardKey(G, Letters, new KeyboardKeyValue(Normal, "g"), new KeyboardKeyValue(Accentuated, "ó")),
                    new KeyboardKey(H, Letters, new KeyboardKeyValue(Normal, "h"), new KeyboardKeyValue(Accentuated, "ô")),
                    new KeyboardKey(J, Letters, new KeyboardKeyValue(Normal, "j"), new KeyboardKeyValue(Accentuated, "õ")),
                    new KeyboardKey(K, Letters, new KeyboardKeyValue(Normal, "k"), new KeyboardKeyValue(Accentuated, "$")),
                    new KeyboardKey(L, Letters, new KeyboardKeyValue(Normal, "l"), new KeyboardKeyValue(Accentuated, "%","{%}")),
                    new KeyboardKey(UNDERSCORE, Others, "_"),
                    new KeyboardKey(AT, Others, "@"),
                    new KeyboardKeyNone(), // Colonne 13
                    new KeyboardKey(SEVEN, Others, "7"),
                    new KeyboardKey(EIGHT, Others, "8"),
                    new KeyboardKey(NINE, Others, "9"),
                    #endregion
                    
                    #region Rangée 4
                    new KeyboardKey(ACCENTS_LETTERS, Others, 2, "àéç"),
                    new KeyboardKey(Z, Letters, new KeyboardKeyValue(Normal, "z"), new KeyboardKeyValue(Accentuated, "ù")),
                    new KeyboardKey(X, Letters, new KeyboardKeyValue(Normal, "x"), new KeyboardKeyValue(Accentuated, "ú")),
                    new KeyboardKey(C, Letters, new KeyboardKeyValue(Normal, "c"), new KeyboardKeyValue(Accentuated, "û")),
                    new KeyboardKey(V, Letters, new KeyboardKeyValue(Normal, "v"), new KeyboardKeyValue(Accentuated, "#")),
                    new KeyboardKey(B, Letters, new KeyboardKeyValue(Normal, "b"), new KeyboardKeyValue(Accentuated, "!")),
                    new KeyboardKey(N, Letters, new KeyboardKeyValue(Normal, "n"), new KeyboardKeyValue(Accentuated, "?")),
                    new KeyboardKey(M, Letters, new KeyboardKeyValue(Normal, "m"), new KeyboardKeyValue(Accentuated, "&")),
                    new KeyboardKey(PERIOD, Others, "."),
                    new KeyboardKey(DOT_COM, Others, 2, ".com"),
                    new KeyboardKeyNone(), // Colonne 13
                    new KeyboardKey(ZERO, Others, 3, "0"),
                    #endregion
                };
                _emailKeys = keys.ToArray();

                return _emailKeys;
            }
        }
    }

    /// <summary>
    /// Indique le type de clé, qui indique comment la clé réagit sur un modificateur.
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// Les lettres réagissent au modificateur CAP LOCK, mais pas les autres touches (incluant les chiffres)
        /// </summary>
        Letters,
        /// <summary>
        /// Une touche de contrôle à souvent une fonction spéciale, et son utilisation ne débloque pas l'état "shift".
        /// </summary>
        Control,
        /// <summary>
        /// Tout les autres types de touches, symboles et chiffres inclus.
        /// </summary>
        Others
    }

    [SuppressMessage("ReSharper", "MissingXmlDoc")]
    [PublicAPI]
    public enum Key
    {
        // Clé vide, n'apparaît pas à l'écran et n'est pas existante en tant que bouton.
        NONE,
        AT, // Arobas, "@"
        ONE,
        TWO,
        THREE,
        FOUR,
        FIVE,
        SIX,
        SEVEN,
        EIGHT,
        NINE,
        ZERO,
        BACKSPACE,
        TAB,
        Q,
        W,
        E,
        R,
        T,
        Y,
        U,
        I,
        O,
        P,
        PLUS,
        EQUAL,
        UNDERSCORE,
        MINUS,
        CAPS_LOCK,
        A,
        S,
        D,
        F,
        G,
        H,
        J,
        K,
        L,
        ENTER,
        LEFT_SHIFT,
        Z,
        X,
        C,
        V,
        B,
        N,
        M,
        POINT,
        COLON,
        APOSTROPHE,
        ACCENTS_LETTERS,
        SPACE,
        ARROW_LEFT,
        ARROW_RIGHT,
        ARROW_UP,
        ARROW_DOWN,
        SLASH,
        ASTERISK,
        SQUARE, // Aussi appellé "dièse", "pound" ou "hashtag", mais le vrai nom est "octothorp"
        BACK_SLASH,
        LEFT_BRACKET,
        RIGHT_BRACKET,
        HYPHEN,
        DASH, // N'est pas équivalent à un "hyphen"
        HYPHEN_MINUS,
        SEMICOLON,
        COMMA,
        SINGLE_QUOTE,
        PERIOD,
        /* Codes spéciaux pour touches composites, soit plus que un caractères. */
        DOT_COM,
        GMAIL_COM,
        YAHOO_COM,
        HOTMAIL_COM,
        OUTLOOK_COM,
        ICLOUD_COM
    }
}
