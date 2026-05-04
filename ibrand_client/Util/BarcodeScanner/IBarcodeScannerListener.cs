namespace NomadLogic.iBrandDesktop.Util.BarcodeScanner
{
    /// <summary>
    /// Cette interface défini un écoutant quand on fait une lecture d'un code barre via un scanner Motorola.
    /// </summary>
    public interface IBarcodeScannerListener
    {
        /// <summary>
        /// Ceci est la méthode à implémenter pour réagir sur les événements de scanner.
        /// </summary>
        /// <param name="sender">L'envoyeur.</param>
        /// <param name="args">L'événement de lecture.</param>
        void Scanner_Read(object sender, ScanEventArgs args);
        /// <summary>
        /// Indique qu'il faut ajouter les informations de code barre les un après les autres. Si faux, on ne fait que remplacer la valeur en mémoire à chaque lecture.
        /// Par exemple, une implémentation possible est que quand on a rien lu, la valeur est "". Après une lecture, la valeur devient "VAL1", avec une deuxième lecture, la valeur de vient "VAL1;VAL2".
        /// La façon que l'on gère les lectures multiples varie selon les classes d'implémentations.
        /// </summary>
        bool AppendBarCode { set; }
        /// <summary>
        /// Indique si l'option d'écouter un événement de scan est actif.
        /// </summary>
        bool BarCodeActivated { get; set; }
    }
}
