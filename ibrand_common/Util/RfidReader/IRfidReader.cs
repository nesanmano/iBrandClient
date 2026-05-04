using System;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NomadLogic.Common.Ui.NomadScreen;
using NomadLogic.Common.Util.Json;
using NomadLogic.Common.Util.String;

namespace NomadLogic.Common.Util.RfidReader
{
    /// <summary>
    /// Interface de base pour un lecteur RFID.
    /// </summary>
    [PublicAPI]
    [JsonConverter(typeof(RfidReaderConverter))]
    public interface IRfidReader : IDisposable, IExternalFileConfig, IJsonConfig, IInitialisable
    {
        /// <summary>
        /// Initialise l'instance. Tente à ce moment de faire la jonction matérielle avec l'équipement, si approprié.
        /// </summary>
        /// <returns>TRUE si l'instance est prête à l'utilisation.</returns>
        bool ConnectReader();

        /// Indique le temps minimal entre la relecture de la même carte. Par défaut, ce temps est infini, donc on ne lit pas deux fois de suite la même carte.
        /// Une valeur négative à le même effet que zéro, soit une lecture constante de la même carte très rapidement.
        TimeSpan TimeBetweenSameCardRead { get; set; }

        /// Temps minimal entre les lectures. Par défaut, 3 secondes.
        TimeSpan TimeBetweenRead { get; set; }

        /// <summary>
        /// Certains lecteurs doivent faire plusieures transformations sur ce qui est lu par le lecteur.
        /// Parfois il sagit d'un encodage hexadécial, parfois d'unpassage binaire à ascii, etc...
        /// De plus, il est parfois demandé de modifier par la suite ce résultat, et/ou de le valider.
        /// Comme la façon de faire et l'ordre des opérations peut varier selon la nature du lecteur, cette méthode est la principal point où l'on fait toutes ces
        /// opérations.
        /// </summary>
        /// <param name="sequenceRead">La valeur brute lue par le lecteur.</param>
        /// <returns>Une strtucture avec les résultats des analyses et transformations effectuées sur la séquence.</returns>
        RfidTransformationResult ExecuteSequenceAnalisys(string sequenceRead);

        /// Certains lecteurs envoient des caractères comme claviers. Si c'est le cas, on peut indiquer ici combien de caractères forment un code à valider.
        int ScanTriggerOnCharacterNumber { get; }

        /// <summary>
        /// Indique que l'on valide quand le caractère "retour" est détecté. À préséance sur le nombre de caractères.
        /// </summary>
        bool ScanTriggerOnEnter { get; set; }

        /// <summary>
        /// Indique que la connexion est prête et que le module peut envoyer des événements.
        /// </summary>
        /// <returns></returns>
        bool IsReady();

        /// <summary>
        /// Classe pour ajouter un préfixe ou modifier la case du code.
        /// </summary>
        DatePrefixGenerator PrefixGenerator { get; set; }

        /// <summary>
        /// Si on veut vérifier qu'un code lu (avant préfixe, mais après transformation par le lecteur) possède une forme précise vérifiable par REGEX.
        /// Si null, aucune vérification n'est faite.
        /// Cette étape est préalable à la vérification d'une liste pré-établie, et quoique un peu redondante, peut être utilisé simultanément (par exemple, pour éviter une lourde recherche si le format est clairement invalide).
        /// </summary>
        RegexWrapper RegexValidation { get; set; }

        /// <summary>
        /// Indique que l'on veut modifier la case du code (avant ajout préfixe).
        /// Par défaut: aucune modification.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        CaseTransformationEnum CaseTransformation { get; set; }
    }

    /// <summary>
    /// Énumération des possibles changements de case.
    /// </summary>
    public enum CaseTransformationEnum
    {
        None,
        ToLowerCase,
        ToUpperCase
    }

    public struct RfidTransformationResult
    {
        /// <summary>
        /// Indique si le code en cractère était conforme à une forme de validation Regex.
        /// S'il n'y avait aucune validation regex, cette valeur sera à vrai.
        /// </summary>
        public bool IsRegexValid { get; set; }

        /// <summary>
        /// Le code tel que lu de façon brute par le lecteur. Selon la nature du lecteur, peut être binaire, haxedécimale, inversée, etc...
        /// Si cette valeur est égale à la valeur en caractère, c'est que le lecteur, au niveau matériel, fait déjà cette conversion.
        /// </summary>
        public string RawCode { get; set; }

        /// <summary>
        /// Le code tel qu'il devrait apparaître après la conversion matérielle, lisible par un humain.
        /// </summary>
        public string StringCode { get; set; }

        /// <summary>
        /// Si on ajoute des préfixes, modifications
        /// </summary>
        public string FinalCode { get; set; }
    }
}