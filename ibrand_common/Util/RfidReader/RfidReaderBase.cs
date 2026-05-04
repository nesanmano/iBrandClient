using System;
using NomadLogic.Common.Util.String;

namespace NomadLogic.Common.Util.RfidReader
{
    /// <summary>
    /// Implémentation de base pour un lecteur simple, probablement un émulateur clavier, sans événements systèmes et configuration matérielle inexistante.
    /// </summary>
    public abstract class RfidReaderBase : IRfidReader
    {
        #region Properties
        public DatePrefixGenerator PrefixGenerator { get; set; }
        public RegexWrapper RegexValidation { get; set; }
        public CaseTransformationEnum CaseTransformation { get; set; } = CaseTransformationEnum.None;
        public TimeSpan TimeBetweenSameCardRead { get; set; }
        public TimeSpan TimeBetweenRead { get; set; }
        public int ScanTriggerOnCharacterNumber { get; set; }
        public bool ScanTriggerOnEnter { get; set; }
        #endregion

        public virtual void Dispose() { }

        public string ConfigFileName { get; set; }
       
        public abstract string DefaultConfigFileName { get; }
        
        public virtual bool Init()
        {
            return true; // Vrai par défaut
        }

        public virtual bool ConnectReader()
        {
            return true; // Vrai par défaut
        }
        
        public RfidTransformationResult ExecuteSequenceAnalisys(string sequenceRead)
        {
            var rez = new RfidTransformationResult
            {
                RawCode = sequenceRead,
                // Basé sur le concept de "template method" on fait les analises et transformations base ici, et on laisse les trucs bizarre aux implémentations enfants.
                StringCode = TransformSequence(sequenceRead)
            };
            
            switch (CaseTransformation)
            {
                case CaseTransformationEnum.ToLowerCase:
                    rez.FinalCode = rez.StringCode.ToLowerInvariant();
                    break;
                case CaseTransformationEnum.ToUpperCase:
                    rez.FinalCode = rez.StringCode.ToUpperInvariant();
                    break;
                
                // Cas où on prend la valeur comme telle.
                case CaseTransformationEnum.None:
                default:
                    rez.FinalCode = rez.StringCode;
                    break;
            }

            rez.IsRegexValid = RegexValidation?.ExactMatch(rez.StringCode) ?? true;

            // À partir d'ici on fait des transformations subséquentes sur le code final.
            if (PrefixGenerator != null)
            {
                rez.FinalCode = PrefixGenerator.ModifyString(rez.FinalCode);
            }

            return rez;
        }
        
        public virtual bool IsReady()
        {
            return true;
        }

        /// <summary>
        /// Transformation de la séquence brute en séquence lisible finale.
        /// </summary>
        protected virtual string TransformSequence(string rawSequence)
        {
            return rawSequence;
        }

        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}