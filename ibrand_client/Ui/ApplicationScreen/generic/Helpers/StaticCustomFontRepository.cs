using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using NLog;
using NomadLogic.Common.Util;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.Generic.Helpers
{
    /// <summary>
    /// Comme un objet 'Font' doit être utilisé sur le même fil d'exécution qu'il a été instancié,
    /// et que la plupart du temps ce fil est le fil principal UI, cette classe a été créée pour stocker au même endroit
    /// toutes les instances de font pour le UI dans un projet plein écran.
    /// Comme cette classe est statique, elle fonctionne comme un Singleton.
    /// </summary>
    public static class StaticCustomFontRepository
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static readonly IDictionary<int, Font> CUSTOM_FONTS = new Dictionary<int, Font>();
        private static readonly HashSet<string> FONT_FILE_USED = new HashSet<string>();
        private static readonly PrivateFontCollection FONT_COLLECTION = new PrivateFontCollection();

        /// <summary>
        /// Créé une instance de fonte complète selon une instance de définition. Si nulle, on retourne une fonte basique, avec comme hauteur
        /// le deuxième paramètre.
        /// Cette fonte par défaut ne sera pas mise en mémoire pour éviter les duplicats.
        /// </summary>
        /// <param name="fdi">L'instance de définition.</param>
        /// <param name="height">Hauteur du fonte basique.</param>
        /// <returns>Une nouvelle instance de fonte.</returns>
        [Obsolete]
        public static Font CreateCustomFont(FontDefinitionInfo fdi, float height)
        {
            return fdi == null ? new Font("Times", height) : CreateCustomFont(fdi);
        }

        /// <summary>
        /// Créé une instance de fonte basée sur des paramètres personalisés. La fonte résultante sera mise en mémoire, indexée
        /// par le hash de la définition.
        /// </summary>
        /// <param name="fdi">La définition de fonte à créer.</param>
        /// <returns>Retourne la fonte créée. Si null, l'argument était null ou il est impossible de créé la fonte.</returns>
        public static Font CreateCustomFont(FontDefinitionInfo fdi)
        {
            if (fdi == null) return null;

            var hashCode = fdi.GetHashCode();
            if (CUSTOM_FONTS.TryGetValue(hashCode, out var cachedFont)) return cachedFont;

            FontFamily fontFam;
            if (fdi.FontFile != null)
            {
                var fontFileInfo = new FileInfo(fdi.FontFile);
                if (!fontFileInfo.Exists)
                {
                    log.Warn("Custom Font file not found: {0}" + fontFileInfo.FullName);
                    return null;
                }

                AddFontFromFile(fontFileInfo);
                fontFam = GetFontFamilly(fdi.FamilyName);
            }
            else
            {
                fontFam = new FontFamily(fdi.FamilyName);
            }

            var createdFont = new Font(fontFam, fdi.Size, fdi.ParseStyleValueToEnum(), fdi.GraphicsUnit);

            CUSTOM_FONTS.Add(hashCode, createdFont);

            return createdFont;
        }

        public static void AddFontFromFile(FileInfo fontFileInfo)
        {
            if (fontFileInfo == null || !fontFileInfo.Exists) return;

            var fontFile = fontFileInfo.FullName;

            if (FONT_FILE_USED.Contains(fontFile)) return;
            FONT_FILE_USED.Add(fontFile);
            FONT_COLLECTION.AddFontFile(fontFile);
        }

        public static FontFamily GetFontFamilly(string name)
        {
            return FONT_COLLECTION.Families.FirstOrDefault(fontCheck => fontCheck.Name.Contains(name));
        }

        public static void ClearAll()
        {
            FONT_FILE_USED.Clear();
            FONT_COLLECTION.Dispose();
            CUSTOM_FONTS.Clear();
        }

        /// <summary>
        /// Créer un "Brush" basé sur une définition de fonte pour dessiner en GDI+.
        /// </summary>
        /// <param name="fdi">La définition de fonte.</param>
        /// <returns>Une instance de la brosse de dessein voulu.</returns>
        public static Brush CreateCustomTextBrush(FontDefinitionInfo fdi)
        {
            return fdi == null ? new SolidBrush(Color.Black) : new SolidBrush(fdi.Color);
        }
    }
}