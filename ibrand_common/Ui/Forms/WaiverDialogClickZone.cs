using System.Drawing;
using System.IO;

namespace NomadLogic.Common.Ui.Forms
{
    /// <summary>
    /// Structure pour établir une zone cliquable avec un fichier HTML particulier, dans le but de faire
    /// apparaître un dialogue "Waiver" 
    /// </summary>
    public struct WaiverDialogClickZone
    {
        public WaiverDialogClickZone(FileInfo file, Rectangle zone)
        {
            HtmlFile = file;
            ClickZone = zone;
        }

        public FileInfo HtmlFile { get; private set; }
        public Rectangle ClickZone { get; private set; }
    }
}