using NLog;

namespace NomadLogic.Common.Util.RfidReader
{
    /// <summary>
    /// Classe pour le lecteur "chinois", qui ne fait que écrire la valeur de la carte en valeur entière (long) comme input clavier.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class Simple_RfidReader : RfidReaderBase
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        
        public override string DefaultConfigFileName => "simple_rfid_config.json";

        public bool KeepSameByteOrder { get; set; }
        
        /// <summary>
        /// Permet l'utilisation AS IS de ce qui est entré comme texte. Par défaut, tente de faire une conversion hexadécimale.
        /// </summary>
        public bool AllowDirectHexData { get; set; }

        protected override string TransformSequence(string rawSequence)
        {
            if (string.IsNullOrWhiteSpace(rawSequence)) return rawSequence;
            if (AllowDirectHexData) return rawSequence;

            if (ScanTriggerOnCharacterNumber > 0 && rawSequence.Length >= ScanTriggerOnCharacterNumber)
            {
                return StringUtil.ReverseOrderHexString(rawSequence, KeepSameByteOrder);
            }

            log.Debug("Sequence shorter than expected. Returning it unaltered: {0}", rawSequence);
            return rawSequence;
        }
    }
}