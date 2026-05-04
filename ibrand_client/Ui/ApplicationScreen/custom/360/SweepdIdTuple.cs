using System;
using Newtonsoft.Json;

namespace NomadLogic.iBrandDesktop.Ui.ApplicationScreen.custom._360
{
    public class SweepdIdTuple
    {
        [JsonProperty("Item1")]
        public string Rfid { get; set; }
        
        [JsonProperty("Item2")]
        public string Description { get; set; }
        
        [JsonProperty("Item3")]
        public int CodePrix { get; set; }
        
        /// <summary>
        /// N'est plus utilisé. Utilisez la méthode statique.
        /// </summary>
        [JsonProperty("Item4")]
        [Obsolete]
        public string LocalFileName { get; set; }
        
        /// <summary>
        /// N'est plus utilisé. Utilisez la méthode statique.
        /// </summary>
        [JsonProperty("Item5")]
        [Obsolete]
        public string Text { get; set; }

        public static string GetText(int code)
        {
            switch (code)
            {
                case 0: return "1 iPad";
                case 1: return "1 carte-cadeau de la Forfaiterie d’une valeur de 100$";
                case 2: return "4 couvre-touts AlphaGene d’une valeur totale de 200 $";
                case 3: return "1 prix semence Duroc Alpha d’une valeur de 500$ valide pour 32 truies saillies";
                case 4: return "1 bouteille thermos AlphaGene";
                case 5: return "1 casquette AlphaGene";
                case 6: return "1 casquette AlphaGene";
                case 7: return "1 bloc-note AlphaGene";
                case 8: return "1 tasse AlphaGene";
                case 9: return "1 sac réutilisable AlphaGene";
                default:
                {
                    throw new System.Exception($"Aucune valeur descriptive pour le code {code}");
                }
            }
        }

        public static string GetFileName(int code)
        {
            switch (code)
            {
                case 0: return "ipad.jpg";
                case 1: return "certificat cadeau forfaiterie.png";
                case 2: return "4 couvres-toits.jpg";
                case 3: return "semence 32 truies.jpg";
                case 4: return "bouteille thermos.jpg";
                case 5: return "casquette.jpg";
                case 6: return "casquette étudiant.jpg";
                case 7: return "bloc-note et pince.jpg";
                case 8: return "tasse à café.jpg";
                case 9: return "sac réutilisable.jpg";
                default:
                {
                    throw new System.Exception($"Aucune valeur descriptive pour le code {code}");
                }
            }
        }
    }
}
